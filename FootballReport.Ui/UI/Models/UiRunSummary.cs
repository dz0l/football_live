namespace FootballReport.Ui.UI.Models;

public sealed record UiRunSummary(
    int Fetched,
    int Dedup,
    int Included,
    int Blacklisted,
    int NotIncluded,
    int Files);
