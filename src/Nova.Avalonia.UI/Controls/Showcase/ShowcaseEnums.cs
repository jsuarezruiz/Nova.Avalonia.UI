namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Specifies the position of the tooltip relative to the highlighted element.
/// </summary>
public enum ShowcaseTooltipPosition
{
    /// <summary>
    /// Automatically determine the best position.
    /// </summary>
    Auto,
    
    /// <summary>
    /// Position above the target element.
    /// </summary>
    Top,
    
    /// <summary>
    /// Position below the target element.
    /// </summary>
    Bottom,
    
    /// <summary>
    /// Position to the left of the target element.
    /// </summary>
    Left,
    
    /// <summary>
    /// Position to the right of the target element.
    /// </summary>
    Right,
    
    /// <summary>
    /// Position at the center of the screen.
    /// </summary>
    Center
}

/// <summary>
/// Specifies the shape of the highlight cutout in the overlay.
/// </summary>
public enum ShowcaseHighlightShape
{
    /// <summary>
    /// Rectangular highlight.
    /// </summary>
    Rectangle,
    
    /// <summary>
    /// Rounded rectangle highlight.
    /// </summary>
    RoundedRectangle,
    
    /// <summary>
    /// Circular highlight.
    /// </summary>
    Circle
}

/// <summary>
/// Specifies how the underlying UI remains interactive while the showcase is active.
/// </summary>
public enum ShowcaseInteractionMode
{
    /// <summary>
    /// Block interaction with the underlying UI. Only showcase chrome remains interactive.
    /// </summary>
    Modal,

    /// <summary>
    /// Allow interaction with the highlighted target while blocking the rest of the UI.
    /// </summary>
    TargetOnly,

    /// <summary>
    /// Keep the underlying UI interactive while the showcase acts as a visual guide.
    /// </summary>
    Passthrough
}
