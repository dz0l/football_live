using System;
using System.Collections.Generic;
using FootballReport.Filtering;
using FootballReport.Models;
using Xunit;

namespace FootballReport.Tests
{
    public sealed class T_BlacklistPriority
    {
        [Fact]
        public void Blacklist_ShouldAlwaysWin_EvenIfFavoriteClubOrFinal()
        {
            // Матч "Barcelona" (избранный клуб) + "FINAL" (финал),
            // но турнир содержит Women => должен быть исключён blacklist'ом.
            var match = new Match(
                eventId: "EVT-1",
                startDateTimeUtc: DateTimeOffset.Parse("2025-12-22T18:00:00Z"),
                tournamentName: "Super Cup FINAL - Women",
                homeName: "Barcelona",
                awayName: "Real Madrid",
                homeParticipantIds: Array.Empty<string>(),
                awayParticipantIds: Array.Empty<string>()
            );

            var cfg = new AppConfig(
                favoriteClubs: new FavoritesConfig(new List<string> { "Barcelona" }),
                favoriteCompetitions: new FavoritesConfig(new List<string> { "Super Cup" }),
                clubAliases: new AliasesConfig(new Dictionary<string, string>()),
                competitionAliases: new AliasesConfig(new Dictionary<string, string>()),
                blacklistedClubs: new BlacklistConfig(new List<string>()),
                blacklistedCompetitions: new BlacklistConfig(new List<string>()),
                blacklistedTextPatterns: new BlacklistConfig(new List<string> { "Women" })
            );

            var decision = MatchInclusionRule.Decide(match, cfg);

            Assert.False(decision.Include);
            Assert.True(decision.ExcludedByBlacklist);
            Assert.NotNull(decision.BlacklistHit);
        }
    }
}
