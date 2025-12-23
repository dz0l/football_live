using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using FootballReport;
using FootballReport.Time;
using FootballReport.Ui.UI.Controls;
using FootballReport.Ui.UI.Models;
using FootballReport.Ui.UI.Services;

namespace FootballReport.Ui.UI.Views;

public partial class MainForm : Form
{
    private bool _isRunning;
    private const int ProgressSteps = 5;
    private readonly TimeSpan _gridTimezone = TimezoneConverter.GmtPlus3;

    public MainForm()
    {
        InitializeComponent();
        SetupUi();
        WireEvents();
    }

    private void SetupUi()
    {
        lblToday.Text = $"Сегодня: {DateTime.Now:dd.MM.yyyy}";
        progress.Minimum = 0;
        progress.Maximum = ProgressSteps;
        progress.Value = 0;

        gridIncluded.AutoGenerateColumns = true;
        gridExcluded.AutoGenerateColumns = true;

        ResetKpiLabels();
        SetBulbsIdle();
    }

    private void WireEvents()
    {
        btnRun.Click += BtnRun_Click;
        btnOpenOut.Click += BtnOpenOut_Click;
    }

    private void SetBulbsIdle()
    {
        bulbApi.State = StatusBulb.BulbState.Off;
        bulbTv1.State = StatusBulb.BulbState.Off;
        bulbTv2.State = StatusBulb.BulbState.Off;
        bulbTv3.State = StatusBulb.BulbState.Off;
        bulbTv4.State = StatusBulb.BulbState.Off;
        bulbSp1.State = StatusBulb.BulbState.Off;
        bulbSp2.State = StatusBulb.BulbState.Off;
        bulbSp3.State = StatusBulb.BulbState.Off;
    }

    private void ResetKpiLabels()
    {
        lblKpiFetched.Text = "Получено: -";
        lblKpiDedup.Text = "Dedup: -";
        lblKpiIncluded.Text = "Included: -";
        lblKpiBlacklisted.Text = "Blacklisted: -";
        lblKpiNotIncluded.Text = "Not included: -";
        lblKpiFiles.Text = "Files: -";
    }

    private async void BtnRun_Click(object? sender, EventArgs e)
    {
        if (_isRunning) return;

        _isRunning = true;
        btnRun.Enabled = false;

        ResetUiBeforeRun();

        var progressReporter = new Progress<ReportPipeline.ProgressUpdate>(UpdateProgress);
        var logReporter = new Progress<string>(msg => UiLog.AppendLine(txtLog, msg));

        try
        {
            bulbApi.State = StatusBulb.BulbState.Off;

            var result = await ReportPipeline.RunAsync(CancellationToken.None, progressReporter, logReporter);

            bulbApi.State = SiteAvailabilityService.FromStatus(result.Success);

            ApplyResult(result);
        }
        catch (Exception ex)
        {
            UiLog.AppendLine(txtLog, "Ошибка: " + ex.Message);
            bulbApi.State = StatusBulb.BulbState.Down;
        }
        finally
        {
            progress.Value = ProgressSteps;
            btnRun.Enabled = true;
            _isRunning = false;
        }
    }

    private void ResetUiBeforeRun()
    {
        progress.Value = 0;
        UiLog.Clear(txtLog);

        gridIncluded.DataSource = null;
        gridExcluded.DataSource = null;

        ResetKpiLabels();
        SetBulbsIdle();
    }

    private void UpdateProgress(ReportPipeline.ProgressUpdate update)
    {
        progress.Maximum = update.TotalSteps;
        var next = Math.Max(progress.Minimum, Math.Min(update.StepIndex, update.TotalSteps));
        progress.Value = next;
    }

    private void ApplyResult(ReportPipeline.RunResult result)
    {
        var summary = new UiRunSummary(
            result.RawCount,
            result.DedupCount,
            result.Included.Count,
            result.BlacklistedCount,
            result.NotIncludedCount,
            result.FilesRendered);

        SetKpis(summary);

        gridIncluded.DataSource = BuildRows(result.Included);
        gridExcluded.DataSource = BuildRows(result.Excluded);

        if (result.Included.Count == 0 && result.Success)
            UiLog.AppendLine(txtLog, "Сегодня матчей нет");

        lblToday.Text = $"Сегодня: {DateTime.Now:dd.MM.yyyy}";
    }

    private void SetKpis(UiRunSummary summary)
    {
        lblKpiFetched.Text = $"Получено: {summary.Fetched}";
        lblKpiDedup.Text = $"Dedup: {summary.Dedup}";
        lblKpiIncluded.Text = $"Included: {summary.Included}";
        lblKpiBlacklisted.Text = $"Blacklisted: {summary.Blacklisted}";
        lblKpiNotIncluded.Text = $"Not included: {summary.NotIncluded}";
        lblKpiFiles.Text = $"Files: {summary.Files}";
    }

    private List<MatchRowUi> BuildRows(IReadOnlyList<ReportPipeline.ProcessedMatch> items)
    {
        var rows = new List<MatchRowUi>(items.Count);

        foreach (var item in items)
        {
            var local = TimezoneConverter.ConvertUtcToOffset(item.Match.StartDateTimeUtc, _gridTimezone);
            var time = TimezoneConverter.FormatTimeHm(local);

            rows.Add(new MatchRowUi(
                Tournament: item.Match.TournamentName,
                Match: $"{item.Match.HomeName} vs {item.Match.AwayName}",
                Time: time,
                Reason: item.ReasonCategory));
        }

        return rows;
    }

    private void BtnOpenOut_Click(object? sender, EventArgs e)
    {
        try
        {
            var folder = OpenPathService.EnsureFolder(ProjectPaths.OutDir);
            OpenPathService.OpenFolder(folder);
            UiLog.AppendLine(txtLog, $"Открыт каталог: {folder}");
        }
        catch (Exception ex)
        {
            UiLog.AppendLine(txtLog, "Не удалось открыть out/: " + ex.Message);
        }
    }
}
