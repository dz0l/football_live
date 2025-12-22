using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FootballReport.Models;
using FootballReport.Time;

namespace FootballReport.Rendering
{
    /// <summary>
    /// Рендеринг HTML-отчета по шаблону templates/report_template.html.
    /// </summary>
    public static class HtmlReportRenderer
    {
        public sealed class RenderOptions
        {
            public string Title { get; }
            public string TimezoneLabel { get; }
            public TimeSpan TimezoneOffset { get; }
            public DateTimeOffset ReportDateLocal { get; }
            public string InlineCss { get; }

            public RenderOptions(string title, string timezoneLabel, TimeSpan timezoneOffset, DateTimeOffset reportDateLocal, string inlineCss)
            {
                Title = title ?? throw new ArgumentNullException(nameof(title));
                TimezoneLabel = timezoneLabel ?? throw new ArgumentNullException(nameof(timezoneLabel));
                TimezoneOffset = timezoneOffset;
                ReportDateLocal = reportDateLocal;
                InlineCss = inlineCss ?? string.Empty;
            }
        }

        public static string RenderHtml(string templateHtml, IReadOnlyList<Match> matchesUtcSorted, RenderOptions options)
        {
            if (templateHtml == null) throw new ArgumentNullException(nameof(templateHtml));
            if (matchesUtcSorted == null) throw new ArgumentNullException(nameof(matchesUtcSorted));
            if (options == null) throw new ArgumentNullException(nameof(options));

            var tableRowsHtml = BuildRows(matchesUtcSorted, options.TimezoneOffset);
            var createdAt = options.ReportDateLocal.ToString("dd.MM.yyyy");

            var html = templateHtml
                .Replace("{{TITLE}}", HtmlEscaper.Escape(options.Title), StringComparison.Ordinal)
                .Replace("{{DATE}}", HtmlEscaper.Escape(createdAt), StringComparison.Ordinal)
                .Replace("{{TZ}}", HtmlEscaper.Escape(options.TimezoneLabel), StringComparison.Ordinal)
                .Replace("{{ROWS}}", tableRowsHtml, StringComparison.Ordinal)
                .Replace("{{CREATED_AT}}", HtmlEscaper.Escape(createdAt), StringComparison.Ordinal)
                .Replace("{{INLINE_CSS}}", options.InlineCss, StringComparison.Ordinal);

            return html;
        }

        public static void SaveHtmlToFile(string outputPath, string html)
        {
            if (string.IsNullOrWhiteSpace(outputPath))
                throw new ArgumentException("outputPath is required.", nameof(outputPath));

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? ".");
            File.WriteAllText(outputPath, html, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }

        private static string BuildRows(IReadOnlyList<Match> matchesUtcSorted, TimeSpan offset)
        {
            var sb = new StringBuilder(capacity: Math.Max(1024, matchesUtcSorted.Count * 180));

            foreach (var m in matchesUtcSorted)
            {
                var local = TimezoneConverter.ConvertUtcToOffset(m.StartDateTimeUtc, offset);
                var timeHm = TimezoneConverter.FormatTimeHm(local);

                const string dash = "—";

                sb.AppendLine("<tr>");
                sb.Append("<td class=\"col-tournament\">").Append(HtmlEscaper.Escape(m.TournamentName)).AppendLine("</td>");
                sb.Append("<td class=\"col-match\"><strong>")
                  .Append(HtmlEscaper.Escape($"{m.HomeName} v {m.AwayName}"))
                  .AppendLine("</strong></td>");
                sb.Append("<td class=\"col-time\">").Append(HtmlEscaper.Escape(timeHm)).AppendLine("</td>");
                sb.Append("<td class=\"col-provider\">").Append(dash).AppendLine("</td>");
                sb.Append("<td class=\"col-channel\">").Append(dash).AppendLine("</td>");
                sb.Append("<td class=\"col-chno\">").Append(dash).AppendLine("</td>");
                sb.AppendLine("</tr>");
            }

            return sb.ToString();
        }
    }
}
