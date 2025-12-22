using System;

namespace FootballReport.Rendering
{
    /// <summary>
    /// Минимальное экранирование HTML (чтобы не ломать документ).
    /// Без внешних зависимостей.
    /// </summary>
    public static class HtmlEscaper
    {
        public static string Escape(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            // Порядок важен: сначала & потом остальное
            return value
                .Replace("&", "&amp;", StringComparison.Ordinal)
                .Replace("<", "&lt;", StringComparison.Ordinal)
                .Replace(">", "&gt;", StringComparison.Ordinal)
                .Replace("\"", "&quot;", StringComparison.Ordinal)
                .Replace("'", "&#39;", StringComparison.Ordinal);
        }
    }
}
