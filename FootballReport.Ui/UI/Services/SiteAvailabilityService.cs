using FootballReport.Ui.UI.Controls;

namespace FootballReport.Ui.UI.Services;

internal static class SiteAvailabilityService
{
    internal static StatusBulb.BulbState FromStatus(bool? available)
    {
        if (!available.HasValue)
            return StatusBulb.BulbState.Off;

        return available.Value ? StatusBulb.BulbState.Ok : StatusBulb.BulbState.Down;
    }
}
