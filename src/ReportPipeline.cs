using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FootballReport.Api;
using FootballReport.Filtering;
using FootballReport.Models;
using FootballReport.Net;
using FootballReport.Rendering;
using FootballReport.Time;

namespace FootballReport
{
    public static class ReportPipeline
    {
        public sealed record ProgressUpdate(string Step, int StepIndex, int TotalSteps, string Message);

        public sealed record ProcessedMatch(Match Match, string ReasonCategory, MatchInclusionRule.InclusionDecision Decision);

        public sealed record RunResult(
            IReadOnlyList<ProcessedMatch> Included,
            IReadOnlyList<ProcessedMatch> Excluded,
            int RawCount,
            int DedupCount,
            int BlacklistedCount,
            int NotIncludedCount,
            int FilesRendered,
            IReadOnlyList<string> RenderedFiles,
            DateTimeOffset RunAtUtc,
            bool Success,
            string? ErrorMessage);

        private sealed record FilterResult(
            IReadOnlyList<ProcessedMatch> Included,
            IReadOnlyList<ProcessedMatch> Excluded,
            int BlacklistedCount,
            int NotIncludedCount);

        private static readonly string[] Steps = { "Fetch", "Parse", "Dedup", "Filter", "Render" };

        public static async Task<RunResult> RunAsync(
            CancellationToken ct,
            IProgress<ProgressUpdate>? progress = null,
            IProgress<string>? log = null)
        {
            var nowUtc = DateTimeOffset.UtcNow;

            void Report(string step, int idx, string message)
                => progress?.Report(new ProgressUpdate(step, idx, Steps.Length, message));

            try
            {
                if (!ConnectivityCheck.HasInternetByDns())
                {
                    const string msg = "Нет доступа к DNS (offline?)";
                    log?.Report(msg);
                    return Failure(nowUtc, msg);
                }

                var config = AppConfigLoader.Load();

                if (!File.Exists(ProjectPaths.TemplateHtmlPath))
                {
                    var msg = $"Не найден HTML-шаблон: {ProjectPaths.TemplateHtmlPath}";
                    log?.Report(msg);
                    return Failure(nowUtc, msg);
                }

                if (!File.Exists(ProjectPaths.TemplateCssPath))
                {
                    var msg = $"Не найден CSS-шаблон: {ProjectPaths.TemplateCssPath}";
                    log?.Report(msg);
                    return Failure(nowUtc, msg);
                }

                var templateHtml = File.ReadAllText(ProjectPaths.TemplateHtmlPath, Encoding.UTF8);
                var templateCss = File.ReadAllText(ProjectPaths.TemplateCssPath, Encoding.UTF8);

                Report("Fetch", 1, "Запрос /api/flashscore/football/live");
                using var client = new FlashscoreClient(Secrets.ApiKey, Secrets.BaseUrl);
                var rawMatches = await client.GetTodayMatchesAsync(ct).ConfigureAwait(false);
                log?.Report($"Fetch -> OK");

                Report("Parse", 2, $"Matches: {rawMatches.Count}");

                Report("Dedup", 3, "Удаление дублей eventId");
                var deduped = Program.DedupByEventId(rawMatches)
                    .OrderBy(m => m.StartDateTimeUtc)
                    .ThenBy(m => m.EventId, StringComparer.Ordinal)
                    .ToList();
                log?.Report($"Dedup -> After dedup: {deduped.Count}");

                Report("Filter", 4, "Применение blacklist/favorites");
                var filter = FilterMatches(deduped, config);
                log?.Report($"Filter -> Included: {filter.Included.Count} | Blacklisted: {filter.BlacklistedCount}");

                Report("Render", 5, "Формирование HTML");
                var renderedFiles = new List<string>(3);
                if (filter.Included.Count > 0)
                {
                    renderedFiles = RenderReports(filter.Included.Select(i => i.Match).ToList(), templateHtml, templateCss, nowUtc);
                    log?.Report($"Render -> Rendered: {renderedFiles.Count} files (GMT+3/+4/+5)");
                }
                else
                {
                    log?.Report("Сегодня матчей нет");
                }

                return new RunResult(
                    filter.Included,
                    filter.Excluded,
                    rawMatches.Count,
                    deduped.Count,
                    filter.BlacklistedCount,
                    filter.NotIncludedCount,
                    renderedFiles.Count,
                    renderedFiles,
                    nowUtc,
                    true,
                    null);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var msg = $"Ошибка: {ex.Message}";
                log?.Report(msg);
                return Failure(nowUtc, ex.Message);
            }
        }

        private static RunResult Failure(DateTimeOffset nowUtc, string? error)
        {
            return new RunResult(
                Array.Empty<ProcessedMatch>(),
                Array.Empty<ProcessedMatch>(),
                0,
                0,
                0,
                0,
                0,
                Array.Empty<string>(),
                nowUtc,
                false,
                error);
        }

        private static FilterResult FilterMatches(IReadOnlyList<Match> matches, AppConfig config)
        {
            var included = new List<ProcessedMatch>(matches.Count);
            var excluded = new List<ProcessedMatch>(matches.Count);

            int blacklisted = 0;

            foreach (var m in matches)
            {
                var decision = MatchInclusionRule.Decide(m, config);
                var reason = BuildReason(decision);

                var processed = new ProcessedMatch(m, reason, decision);

                if (decision.Include)
                {
                    included.Add(processed);
                }
                else
                {
                    excluded.Add(processed);
                    if (decision.ExcludedByBlacklist)
                        blacklisted++;
                }
            }

            var notIncluded = Math.Max(0, excluded.Count - blacklisted);

            return new FilterResult(included, excluded, blacklisted, notIncluded);
        }

        private static string BuildReason(MatchInclusionRule.InclusionDecision decision)
        {
            if (decision == null)
                return "Unknown";

            if (decision.ExcludedByBlacklist)
                return MapBlacklistReason(decision.BlacklistHit);

            if (!decision.Include)
                return "NotFavorite";

            if (decision.FavoritesHit.IsFavoriteCompetition)
                return "Included: fav tournament";

            if (decision.FavoritesHit.IsFavoriteClubMatch)
                return "Included: fav club";

            if (decision.S3.HasFinalMarker)
                return "Included: FINAL";

            if (decision.S3.HasSemiMarker)
                return "Included: SEMI";

            if (decision.S3.HasHalfMarker)
                return "Included: 1/2";

            return decision.Reason ?? "Included";
        }

        private static string MapBlacklistReason(BlacklistMatcher.BlacklistHit? hit)
        {
            if (hit == null)
                return "Blacklisted: pattern";

            var reason = hit.Reason ?? string.Empty;

            if (reason.Contains("competition", StringComparison.OrdinalIgnoreCase))
                return "Blacklisted: competition";

            if (reason.Contains("club", StringComparison.OrdinalIgnoreCase))
                return "Blacklisted: club";

            return "Blacklisted: pattern";
        }

        private static List<string> RenderReports(
            IReadOnlyList<Match> includedUtcSorted,
            string templateHtml,
            string templateCss,
            DateTimeOffset nowUtc)
        {
            var files = new List<string>(3);

            RenderFor(TimezoneConverter.GmtPlus3, "GMT+3");
            RenderFor(TimezoneConverter.GmtPlus4, "GMT+4");
            RenderFor(TimezoneConverter.GmtPlus5, "GMT+5");

            return files;

            void RenderFor(TimeSpan offset, string tzLabel)
            {
                var reportDateLocal = nowUtc.ToOffset(offset);
                var dateStr = TimezoneConverter.FormatDateForFilename(reportDateLocal);

                var title = $"Report {dateStr} - {tzLabel}";

                var options = new HtmlReportRenderer.RenderOptions(
                    title: title,
                    timezoneLabel: tzLabel,
                    timezoneOffset: offset,
                    reportDateLocal: reportDateLocal,
                    inlineCss: templateCss
                );

                var html = HtmlReportRenderer.RenderHtml(templateHtml, includedUtcSorted, options);

                var outFileName = $"Report_{dateStr} - {tzLabel}.html";
                var outPath = Path.Combine(ProjectPaths.OutDir, outFileName);

                HtmlReportRenderer.SaveHtmlToFile(outPath, html);
                files.Add(outPath);
            }
        }
    }
}
