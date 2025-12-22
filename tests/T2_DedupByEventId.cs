using System;
using System.Collections.Generic;
using System.Reflection;
using FootballReport.Models;
using Xunit;

namespace FootballReport.Tests
{
    public sealed class T2_DedupByEventId
    {
        [Fact]
        public void Dedup_ShouldKeepFirstOccurrence_AndRemoveDuplicatesByEventId()
        {
            var m1 = new Match(
                eventId: "E1",
                startDateTimeUtc: DateTimeOffset.Parse("2025-12-22T10:00:00Z"),
                tournamentName: "Test League",
                homeName: "A",
                awayName: "B",
                homeParticipantIds: Array.Empty<string>(),
                awayParticipantIds: Array.Empty<string>()
            );

            // Дубликат по EventId = E1, но другое время/названия
            var m1dup = new Match(
                eventId: "E1",
                startDateTimeUtc: DateTimeOffset.Parse("2025-12-22T11:00:00Z"),
                tournamentName: "Another League",
                homeName: "X",
                awayName: "Y",
                homeParticipantIds: Array.Empty<string>(),
                awayParticipantIds: Array.Empty<string>()
            );

            var m2 = new Match(
                eventId: "E2",
                startDateTimeUtc: DateTimeOffset.Parse("2025-12-22T12:00:00Z"),
                tournamentName: "Test League",
                homeName: "C",
                awayName: "D",
                homeParticipantIds: Array.Empty<string>(),
                awayParticipantIds: Array.Empty<string>()
            );

            var input = new List<Match> { m1, m1dup, m2 };

            // Вызов private static IReadOnlyList<Match> DedupByEventId(IReadOnlyList<Match>)
            var progType = Type.GetType("FootballReport.Program, FootballReport", throwOnError: true)!;

            var mi = progType.GetMethod(
                "DedupByEventId",
                BindingFlags.NonPublic | BindingFlags.Static
            );

            Assert.NotNull(mi);

            var output = (IReadOnlyList<Match>)mi!.Invoke(null, new object[] { input })!;

            Assert.Equal(2, output.Count);
            Assert.Equal("E1", output[0].EventId); // должен остаться первый E1 (m1)
            Assert.Equal("E2", output[1].EventId);
            Assert.Equal("Test League", output[0].TournamentName); // подтверждаем, что сохранился именно m1
        }
    }
}
