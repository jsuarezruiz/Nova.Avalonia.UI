using Avalonia.Media;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Defines the visual styling for a fortune item.
/// </summary>
public class FortuneItemStyle
{
    /// <summary>
    /// Gets or sets the background brush for the item.
    /// </summary>
    public IBrush? Background { get; set; }

    /// <summary>
    /// Gets or sets the border brush for the item.
    /// </summary>
    public IBrush? BorderBrush { get; set; }

    /// <summary>
    /// Gets or sets the border thickness for the item.
    /// </summary>
    public double BorderThickness { get; set; }

    /// <summary>
    /// Gets or sets the foreground brush for text content.
    /// </summary>
    public IBrush? Foreground { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FortuneItemStyle"/> class.
    /// </summary>
    public FortuneItemStyle()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FortuneItemStyle"/> class with a background.
    /// </summary>
    /// <param name="background">The background brush.</param>
    public FortuneItemStyle(IBrush background)
    {
        Background = background;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FortuneItemStyle"/> class with full styling.
    /// </summary>
    /// <param name="background">The background brush.</param>
    /// <param name="foreground">The foreground brush.</param>
    /// <param name="borderBrush">The border brush.</param>
    /// <param name="borderThickness">The border thickness.</param>
    public FortuneItemStyle(IBrush? background, IBrush? foreground, IBrush? borderBrush = null, double borderThickness = 0)
    {
        Background = background;
        Foreground = foreground;
        BorderBrush = borderBrush;
        BorderThickness = borderThickness;
    }
}
