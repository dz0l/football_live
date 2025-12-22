using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FootballReport.Models;
using FootballReport.Time;

namespace FootballReport.Rendering
{
    /// <summary>
    /// Генерация печатного HTML-отчёта по шаблону templates/report_template.html.
    /// Стили отдельно: templates/report_styles.css (шаблон подключает CSS).
    ///
    /// A4 landscape, Times New Roman, 16pt — задаётся в CSS/шаблоне.
    /// </summary>
    public static class HtmlReportRenderer
    {
        public sealed class RenderOptions
        {
            public string Title { get; }
            public string TimezoneLabel { get; } // "GMT+3"
            public TimeSpan TimezoneOffset { get; }
            public DateTimeOffset ReportDateLocal { get; } // дата "в целевой таймзоне"

            public RenderOptions(string title, string timezoneLabel, TimeSpan timezoneOffset, DateTimeOffset reportDateLocal)
            {
                Title = title;
                TimezoneLabel = timezoneLabel;
                TimezoneOffset = timezoneOffset;
                ReportDateLocal = reportDateLocal;
            }
        }

        /// <summary>
        /// Рендерит HTML: подставляет плейсхолдеры в шаблоне и строит таблицу матчей.
        /// </summary>
        public static string RenderHtml(string templateHtml, IReadOnlyList<Match> matchesUtcSorted, RenderOptions options)
        {
            if (templateHtml == null) throw new ArgumentNullException(nameof(templateHtml));
            if (matchesUtcSorted == null) throw new ArgumentNullException(nameof(matchesUtcSorted));
            if (options == null) throw new ArgumentNullException(nameof(options));

            var tableRowsHtml = BuildRows(matchesUtcSorted, options.TimezoneOffset);

            // Простая подстановка плейсхолдеров (без шаблонизаторов).
            // Плейсхолдеры должны существовать в report_template.html:
            // {{TITLE}}, {{DATE}}, {{TZ}}, {{ROWS}}
            var html = templateHtml
                .Replace("{{TITLE}}", HtmlEscaper.Escape(options.Title), StringComparison.Ordinal)
                .Replace("{{DATE}}", HtmlEscaper.Escape(options.ReportDateLocal.ToString("dd.MM.yyyy")), StringComparison.Ordinal)
                .Replace("{{TZ}}", HtmlEscaper.Escape(options.TimezoneLabel), StringComparison.Ordinal)
                .Replace("{{ROWS}}", tableRowsHtml, StringComparison.Ordinal);

            return html;
        }

        /// <summary>
        /// Сохраняет HTML в файл (UTF-8).
        /// </summary>
        public static void SaveHtmlToFile(string outputPath, string html)
        {
            if (string.IsNullOrWhiteSpace(outputPath))
                throw new ArgumentException("outputPath is required.", nameof(outputPath));

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? ".");
            File.WriteAllText(outputPath, html, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }

        private static string BuildRows(IReadOnlyList<Match> matchesUtcSorted, TimeSpan offset)
        {
            var sb = new StringBuilder(capacity: Math.Max(1024, matchesUtcSorted.Count * 160));

            foreach (var m in matchesUtcSorted)
            {
                var local = TimezoneConverter.ConvertUtcToOffset(m.StartDateTimeUtc, offset);
                var timeHm = TimezoneConverter.FormatTimeHm(local);

                // ТВ-колонки на Этапе 1 пустые / "—"
                const string dash = "—";

                sb.AppendLine("<tr>");
                sb.Append("<td>").Append(HtmlEscaper.Escape(m.TournamentName)).AppendLine("</td>");
                sb.Append("<td>").Append(HtmlEscaper.Escape($"{m.HomeName} vs {m.AwayName}")).AppendLine("</td>");
                sb.Append("<td class=\"col-time\">").Append(HtmlEscaper.Escape(timeHm)).AppendLine("</td>");
                sb.Append("<td>").Append(dash).AppendLine("</td>");
                sb.Append("<td>").Append(dash).AppendLine("</td>");
                sb.Append("<td>").Append(dash).AppendLine("</td>");
                sb.AppendLine("</tr>");
            }

            return sb.ToString();
        }
    }
}
