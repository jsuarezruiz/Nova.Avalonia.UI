using System;
using Avalonia;
using Avalonia.Media;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Configuration for a sprite sheet used for particle rendering.
/// </summary>
public class ParticleSpriteSheet
{
    public IImage? Image { get; set; }

    public int FrameWidth { get; set; }

    public int FrameHeight { get; set; }

    public int Columns { get; set; } = 1;

    /// <summary>
    /// Gets the total number of frames.
    /// </summary>
    public int FrameCount
    {
        get
        {
            if (Image == null || FrameWidth <= 0 || FrameHeight <= 0) return 0;

            var columns = GetEffectiveColumns();
            var rows = GetEffectiveRows();

            return columns <= 0 || rows <= 0 ? 0 : columns * rows;
        }
    }

    /// <summary>
    /// Gets the source rectangle for the specified frame index.
    /// </summary>
    public Rect GetFrameRect(int frame)
    {
        if (FrameWidth <= 0 || FrameHeight <= 0)
            return default;

        var columns = GetEffectiveColumns();
        var frameCount = FrameCount;
        if (frameCount > 0)
        {
            frame = Math.Clamp(frame, 0, frameCount - 1);
        }
        else if (Image != null)
        {
            return default;
        }
        else
        {
            frame = Math.Max(0, frame);
            columns = Math.Max(1, Columns);
        }

        var col = frame % columns;
        var row = frame / columns;

        return new Rect(
            col * FrameWidth,
            row * FrameHeight,
            FrameWidth,
            FrameHeight);
    }

    private int GetEffectiveColumns()
    {
        if (Image == null)
            return Math.Max(1, Columns);

        if (!double.IsFinite(Image.Size.Width) || Image.Size.Width <= 0)
            return 0;

        var maxColumns = (int)Math.Floor(Image.Size.Width / FrameWidth);
        return Math.Min(Math.Max(1, Columns), maxColumns);
    }

    private int GetEffectiveRows()
    {
        if (Image == null)
            return 0;

        if (!double.IsFinite(Image.Size.Height) || Image.Size.Height <= 0)
            return 0;

        return (int)Math.Floor(Image.Size.Height / FrameHeight);
    }
}
