using System;
using System.Collections.Generic;
using FootballReport.Models;
using FootballReport.Normalization;

namespace FootballReport.Filtering
{
    /// <summary>
    /// Blacklist имеет ПРИОРИТЕТ №1:
    /// если матч попадает под blacklist — он исключается всегда,
    /// независимо от избранных турниров/клубов и маркеров FINAL/SEMI/1/2.
    /// </summary>
    public static class BlacklistMatcher
    {
        public sealed class BlacklistHit
        {
            public string Reason { get; }
            public string Value { get; }

            public BlacklistHit(string reason, string value)
            {
                Reason = reason;
                Value = value;
            }

            public override string ToString() => $"{Reason}: {Value}";
        }

        /// <summary>
        /// Проверяет матч на попадание под blacklist.
        /// Возвращает true + причину, если матч надо исключить.
        ///
        /// Источники проверки:
        /// - tournamentName (с учётом алиасов по турнирам)
        /// - homeName/awayName (с учётом алиасов по клубам)
        /// - общий список текстовых паттернов (ищем подстрокой)
        /// - отдельные списки blacklist клубов и турниров (тоже подстрокой)
        ///
        /// Важно: это детерминированная подстрочная проверка по NormalizeForCompare().
        /// </summary>
        public static bool IsBlacklisted(Match match, AppConfig config, out BlacklistHit? hit)
        {
            hit = null;

            if (match == null) throw new ArgumentNullException(nameof(match));
            if (config == null) throw new ArgumentNullException(nameof(config));

            // 1) Подготовка строк (с алиасами) — чтобы blacklist работал на канонических значениях тоже.
            var tournamentCanonical = TextNormalizer.ApplyAliasOrSelf(match.TournamentName, config.CompetitionAliases.Map);
            var homeCanonical = TextNormalizer.ApplyAliasOrSelf(match.HomeName, config.ClubAliases.Map);
            var awayCanonical = TextNormalizer.ApplyAliasOrSelf(match.AwayName, config.ClubAliases.Map);

            var tournamentNorm = TextNormalizer.NormalizeForCompare(tournamentCanonical);
            var homeNorm = TextNormalizer.NormalizeForCompare(homeCanonical);
            var awayNorm = TextNormalizer.NormalizeForCompare(awayCanonical);

            // 2) Проверка blacklist турниров
            if (ContainsAnyPattern(tournamentNorm, config.BlacklistedCompetitions.Items, out var compPattern))
            {
                hit = new BlacklistHit("Blacklisted competition", compPattern);
                return true;
            }

            // 3) Проверка blacklist клубов (по home/away)
            if (ContainsAnyPattern(homeNorm, config.BlacklistedClubs.Items, out var clubPatternHome))
            {
                hit = new BlacklistHit("Blacklisted club (home)", clubPatternHome);
                return true;
            }

            if (ContainsAnyPattern(awayNorm, config.BlacklistedClubs.Items, out var clubPatternAway))
            {
                hit = new BlacklistHit("Blacklisted club (away)", clubPatternAway);
                return true;
            }

            // 4) Общие текстовые паттерны — применяем ко всем трём полям
            if (ContainsAnyPattern(tournamentNorm, config.BlacklistedTextPatterns.Items, out var textPatternT))
            {
                hit = new BlacklistHit("Blacklisted text pattern (tournament)", textPatternT);
                return true;
            }

            if (ContainsAnyPattern(homeNorm, config.BlacklistedTextPatterns.Items, out var textPatternH))
            {
                hit = new BlacklistHit("Blacklisted text pattern (home)", textPatternH);
                return true;
            }

            if (ContainsAnyPattern(awayNorm, config.BlacklistedTextPatterns.Items, out var textPatternA))
            {
                hit = new BlacklistHit("Blacklisted text pattern (away)", textPatternA);
                return true;
            }

            return false;
        }

        private static bool ContainsAnyPattern(string haystackNorm, IReadOnlyList<string> patterns, out string matchedPatternRaw)
        {
            matchedPatternRaw = string.Empty;

            if (string.IsNullOrEmpty(haystackNorm))
                return false;

            if (patterns == null || patterns.Count == 0)
                return false;

            for (int i = 0; i < patterns.Count; i++)
            {
                var pRaw = patterns[i] ?? string.Empty;
                var pNorm = TextNormalizer.NormalizeForCompare(pRaw);

                if (string.IsNullOrEmpty(pNorm))
                    continue;

                // Подстрочное сравнение в нормализованном (lowercase) виде
                if (haystackNorm.Contains(pNorm, StringComparison.Ordinal))
                {
                    matchedPatternRaw = pRaw.Trim();
                    return true;
                }
            }

            return false;
        }
    }
}
