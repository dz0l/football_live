using System;
using System.Text;
using System.Text.RegularExpressions;

namespace FootballReport.Normalization
{
    /// <summary>
    /// Единственная точка нормализации текста в проекте.
    /// Используется для:
    /// - сопоставления favorites
    /// - применения aliases
    /// - проверки blacklist
    /// - подготовки строк для S-3 (FINAL/SEMI/1/2)
    ///
    /// Важно: нормализация НЕ должна "угадывать" смысл, только приводить строки
    /// к стабильному виду для сравнения.
    /// </summary>
    public static class TextNormalizer
    {
        // Схлопываем пробелы/табуляции/переводы строк в один пробел
        private static readonly Regex MultiWhitespace = new Regex(@"\s+", RegexOptions.Compiled);

        /// <summary>
        /// Нормализует строку для точного сравнения.
        /// - Trim
        /// - Схлопывает любые whitespace в один пробел
        /// - Приводит к lower-case инвариантно
        ///
        /// Подходит для:
        /// - ключей алиасов
        /// - сравнения favorites
        /// - сравнения blacklist (подстрочное)
        /// </summary>
        public static string NormalizeForCompare(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var s = input.Trim();
            s = MultiWhitespace.Replace(s, " ");
            s = s.ToLowerInvariant();

            return s;
        }

        /// <summary>
        /// Нормализация для поиска маркеров стадий (S-3) в tournamentName:
        /// - приводит к upper-case
        /// - заменяет все whitespace на одиночный пробел
        /// - приводит разные варианты слэшей к "/"
        /// - удаляет "мусорные" повторяющиеся разделители
        ///
        /// НЕ удаляет цифры и "/" чтобы надёжно ловить "1/2".
        /// </summary>
        public static string NormalizeForStageMarkerScan(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var s = input.Trim();

            // Приводим все типы слэшей к обычному "/"
            // (встречается редко, но дешево и стабильно)
            s = s.Replace('∕', '/')
                 .Replace('／', '/')
                 .Replace('\\', '/');

            // Схлопываем whitespace
            s = MultiWhitespace.Replace(s, " ");

            // Uppercase для маркеров FINAL/SEMI/...
            s = s.ToUpperInvariant();

            // Нормализуем варианты вокруг слэша: "1 / 2" -> "1/2"
            s = Regex.Replace(s, @"\s*/\s*", "/", RegexOptions.Compiled);

            return s;
        }

        /// <summary>
        /// Утилита: безопасная проверка "подстрока содержит подстроку" с нормализацией.
        /// </summary>
        public static bool ContainsNormalized(string haystack, string needle)
        {
            if (string.IsNullOrEmpty(needle))
                return false;

            return haystack.Contains(needle, StringComparison.Ordinal);
        }

        /// <summary>
        /// Применяет алиасы по правилу:
        /// - ключи сравниваются через NormalizeForCompare
        /// - если алиас найден — возвращаем canonical как есть (но Trim)
        /// - если не найден — возвращаем исходное значение (Trim)
        /// </summary>
        public static string ApplyAliasOrSelf(string? rawValue, System.Collections.Generic.IReadOnlyDictionary<string, string> aliasMap)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
                return string.Empty;

            var trimmed = rawValue.Trim();
            if (aliasMap == null || aliasMap.Count == 0)
                return trimmed;

            var key = NormalizeForCompare(trimmed);

            // aliasMap ожидается уже в виде "raw -> canonical".
            // Чтобы сделать поиск стабильным, вызывающая сторона должна
            // при загрузке построить нормализованный индекс.
            // Но на Этапе 1 допустим и простой проход, если map маленькая.
            //
            // Здесь — вариант "простого прохода" без новых файлов:
            foreach (var kv in aliasMap)
            {
                var kNorm = NormalizeForCompare(kv.Key);
                if (kNorm == key)
                    return (kv.Value ?? string.Empty).Trim();
            }

            return trimmed;
        }
    }
}
