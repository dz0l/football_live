using System;
using System.Windows.Forms;

namespace FootballReport.Ui.UI.Services;

internal static class UiLog
{
    internal static void AppendLine(RichTextBox box, string message)
    {
        if (box.IsDisposed) return;

        void Append()
        {
            box.AppendText(message + Environment.NewLine);
            box.ScrollToCaret();
        }

        if (box.InvokeRequired)
            box.Invoke((MethodInvoker)Append);
        else
            Append();
    }

    internal static void Clear(RichTextBox box)
    {
        if (box.IsDisposed) return;

        void DoClear() => box.Clear();

        if (box.InvokeRequired)
            box.Invoke((MethodInvoker)DoClear);
        else
            DoClear();
    }
}
