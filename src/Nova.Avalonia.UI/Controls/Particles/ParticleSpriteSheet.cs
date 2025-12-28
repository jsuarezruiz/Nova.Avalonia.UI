using System;
using Avalonia;
using Avalonia.Media;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Configuration for a sprite sheet used for particle rendering.
/// </summary>
public class ParticleSpriteSheet
{
    /// <summary>
    /// Gets or sets the source image containing all sprite frames.
    /// </summary>
    public IImage? Image { get; set; }

    /// <summary>
    /// Gets or sets the width of each frame in pixels.
    /// </summary>
    public int FrameWidth { get; set; }

    /// <summary>
    /// Gets or sets the height of each frame in pixels.
    /// </summary>
    public int FrameHeight { get; set; }

    /// <summary>
    /// Gets or sets the number of columns in the sprite sheet.
    /// </summary>
    public int Columns { get; set; } = 1;

    /// <summary>
    /// Gets the total number of frames in the sprite sheet.
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
    /// <param name="frame">The frame index (0-based).</param>
    /// <returns>The source rectangle within the sprite sheet.</returns>
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
