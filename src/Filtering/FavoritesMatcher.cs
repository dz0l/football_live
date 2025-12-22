using System;
using System.Collections.Generic;
using FootballReport.Models;
using FootballReport.Normalization;

namespace FootballReport.Filtering
{
    /// <summary>
    /// Проверка "избранного" (favorites):
    /// - Турнир в favorites_competitions.json
    /// - Любая из команд (home/away) в favorites_clubs.json
    ///
    /// Сопоставление делается детерминированно:
    /// 1) применяем алиасы (raw -> canonical)
    /// 2) NormalizeForCompare()
    /// 3) сравнение по точному совпадению нормализованных строк
    /// </summary>
    public static class FavoritesMatcher
    {
        public sealed class FavoritesHit
        {
            public bool IsFavoriteCompetition { get; }
            public bool IsFavoriteClubMatch { get; }

            public string CompetitionCanonical { get; }
            public string HomeCanonical { get; }
            public string AwayCanonical { get; }

            public string? MatchedCompetition { get; }
            public string? MatchedClub { get; } // если совпали оба клуба, возвращаем первый найденный детерминированно (home -> away)

            public FavoritesHit(
                bool isFavoriteCompetition,
                bool isFavoriteClubMatch,
                string competitionCanonical,
                string homeCanonical,
                string awayCanonical,
                string? matchedCompetition,
                string? matchedClub)
            {
                IsFavoriteCompetition = isFavoriteCompetition;
                IsFavoriteClubMatch = isFavoriteClubMatch;

                CompetitionCanonical = competitionCanonical;
                HomeCanonical = homeCanonical;
                AwayCanonical = awayCanonical;

                MatchedCompetition = matchedCompetition;
                MatchedClub = matchedClub;
            }

            public override string ToString()
            {
                return $"FavCompetition={IsFavoriteCompetition} ({MatchedCompetition ?? "-"}) | " +
                       $"FavClubMatch={IsFavoriteClubMatch} ({MatchedClub ?? "-"}) | " +
                       $"{HomeCanonical} vs {AwayCanonical}";
            }
        }

        public static FavoritesHit Evaluate(Match match, AppConfig config)
        {
            if (match == null) throw new ArgumentNullException(nameof(match));
            if (config == null) throw new ArgumentNullException(nameof(config));

            // 1) Применяем алиасы (raw -> canonical)
            var competitionClean = TextNormalizer.StripPrefixBeforeColon(match.TournamentName);
            var competitionCanonical = TextNormalizer.ApplyAliasOrSelf(competitionClean, config.CompetitionAliases.Map);
            var homeCanonical = TextNormalizer.ApplyAliasOrSelf(match.HomeName, config.ClubAliases.Map);
            var awayCanonical = TextNormalizer.ApplyAliasOrSelf(match.AwayName, config.ClubAliases.Map);

            // 2) Нормализуем для сравнения
            var compNorm = TextNormalizer.NormalizeForCompare(competitionCanonical);
            var homeNorm = TextNormalizer.NormalizeForCompare(homeCanonical);
            var awayNorm = TextNormalizer.NormalizeForCompare(awayCanonical);

            // 3) Готовим нормализованные множества favorites
            var favoriteCompetitions = BuildNormalizedSet(config.FavoriteCompetitions.Items);
            var favoriteClubs = BuildNormalizedSet(config.FavoriteClubs.Items);

            // 4) Проверка турнира
            bool isFavCompetition = false;
            string? matchedCompetition = null;

            if (!string.IsNullOrEmpty(compNorm) && favoriteCompetitions.TryGetValue(compNorm, out var compOriginal))
            {
                isFavCompetition = true;
                matchedCompetition = compOriginal;
            }

            // 5) Проверка клубов (детерминированный порядок: home -> away)
            bool isFavClubMatch = false;
            string? matchedClub = null;

            if (!string.IsNullOrEmpty(homeNorm) && favoriteClubs.TryGetValue(homeNorm, out var homeOriginal))
            {
                isFavClubMatch = true;
                matchedClub = homeOriginal;
            }
            else if (!string.IsNullOrEmpty(awayNorm) && favoriteClubs.TryGetValue(awayNorm, out var awayOriginal))
            {
                isFavClubMatch = true;
                matchedClub = awayOriginal;
            }

            return new FavoritesHit(
                isFavoriteCompetition: isFavCompetition,
                isFavoriteClubMatch: isFavClubMatch,
                competitionCanonical: competitionCanonical,
                homeCanonical: homeCanonical,
                awayCanonical: awayCanonical,
                matchedCompetition: matchedCompetition,
                matchedClub: matchedClub
            );
        }

        /// <summary>
        /// Возвращает словарь "normalized -> originalTrimmed",
        /// чтобы при совпадении мы могли вернуть красивое каноническое значение из конфигов.
        /// </summary>
        private static Dictionary<string, string> BuildNormalizedSet(IReadOnlyList<string> items)
        {
            var dict = new Dictionary<string, string>(StringComparer.Ordinal);

            if (items == null || items.Count == 0)
                return dict;

            for (int i = 0; i < items.Count; i++)
            {
                var raw = (items[i] ?? string.Empty).Trim();
                if (raw.Length == 0)
                    continue;

                var norm = TextNormalizer.NormalizeForCompare(raw);
                if (norm.Length == 0)
                    continue;

                // Детерминированность: при коллизиях оставляем первое встретившееся.
                if (!dict.ContainsKey(norm))
                    dict[norm] = raw;
            }

            return dict;
        }
    }
}
