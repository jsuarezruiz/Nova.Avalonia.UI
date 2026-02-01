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
            if (Image == null || FrameHeight <= 0) return 0;
            var rows = (int)Math.Ceiling(Image.Size.Height / FrameHeight);
            return Columns * rows;
        }
    }

    /// <summary>
    /// Gets the source rectangle for the specified frame index.
    /// </summary>
    public Rect GetFrameRect(int frame)
    {
        var col = frame % Columns;
        var row = frame / Columns;

        return new Rect(
            col * FrameWidth,
            row * FrameHeight,
            FrameWidth,
            FrameHeight);
    }
}
