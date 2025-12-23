using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace FootballReport.Ui.UI.Controls;

public sealed class StatusBulb : Control
{
    public enum BulbState { Off = 0, Ok = 1, Down = 2 }

    private BulbState _state = BulbState.Off;

    [DefaultValue(BulbState.Off)]
    public BulbState State
    {
        get => _state;
        set { _state = value; Invalidate(); }
    }

    public StatusBulb()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.UserPaint, true);

        Size = new Size(14, 14);
        Cursor = Cursors.Hand;
        BackColor = SystemColors.Control;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        // "псевдо-прозрачность": рисуем фон родителя
        var bg = Parent?.BackColor ?? SystemColors.Control;
        using (var bgBrush = new SolidBrush(bg))
            e.Graphics.FillRectangle(bgBrush, ClientRectangle);

        var rect = ClientRectangle;
        rect.Inflate(-1, -1);

        var fill = State switch
        {
            BulbState.Ok => Color.FromArgb(94, 226, 155),
            BulbState.Down => Color.FromArgb(255, 106, 106),
            _ => Color.FromArgb(120, 130, 150),
        };

        using var brush = new SolidBrush(fill);
        using var pen = new Pen(Color.FromArgb(60, 80, 110), 1);

        e.Graphics.FillEllipse(brush, rect);
        e.Graphics.DrawEllipse(pen, rect);

        if (State is BulbState.Ok or BulbState.Down)
        {
            using var glow = new SolidBrush(Color.FromArgb(60, fill));
            var glowRect = rect;
            glowRect.Inflate(2, 2);
            e.Graphics.FillEllipse(glow, glowRect);
        }
    }
}
