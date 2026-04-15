using Avalonia;
using Avalonia.Controls.Templates;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Represents a single step in a showcase tutorial sequence.
/// </summary>
public class ShowcaseStep
{
    /// <summary>
    /// Gets or sets the unique key identifying the target element.
    /// Must match the Showcase.Key attached property on the target control.
    /// </summary>
    public required string Key { get; set; }

    /// <summary>
    /// Gets or sets the title displayed in the tooltip.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description displayed in the tooltip.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the preferred position of the tooltip.
    /// </summary>
    public ShowcaseTooltipPosition TooltipPosition { get; set; } = ShowcaseTooltipPosition.Auto;

    /// <summary>
    /// Gets or sets an optional interaction mode override for this step.
    /// If null, the parent <see cref="Showcase"/> control's InteractionMode is used.
    /// </summary>
    public ShowcaseInteractionMode? InteractionMode { get; set; }

    /// <summary>
    /// Gets or sets the shape of the highlight cutout.
    /// </summary>
    public ShowcaseHighlightShape HighlightShape { get; set; } = ShowcaseHighlightShape.RoundedRectangle;

    /// <summary>
    /// Gets or sets the padding around the highlighted element.
    /// </summary>
    public Thickness HighlightPadding { get; set; } = new Thickness(8);

    /// <summary>
    /// Gets or sets the corner radius for rounded rectangle highlights.
    /// </summary>
    public double HighlightCornerRadius { get; set; } = 8;

    /// <summary>
    /// Gets or sets a tooltip body template for this step.
    /// If null, the default title/description body is used.
    /// The element-level <c>Showcase.TooltipTemplate</c> attached property takes priority over this.
    /// </summary>
    public IDataTemplate? TooltipTemplate { get; set; }
}
