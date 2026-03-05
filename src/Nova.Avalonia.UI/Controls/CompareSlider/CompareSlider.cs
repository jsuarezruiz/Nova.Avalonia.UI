using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Nova.Avalonia.UI.Controls.AutomationPeers;

namespace Nova.Avalonia.UI.Controls
{
    /// <summary>
    /// A control that allows comparing two pieces of content side-by-side with a draggable slider.
    /// Inherits from <see cref="RangeBase"/>, so <see cref="RangeBase.Value"/> in the range
    /// [<see cref="RangeBase.Minimum"/>, <see cref="RangeBase.Maximum"/>] maps to the divider position.
    /// </summary>
    [TemplatePart("PART_BeforePanel", typeof(Panel))]
    [TemplatePart("PART_AfterPanel", typeof(Panel))]
    [TemplatePart("PART_Divider", typeof(Line))]
    [TemplatePart("PART_Thumb", typeof(Thumb))]
    [PseudoClasses(":dragging", ":horizontal", ":vertical")]
    public class CompareSlider : RangeBase
    {
        private Panel? _beforePanel;
        private Panel? _afterPanel;
        private Line? _divider;
        private Thumb? _thumb;
        private CancellationTokenSource? _animationCts;
        private readonly RectangleGeometry _beforeClip = new();
        private readonly RectangleGeometry _afterClip = new();

        static CompareSlider()
        {
            ValueProperty.Changed.AddClassHandler<CompareSlider>((x, e) => x.UpdateClipping());
            BoundsProperty.Changed.AddClassHandler<CompareSlider>((x, e) => x.UpdateClipping());
            IsDirectionReversedProperty.Changed.AddClassHandler<CompareSlider>((x, e) => x.UpdateClipping());
            OrientationProperty.Changed.AddClassHandler<CompareSlider>((x, e) =>
            {
                x.UpdateClipping();
                x.UpdateOrientationPseudoClasses(e.GetNewValue<Orientation>());
            });
        }

        public CompareSlider()
        {
            Value = 0.5;
            Minimum = 0.0;
            Maximum = 1.0;
            SmallChange = 0.01;
            LargeChange = 0.1;

            UpdateOrientationPseudoClasses(Orientation);
        }

        private void UpdateOrientationPseudoClasses(Orientation orientation)
        {
            PseudoClasses.Set(":horizontal", orientation == Orientation.Horizontal);
            PseudoClasses.Set(":vertical", orientation == Orientation.Vertical);
        }

        /// <summary>
        /// Identifies the <see cref="BeforeContent"/> property.
        /// </summary>
        public static readonly StyledProperty<object?> BeforeContentProperty =
            AvaloniaProperty.Register<CompareSlider, object?>(nameof(BeforeContent));

        /// <summary>
        /// Identifies the <see cref="BeforeContentTemplate"/> property.
        /// </summary>
        public static readonly StyledProperty<IDataTemplate?> BeforeContentTemplateProperty =
            AvaloniaProperty.Register<CompareSlider, IDataTemplate?>(nameof(BeforeContentTemplate));

        /// <summary>
        /// Identifies the <see cref="AfterContent"/> property.
        /// </summary>
        public static readonly StyledProperty<object?> AfterContentProperty =
            AvaloniaProperty.Register<CompareSlider, object?>(nameof(AfterContent));

        /// <summary>
        /// Identifies the <see cref="AfterContentTemplate"/> property.
        /// </summary>
        public static readonly StyledProperty<IDataTemplate?> AfterContentTemplateProperty =
            AvaloniaProperty.Register<CompareSlider, IDataTemplate?>(nameof(AfterContentTemplate));

        /// <summary>
        /// Identifies the <see cref="Orientation"/> property.
        /// </summary>
        public static readonly StyledProperty<Orientation> OrientationProperty =
            AvaloniaProperty.Register<CompareSlider, Orientation>(nameof(Orientation), Orientation.Horizontal);

        /// <summary>
        /// Identifies the <see cref="IsDirectionReversed"/> property.
        /// </summary>
        public static readonly StyledProperty<bool> IsDirectionReversedProperty =
            AvaloniaProperty.Register<CompareSlider, bool>(nameof(IsDirectionReversed));

        /// <summary>
        /// Identifies the <see cref="IsMoveToPointEnabled"/> property.
        /// </summary>
        public static readonly StyledProperty<bool> IsMoveToPointEnabledProperty =
            AvaloniaProperty.Register<CompareSlider, bool>(nameof(IsMoveToPointEnabled), true);

        /// <summary>
        /// Identifies the <see cref="IsDragging"/> property.
        /// </summary>
        public static readonly DirectProperty<CompareSlider, bool> IsDraggingProperty =
            AvaloniaProperty.RegisterDirect<CompareSlider, bool>(nameof(IsDragging), o => o.IsDragging);

        /// <summary>
        /// Identifies the <see cref="DragStarted"/> event.
        /// </summary>
        public static readonly RoutedEvent<VectorEventArgs> DragStartedEvent =
            RoutedEvent.Register<CompareSlider, VectorEventArgs>(nameof(DragStarted), RoutingStrategies.Bubble);

        /// <summary>
        /// Identifies the <see cref="DragDelta"/> event.
        /// </summary>
        public static readonly RoutedEvent<VectorEventArgs> DragDeltaEvent =
            RoutedEvent.Register<CompareSlider, VectorEventArgs>(nameof(DragDelta), RoutingStrategies.Bubble);

        /// <summary>
        /// Identifies the <see cref="DragCompleted"/> event.
        /// </summary>
        public static readonly RoutedEvent<VectorEventArgs> DragCompletedEvent =
            RoutedEvent.Register<CompareSlider, VectorEventArgs>(nameof(DragCompleted), RoutingStrategies.Bubble);

        /// <summary>
        /// Gets or sets the content displayed on the "before" (left/top) side of the divider.
        /// </summary>
        public object? BeforeContent
        {
            get => GetValue(BeforeContentProperty);
            set => SetValue(BeforeContentProperty, value);
        }

        /// <summary>
        /// Gets or sets the data template used to display <see cref="BeforeContent"/>.
        /// </summary>
        public IDataTemplate? BeforeContentTemplate
        {
            get => GetValue(BeforeContentTemplateProperty);
            set => SetValue(BeforeContentTemplateProperty, value);
        }

        /// <summary>
        /// Gets or sets the content displayed on the "after" (right/bottom) side of the divider.
        /// </summary>
        public object? AfterContent
        {
            get => GetValue(AfterContentProperty);
            set => SetValue(AfterContentProperty, value);
        }

        /// <summary>
        /// Gets or sets the data template used to display <see cref="AfterContent"/>.
        /// </summary>
        public IDataTemplate? AfterContentTemplate
        {
            get => GetValue(AfterContentTemplateProperty);
            set => SetValue(AfterContentTemplateProperty, value);
        }

        /// <summary>
        /// Gets or sets the axis along which the divider moves.
        /// Defaults to <see cref="Avalonia.Layout.Orientation.Horizontal"/>.
        /// </summary>
        public Orientation Orientation
        {
            get => GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the direction of increasing <see cref="RangeBase.Value"/>
        /// is reversed relative to the visual position of the divider.
        /// </summary>
        public bool IsDirectionReversed
        {
            get => GetValue(IsDirectionReversedProperty);
            set => SetValue(IsDirectionReversedProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether clicking anywhere on the control
        /// moves the divider to that point. Defaults to <see langword="true"/>.
        /// </summary>
        public bool IsMoveToPointEnabled
        {
            get => GetValue(IsMoveToPointEnabledProperty);
            set => SetValue(IsMoveToPointEnabledProperty, value);
        }

        private bool _isDragging;

        /// <summary>
        /// Gets a value indicating whether the thumb is currently being dragged.
        /// </summary>
        public bool IsDragging
        {
            get => _isDragging;
            private set
            {
                if (SetAndRaise(IsDraggingProperty, ref _isDragging, value))
                {
                    PseudoClasses.Set(":dragging", value);
                }
            }
        }

        /// <summary>
        /// Occurs when the thumb drag begins.
        /// </summary>
        public event EventHandler<VectorEventArgs> DragStarted
        {
            add => AddHandler(DragStartedEvent, value);
            remove => RemoveHandler(DragStartedEvent, value);
        }

        /// <summary>
        /// Occurs each time the thumb moves during a drag operation.
        /// </summary>
        public event EventHandler<VectorEventArgs> DragDelta
        {
            add => AddHandler(DragDeltaEvent, value);
            remove => RemoveHandler(DragDeltaEvent, value);
        }

        /// <summary>
        /// Occurs when the thumb drag is completed.
        /// </summary>
        public event EventHandler<VectorEventArgs> DragCompleted
        {
            add => AddHandler(DragCompletedEvent, value);
            remove => RemoveHandler(DragCompletedEvent, value);
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new CompareSliderAutomationPeer(this);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            if (_thumb != null)
            {
                _thumb.DragStarted -= OnThumbDragStarted;
                _thumb.DragDelta -= OnThumbDragDelta;
                _thumb.DragCompleted -= OnThumbDragCompleted;
                _thumb.PropertyChanged -= OnThumbPropertyChanged;
            }

            _beforePanel = e.NameScope.Find<Panel>("PART_BeforePanel");
            _afterPanel = e.NameScope.Find<Panel>("PART_AfterPanel");
            _divider = e.NameScope.Find<Line>("PART_Divider");
            _thumb = e.NameScope.Find<Thumb>("PART_Thumb");

            if (_thumb != null)
            {
                _thumb.DragStarted += OnThumbDragStarted;
                _thumb.DragDelta += OnThumbDragDelta;
                _thumb.DragCompleted += OnThumbDragCompleted;
                _thumb.PropertyChanged += OnThumbPropertyChanged;
            }

            if (_beforePanel != null) _beforePanel.Clip = _beforeClip;
            if (_afterPanel != null) _afterPanel.Clip = _afterClip;

            UpdateClipping();
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);

            _animationCts?.Cancel();
            _animationCts?.Dispose();
            _animationCts = null;

            if (_thumb != null)
            {
                _thumb.DragStarted -= OnThumbDragStarted;
                _thumb.DragDelta -= OnThumbDragDelta;
                _thumb.DragCompleted -= OnThumbDragCompleted;
                _thumb.PropertyChanged -= OnThumbPropertyChanged;
            }
        }

        private void OnThumbPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == BoundsProperty)
            {
                UpdateClipping();
            }
        }

        private void OnThumbDragStarted(object? sender, VectorEventArgs e)
        {
            IsDragging = true;
            RaiseEvent(new VectorEventArgs { RoutedEvent = DragStartedEvent, Vector = e.Vector });
        }

        private void OnThumbDragDelta(object? sender, VectorEventArgs e)
        {
            var range = Maximum - Minimum;
            if (range <= 0) return;

            double changeRatio = 0;
            if (Orientation == Orientation.Horizontal)
            {
                changeRatio = Bounds.Width > 0 ? e.Vector.X / Bounds.Width : 0;
            }
            else
            {
                changeRatio = Bounds.Height > 0 ? e.Vector.Y / Bounds.Height : 0;
            }

            if (IsDirectionReversed)
            {
                changeRatio = -changeRatio;
            }

            Value = Math.Clamp(Value + (changeRatio * range), Minimum, Maximum);

            RaiseEvent(new VectorEventArgs { RoutedEvent = DragDeltaEvent, Vector = e.Vector });
        }

        private void OnThumbDragCompleted(object? sender, VectorEventArgs e)
        {
            IsDragging = false;
            RaiseEvent(new VectorEventArgs { RoutedEvent = DragCompletedEvent, Vector = e.Vector });
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);

            if (!IsMoveToPointEnabled || e.Handled || _thumb?.IsPointerOver == true) return;

            var range = Maximum - Minimum;
            if (range <= 0) return;

            var point = e.GetPosition(this);
            var targetRatio = Orientation == Orientation.Horizontal
                ? (Bounds.Width > 0 ? point.X / Bounds.Width : 0)
                : (Bounds.Height > 0 ? point.Y / Bounds.Height : 0);

            if (IsDirectionReversed)
            {
                targetRatio = 1.0 - targetRatio;
            }

            Value = Math.Clamp(Minimum + (targetRatio * range), Minimum, Maximum);
            e.Handled = true;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Handled) return;

            // For vertical orientation, Up/Down/PageUp/PageDown map to visual movement
            // (Up = divider moves up = decrease, Down = divider moves down = increase).
            // Left/Right always map to decrease/increase regardless of orientation.
            var isVertical = Orientation == Orientation.Vertical;
            double change = 0;

            switch (e.Key)
            {
                case Key.Left:
                    change = -SmallChange;
                    break;
                case Key.Right:
                    change = SmallChange;
                    break;
                case Key.Up:
                    change = isVertical ? -SmallChange : SmallChange;
                    break;
                case Key.Down:
                    change = isVertical ? SmallChange : -SmallChange;
                    break;
                case Key.PageUp:
                    change = isVertical ? -LargeChange : LargeChange;
                    break;
                case Key.PageDown:
                    change = isVertical ? LargeChange : -LargeChange;
                    break;
                case Key.Home:
                    Value = Minimum;
                    e.Handled = true;
                    return;
                case Key.End:
                    Value = Maximum;
                    e.Handled = true;
                    return;
            }

            if (change != 0)
            {
                if (IsDirectionReversed) change = -change;
                Value = Math.Clamp(Value + change, Minimum, Maximum);
                e.Handled = true;
            }
        }

        private void UpdateClipping()
        {
            if (_beforePanel == null || _afterPanel == null || _divider == null || _thumb == null) return;
            if (Bounds.Width <= 0 || Bounds.Height <= 0) return;

            var range = Maximum - Minimum;
            var ratio = range > 0 ? (Value - Minimum) / range : 0;

            if (IsDirectionReversed) ratio = 1.0 - ratio;

            ratio = Math.Clamp(ratio, 0.0, 1.0);

            if (Orientation == Orientation.Horizontal)
            {
                var position = ratio * Bounds.Width;

                _beforeClip.Rect = new Rect(0, 0, position, Bounds.Height);
                _afterClip.Rect = new Rect(position, 0, Bounds.Width - position, Bounds.Height);

                Canvas.SetLeft(_divider, position);
                Canvas.SetLeft(_thumb, position - (_thumb.Bounds.Width / 2));

                Canvas.SetTop(_divider, 0);
                Canvas.SetTop(_thumb, (Bounds.Height / 2) - (_thumb.Bounds.Height / 2));

                _divider.StartPoint = new Point(0, 0);
                _divider.EndPoint = new Point(0, Bounds.Height);

                _beforePanel.InvalidateVisual();
                _afterPanel.InvalidateVisual();
            }
            else
            {
                var position = ratio * Bounds.Height;

                _beforeClip.Rect = new Rect(0, 0, Bounds.Width, position);
                _afterClip.Rect = new Rect(0, position, Bounds.Width, Bounds.Height - position);

                Canvas.SetTop(_divider, position);
                Canvas.SetTop(_thumb, position - (_thumb.Bounds.Height / 2));

                Canvas.SetLeft(_divider, 0);
                Canvas.SetLeft(_thumb, (Bounds.Width / 2) - (_thumb.Bounds.Width / 2));

                _divider.StartPoint = new Point(0, 0);
                _divider.EndPoint = new Point(Bounds.Width, 0);

                _beforePanel.InvalidateVisual();
                _afterPanel.InvalidateVisual();
            }
        }

        /// <summary>
        /// Animates the divider to the specified <paramref name="value"/> position.
        /// Any in-progress animation is cancelled before the new one starts.
        /// </summary>
        /// <param name="value">Target value, clamped to [<see cref="RangeBase.Minimum"/>, <see cref="RangeBase.Maximum"/>].</param>
        /// <param name="duration">Animation duration. Defaults to 300 ms.</param>
        /// <param name="cancellationToken">Token to cancel the animation externally.</param>
        public async Task AnimateTo(double value, TimeSpan? duration = null, CancellationToken cancellationToken = default)
        {
            _animationCts?.Cancel();
            _animationCts?.Dispose();
            _animationCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var token = _animationCts.Token;

            try
            {
                var end = Math.Clamp(value, Minimum, Maximum);
                var time = duration ?? TimeSpan.FromMilliseconds(300);

                var animation = new Animation
                {
                    Duration = time,
                    FillMode = FillMode.Forward,
                    Easing = new CubicEaseOut(),
                    Children =
                    {
                        new KeyFrame
                        {
                            Cue = new Cue(1d),
                            Setters = { new Setter { Property = ValueProperty, Value = end } }
                        }
                    }
                };

                await animation.RunAsync(this, token);

                if (!token.IsCancellationRequested)
                {
                    Value = end;
                    _animationCts?.Dispose();
                    _animationCts = null;
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        /// <summary>
        /// Moves the divider to the center of the value range.
        /// </summary>
        /// <param name="animate">When <see langword="true"/> (default), animates the movement.</param>
        /// <returns>
        /// A <see cref="Task"/> that completes when the divider reaches the center.
        /// Returns a completed task when <paramref name="animate"/> is <see langword="false"/>.
        /// </returns>
        public Task Reset(bool animate = true)
        {
            var targetValue = Minimum + (Maximum - Minimum) * 0.5;

            if (animate)
                return AnimateTo(targetValue);

            Value = targetValue;
            return Task.CompletedTask;
        }
    }
}
