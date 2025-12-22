using System;
using FootballReport.Time;
using Xunit;

namespace FootballReport.Tests
{
    public sealed class T1_TimezoneConversion
    {
        [Fact]
        public void Utc_to_GmtPlus3_ShouldConvertCorrectly()
        {
            // 2025-12-22 12:30:00Z => GMT+3 = 15:30
            var utc = DateTimeOffset.Parse("2025-12-22T12:30:00Z");

            var local = TimezoneConverter.ConvertUtcToOffset(utc, TimezoneConverter.GmtPlus3);

            Assert.Equal(TimeSpan.FromHours(3), local.Offset);
            Assert.Equal("15:30", TimezoneConverter.FormatTimeHm(local));
        }

        [Fact]
        public void Utc_to_GmtPlus4_ShouldConvertCorrectly()
        {
            // 2025-12-22 23:10:00Z => GMT+4 = 03:10 next day
            var utc = DateTimeOffset.Parse("2025-12-22T23:10:00Z");

            var local = TimezoneConverter.ConvertUtcToOffset(utc, TimezoneConverter.GmtPlus4);

            Assert.Equal(TimeSpan.FromHours(4), local.Offset);
            Assert.Equal("03:10", TimezoneConverter.FormatTimeHm(local));
            Assert.Equal("23.12.2025", local.ToString("dd.MM.yyyy"));
        }

        [Fact]
        public void Utc_to_GmtPlus5_ShouldConvertCorrectly()
        {
            // 2025-12-22 00:05:00Z => GMT+5 = 05:05
            var utc = DateTimeOffset.Parse("2025-12-22T00:05:00Z");

            var local = TimezoneConverter.ConvertUtcToOffset(utc, TimezoneConverter.GmtPlus5);

            Assert.Equal(TimeSpan.FromHours(5), local.Offset);
            Assert.Equal("05:05", TimezoneConverter.FormatTimeHm(local));
        }
    }
}
