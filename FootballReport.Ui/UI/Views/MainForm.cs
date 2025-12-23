using FootballReport.Ui.UI.Controls;

namespace FootballReport.Ui.UI.Views;

public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
        InitDemo();
    }

    private void InitDemo()
    {
        lblToday.Text = $"Сегодня: {DateTime.Now:dd.MM.yyyy}";

        bulbApi.State = StatusBulb.BulbState.Ok;
        bulbTv1.State = StatusBulb.BulbState.Ok;
        bulbTv2.State = StatusBulb.BulbState.Ok;
        bulbTv3.State = StatusBulb.BulbState.Down;
        bulbTv4.State = StatusBulb.BulbState.Ok;
        bulbSp1.State = StatusBulb.BulbState.Off;
        bulbSp2.State = StatusBulb.BulbState.Off;
        bulbSp3.State = StatusBulb.BulbState.Off;

        lblKpiFetched.Text = "Получено: 42";
        lblKpiDedup.Text = "Dedup: 39";
        lblKpiIncluded.Text = "Included: 12";
        lblKpiBlacklisted.Text = "Blacklisted: 3";
        lblKpiNotIncluded.Text = "Not included: 24";
        lblKpiFiles.Text = "Files: 3";

        gridIncluded.DataSource = new[]
        {
            new { Tournament = "Italian Super Cup - FINAL", Match = "Napoli v Bologna", Time = "22:00", Reason = "fav tournament" },
            new { Tournament = "English Premier League", Match = "Fulham v Nottingham Forest", Time = "23:00", Reason = "fav club" }
        };

        gridExcluded.DataSource = new[]
        {
            new { Tournament = "Friendly", Match = "Team A v Team B", Time = "18:00", Reason = "NotFavorite" },
            new { Tournament = "Some League", Match = "X v Y", Time = "19:00", Reason = "Blacklisted: club" }
        };

        txtLog.AppendText("Fetch OK → /api/flashscore/football/live\r\n");
        txtLog.AppendText("Parse OK → Matches: 42\r\n");
        txtLog.AppendText("Dedup OK → After dedup: 39\r\n");
        txtLog.AppendText("Filter OK → Included: 12 → Blacklisted: 3\r\n");
        txtLog.AppendText("Render OK → Rendered: 3 files (GMT+3/+4/+5)\r\n");
    }
}
