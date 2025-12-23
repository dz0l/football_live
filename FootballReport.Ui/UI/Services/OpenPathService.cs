using System.Diagnostics;
using System.IO;

namespace FootballReport.Ui.UI.Services;

internal static class OpenPathService
{
    internal static string EnsureFolder(string path)
    {
        Directory.CreateDirectory(path);
        return path;
    }

    internal static void OpenFolder(string path)
    {
        var target = EnsureFolder(path);

        Process.Start(new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = target,
            UseShellExecute = true
        });
    }
}
