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
    /// Circular/elliptical highlight.
    /// </summary>
    Circle
}
