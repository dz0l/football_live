using System;
using System.Text.RegularExpressions;

namespace FootballReport.Normalization
{
    /// <summary>
    /// Определение стадий для включения матчей без фаворитов:
    /// - FINAL / FINALS
    /// - SEMI / SEMIFINAL
    /// - 1/2
    /// - SUPER CUP (считаем финалом, одиночный матч)
    /// </summary>
    public static class TournamentNameParserS3
    {
        // Токенные проверки (чтобы FINAL не матчился внутри SEMIFINAL)
        private static readonly Regex FinalToken =
            new Regex(@"\bFINALS?\b", RegexOptions.Compiled);

        private static readonly Regex SemiToken =
            new Regex(@"\bSEMI\b|\bSEMIFINALS?\b", RegexOptions.Compiled);

        private static readonly Regex SuperCupToken =
            new Regex(@"\bSUPER\s+CUP\b", RegexOptions.Compiled);

        public sealed class S3ParseResult
        {
            public string Raw { get; }
            public string NormalizedForScan { get; }

            public string Prefix { get; }
            public string Base { get; }
            public string Suffix { get; }

            public bool HasFinalMarker { get; }
            public bool HasSemiMarker { get; }
            public bool HasHalfMarker { get; } // "1/2"

            public bool HasAnyStageMarker => HasFinalMarker || HasSemiMarker || HasHalfMarker;

            public S3ParseResult(
                string raw,
                string normalizedForScan,
                string prefix,
                string @base,
                string suffix,
                bool hasFinalMarker,
                bool hasSemiMarker,
                bool hasHalfMarker)
            {
                Raw = raw;
                NormalizedForScan = normalizedForScan;

                Prefix = prefix;
                Base = @base;
                Suffix = suffix;

                HasFinalMarker = hasFinalMarker;
                HasSemiMarker = hasSemiMarker;
                HasHalfMarker = hasHalfMarker;
            }
        }

        /// <summary>
        /// Парсинг стадий S-3.
        /// </summary>
        public static S3ParseResult Parse(string? tournamentName)
        {
            var raw = (tournamentName ?? string.Empty).Trim();

            // 1) Нормализация для поиска маркеров
            var scan = TextNormalizer.NormalizeForStageMarkerScan(raw);

            // FINAL/FINALS как отдельные токены
            var hasFinal = FinalToken.IsMatch(scan);

            // SEMI/SEMIFINAL(S) как отдельные токены
            var hasSemi = SemiToken.IsMatch(scan);

            // SUPER CUP считаем финальной стадией
            var hasSuperCup = SuperCupToken.IsMatch(scan);

            // 1/2 (NormalizeForStageMarkerScan: "1 / 2" => "1/2")
            var hasHalf = scan.Contains("1/2", StringComparison.Ordinal);

            // 2) Prefix/Base/Suffix в строке (для диагностики)
            SplitPrefixBaseSuffix(raw, out var prefix, out var @base, out var suffix);

            return new S3ParseResult(
                raw: raw,
                normalizedForScan: scan,
                prefix: prefix,
                @base: @base,
                suffix: suffix,
                hasFinalMarker: hasFinal || hasSuperCup,
                hasSemiMarker: hasSemi,
                hasHalfMarker: hasHalf
            );
        }

        private static void SplitPrefixBaseSuffix(string raw, out string prefix, out string @base, out string suffix)
        {
            prefix = string.Empty;
            @base = raw;
            suffix = string.Empty;

            if (string.IsNullOrWhiteSpace(raw))
            {
                @base = string.Empty;
                return;
            }

            const string sep = " - ";
            var first = raw.IndexOf(sep, StringComparison.Ordinal);
            if (first < 0)
                return;

            var last = raw.LastIndexOf(sep, StringComparison.Ordinal);
            if (last < 0)
                return;

            prefix = raw.Substring(0, first).Trim();
            suffix = raw.Substring(last + sep.Length).Trim();

            var middleStart = first + sep.Length;
            var middleLen = last - middleStart;

            if (middleLen <= 0)
            {
                @base = suffix; // "A - B"
            }
            else
            {
                @base = raw.Substring(middleStart, middleLen).Trim();
            }

            if (string.IsNullOrWhiteSpace(@base))
                @base = suffix;
        }
    }
}
