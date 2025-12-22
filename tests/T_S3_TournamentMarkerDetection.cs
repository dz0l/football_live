using FootballReport.Normalization;
using Xunit;

namespace FootballReport.Tests
{
    public sealed class T_S3_TournamentMarkerDetection
    {
        [Theory]
        [InlineData("UEFA Champions League - FINAL", true, false, false)]
        [InlineData("Cup FINALS", true, false, false)]
        [InlineData("Some League - SemiFinal", false, true, false)]
        [InlineData("Some League - SEMI", false, true, false)]
        [InlineData("Playoffs 1/2", false, false, true)]
        [InlineData("Playoffs 1 / 2", false, false, true)]
        public void S3_ShouldDetectStageMarkers(string tournamentName, bool expFinal, bool expSemi, bool expHalf)
        {
            var r = TournamentNameParserS3.Parse(tournamentName);

            Assert.Equal(expFinal, r.HasFinalMarker);
            Assert.Equal(expSemi, r.HasSemiMarker);
            Assert.Equal(expHalf, r.HasHalfMarker);

            Assert.Equal(expFinal || expSemi || expHalf, r.HasAnyStageMarker);
        }

        [Fact]
        public void S3_ShouldSplitPrefixBaseSuffix_Deterministically()
        {
            var r = TournamentNameParserS3.Parse("England - FA Cup - FINAL");

            Assert.Equal("England", r.Prefix);
            Assert.Equal("FA Cup", r.Base);
            Assert.Equal("FINAL", r.Suffix);
        }
    }
}
