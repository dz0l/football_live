using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using FootballReport.Filtering;
using FootballReport.Models;
using FootballReport.Rendering;
using FootballReport.Time;

namespace FootballReport
{
    internal static class Program
    {
        private static readonly string ProjectRoot = Directory.GetCurrentDirectory();
        private static readonly string ConfigDir = Path.Combine(ProjectRoot, "config");
        private static readonly string TemplatesDir = Path.Combine(ProjectRoot, "templates");
        private static readonly string OutDir = Path.Combine(ProjectRoot, "out");
        private static readonly string TemplateHtmlPath = Path.Combine(TemplatesDir, "report_template.html");
        private static readonly string TemplateCssPath = Path.Combine(TemplatesDir, "report_styles.css");
        private static readonly string FavoritesClubsPath = Path.Combine(ConfigDir, "favorites_clubs.json");
        private static readonly string FavoritesCompetitionsPath = Path.Combine(ConfigDir, "favorites_competitions.json");
        private static readonly string AliasesClubsPath = Path.Combine(ConfigDir, "aliases_clubs.json");
        private static readonly string AliasesCompetitionsPath = Path.Combine(ConfigDir, "aliases_competitions.json");
        private static readonly string BlacklistClubsPath = Path.Combine(ConfigDir, "blacklist_clubs.json");
        private static readonly string BlacklistCompetitionsPath = Path.Combine(ConfigDir, "blacklist_competitions.json");
        private static readonly string BlacklistTextPatternsPath = Path.Combine(ConfigDir, "blacklist_text_patterns.json");

        public static async Task<int> Main(string[] args)
        {
            try
            {
                var cts = new CancellationTokenSource();
                Console.CancelKeyPress += (_, e) =>
                {
                    e.Cancel = true;
                    cts.Cancel();
                };

                var apiKey = Secrets.ApiKey;
                var baseUrl = Secrets.BaseUrl;

                if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(baseUrl))
                {
                    Console.WriteLine("Secrets.ApiKey и Secrets.BaseUrl должны быть заданы.");
                    return 2;
                }

                if (!HasInternetByDns())
                {
                    Console.WriteLine("Нет доступа к сети (DNS check).");
                    return 3;
                }

                var config = LoadAppConfig();

                if (!File.Exists(TemplateHtmlPath))
                {
                    Console.WriteLine($"Не найден HTML-шаблон: {TemplateHtmlPath}");
                    return 4;
                }
                if (!File.Exists(TemplateCssPath))
                {
                    Console.WriteLine($"Не найден файл стилей: {TemplateCssPath}");
                    return 4;
                }

                var templateHtml = File.ReadAllText(TemplateHtmlPath, Encoding.UTF8);
                var templateCss = File.ReadAllText(TemplateCssPath, Encoding.UTF8);

                var client = new Api.FlashscoreClient(apiKey, baseUrl);
                var rawMatches = await client.GetTodayMatchesAsync(cts.Token);

                var matches = DedupByEventId(rawMatches)
                    .OrderBy(m => m.StartDateTimeUtc)
                    .ThenBy(m => m.EventId, StringComparer.Ordinal)
                    .ToList();

                var included = new List<Match>(capacity: matches.Count);
                foreach (var m in matches)
                {
                    var decision = MatchInclusionRule.Decide(m, config);
                    if (decision.Include)
                        included.Add(m);
                }

                if (included.Count == 0)
                {
                    Console.WriteLine("Подходящих матчей нет.");
                    return 0;
                }

                Directory.CreateDirectory(OutDir);

                var nowUtc = DateTimeOffset.UtcNow;

                RenderAndSaveForOffset(included, templateHtml, templateCss, nowUtc, TimezoneConverter.GmtPlus3, "GMT+3");
                RenderAndSaveForOffset(included, templateHtml, templateCss, nowUtc, TimezoneConverter.GmtPlus4, "GMT+4");
                RenderAndSaveForOffset(included, templateHtml, templateCss, nowUtc, TimezoneConverter.GmtPlus5, "GMT+5");

                Console.WriteLine($"Готово. Сформировано HTML файлов: 3 (out/). Матчей: {included.Count}");
                return 0;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Операция отменена.");
                return 130;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
                Console.WriteLine(ex);
                return 1;
            }
        }

        private static void RenderAndSaveForOffset(
            IReadOnlyList<Match> includedUtcSorted,
            string templateHtml,
            string templateCss,
            DateTimeOffset nowUtc,
            TimeSpan offset,
            string tzLabel)
        {
            var reportDateLocal = nowUtc.ToOffset(offset);
            var dateStr = TimezoneConverter.FormatDateForFilename(reportDateLocal);

            var title = $"Футбол {dateStr} - {tzLabel}";

            var options = new HtmlReportRenderer.RenderOptions(
                title: title,
                timezoneLabel: tzLabel,
                timezoneOffset: offset,
                reportDateLocal: reportDateLocal,
                inlineCss: templateCss
            );

            var html = HtmlReportRenderer.RenderHtml(templateHtml, includedUtcSorted, options);

            var outFileName = $"Футбол_{dateStr} - {tzLabel}.html";
            var outPath = Path.Combine(OutDir, outFileName);

            HtmlReportRenderer.SaveHtmlToFile(outPath, html);
        }

        private static IReadOnlyList<Match> DedupByEventId(IReadOnlyList<Match> matches)
        {
            var result = new List<Match>(matches?.Count ?? 0);
            if (matches == null || matches.Count == 0)
                return result;

            var seen = new HashSet<string>(StringComparer.Ordinal);

            foreach (var m in matches)
            {
                if (m == null) continue;

                var id = (m.EventId ?? string.Empty).Trim();
                if (id.Length == 0) continue;

                if (seen.Add(id))
                    result.Add(m);
            }

            return result;
        }

        private static AppConfig LoadAppConfig()
        {
            var favClubs = new FavoritesConfig(LoadStringArray(FavoritesClubsPath));
            var favComps = new FavoritesConfig(LoadStringArray(FavoritesCompetitionsPath));

            var aliasClubs = new AliasesConfig(LoadStringMap(AliasesClubsPath));
            var aliasComps = new AliasesConfig(LoadStringMap(AliasesCompetitionsPath));

            var blClubs = new BlacklistConfig(LoadStringArray(BlacklistClubsPath));
            var blComps = new BlacklistConfig(LoadStringArray(BlacklistCompetitionsPath));
            var blText = new BlacklistConfig(LoadStringArray(BlacklistTextPatternsPath));

            return new AppConfig(
                favoriteClubs: favClubs,
                favoriteCompetitions: favComps,
                clubAliases: aliasClubs,
                competitionAliases: aliasComps,
                blacklistedClubs: blClubs,
                blacklistedCompetitions: blComps,
                blacklistedTextPatterns: blText
            );
        }

        private static IReadOnlyList<string> LoadStringArray(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Config file not found: {path}");

            var json = File.ReadAllText(path, Encoding.UTF8);

            var arr = JsonSerializer.Deserialize<List<string>>(json, new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Disallow,
                AllowTrailingCommas = false
            });

            return arr ?? new List<string>();
        }

        private static IReadOnlyDictionary<string, string> LoadStringMap(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Config file not found: {path}");

            var json = File.ReadAllText(path, Encoding.UTF8);

            var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json, new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Disallow,
                AllowTrailingCommas = false
            });

            return dict ?? new Dictionary<string, string>();
        }

        private static bool HasInternetByDns()
        {
            try
            {
                _ = System.Net.Dns.GetHostEntry("example.com");
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
