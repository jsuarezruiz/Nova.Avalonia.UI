using System;
using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Controls.Metadata;
using global::Avalonia.Controls.Presenters;
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
[TemplatePart("PART_ContentPresenter", typeof(ContentPresenter))]
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

    private Border? _badgeContainer;
    private ContentPresenter? _contentPresenter;
    
    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new BadgeAutomationPeer(this);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_badgeContainer != null)
            _badgeContainer.SizeChanged -= OnLayoutSizeChanged;
        if (_contentPresenter != null)
            _contentPresenter.LayoutUpdated -= OnContentLayoutUpdated;
        SizeChanged -= OnLayoutSizeChanged;

        base.OnApplyTemplate(e);
        _badgeContainer = e.NameScope.Find<Border>("PART_BadgeContainer");
        _contentPresenter = e.NameScope.Find<ContentPresenter>("PART_ContentPresenter");

        if (_badgeContainer != null)
        {
            _badgeContainer.SizeChanged += OnLayoutSizeChanged;
            SizeChanged += OnLayoutSizeChanged;
            if (_contentPresenter != null)
                _contentPresenter.LayoutUpdated += OnContentLayoutUpdated;
            UpdateLayoutState();
            UpdatePosition();
            UpdateDisplayContent();
        }
    }

    private void OnLayoutSizeChanged(object? sender, SizeChangedEventArgs e) => UpdatePosition();

    private void OnContentLayoutUpdated(object? sender, EventArgs e) => UpdatePosition();

    private void OnBadgeContentChanged(AvaloniaPropertyChangedEventArgs e)
    {
        UpdateLayoutState();
        UpdateDisplayContent();
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

        // Arrange the badge from a stable origin, then position its center on the
        // requested edge of the actual wrapped visual. The Badge itself may be
        // stretched by a Grid or UniformGrid, so its Bounds are not a reliable
        // proxy for the bounds of its content.
        _badgeContainer.HorizontalAlignment = HorizontalAlignment.Left;
        _badgeContainer.VerticalAlignment = VerticalAlignment.Top;
        var bounds = _badgeContainer.Bounds;
        if (bounds.Width == 0 || bounds.Height == 0) return;

        var anchorBounds = GetContentBounds();
        double halfW = bounds.Width / 2.0;
        double halfH = bounds.Height / 2.0;
        double off = BadgeOffset;
        double left = anchorBounds.Left + off;
        double centerX = anchorBounds.Center.X;
        double right = anchorBounds.Right - off;
        double top = anchorBounds.Top + off;
        double centerY = anchorBounds.Center.Y;
        double bottom = anchorBounds.Bottom - off;
        var badgeCenter = BadgePlacement switch
        {
            BadgePlacement.TopLeft => new Point(left, top),
            BadgePlacement.Top => new Point(centerX, top),
            BadgePlacement.TopRight => new Point(right, top),
            BadgePlacement.Right => new Point(right, centerY),
            BadgePlacement.BottomRight => new Point(right, bottom),
            BadgePlacement.Bottom => new Point(centerX, bottom),
            BadgePlacement.BottomLeft => new Point(left, bottom),
            BadgePlacement.Left => new Point(left, centerY),
            _ => anchorBounds.Center
        };

        _badgeContainer.RenderTransform = new TranslateTransform(
            badgeCenter.X - halfW,
            badgeCenter.Y - halfH);
    }

    private Rect GetContentBounds()
    {
        if (Content is Visual contentVisual &&
            contentVisual.TranslatePoint(default, this) is { } contentOrigin)
        {
            return new Rect(contentOrigin, contentVisual.Bounds.Size);
        }

        if (_contentPresenter != null &&
            _contentPresenter.TranslatePoint(default, this) is { } presenterOrigin)
        {
            return new Rect(presenterOrigin, _contentPresenter.Bounds.Size);
        }

        return new Rect(Bounds.Size);
    }
}
