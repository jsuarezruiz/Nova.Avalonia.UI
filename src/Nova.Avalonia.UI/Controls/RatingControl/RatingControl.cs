using System;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;

namespace Nova.Avalonia.UI.Controls
{
    /// <summary>
    /// A Rating Control with support to various shapes or precision levels.
    /// </summary>
    public class RatingControl : TemplatedControl
    {
        private ItemsControl? _itemsHost;
        private readonly AvaloniaList<RatingItem> _items = new();
        private bool _isHovering;
        private double _previewValue;

        /// <summary>
        /// Defines the <see cref="Value"/> property.
        /// </summary>
        public static readonly StyledProperty<double> ValueProperty =
            AvaloniaProperty.Register<RatingControl, double>(nameof(Value), 0.0, coerce: CoerceValue);

        /// <summary>
        /// Defines the <see cref="ItemCount"/> property.
        /// </summary>
        public static readonly StyledProperty<int> ItemCountProperty =
            AvaloniaProperty.Register<RatingControl, int>(nameof(ItemCount), 5, coerce: CoerceItemCount);

        /// <summary>
        /// Defines the <see cref="Precision"/> property.
        /// </summary>
        public static readonly StyledProperty<RatingPrecision> PrecisionProperty =
            AvaloniaProperty.Register<RatingControl, RatingPrecision>(nameof(Precision), RatingPrecision.Full);

        /// <summary>
        /// Defines the <see cref="IsReadOnly"/> property.
        /// </summary>
        public static readonly StyledProperty<bool> IsReadOnlyProperty =
            AvaloniaProperty.Register<RatingControl, bool>(nameof(IsReadOnly), false);

        /// <summary>
        /// Defines the <see cref="Shape"/> property.
        /// </summary>
        public static readonly StyledProperty<RatingShape> ShapeProperty =
            AvaloniaProperty.Register<RatingControl, RatingShape>(nameof(Shape), RatingShape.Star);

        /// <summary>
        /// Defines the <see cref="CustomGeometry"/> property.
        /// </summary>
        public static readonly StyledProperty<Geometry?> CustomGeometryProperty =
            AvaloniaProperty.Register<RatingControl, Geometry?>(nameof(CustomGeometry));

        /// <summary>
        /// Defines the <see cref="ItemSize"/> property.
        /// </summary>
        public static readonly StyledProperty<double> ItemSizeProperty =
            AvaloniaProperty.Register<RatingControl, double>(nameof(ItemSize), 32.0);

        /// <summary>
        /// Defines the <see cref="ItemSpacing"/> property.
        /// </summary>
        public static readonly StyledProperty<double> ItemSpacingProperty =
            AvaloniaProperty.Register<RatingControl, double>(nameof(ItemSpacing), 6.0);

        /// <summary>
        /// Defines the <see cref="Orientation"/> property.
        /// </summary>
        public static readonly StyledProperty<Orientation> OrientationProperty =
            AvaloniaProperty.Register<RatingControl, Orientation>(nameof(Orientation), Orientation.Horizontal);

        /// <summary>
        /// Defines the <see cref="RatedFill"/> property.
        /// </summary>
        public static readonly StyledProperty<IBrush?> RatedFillProperty =
            AvaloniaProperty.Register<RatingControl, IBrush?>(nameof(RatedFill), Brushes.Gold);

        /// <summary>
        /// Defines the <see cref="UnratedFill"/> property.
        /// </summary>
        public static readonly StyledProperty<IBrush?> UnratedFillProperty =
            AvaloniaProperty.Register<RatingControl, IBrush?>(nameof(UnratedFill), Brushes.LightGray);

        /// <summary>
        /// Defines the <see cref="RatedStroke"/> property.
        /// </summary>
        public static readonly StyledProperty<IBrush?> RatedStrokeProperty =
            AvaloniaProperty.Register<RatingControl, IBrush?>(nameof(RatedStroke));

        /// <summary>
        /// Defines the <see cref="UnratedStroke"/> property.
        /// </summary>
        public static readonly StyledProperty<IBrush?> UnratedStrokeProperty =
            AvaloniaProperty.Register<RatingControl, IBrush?>(nameof(UnratedStroke));

        /// <summary>
        /// Defines the <see cref="StrokeThickness"/> property.
        /// </summary>
        public static readonly StyledProperty<double> StrokeThicknessProperty =
            AvaloniaProperty.Register<RatingControl, double>(nameof(StrokeThickness), 0.0);

        /// <summary>
        /// Defines the <see cref="PreviewFill"/> property.
        /// </summary>
        public static readonly StyledProperty<IBrush?> PreviewFillProperty =
            AvaloniaProperty.Register<RatingControl, IBrush?>(nameof(PreviewFill), Brushes.Orange);

        /// <summary>
        /// Defines the <see cref="PreviewStroke"/> property.
        /// </summary>
        public static readonly StyledProperty<IBrush?> PreviewStrokeProperty =
            AvaloniaProperty.Register<RatingControl, IBrush?>(nameof(PreviewStroke));

        /// <summary>
        /// Defines the <see cref="ValueChanged"/> routed event.
        /// </summary>
        public static readonly RoutedEvent<RoutedEventArgs> ValueChangedEvent =
            RoutedEvent.Register<RatingControl, RoutedEventArgs>(nameof(ValueChanged), RoutingStrategies.Bubble);

        /// <summary>
        /// Gets or sets the current value of the rating.
        /// </summary>
        public double Value { get => GetValue(ValueProperty); set => SetValue(ValueProperty, value); }

        /// <summary>
        /// Gets or sets the number of items (stars, hearts, etc.) to display.
        /// </summary>
        public int ItemCount { get => GetValue(ItemCountProperty); set => SetValue(ItemCountProperty, value); }

        /// <summary>
        /// Gets or sets the precision of the rating (Full, Half, or Exact).
        /// </summary>
        public RatingPrecision Precision { get => GetValue(PrecisionProperty); set => SetValue(PrecisionProperty, value); }

        /// <summary>
        /// Gets or sets a value indicating whether the control is read-only.
        /// </summary>
        public bool IsReadOnly { get => GetValue(IsReadOnlyProperty); set => SetValue(IsReadOnlyProperty, value); }

        /// <summary>
        /// Gets or sets the shape of the rating items.
        /// </summary>
        public RatingShape Shape { get => GetValue(ShapeProperty); set => SetValue(ShapeProperty, value); }

        /// <summary>
        /// Gets or sets the custom geometry to use when Shape is set to Custom.
        /// </summary>
        public Geometry? CustomGeometry { get => GetValue(CustomGeometryProperty); set => SetValue(CustomGeometryProperty, value); }

        /// <summary>
        /// Gets or sets the size of each rating item.
        /// </summary>
        public double ItemSize { get => GetValue(ItemSizeProperty); set => SetValue(ItemSizeProperty, value); }

        /// <summary>
        /// Gets or sets the spacing between rating items.
        /// </summary>
        public double ItemSpacing { get => GetValue(ItemSpacingProperty); set => SetValue(ItemSpacingProperty, value); }

        /// <summary>
        /// Gets or sets the orientation of the rating items (Horizontal or Vertical).
        /// </summary>
        public Orientation Orientation { get => GetValue(OrientationProperty); set => SetValue(OrientationProperty, value); }

        /// <summary>
        /// Gets or sets the fill brush for rated items (active state).
        /// </summary>
        public IBrush? RatedFill { get => GetValue(RatedFillProperty); set => SetValue(RatedFillProperty, value); }

        /// <summary>
        /// Gets or sets the fill brush for unrated items (inactive state).
        /// </summary>
        public IBrush? UnratedFill { get => GetValue(UnratedFillProperty); set => SetValue(UnratedFillProperty, value); }

        /// <summary>
        /// Gets or sets the stroke brush for rated items.
        /// </summary>
        public IBrush? RatedStroke { get => GetValue(RatedStrokeProperty); set => SetValue(RatedStrokeProperty, value); }

        /// <summary>
        /// Gets or sets the stroke brush for unrated items.
        /// </summary>
        public IBrush? UnratedStroke { get => GetValue(UnratedStrokeProperty); set => SetValue(UnratedStrokeProperty, value); }

        /// <summary>
        /// Gets or sets the thickness of the stroke for items.
        /// </summary>
        public double StrokeThickness { get => GetValue(StrokeThicknessProperty); set => SetValue(StrokeThicknessProperty, value); }

        /// <summary>
        /// Gets or sets the fill brush used when hovering over items.
        /// </summary>
        public IBrush? PreviewFill { get => GetValue(PreviewFillProperty); set => SetValue(PreviewFillProperty, value); }

        /// <summary>
        /// Gets or sets the stroke brush used when hovering over items.
        /// </summary>
        public IBrush? PreviewStroke { get => GetValue(PreviewStrokeProperty); set => SetValue(PreviewStrokeProperty, value); }

        /// <summary>
        /// Occurs when the <see cref="Value"/> property changes.
        /// </summary>
        public event EventHandler<RoutedEventArgs>? ValueChanged
        {
            add => AddHandler(ValueChangedEvent, value);
            remove => RemoveHandler(ValueChangedEvent, value);
        }

        private const string PC_ReadOnly = ":readonly";

        static RatingControl()
        {
            // A11y: Enable Focus so users can Tab to it
            FocusableProperty.OverrideDefaultValue<RatingControl>(true);

            AvaloniaProperty[] affectsRender = { 
                RatedFillProperty, UnratedFillProperty, 
                RatedStrokeProperty, UnratedStrokeProperty, 
                StrokeThicknessProperty, PreviewFillProperty, PreviewStrokeProperty 
            };
            
            foreach (var prop in affectsRender)
                prop.Changed.AddClassHandler<RatingControl>((x, _) => x.UpdateItemVisuals());

            AvaloniaProperty[] affectsRegen = { 
                ItemCountProperty, ShapeProperty, 
                CustomGeometryProperty, ItemSizeProperty, 
                ItemSpacingProperty, OrientationProperty 
            };
            
            foreach (var prop in affectsRegen)
                prop.Changed.AddClassHandler<RatingControl>((x, _) => x.RegenerateItems());
            
            ValueProperty.Changed.AddClassHandler<RatingControl>((x, e) => x.OnValueChanged(e));
            ItemCountProperty.Changed.AddClassHandler<RatingControl>(
                (x, _) => x.SetCurrentValue(ValueProperty, Math.Clamp(x.Value, 0, x.ItemCount)));
            IsReadOnlyProperty.Changed.AddClassHandler<RatingControl>((x, e) => x.UpdatePseudoClasses(e.NewValue is true));
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            _itemsHost = e.NameScope.Find<ItemsControl>("PART_ItemsHost");
            if (_itemsHost != null)
            {
                _itemsHost.ItemsSource = _items;
            }
            RegenerateItems();
            UpdatePseudoClasses(IsReadOnly);
        }

        private void UpdatePseudoClasses(bool isReadOnly)
        {
            PseudoClasses.Set(PC_ReadOnly, isReadOnly);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (!IsReadOnly)
            {
                double step = Precision switch
                {
                    RatingPrecision.Full => 1.0,
                    RatingPrecision.Half => 0.5,
                    RatingPrecision.Exact => 0.1,
                    _ => 1.0
                };

                double current = Value;
                bool handled = true;

                switch (e.Key)
                {
                    case Key.Left:
                    case Key.Down:
                        current -= step;
                        break;
                    case Key.Right:
                    case Key.Up:
                        current += step;
                        break;
                    case Key.Home:
                        current = 0;
                        break;
                    case Key.End:
                        current = ItemCount;
                        break;
                    default:
                        handled = false;
                        break;
                }

                if (handled)
                {
                    Value = Math.Clamp(current, 0, ItemCount);
                    e.Handled = true;
                    return;
                }
            }
            base.OnKeyDown(e);
        }

        protected override void OnPointerEntered(PointerEventArgs e)
        {
            base.OnPointerEntered(e);
            if (IsReadOnly) return;
            _isHovering = true;
            UpdatePreview(e);
        }

        protected override void OnPointerExited(PointerEventArgs e)
        {
            base.OnPointerExited(e);
            if (IsReadOnly) return;
            _isHovering = false;
            UpdateItemStates();
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);
            if (IsReadOnly) return;
            UpdatePreview(e);
            e.Handled = true;
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            if (IsReadOnly) return;

            _isHovering = true;
            _previewValue = CalculateValueFromPointer(e);
            Value = _previewValue;

            e.Pointer.Capture(this);
            e.Handled = true;

            Focus();
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);
            if (Equals(e.Pointer.Captured, this))
            {
                Value = _previewValue;
                e.Pointer.Capture(null);
                _isHovering = false;
                UpdateItemStates();
                e.Handled = true;
            }
        }

        private void UpdatePreview(PointerEventArgs e)
        {
            _previewValue = CalculateValueFromPointer(e);
            UpdateItemStates();
        }

        private double CalculateValueFromPointer(PointerEventArgs e)
        {
            bool isVertical = Orientation == Orientation.Vertical;

            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                var pointInItem = e.GetPosition(item);

                bool isInsideItem = pointInItem.X >= 0 && pointInItem.X <= item.Bounds.Width &&
                                    pointInItem.Y >= 0 && pointInItem.Y <= item.Bounds.Height;

                if (isInsideItem)
                {
                    double localCoord = isVertical ? pointInItem.Y : pointInItem.X;
                    double itemDim = isVertical ? item.Bounds.Height : item.Bounds.Width;
                    double ratio = Math.Clamp(localCoord / itemDim, 0, 1);
                    double preciseAdd = CalculatePrecision(ratio);
                    return Math.Clamp(i + preciseAdd, 0, ItemCount);
                }
            }

            var pointInControl = e.GetPosition(this);
            double coordinate = isVertical ? pointInControl.Y : pointInControl.X;
            double controlDim = isVertical ? Bounds.Height : Bounds.Width;

            if (coordinate <= 0)
                return 0;
            if (coordinate >= controlDim)
                return ItemCount;

            for (int i = 0; i < _items.Count - 1; i++)
            {
                var currentItem = _items[i];
                var nextItem = _items[i + 1];

                var currentEnd = isVertical
                    ? currentItem.TranslatePoint(new Point(0, currentItem.Bounds.Height), this)?.Y ?? 0
                    : currentItem.TranslatePoint(new Point(currentItem.Bounds.Width, 0), this)?.X ?? 0;

                var nextStart = isVertical
                    ? nextItem.TranslatePoint(new Point(0, 0), this)?.Y ?? 0
                    : nextItem.TranslatePoint(new Point(0, 0), this)?.X ?? 0;

                if (coordinate >= currentEnd && coordinate <= nextStart)
                {
                    return i + 1;
                }
            }

            return ItemCount;
        }

        private double CalculatePrecision(double ratio)
        {
            return Precision switch
            {
                RatingPrecision.Full => 1.0,
                RatingPrecision.Half => ratio <= 0.5 ? 0.5 : 1.0,
                RatingPrecision.Exact => ratio,
                _ => ratio
            };
        }

        private static double CoerceValue(AvaloniaObject obj, double value)
        {
            if (obj is not RatingControl rating) return value;
            return Math.Clamp(value, 0, rating.ItemCount);
        }

        private static int CoerceItemCount(AvaloniaObject obj, int value)
        {
            return Math.Max(1, value);
        }

        private void OnValueChanged(AvaloniaPropertyChangedEventArgs e)
        {
            UpdateItemStates();
            RaiseEvent(new RoutedEventArgs(ValueChangedEvent));
        }

        private void RegenerateItems()
        {
            _items.Clear();
            var geo = GetGeometry();
            
            bool isVertical = Orientation == Orientation.Vertical;

            for (int i = 0; i < ItemCount; i++)
            {
                var item = new RatingItem
                {
                    Geometry = geo,
                    Width = ItemSize,
                    Height = ItemSize,
                    ClipToBounds = false 
                };

                if (i < ItemCount - 1)
                {
                    item.Margin = isVertical 
                        ? new Thickness(0, 0, 0, ItemSpacing) 
                        : new Thickness(0, 0, ItemSpacing, 0);
                }

                _items.Add(item);
            }

            UpdateItemVisuals();
        }

        private void UpdateItemVisuals()
        {
            foreach (var item in _items)
            {
                item.StrokeThickness = StrokeThickness;
            }
            UpdateItemStates();
        }

        private void UpdateItemStates()
        {
            double effectiveValue = _isHovering ? _previewValue : Value;
            IBrush? effectiveFill = _isHovering ? (PreviewFill ?? RatedFill) : RatedFill;
            IBrush? effectiveStroke = _isHovering ? (PreviewStroke ?? RatedStroke) : RatedStroke;

            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                var fillRatio = Math.Clamp(effectiveValue - i, 0, 1);

                item.FillRatio = fillRatio;
                item.RatedFill = effectiveFill;
                item.UnratedFill = UnratedFill;
                item.RatedStroke = effectiveStroke;
                item.UnratedStroke = UnratedStroke;
            }
        }

        private Geometry GetGeometry()
        {
            return Shape switch
            {
                RatingShape.Star => StreamGeometry.Parse("M 12,2 L 14.5,9.5 L 22,10 L 17,15 L 18.5,22 L 12,18 L 5.5,22 L 7,15 L 2,10 L 9.5,9.5 Z"),
                RatingShape.Heart => StreamGeometry.Parse("M 12,21 C 12,21 3,14 3,8.5 C 3,5.5 5,3 8,3 C 10,3 11,4 12,6 C 13,4 14,3 16,3 C 19,3 21,5.5 21,8.5 C 21,14 12,21 12,21 Z"),
                RatingShape.Circle => new EllipseGeometry(new Rect(2, 2, 20, 20)),
                RatingShape.Diamond => StreamGeometry.Parse("M 12,2 L 22,12 L 12,22 L 2,12 Z"),
                RatingShape.Custom when CustomGeometry != null => CustomGeometry,
                _ => StreamGeometry.Parse("M 12,2 L 14.5,9.5 L 22,10 L 17,15 L 18.5,22 L 12,18 L 5.5,22 L 7,15 L 2,10 L 9.5,9.5 Z")
            };
        }
    }
}
