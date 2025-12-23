using System.Windows.Forms;
using FootballReport.Ui.UI.Controls;
using FootballReport.Ui.UI.Views;

namespace FootballReport.Ui.UI.Views
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        private Panel headerPanel;
        private Label lblTitle;
        private Label lblToday;
        private ProgressBar progress;
        private Button btnRun;
        private Button btnOpenOut;

        private FlowLayoutPanel bulbsPanel;
        private ToolTip toolTip;

        private SplitContainer split;
        private SplitContainer leftSplit;

        private TabControl leftTabs;
        private TabPage tabIncluded;
        private TabPage tabExcluded;
        private TabPage tabPreview;

        private DataGridView gridIncluded;
        private DataGridView gridExcluded;

        private Panel kpiPanel;
        private Label lblKpiFetched;
        private Label lblKpiDedup;
        private Label lblKpiIncluded;
        private Label lblKpiBlacklisted;
        private Label lblKpiNotIncluded;
        private Label lblKpiFiles;

        private TabControl rightTabs;
        private TabPage tabLog;
        private TabPage tabConfig;
        private TabPage tabPrint;
        private TabPage tabTest;

        private RichTextBox txtLog;
        private ConfigEditorControl configEditor;

        private StatusBulb bulbApi;
        private StatusBulb bulbTv1;
        private StatusBulb bulbTv2;
        private StatusBulb bulbTv3;
        private StatusBulb bulbTv4;
        private StatusBulb bulbSp1;
        private StatusBulb bulbSp2;
        private StatusBulb bulbSp3;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            toolTip = new ToolTip(components);

            headerPanel = new Panel();
            lblTitle = new Label();
            lblToday = new Label();
            progress = new ProgressBar();
            btnRun = new Button();
            btnOpenOut = new Button();
            bulbsPanel = new FlowLayoutPanel();

            split = new SplitContainer();
            leftSplit = new SplitContainer();

            leftTabs = new TabControl();
            tabIncluded = new TabPage();
            tabExcluded = new TabPage();
            tabPreview = new TabPage();

            gridIncluded = new DataGridView();
            gridExcluded = new DataGridView();

            kpiPanel = new Panel();
            lblKpiFetched = new Label();
            lblKpiDedup = new Label();
            lblKpiIncluded = new Label();
            lblKpiBlacklisted = new Label();
            lblKpiNotIncluded = new Label();
            lblKpiFiles = new Label();

            rightTabs = new TabControl();
            tabLog = new TabPage();
            tabConfig = new TabPage();
            tabPrint = new TabPage();
            tabTest = new TabPage();

            txtLog = new RichTextBox();
            configEditor = new ConfigEditorControl();

            bulbApi = new StatusBulb();
            bulbTv1 = new StatusBulb();
            bulbTv2 = new StatusBulb();
            bulbTv3 = new StatusBulb();
            bulbTv4 = new StatusBulb();
            bulbSp1 = new StatusBulb();
            bulbSp2 = new StatusBulb();
            bulbSp3 = new StatusBulb();

            SuspendLayout();

            // MainForm
            AutoScaleMode = AutoScaleMode.Font;
            Text = "Football Report — UI";
            Width = 1300;
            Height = 780;
            StartPosition = FormStartPosition.CenterScreen;

            // headerPanel
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Height = 64;
            headerPanel.Padding = new Padding(12, 10, 12, 10);

            // lblTitle
            lblTitle.AutoSize = true;
            lblTitle.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            lblTitle.Text = "Football Report";
            lblTitle.Location = new System.Drawing.Point(12, 10);

            // lblToday
            lblToday.AutoSize = true;
            lblToday.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblToday.Text = "Сегодня: --.--.----";
            lblToday.Location = new System.Drawing.Point(12, 34);

            // progress
            progress.Width = 220;
            progress.Height = 18;
            progress.Location = new System.Drawing.Point(260, 22);

            // bulbsPanel (compact bulbs, tooltip only)
            bulbsPanel.Location = new System.Drawing.Point(500, 18);
            bulbsPanel.Size = new System.Drawing.Size(360, 28);
            bulbsPanel.WrapContents = false;
            bulbsPanel.AutoSize = false;
            bulbsPanel.FlowDirection = FlowDirection.LeftToRight;

            bulbsPanel.Controls.Add(bulbApi);
            bulbsPanel.Controls.Add(bulbTv1);
            bulbsPanel.Controls.Add(bulbTv2);
            bulbsPanel.Controls.Add(bulbTv3);
            bulbsPanel.Controls.Add(bulbTv4);
            bulbsPanel.Controls.Add(bulbSp1);
            bulbsPanel.Controls.Add(bulbSp2);
            bulbsPanel.Controls.Add(bulbSp3);

            toolTip.SetToolTip(bulbApi, "API: sportdb.dev");
            toolTip.SetToolTip(bulbTv1, "TV #1 (provider) — TBD");
            toolTip.SetToolTip(bulbTv2, "TV #2 (provider) — TBD");
            toolTip.SetToolTip(bulbTv3, "TV #3 (provider) — TBD");
            toolTip.SetToolTip(bulbTv4, "TV #4 (provider) — TBD");
            toolTip.SetToolTip(bulbSp1, "Spare #1");
            toolTip.SetToolTip(bulbSp2, "Spare #2");
            toolTip.SetToolTip(bulbSp3, "Spare #3");

            // btnRun
            btnRun.Text = "Сформировать (сегодня)";
            btnRun.Width = 190;
            btnRun.Height = 30;
            btnRun.Location = new System.Drawing.Point(880, 17);

            // btnOpenOut
            btnOpenOut.Text = "Открыть out";
            btnOpenOut.Width = 120;
            btnOpenOut.Height = 30;
            btnOpenOut.Location = new System.Drawing.Point(1080, 17);

            headerPanel.Controls.Add(lblTitle);
            headerPanel.Controls.Add(lblToday);
            headerPanel.Controls.Add(progress);
            headerPanel.Controls.Add(bulbsPanel);
            headerPanel.Controls.Add(btnRun);
            headerPanel.Controls.Add(btnOpenOut);

            // split (Left/Right)
            split.Dock = DockStyle.Fill;
            split.Orientation = Orientation.Vertical;
            split.SplitterDistance = 800;

            // leftSplit (Top tabs / Bottom KPI) — tabs сверху, KPI снизу
            leftSplit.Dock = DockStyle.Fill;
            leftSplit.Orientation = Orientation.Horizontal;
            leftSplit.SplitterDistance = 470;

            // leftTabs
            leftTabs.Dock = DockStyle.Fill;
            leftTabs.TabPages.Add(tabIncluded);
            leftTabs.TabPages.Add(tabExcluded);
            leftTabs.TabPages.Add(tabPreview);

            tabIncluded.Text = "Included";
            tabExcluded.Text = "Excluded";
            tabPreview.Text = "Превью";

            // gridIncluded
            gridIncluded.Dock = DockStyle.Fill;
            gridIncluded.ReadOnly = true;
            gridIncluded.AllowUserToAddRows = false;
            gridIncluded.AllowUserToDeleteRows = false;
            gridIncluded.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridIncluded.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            tabIncluded.Controls.Add(gridIncluded);

            // gridExcluded
            gridExcluded.Dock = DockStyle.Fill;
            gridExcluded.ReadOnly = true;
            gridExcluded.AllowUserToAddRows = false;
            gridExcluded.AllowUserToDeleteRows = false;
            gridExcluded.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridExcluded.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            tabExcluded.Controls.Add(gridExcluded);

            // tabPreview placeholder
            var lblPreview = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Text = "Превью-зона (позже: WebView2 / открыть HTML в браузере)"
            };
            tabPreview.Controls.Add(lblPreview);

            leftSplit.Panel1.Controls.Add(leftTabs);

            // KPI panel (simple labels, later replace with cards)
            kpiPanel.Dock = DockStyle.Fill;
            kpiPanel.Padding = new Padding(12);

            lblKpiFetched.AutoSize = true;
            lblKpiDedup.AutoSize = true;
            lblKpiIncluded.AutoSize = true;
            lblKpiBlacklisted.AutoSize = true;
            lblKpiNotIncluded.AutoSize = true;
            lblKpiFiles.AutoSize = true;

            lblKpiFetched.Text = "Получено: —";
            lblKpiDedup.Text = "Dedup: —";
            lblKpiIncluded.Text = "Included: —";
            lblKpiBlacklisted.Text = "Blacklisted: —";
            lblKpiNotIncluded.Text = "Not included: —";
            lblKpiFiles.Text = "Files: —";

            lblKpiFetched.Location = new System.Drawing.Point(12, 12);
            lblKpiDedup.Location = new System.Drawing.Point(12, 34);
            lblKpiIncluded.Location = new System.Drawing.Point(12, 56);
            lblKpiBlacklisted.Location = new System.Drawing.Point(220, 12);
            lblKpiNotIncluded.Location = new System.Drawing.Point(220, 34);
            lblKpiFiles.Location = new System.Drawing.Point(220, 56);

            kpiPanel.Controls.Add(lblKpiFetched);
            kpiPanel.Controls.Add(lblKpiDedup);
            kpiPanel.Controls.Add(lblKpiIncluded);
            kpiPanel.Controls.Add(lblKpiBlacklisted);
            kpiPanel.Controls.Add(lblKpiNotIncluded);
            kpiPanel.Controls.Add(lblKpiFiles);

            leftSplit.Panel2.Controls.Add(kpiPanel);

            split.Panel1.Controls.Add(leftSplit);

            // rightTabs
            rightTabs.Dock = DockStyle.Fill;
            rightTabs.TabPages.Add(tabLog);
            rightTabs.TabPages.Add(tabConfig);
            rightTabs.TabPages.Add(tabPrint);
            rightTabs.TabPages.Add(tabTest);

            tabLog.Text = "Лог";
            tabConfig.Text = "Конфиг";
            tabPrint.Text = "Печать";
            tabTest.Text = "Тест";

            // Log
            txtLog.Dock = DockStyle.Fill;
            txtLog.ReadOnly = true;
            txtLog.Font = new System.Drawing.Font("Consolas", 9.5F);
            tabLog.Controls.Add(txtLog);

            // Config editor
            configEditor.Dock = DockStyle.Fill;
            tabConfig.Controls.Add(configEditor);

            // Print placeholder (A4 landscape fixed)
            tabPrint.Controls.Add(new Label { Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleCenter, Text = "Профиль печати: A4 landscape (фиксировано)\n(позже: масштаб/опции)" });

            // Test placeholder
            tabTest.Controls.Add(new Label { Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleCenter, Text = "Test mode (заглушка)" });

            split.Panel2.Controls.Add(rightTabs);

            Controls.Add(split);
            Controls.Add(headerPanel);

            ResumeLayout(false);
        }
    }
}
