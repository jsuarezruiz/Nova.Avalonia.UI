using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Controls.Metadata;
using global::Avalonia.Controls.Primitives;
using global::Avalonia.Media;
using global::Avalonia.Layout;
using global::Avalonia.Automation.Peers;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// A notification badge control that can wrap content (buttons, icons) and display 
/// a count, text, or dot indicator at a configurable position.
/// </summary>
/// <remarks>
/// <para>The Badge control supports:</para>
/// <list type="bullet">
/// <item>Numeric content with automatic "99+" style overflow (via <see cref="MaxCount"/>)</item>
/// <item>Dot mode for simple presence indicators</item>
/// <item>8 placement positions around the wrapped content</item>
/// <item>Full accessibility via <see cref="BadgeAutomationPeer"/></item>
/// </list>
/// </remarks>
[TemplatePart("PART_BadgeContainer", typeof(Border))]
public class Badge : ContentControl
{
    static Badge()
    {
        BadgeContentProperty.Changed.AddClassHandler<Badge>((x, e) => x.OnBadgeContentChanged(e));
        KindProperty.Changed.AddClassHandler<Badge>((x, e) => x.UpdateLayoutState());
        BadgePlacementProperty.Changed.AddClassHandler<Badge>((x, e) => x.UpdatePosition());
        BadgeOffsetProperty.Changed.AddClassHandler<Badge>((x, e) => x.UpdatePosition());
        MaxCountProperty.Changed.AddClassHandler<Badge>((x, e) => x.UpdateDisplayContent());
    }

    #region Styled Properties

    /// <summary>
    /// Defines the <see cref="BadgeContent"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> BadgeContentProperty = 
        AvaloniaProperty.Register<Badge, object?>(nameof(BadgeContent));

    /// <summary>
    /// Defines the <see cref="BadgePlacement"/> property.
    /// </summary>
    public static readonly StyledProperty<BadgePlacement> BadgePlacementProperty = 
        AvaloniaProperty.Register<Badge, BadgePlacement>(nameof(BadgePlacement), BadgePlacement.TopRight);

    /// <summary>
    /// Defines the <see cref="IsBadgeVisible"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsBadgeVisibleProperty = 
        AvaloniaProperty.Register<Badge, bool>(nameof(IsBadgeVisible), true);

    /// <summary>
    /// Defines the <see cref="BadgeOffset"/> property.
    /// </summary>
    public static readonly StyledProperty<double> BadgeOffsetProperty = 
        AvaloniaProperty.Register<Badge, double>(nameof(BadgeOffset), 0.0);

    /// <summary>
    /// Defines the <see cref="Kind"/> property.
    /// </summary>
    public static readonly StyledProperty<BadgeKind> KindProperty = 
        AvaloniaProperty.Register<Badge, BadgeKind>(nameof(Kind), BadgeKind.Content);

    /// <summary>
    /// Defines the <see cref="BadgeBackground"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush> BadgeBackgroundProperty = 
        AvaloniaProperty.Register<Badge, IBrush>(nameof(BadgeBackground), Brushes.Red);

    /// <summary>
    /// Defines the <see cref="BadgeForeground"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush> BadgeForegroundProperty = 
        AvaloniaProperty.Register<Badge, IBrush>(nameof(BadgeForeground), Brushes.White);

    /// <summary>
    /// Defines the <see cref="BadgeBorderBrush"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush> BadgeBorderBrushProperty = 
        AvaloniaProperty.Register<Badge, IBrush>(nameof(BadgeBorderBrush), Brushes.Transparent);

    /// <summary>
    /// Defines the <see cref="BadgeBorderThickness"/> property.
    /// </summary>
    public static readonly StyledProperty<Thickness> BadgeBorderThicknessProperty = 
        AvaloniaProperty.Register<Badge, Thickness>(nameof(BadgeBorderThickness), new Thickness(0));

    /// <summary>
    /// Defines the <see cref="MaxCount"/> property.
    /// </summary>
    public static readonly StyledProperty<int> MaxCountProperty = 
        AvaloniaProperty.Register<Badge, int>(nameof(MaxCount), 99);

    /// <summary>
    /// Defines the <see cref="DisplayContent"/> property.
    /// </summary>
    public static readonly DirectProperty<Badge, object?> DisplayContentProperty = 
        AvaloniaProperty.RegisterDirect<Badge, object?>(nameof(DisplayContent), o => o.DisplayContent);

    #endregion

    #region Properties

    private object? _displayContent;

    /// <summary>
    /// Gets the processed display content, which may show "99+" style overflow text.
    /// </summary>
    public object? DisplayContent 
    { 
        get => _displayContent; 
        private set => SetAndRaise(DisplayContentProperty, ref _displayContent, value); 
    }

    /// <summary>
    /// Gets or sets the content displayed in the badge (text, number, etc.).
    /// </summary>
    public object? BadgeContent 
    { 
        get => GetValue(BadgeContentProperty); 
        set => SetValue(BadgeContentProperty, value); 
    }

    /// <summary>
    /// Gets or sets the placement of the badge relative to the wrapped content.
    /// </summary>
    public BadgePlacement BadgePlacement 
    { 
        get => GetValue(BadgePlacementProperty); 
        set => SetValue(BadgePlacementProperty, value); 
    }

    /// <summary>
    /// Gets or sets whether the badge is visible.
    /// </summary>
    public bool IsBadgeVisible 
    { 
        get => GetValue(IsBadgeVisibleProperty); 
        set => SetValue(IsBadgeVisibleProperty, value); 
    }

    /// <summary>
    /// Gets or sets the offset (in pixels) to adjust badge position inward from the corner.
    /// </summary>
    public double BadgeOffset 
    { 
        get => GetValue(BadgeOffsetProperty); 
        set => SetValue(BadgeOffsetProperty, value); 
    }

    /// <summary>
    /// Gets or sets the badge kind (Content or Dot).
    /// </summary>
    public BadgeKind Kind 
    { 
        get => GetValue(KindProperty); 
        set => SetValue(KindProperty, value); 
    }

    /// <summary>
    /// Gets or sets the background brush for the badge.
    /// </summary>
    public IBrush BadgeBackground 
    { 
        get => GetValue(BadgeBackgroundProperty); 
        set => SetValue(BadgeBackgroundProperty, value); 
    }

    /// <summary>
    /// Gets or sets the foreground brush for badge text.
    /// </summary>
    public IBrush BadgeForeground 
    { 
        get => GetValue(BadgeForegroundProperty); 
        set => SetValue(BadgeForegroundProperty, value); 
    }

    /// <summary>
    /// Gets or sets the border brush for the badge.
    /// </summary>
    public IBrush BadgeBorderBrush 
    { 
        get => GetValue(BadgeBorderBrushProperty); 
        set => SetValue(BadgeBorderBrushProperty, value); 
    }

    /// <summary>
    /// Gets or sets the border thickness for the badge.
    /// </summary>
    public Thickness BadgeBorderThickness 
    { 
        get => GetValue(BadgeBorderThicknessProperty); 
        set => SetValue(BadgeBorderThicknessProperty, value); 
    }

    /// <summary>
    /// Gets or sets the maximum count before displaying overflow text (e.g., "99+").
    /// </summary>
    public int MaxCount 
    { 
        get => GetValue(MaxCountProperty); 
        set => SetValue(MaxCountProperty, value); 
    }

    #endregion

    private Border? _badgeContainer;

    // 1. Override to provide the custom peer
    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new BadgeAutomationPeer(this);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _badgeContainer = e.NameScope.Find<Border>("PART_BadgeContainer");

        if (_badgeContainer != null)
        {
            _badgeContainer.SizeChanged += (s, ev) => UpdatePosition();
            this.SizeChanged += (s, ev) => UpdatePosition();
            UpdateLayoutState();
            UpdatePosition();
            UpdateDisplayContent();
        }
    }

    private void OnBadgeContentChanged(AvaloniaPropertyChangedEventArgs e)
    {
        UpdateLayoutState();
        UpdateDisplayContent();
        
        // NOTE: We do NOT manually notify the peer here. 
        // The peer listens to PropertyChanged events itself.
    }

    private void UpdateDisplayContent()
    {
        var content = BadgeContent;
        if (content != null && int.TryParse(content.ToString(), out int count))
        {
            if (count > MaxCount)
            {
                DisplayContent = $"{MaxCount}+";
                return;
            }
        }
        DisplayContent = content;
    }

    private void UpdateLayoutState()
    {
        bool isDot = Kind == BadgeKind.Dot;
        if (!isDot && (BadgeContent == null || (BadgeContent is string s && string.IsNullOrEmpty(s))))
            isDot = true;

        if (isDot) Classes.Add("Dot");
        else Classes.Remove("Dot");
    }

    private void UpdatePosition()
    {
        if (_badgeContainer == null) return;
        if (Content == null)
        {
            _badgeContainer.RenderTransform = null;
            _badgeContainer.HorizontalAlignment = HorizontalAlignment.Center;
            _badgeContainer.VerticalAlignment = VerticalAlignment.Center;
            return;
        }
        _badgeContainer.HorizontalAlignment = GetHorizontalAlignment(BadgePlacement);
        _badgeContainer.VerticalAlignment = GetVerticalAlignment(BadgePlacement);
        var bounds = _badgeContainer.Bounds;
        if (bounds.Width == 0) return;
        var inset = Padding;
        if (Content is Control child) inset += child.Margin;
        double halfW = bounds.Width / 2.0;
        double halfH = bounds.Height / 2.0;
        double off = BadgeOffset;
        double x = 0, y = 0;
        switch (BadgePlacement) {
            case BadgePlacement.TopLeft: x = -halfW + off + inset.Left; y = -halfH + off + inset.Top; break;
            case BadgePlacement.Top: y = -halfH + off + inset.Top; break;
            case BadgePlacement.TopRight: x = halfW - off - inset.Right; y = -halfH + off + inset.Top; break;
            case BadgePlacement.Right: x = halfW - off - inset.Right; break;
            case BadgePlacement.BottomRight: x = halfW - off - inset.Right; y = halfH - off - inset.Bottom; break;
            case BadgePlacement.Bottom: y = halfH - off - inset.Bottom; break;
            case BadgePlacement.BottomLeft: x = -halfW + off + inset.Left; y = halfH - off - inset.Bottom; break;
            case BadgePlacement.Left: x = -halfW + off + inset.Left; break;
        }
        _badgeContainer.RenderTransform = new TranslateTransform(x, y);
    }

    private static HorizontalAlignment GetHorizontalAlignment(BadgePlacement p) => p switch {
        BadgePlacement.TopLeft or BadgePlacement.Left or BadgePlacement.BottomLeft => HorizontalAlignment.Left,
        BadgePlacement.TopRight or BadgePlacement.Right or BadgePlacement.BottomRight => HorizontalAlignment.Right,
        _ => HorizontalAlignment.Center
    };

    private static VerticalAlignment GetVerticalAlignment(BadgePlacement p) => p switch {
        BadgePlacement.TopLeft or BadgePlacement.Top or BadgePlacement.TopRight => VerticalAlignment.Top,
        BadgePlacement.BottomLeft or BadgePlacement.Bottom or BadgePlacement.BottomRight => VerticalAlignment.Bottom,
        _ => VerticalAlignment.Center
    };
}