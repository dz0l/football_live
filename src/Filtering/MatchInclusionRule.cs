using System;
using FootballReport.Models;
using FootballReport.Normalization;

namespace FootballReport.Filtering
{
    /// <summary>
    /// Итоговое правило включения матча в отчёт (Этап 1):
    ///
    /// 1) Blacklist (ПРИОРИТЕТ №1):
    ///    Если матч попал в blacklist — он исключается всегда.
    ///
    /// 2) Иначе матч включается, если выполняется ХОТЯ БЫ ОДНО:
    ///    - турнир в избранных
    ///    - участвует избранный клуб
    ///    - S-3 маркер в tournamentName: FINAL/FINALS/SEMI/SEMIFINAL/1/2
    /// </summary>
    public static class MatchInclusionRule
    {
        public sealed class InclusionDecision
        {
            public bool Include { get; }

            public bool ExcludedByBlacklist { get; }
            public BlacklistMatcher.BlacklistHit? BlacklistHit { get; }

            public FavoritesMatcher.FavoritesHit FavoritesHit { get; }

            public TournamentNameParserS3.S3ParseResult S3 { get; }

            public string Reason { get; }

            public InclusionDecision(
                bool include,
                bool excludedByBlacklist,
                BlacklistMatcher.BlacklistHit? blacklistHit,
                FavoritesMatcher.FavoritesHit favoritesHit,
                TournamentNameParserS3.S3ParseResult s3,
                string reason)
            {
                Include = include;
                ExcludedByBlacklist = excludedByBlacklist;
                BlacklistHit = blacklistHit;
                FavoritesHit = favoritesHit;
                S3 = s3;
                Reason = reason;
            }

            public override string ToString()
            {
                if (ExcludedByBlacklist)
                    return $"EXCLUDE (blacklist): {BlacklistHit}";

                return Include
                    ? $"INCLUDE: {Reason}"
                    : $"EXCLUDE: {Reason}";
            }
        }

        public static InclusionDecision Decide(Match match, AppConfig config)
        {
            if (match == null) throw new ArgumentNullException(nameof(match));
            if (config == null) throw new ArgumentNullException(nameof(config));

            // 1) Blacklist priority
            if (BlacklistMatcher.IsBlacklisted(match, config, out var hit))
            {
                // Считаем Favorites/S3 для полноты отладки
                // прозрачности логов.
                var favDbg = FavoritesMatcher.Evaluate(match, config);
                var s3Dbg = TournamentNameParserS3.Parse(match.TournamentName);

                return new InclusionDecision(
                    include: false,
                    excludedByBlacklist: true,
                    blacklistHit: hit,
                    favoritesHit: favDbg,
                    s3: s3Dbg,
                    reason: "Blacklisted"
                );
            }

            // 2) Favorites
            var fav = FavoritesMatcher.Evaluate(match, config);

            // 3) S-3 stage marker detection
            var s3 = TournamentNameParserS3.Parse(match.TournamentName);

            // 4) Inclusion logic
            if (fav.IsFavoriteCompetition)
            {
                return new InclusionDecision(
                    include: true,
                    excludedByBlacklist: false,
                    blacklistHit: null,
                    favoritesHit: fav,
                    s3: s3,
                    reason: $"Favorite competition: {fav.MatchedCompetition}"
                );
            }

            if (fav.IsFavoriteClubMatch)
            {
                return new InclusionDecision(
                    include: true,
                    excludedByBlacklist: false,
                    blacklistHit: null,
                    favoritesHit: fav,
                    s3: s3,
                    reason: $"Favorite club match: {fav.MatchedClub}"
                );
            }

            if (s3.HasAnyStageMarker)
            {
                var markerReason =
                    s3.HasFinalMarker ? "FINAL/FINALS" :
                    s3.HasSemiMarker ? "SEMI/SEMIFINAL" :
                    "1/2";

                return new InclusionDecision(
                    include: true,
                    excludedByBlacklist: false,
                    blacklistHit: null,
                    favoritesHit: fav,
                    s3: s3,
                    reason: $"Stage marker detected: {markerReason}"
                );
            }

            return new InclusionDecision(
                include: false,
                excludedByBlacklist: false,
                blacklistHit: null,
                favoritesHit: fav,
                s3: s3,
                reason: "Not in favorites and no stage marker"
            );
        }
    }
}
