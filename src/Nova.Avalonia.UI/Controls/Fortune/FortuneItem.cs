using Avalonia.Controls.Templates;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Represents an individual item in a Fortune control (wheel or bar).
/// </summary>
public class FortuneItem
{
    /// <summary>
    /// Gets or sets the content to display for this item.
    /// </summary>
    public object? Content { get; set; }

    /// <summary>
    /// Gets or sets a user-friendly name for this item.
    /// Useful when Content is a complex object (like an Image).
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the template used to display the content.
    /// </summary>
    public IDataTemplate? ContentTemplate { get; set; }

    /// <summary>
    /// Gets or sets custom styling for this specific item.
    /// When null, the control's StyleStrategy is used.
    /// </summary>
    public FortuneItemStyle? Style { get; set; }

    /// <summary>
    /// Gets or sets the weight for weighted random selection.
    /// Higher values increase the probability of this item being selected.
    /// Default is 1.0.
    /// </summary>
    public double Weight { get; set; } = 1.0;

    /// <summary>
    /// Initializes a new instance of the <see cref="FortuneItem"/> class.
    /// </summary>
    public FortuneItem()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FortuneItem"/> class with content.
    /// </summary>
    /// <param name="content">The content to display.</param>
    public FortuneItem(object? content)
    {
        Content = content;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FortuneItem"/> class with content and style.
    /// </summary>
    /// <param name="content">The content to display.</param>
    /// <param name="style">Custom styling for this item.</param>
    public FortuneItem(object? content, FortuneItemStyle? style)
    {
        Content = content;
        Style = style;
    }
}
