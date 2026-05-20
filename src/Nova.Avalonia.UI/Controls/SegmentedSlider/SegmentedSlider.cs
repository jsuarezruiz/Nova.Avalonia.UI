using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Metadata;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// A horizontal range slider divided into visual segments with optional labels and proportional segment widths.
/// </summary>
[PseudoClasses(":readonly")]
public class SegmentedSlider : RangeBase
{
    private const int DefaultSegmentCount = 5;
    private const double DefaultSpacing = 4.0;
    private const double DefaultTrackHeight = 4.0;
    private const double DefaultTitleFontSize = 11.0;
    private const int MinSegmentCount = 1;
    private const double DefaultThumbSize = 18.0;
    private const string TrackHeightResourceKey = "SegmentedSliderTrackHeight";
    internal const double LargeChangeRatio = 0.25;

    private static readonly IBrush DefaultBackground = new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0)).ToImmutable();
    private static readonly IBrush DefaultForeground = new SolidColorBrush(Colors.DodgerBlue).ToImmutable();

    private Panel? _trackPanel;
    private Thumb? _thumb;
    private Grid? _segmentGrid;
    private readonly List<Rectangle> _trackRects = [];
    private readonly List<Rectangle> _fillRects = [];
    private readonly List<TextBlock> _titleBlocks = [];
    private readonly HashSet<SegmentedSliderSegment> _subscribedSegments = [];
    private INotifyCollectionChanged? _subscribedSegmentCollection;
    private double[]? _cachedSegmentRatios;
    private int _currentSegmentIndex = -1;
    private double _trackHeight = DefaultTrackHeight;
    private bool _isDragging;
    private bool _templatePartsHooked;

    /// <summary>
    /// Defines the <see cref="SegmentCount"/> property.
    /// </summary>
    public static readonly StyledProperty<int> SegmentCountProperty =
        AvaloniaProperty.Register<SegmentedSlider, int>(
            nameof(SegmentCount),
            DefaultSegmentCount,
            coerce: CoerceSegmentCount);

    /// <summary>
    /// Defines the <see cref="Segments"/> property.
    /// </summary>
    public static readonly StyledProperty<IList<SegmentedSliderSegment>> SegmentsProperty =
        AvaloniaProperty.Register<SegmentedSlider, IList<SegmentedSliderSegment>>(
            nameof(Segments),
            new AvaloniaList<SegmentedSliderSegment>(),
            coerce: static (_, value) => value ?? new AvaloniaList<SegmentedSliderSegment>());

    /// <summary>
    /// Defines the <see cref="Spacing"/> property.
    /// </summary>
    public static readonly StyledProperty<double> SpacingProperty =
        AvaloniaProperty.Register<SegmentedSlider, double>(
            nameof(Spacing),
            DefaultSpacing,
            coerce: CoerceNonNegative);

    /// <summary>
    /// Defines the <see cref="TitleVisibility"/> property.
    /// </summary>
    public static readonly StyledProperty<SegmentTitleVisibility> TitleVisibilityProperty =
        AvaloniaProperty.Register<SegmentedSlider, SegmentTitleVisibility>(
            nameof(TitleVisibility),
            SegmentTitleVisibility.AlwaysVisible,
            coerce: static (_, value) => Enum.IsDefined(value) ? value : SegmentTitleVisibility.AlwaysVisible);

    /// <summary>
    /// Defines the <see cref="IsReadOnly"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        AvaloniaProperty.Register<SegmentedSlider, bool>(nameof(IsReadOnly));

    /// <summary>
    /// Defines the <see cref="IsSnapToSegmentEnabled"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsSnapToSegmentEnabledProperty =
        AvaloniaProperty.Register<SegmentedSlider, bool>(nameof(IsSnapToSegmentEnabled));

    /// <summary>
    /// Gets or sets the number of equal-width segments. Ignored when <see cref="Segments"/> contains items.
    /// </summary>
    public int SegmentCount { get => GetValue(SegmentCountProperty); set => SetValue(SegmentCountProperty, value); }

    /// <summary>
    /// Gets or sets custom segments with optional labels, brushes, and proportional widths.
    /// </summary>
    [Content]
    public IList<SegmentedSliderSegment> Segments { get => GetValue(SegmentsProperty); set => SetValue(SegmentsProperty, value); }

    /// <summary>
    /// Gets or sets the spacing between segment track pieces.
    /// </summary>
    public double Spacing { get => GetValue(SpacingProperty); set => SetValue(SpacingProperty, value); }

    /// <summary>
    /// Gets or sets when segment titles are visible.
    /// </summary>
    public SegmentTitleVisibility TitleVisibility { get => GetValue(TitleVisibilityProperty); set => SetValue(TitleVisibilityProperty, value); }

    /// <summary>
    /// Gets or sets a value indicating whether user interaction can change <see cref="RangeBase.Value"/>.
    /// </summary>
    public bool IsReadOnly { get => GetValue(IsReadOnlyProperty); set => SetValue(IsReadOnlyProperty, value); }

    /// <summary>
    /// Gets or sets a value indicating whether dragging snaps to the nearest segment center when completed.
    /// </summary>
    public bool IsSnapToSegmentEnabled { get => GetValue(IsSnapToSegmentEnabledProperty); set => SetValue(IsSnapToSegmentEnabledProperty, value); }

    /// <summary>
    /// Occurs when the active segment changes.
    /// </summary>
    public event EventHandler<SegmentChangedEventArgs>? SegmentChanged;

    static SegmentedSlider()
    {
        AffectsMeasure<SegmentedSlider>(
            SegmentCountProperty,
            SpacingProperty,
            CornerRadiusProperty,
            TitleVisibilityProperty,
            FontSizeProperty);

        MinimumProperty.Changed.AddClassHandler<SegmentedSlider>((slider, _) => slider.CoerceValueToRange());
        MaximumProperty.Changed.AddClassHandler<SegmentedSlider>((slider, _) => slider.CoerceValueToRange());

        FocusableProperty.OverrideDefaultValue<SegmentedSlider>(true);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SegmentedSlider"/> class.
    /// </summary>
    public SegmentedSlider()
    {
        Segments = new AvaloniaList<SegmentedSliderSegment>();
        ResourcesChanged += OnResourcesChanged;
        ActualThemeVariantChanged += OnActualThemeVariantChanged;
        RefreshCurrentSegmentIndex();
        UpdatePseudoClasses();
    }

    /// <inheritdoc/>
    protected override AutomationPeer OnCreateAutomationPeer() => new SegmentedSliderAutomationPeer(this);

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        UnhookTemplateParts();

        base.OnApplyTemplate(e);

        _trackPanel = e.NameScope.Find<Panel>("PART_Track");
        _thumb = e.NameScope.Find<Thumb>("PART_Thumb");

        ApplyTrackHeightResource();
        HookTemplateParts();

        RebuildAndUpdate();
    }

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        SubscribeSegments(Segments);
        HookTemplateParts();
        RebuildAndUpdate();
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        UnhookTemplateParts();
        UnsubscribeSegments(Segments);
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var result = base.ArrangeOverride(finalSize);
        UpdateFill();
        UpdateThumbPosition();
        return result;
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ValueProperty)
        {
            UpdateFill();
            UpdateThumbPosition();
            UpdateTitleVisibility();
            DetectSegmentChange();
        }
        else if (change.Property == MinimumProperty || change.Property == MaximumProperty)
        {
            RebuildAndUpdate();
        }
        else if (change.Property == SegmentsProperty)
        {
            var oldSegments = change.GetOldValue<IList<SegmentedSliderSegment>?>();
            var newSegments = change.GetNewValue<IList<SegmentedSliderSegment>?>();
            UnsubscribeSegments(oldSegments);
            SubscribeSegments(newSegments);
            RebuildAndUpdate();
        }
        else if (change.Property == SegmentCountProperty ||
                 change.Property == SpacingProperty ||
                 change.Property == CornerRadiusProperty)
        {
            RebuildAndUpdate();
        }
        else if (change.Property == BackgroundProperty || change.Property == ForegroundProperty)
        {
            ApplyBrushes();
        }
        else if (change.Property == TitleVisibilityProperty)
        {
            UpdateTitleVisibility();
        }
        else if (change.Property == IsReadOnlyProperty)
        {
            UpdatePseudoClasses();
        }
        else if (change.Property == FontSizeProperty)
        {
            var titleFontSize = GetTitleFontSize();
            foreach (var titleBlock in _titleBlocks)
                titleBlock.FontSize = titleFontSize;
        }
        else if (change.Property == IsEnabledProperty)
        {
            UpdatePseudoClasses();
        }
    }

    /// <inheritdoc/>
    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        CompleteDrag();
    }

    /// <inheritdoc/>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (IsReadOnly || !IsEnabled)
        {
            base.OnKeyDown(e);
            return;
        }

        var smallChange = GetSmallInteractionChange();
        var largeChange = GetLargeInteractionChange();

        switch (e.Key)
        {
            case Key.Right:
            case Key.Up:
                Value = Math.Min(Value + smallChange, Maximum);
                e.Handled = true;
                break;
            case Key.Left:
            case Key.Down:
                Value = Math.Max(Value - smallChange, Minimum);
                e.Handled = true;
                break;
            case Key.Home:
                Value = Minimum;
                e.Handled = true;
                break;
            case Key.End:
                Value = Maximum;
                e.Handled = true;
                break;
            case Key.PageUp:
                Value = Math.Min(Value + largeChange, Maximum);
                e.Handled = true;
                break;
            case Key.PageDown:
                Value = Math.Max(Value - largeChange, Minimum);
                e.Handled = true;
                break;
            default:
                base.OnKeyDown(e);
                break;
        }
    }

    internal int CalculateSegmentIndex(double value)
    {
        var count = GetEffectiveSegmentCount();
        var range = Maximum - Minimum;
        if (count <= 0 || range <= 0)
            return 0;

        var ratio = Math.Clamp((value - Minimum) / range, 0, 1);

        if (Segments is { Count: > 0 })
        {
            var totalRatio = 0.0;
            foreach (var segment in Segments)
                totalRatio += CoerceSegmentRatio(segment.WidthRatio);

            if (totalRatio <= 0)
                return 0;

            var cumulative = 0.0;
            for (var i = 0; i < Segments.Count; i++)
            {
                cumulative += CoerceSegmentRatio(Segments[i].WidthRatio) / totalRatio;
                if (ratio < cumulative || i == Segments.Count - 1)
                    return i;
            }
        }

        var index = (int)(ratio * count);
        return Math.Clamp(index, 0, count - 1);
    }

    internal double GetSmallInteractionChange()
    {
        if (SmallChange > 0)
            return SmallChange;

        var range = Maximum - Minimum;
        return range > 0 ? range / Math.Max(GetEffectiveSegmentCount(), MinSegmentCount) : 0;
    }

    internal double GetLargeInteractionChange()
    {
        if (LargeChange > 0)
            return LargeChange;

        var range = Maximum - Minimum;
        return range > 0 ? range * LargeChangeRatio : 0;
    }

    internal static double CoerceSegmentRatio(double value)
        => double.IsFinite(value) ? Math.Max(0, value) : 0;

    private static int CoerceSegmentCount(AvaloniaObject sender, int value)
        => Math.Max(value, MinSegmentCount);

    private static double CoerceNonNegative(AvaloniaObject sender, double value)
        => double.IsFinite(value) ? Math.Max(0, value) : 0;

    private void CoerceValueToRange()
    {
        Value = Maximum < Minimum ? Minimum : Math.Clamp(Value, Minimum, Maximum);
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":readonly", IsReadOnly);
    }

    private void OnResourcesChanged(object? sender, ResourcesChangedEventArgs e)
    {
        RefreshTrackHeightResource();
        UpdateThumbPosition();
    }

    private void OnActualThemeVariantChanged(object? sender, EventArgs e)
    {
        RefreshTrackHeightResource();
        UpdateThumbPosition();
    }

    private void UnhookTemplateParts()
    {
        if (!_templatePartsHooked)
            return;

        if (_trackPanel != null)
            _trackPanel.PointerPressed -= OnTrackPointerPressed;

        if (_thumb != null)
        {
            _thumb.DragStarted -= OnThumbDragStarted;
            _thumb.DragDelta -= OnThumbDragDelta;
            _thumb.DragCompleted -= OnThumbDragCompleted;
            _thumb.PointerCaptureLost -= OnThumbPointerCaptureLost;
            _thumb.PropertyChanged -= OnThumbPropertyChanged;
        }

        _templatePartsHooked = false;
    }

    private void HookTemplateParts()
    {
        if (_templatePartsHooked)
            return;

        if (_trackPanel != null)
            _trackPanel.PointerPressed += OnTrackPointerPressed;

        if (_thumb != null)
        {
            _thumb.DragStarted += OnThumbDragStarted;
            _thumb.DragDelta += OnThumbDragDelta;
            _thumb.DragCompleted += OnThumbDragCompleted;
            _thumb.PointerCaptureLost += OnThumbPointerCaptureLost;
            _thumb.PropertyChanged += OnThumbPropertyChanged;
        }

        _templatePartsHooked = _trackPanel != null || _thumb != null;
    }

    private void SubscribeSegments(IList<SegmentedSliderSegment>? segments)
    {
        if (segments == null)
            return;

        foreach (var segment in segments)
            SubscribeSegment(segment);

        if (segments is INotifyCollectionChanged collectionChanged &&
            !ReferenceEquals(_subscribedSegmentCollection, collectionChanged))
        {
            if (_subscribedSegmentCollection != null)
                _subscribedSegmentCollection.CollectionChanged -= OnSegmentsCollectionChanged;

            collectionChanged.CollectionChanged += OnSegmentsCollectionChanged;
            _subscribedSegmentCollection = collectionChanged;
        }
    }

    private void UnsubscribeSegments(IList<SegmentedSliderSegment>? segments)
    {
        if (segments == null)
            return;

        foreach (var segment in segments)
            UnsubscribeSegment(segment);

        if (segments is INotifyCollectionChanged collectionChanged &&
            ReferenceEquals(_subscribedSegmentCollection, collectionChanged))
        {
            collectionChanged.CollectionChanged -= OnSegmentsCollectionChanged;
            _subscribedSegmentCollection = null;
        }
    }

    private void SubscribeSegment(SegmentedSliderSegment segment)
    {
        if (_subscribedSegments.Add(segment))
            segment.PropertyChanged += OnSegmentPropertyChanged;
    }

    private void UnsubscribeSegment(SegmentedSliderSegment segment)
    {
        if (_subscribedSegments.Remove(segment))
            segment.PropertyChanged -= OnSegmentPropertyChanged;
    }

    private void OnSegmentsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            var subscribedSegments = new List<SegmentedSliderSegment>(_subscribedSegments);
            foreach (var segment in subscribedSegments)
                UnsubscribeSegment(segment);

            foreach (var segment in Segments)
                SubscribeSegment(segment);
        }

        if (e.OldItems != null)
        {
            foreach (SegmentedSliderSegment segment in e.OldItems)
                UnsubscribeSegment(segment);
        }

        if (e.NewItems != null)
        {
            foreach (SegmentedSliderSegment segment in e.NewItems)
                SubscribeSegment(segment);
        }

        RebuildAndUpdate();
    }

    private void OnSegmentPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SegmentedSliderSegment.WidthRatio))
        {
            RebuildAndUpdate();
        }
        else if (e.PropertyName == nameof(SegmentedSliderSegment.FillBrush) ||
                 e.PropertyName == nameof(SegmentedSliderSegment.TrackBrush))
        {
            ApplyBrushes();
        }
        else if (e.PropertyName == nameof(SegmentedSliderSegment.Title))
        {
            UpdateSegmentTitles();
        }
    }

    private void OnThumbPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == BoundsProperty)
        {
            var oldBounds = change.GetOldValue<Rect>();
            var newBounds = change.GetNewValue<Rect>();
            if (oldBounds.Size == newBounds.Size)
                return;
        }
        else if (change.Property != WidthProperty && change.Property != HeightProperty)
        {
            return;
        }

        UpdateThumbPosition();
    }

    private void RebuildAndUpdate()
    {
        _cachedSegmentRatios = null;
        BuildSegmentVisuals();
        UpdateFill();
        UpdateThumbPosition();
        UpdateTitleVisibility();
        RefreshCurrentSegmentIndex();
    }

    private int GetEffectiveSegmentCount()
        => Segments is { Count: > 0 } ? Segments.Count : SegmentCount;

    private SegmentedSliderSegment? GetSegment(int index)
        => Segments is { Count: > 0 } && index >= 0 && index < Segments.Count ? Segments[index] : null;

    private void DetectSegmentChange()
    {
        var newIndex = CalculateSegmentIndex(Value);
        if (newIndex == _currentSegmentIndex)
            return;

        var oldIndex = _currentSegmentIndex;
        _currentSegmentIndex = newIndex;

        SegmentChanged?.Invoke(this, new SegmentChangedEventArgs(oldIndex, newIndex, GetSegment(newIndex)));
    }

    private void RefreshCurrentSegmentIndex()
    {
        _currentSegmentIndex = CalculateSegmentIndex(Value);
    }

    private void BuildSegmentVisuals()
    {
        if (_trackPanel == null)
            return;

        _trackPanel.Children.Clear();
        _trackRects.Clear();
        _fillRects.Clear();
        _titleBlocks.Clear();

        var count = GetEffectiveSegmentCount();
        if (count <= 0)
            return;

        var trackBrush = Background ?? DefaultBackground;
        var fillBrush = Foreground ?? DefaultForeground;
        var cornerRadius = GetTrackCornerRadius();
        var trackHeight = GetTrackHeight();
        var titleFontSize = GetTitleFontSize();

        _segmentGrid = new Grid
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            RowDefinitions = new RowDefinitions("Auto,Auto"),
            ColumnDefinitions = BuildColumnDefinitions(count),
            IsHitTestVisible = false
        };

        for (var i = 0; i < count; i++)
        {
            var column = i * 2;
            var segment = GetSegment(i);

            var trackRect = new Rectangle
            {
                Height = trackHeight,
                Fill = segment?.TrackBrush ?? trackBrush,
                RadiusX = cornerRadius,
                RadiusY = cornerRadius,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center
            };
            BindToTrackHeight(trackRect);

            var fillRect = new Rectangle
            {
                Height = trackHeight,
                Fill = segment?.FillBrush ?? fillBrush,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 0
            };
            BindToTrackHeight(fillRect);

            var fillClip = new Border
            {
                CornerRadius = new CornerRadius(cornerRadius),
                ClipToBounds = true,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                Height = trackHeight,
                Child = fillRect
            };
            BindToTrackHeight(fillClip);

            Grid.SetRow(trackRect, 0);
            Grid.SetColumn(trackRect, column);
            Grid.SetRow(fillClip, 0);
            Grid.SetColumn(fillClip, column);

            _segmentGrid.Children.Add(trackRect);
            _segmentGrid.Children.Add(fillClip);

            var titleBlock = new TextBlock
            {
                Text = segment?.Title ?? string.Empty,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                FontSize = titleFontSize,
                Foreground = Foreground,
                Margin = new Thickness(0, 6, 0, 0)
            };

            Grid.SetRow(titleBlock, 1);
            Grid.SetColumn(titleBlock, column);

            _segmentGrid.Children.Add(titleBlock);

            _trackRects.Add(trackRect);
            _fillRects.Add(fillRect);
            _titleBlocks.Add(titleBlock);
        }

        _trackPanel.Children.Add(_segmentGrid);
    }

    private ColumnDefinitions BuildColumnDefinitions(int count)
    {
        var definitions = new ColumnDefinitions();
        var ratios = ComputeSegmentRatios(count);

        for (var i = 0; i < count; i++)
        {
            definitions.Add(new ColumnDefinition(ratios[i], GridUnitType.Star));

            if (i < count - 1)
                definitions.Add(new ColumnDefinition(Spacing, GridUnitType.Pixel));
        }

        return definitions;
    }

    private void UpdateSegmentTitles()
    {
        for (var i = 0; i < _titleBlocks.Count; i++)
            _titleBlocks[i].Text = GetSegment(i)?.Title ?? string.Empty;
    }

    internal void UpdateFill()
    {
        if (_segmentGrid == null || _trackRects.Count == 0)
            return;

        var count = _trackRects.Count;
        var range = Maximum - Minimum;
        var fillRatio = range > 0 ? Math.Clamp((Value - Minimum) / range, 0, 1) : 0;
        var segmentRatios = _cachedSegmentRatios ??= ComputeSegmentRatios(count);
        var consumed = 0.0;

        for (var i = 0; i < count; i++)
        {
            var segmentRatio = segmentRatios[i];
            var segmentWidth = _trackRects[i].Bounds.Width;

            double segmentFill;
            if (fillRatio >= consumed + segmentRatio)
                segmentFill = segmentWidth;
            else if (fillRatio > consumed && segmentRatio > 0)
                segmentFill = ((fillRatio - consumed) / segmentRatio) * segmentWidth;
            else
                segmentFill = 0;

            _fillRects[i].Width = Math.Max(0, segmentFill);
            consumed += segmentRatio;
        }
    }

    private double[] ComputeSegmentRatios(int count)
    {
        var ratios = new double[count];

        if (Segments is { Count: > 0 } && Segments.Count == count)
        {
            var totalRatio = 0.0;
            foreach (var segment in Segments)
                totalRatio += CoerceSegmentRatio(segment.WidthRatio);

            if (totalRatio > 0)
            {
                for (var i = 0; i < count; i++)
                    ratios[i] = CoerceSegmentRatio(Segments[i].WidthRatio) / totalRatio;

                return ratios;
            }
        }

        var equalRatio = 1.0 / count;
        Array.Fill(ratios, equalRatio);
        return ratios;
    }

    private void UpdateThumbPosition()
    {
        if (_thumb == null || _trackPanel == null)
            return;

        var trackWidth = _trackPanel.Bounds.Width;
        if (trackWidth <= 0)
            return;

        var range = Maximum - Minimum;
        var ratio = range > 0 ? Math.Clamp((Value - Minimum) / range, 0, 1) : 0;
        var thumbWidth = _thumb.Bounds.Width > 0 ? _thumb.Bounds.Width : DefaultThumbSize;
        var thumbHeight = _thumb.Bounds.Height > 0 ? _thumb.Bounds.Height : DefaultThumbSize;
        var trackOffset = _trackPanel.Bounds.Position;
        var trackCenterY = GetTrackHeight() / 2;
        var thumbCenterOffset = thumbHeight / 2;
        var trackValueX = GetTrackValuePixel(ratio);

        if (_thumb.RenderTransform is not TranslateTransform transform)
        {
            transform = new TranslateTransform();
            _thumb.RenderTransform = transform;
        }

        transform.X = trackOffset.X + trackValueX - thumbWidth / 2;
        transform.Y = trackOffset.Y + trackCenterY - thumbCenterOffset;
    }

    private double GetTrackValuePixel(double ratio)
    {
        var trackWidth = _trackPanel?.Bounds.Width ?? 0;
        if (trackWidth <= 0)
            return 0;

        if (_trackRects.Count == 0)
            return ratio * trackWidth;

        var segmentRatios = _cachedSegmentRatios ??= ComputeSegmentRatios(_trackRects.Count);
        var consumed = 0.0;

        for (var i = 0; i < _trackRects.Count; i++)
        {
            var segmentRatio = segmentRatios[i];
            var segmentStartRatio = consumed;
            var segmentEndRatio = consumed + segmentRatio;
            var isLast = i == _trackRects.Count - 1;

            if (ratio <= segmentEndRatio || isLast)
            {
                var localRatio = segmentRatio > 0
                    ? Math.Clamp((ratio - segmentStartRatio) / segmentRatio, 0, 1)
                    : 0;
                return _trackRects[i].Bounds.X + localRatio * _trackRects[i].Bounds.Width;
            }

            consumed = segmentEndRatio;
        }

        return trackWidth;
    }

    private double GetRatioFromTrackPixel(double x)
    {
        var trackWidth = _trackPanel?.Bounds.Width ?? 0;
        if (trackWidth <= 0)
            return 0;

        var clampedX = Math.Clamp(x, 0, trackWidth);
        if (_trackRects.Count == 0)
            return clampedX / trackWidth;

        var segmentRatios = _cachedSegmentRatios ??= ComputeSegmentRatios(_trackRects.Count);
        var consumed = 0.0;

        for (var i = 0; i < _trackRects.Count; i++)
        {
            var rect = _trackRects[i].Bounds;
            var segmentRatio = segmentRatios[i];

            if (clampedX < rect.X)
                return consumed;

            if (clampedX <= rect.Right || i == _trackRects.Count - 1)
            {
                var localRatio = rect.Width > 0
                    ? Math.Clamp((clampedX - rect.X) / rect.Width, 0, 1)
                    : 0;
                return Math.Clamp(consumed + localRatio * segmentRatio, 0, 1);
            }

            consumed += segmentRatio;
        }

        return 1;
    }

    private void ApplyBrushes()
    {
        var trackBrush = Background ?? DefaultBackground;
        var fillBrush = Foreground ?? DefaultForeground;

        for (var i = 0; i < _trackRects.Count; i++)
        {
            var segment = GetSegment(i);

            _trackRects[i].Fill = segment?.TrackBrush ?? trackBrush;
            _fillRects[i].Fill = segment?.FillBrush ?? fillBrush;
        }

        foreach (var titleBlock in _titleBlocks)
            titleBlock.Foreground = Foreground;
    }

    private double GetTrackCornerRadius()
    {
        var radius = CornerRadius.TopLeft;
        return double.IsFinite(radius) ? Math.Max(0, radius) : 0;
    }

    private double GetTitleFontSize()
        => double.IsFinite(FontSize) ? Math.Max(1.0, FontSize) : DefaultTitleFontSize;

    private double GetTrackHeight()
        => _trackHeight;

    private void RefreshTrackHeightResource()
    {
        if (!ApplyTrackHeightResource())
            return;

        RebuildAndUpdate();
    }

    private bool ApplyTrackHeightResource()
    {
        var trackHeight = GetTrackHeightResource();
        var changed = Math.Abs(_trackHeight - trackHeight) >= 0.001;

        _trackHeight = trackHeight;
        return changed;
    }

    private double GetTrackHeightResource()
    {
        if (Resources.ContainsKey(TrackHeightResourceKey))
            return CoerceTrackHeightResource(Resources[TrackHeightResourceKey]);

        if (Resources.TryGetResource(TrackHeightResourceKey, ActualThemeVariant, out var resource) ||
            Resources.TryGetResource(TrackHeightResourceKey, null, out resource) ||
            TryGetResource(TrackHeightResourceKey, null, out resource) ||
            TryGetResource(TrackHeightResourceKey, ActualThemeVariant, out resource))
        {
            return CoerceTrackHeightResource(resource);
        }

        return DefaultTrackHeight;
    }

    private static double CoerceTrackHeightResource(object? resource)
        => resource is double height && double.IsFinite(height) ? Math.Max(0, height) : DefaultTrackHeight;

    private void BindToTrackHeight(Layoutable layoutable)
    {
        layoutable.Height = _trackHeight;
    }

    private void UpdateTitleVisibility()
    {
        var activeIndex = CalculateSegmentIndex(Value);

        for (var i = 0; i < _titleBlocks.Count; i++)
        {
            var visible = TitleVisibility switch
            {
                SegmentTitleVisibility.Collapsed => false,
                SegmentTitleVisibility.AlwaysVisible => true,
                SegmentTitleVisibility.ActiveSegmentOnly => i == activeIndex,
                SegmentTitleVisibility.ActiveAndPrevious => i <= activeIndex,
                _ => true
            };

            _titleBlocks[i].IsVisible = visible;
        }
    }

    private void SetValueFromPosition(double x)
    {
        if (_trackPanel == null || IsReadOnly || !IsEnabled)
            return;

        var trackWidth = _trackPanel.Bounds.Width;
        if (trackWidth <= 0)
            return;

        var ratio = GetRatioFromTrackPixel(x);
        Value = Minimum + ratio * (Maximum - Minimum);
    }

    private void OnTrackPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (IsReadOnly || !IsEnabled)
            return;

        if (_trackPanel is null || e.Pointer.Type == PointerType.Touch)
            return;

        var point = e.GetCurrentPoint(_trackPanel);
        if (!point.Properties.IsLeftButtonPressed)
            return;

        Focus();
        SetValueFromPosition(e.GetPosition(_trackPanel).X);
        e.Handled = true;
    }

    private void OnThumbDragStarted(object? sender, VectorEventArgs e)
    {
        if (IsReadOnly || !IsEnabled)
            return;

        _isDragging = true;
    }

    private void OnThumbDragDelta(object? sender, VectorEventArgs e)
    {
        if (IsReadOnly || !IsEnabled || _thumb == null || _trackPanel == null)
            return;

        if (_thumb.RenderTransform is not TranslateTransform transform)
            return;

        var thumbWidth = _thumb.Bounds.Width > 0 ? _thumb.Bounds.Width : DefaultThumbSize;
        var trackOffsetX = _trackPanel.Bounds.Position.X;
        var newThumbCenterX = transform.X + e.Vector.X - trackOffsetX + thumbWidth / 2;
        var ratio = GetRatioFromTrackPixel(newThumbCenterX);
        Value = Minimum + ratio * (Maximum - Minimum);
    }

    private void OnThumbDragCompleted(object? sender, VectorEventArgs e)
    {
        CompleteDrag();
    }

    private void OnThumbPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        CompleteDrag();
    }

    private void CompleteDrag()
    {
        if (!_isDragging)
            return;

        _isDragging = false;

        if (IsSnapToSegmentEnabled)
            SnapValueToSegmentCenter();
    }

    private void SnapValueToSegmentCenter()
    {
        var count = GetEffectiveSegmentCount();
        var range = Maximum - Minimum;
        if (count <= 0 || range <= 0)
            return;

        var ratios = _cachedSegmentRatios ??= ComputeSegmentRatios(count);
        var valueRatio = Math.Clamp((Value - Minimum) / range, 0, 1);
        var cumulative = 0.0;
        var nearestMidpoint = 0.0;
        var nearestDistance = double.PositiveInfinity;

        for (var i = 0; i < ratios.Length; i++)
        {
            var segmentEnd = cumulative + ratios[i];
            var segmentMidpoint = (cumulative + segmentEnd) / 2;
            var distance = Math.Abs(valueRatio - segmentMidpoint);

            if (distance < nearestDistance)
            {
                nearestMidpoint = segmentMidpoint;
                nearestDistance = distance;
            }

            cumulative = segmentEnd;
        }

        Value = Minimum + nearestMidpoint * range;
    }
}
