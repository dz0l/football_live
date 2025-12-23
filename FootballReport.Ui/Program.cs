using System;
using System.Windows.Forms;

namespace FootballReport.Ui;

static class Program
{
    [STAThread]
    static void Main()
    {
        try
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new FootballReport.Ui.UI.Views.MainForm());
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString(), "Startup error");
        }
    }
}
