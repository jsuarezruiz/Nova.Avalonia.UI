using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.Automation.Peers;
using Nova.Avalonia.UI.Controls.AutomationPeers;
using System.Threading;

namespace Nova.Avalonia.UI.Controls
{
    /// <summary>
    /// A control that allows comparing two pieces of content side-by-side with a draggable slider.
    /// </summary>
    [TemplatePart("PART_Container", typeof(Grid))]
    [TemplatePart("PART_BeforePanel", typeof(Panel))]
    [TemplatePart("PART_BeforePresenter", typeof(ContentPresenter))]
    [TemplatePart("PART_AfterPanel", typeof(Panel))]
    [TemplatePart("PART_AfterPresenter", typeof(ContentPresenter))]
    [TemplatePart("PART_Track", typeof(Canvas))]
    [TemplatePart("PART_Divider", typeof(Line))]
    [TemplatePart("PART_Thumb", typeof(Thumb))]
    [PseudoClasses(":dragging")]
    public class CompareSlider : RangeBase
    {
        private Grid? _container;
        private Panel? _beforePanel;
        private Panel? _afterPanel;
        private Canvas? _track;
        private Line? _divider;
        private Thumb? _thumb;
        private CancellationTokenSource? _animationCts;

        static CompareSlider()
        {
            ValueProperty.Changed.AddClassHandler<CompareSlider>((x, e) => x.UpdateClipping());
            BoundsProperty.Changed.AddClassHandler<CompareSlider>((x, e) => x.UpdateClipping());
            OrientationProperty.Changed.AddClassHandler<CompareSlider>((x, e) => x.UpdateClipping());
        }

        public CompareSlider()
        {
            Value = 0.5;
            Minimum = 0.0;
            Maximum = 1.0;
            SmallChange = 0.01;
            LargeChange = 0.1;
        }

        /// <summary>
        /// Defines the <see cref="BeforeContent"/> property.
        /// </summary>
        public static readonly StyledProperty<object?> BeforeContentProperty =
            AvaloniaProperty.Register<CompareSlider, object?>(nameof(BeforeContent));

        /// <summary>
        /// Gets or sets the content to display before the slider (left or top).
        /// </summary>
        public object? BeforeContent
        {
            get => GetValue(BeforeContentProperty);
            set => SetValue(BeforeContentProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="BeforeContentTemplate"/> property.
        /// </summary>
        public static readonly StyledProperty<IDataTemplate?> BeforeContentTemplateProperty =
            AvaloniaProperty.Register<CompareSlider, IDataTemplate?>(nameof(BeforeContentTemplate));

        /// <summary>
        /// Gets or sets the data template used to display the <see cref="BeforeContent"/>.
        /// </summary>
        public IDataTemplate? BeforeContentTemplate
        {
            get => GetValue(BeforeContentTemplateProperty);
            set => SetValue(BeforeContentTemplateProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="AfterContent"/> property.
        /// </summary>
        public static readonly StyledProperty<object?> AfterContentProperty =
            AvaloniaProperty.Register<CompareSlider, object?>(nameof(AfterContent));

        /// <summary>
        /// Gets or sets the content to display after the slider (right or bottom).
        /// </summary>
        public object? AfterContent
        {
            get => GetValue(AfterContentProperty);
            set => SetValue(AfterContentProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="AfterContentTemplate"/> property.
        /// </summary>
        public static readonly StyledProperty<IDataTemplate?> AfterContentTemplateProperty =
            AvaloniaProperty.Register<CompareSlider, IDataTemplate?>(nameof(AfterContentTemplate));

        /// <summary>
        /// Gets or sets the data template used to display the <see cref="AfterContent"/>.
        /// </summary>
        public IDataTemplate? AfterContentTemplate
        {
            get => GetValue(AfterContentTemplateProperty);
            set => SetValue(AfterContentTemplateProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="Orientation"/> property.
        /// </summary>
        public static readonly StyledProperty<Orientation> OrientationProperty =
            AvaloniaProperty.Register<CompareSlider, Orientation>(nameof(Orientation), Orientation.Horizontal);

        /// <summary>
        /// Gets or sets the orientation of the slider.
        /// </summary>
        public Orientation Orientation
        {
            get => GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="IsDirectionReversed"/> property.
        /// </summary>
        public static readonly StyledProperty<bool> IsDirectionReversedProperty =
            AvaloniaProperty.Register<CompareSlider, bool>(nameof(IsDirectionReversed));

        /// <summary>
        /// Gets or sets a value indicating whether the direction of increasing value is reversed.
        /// </summary>
        public bool IsDirectionReversed
        {
            get => GetValue(IsDirectionReversedProperty);
            set => SetValue(IsDirectionReversedProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="IsMoveToPointEnabled"/> property.
        /// </summary>
        public static readonly StyledProperty<bool> IsMoveToPointEnabledProperty =
            AvaloniaProperty.Register<CompareSlider, bool>(nameof(IsMoveToPointEnabled), true);

        /// <summary>
        /// Gets or sets a value indicating whether the thumb moves to the location of the pointer click.
        /// </summary>
        public bool IsMoveToPointEnabled
        {
            get => GetValue(IsMoveToPointEnabledProperty);
            set => SetValue(IsMoveToPointEnabledProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="DragStarted"/> event.
        /// </summary>
        public static readonly RoutedEvent<VectorEventArgs> DragStartedEvent =
            RoutedEvent.Register<CompareSlider, VectorEventArgs>(nameof(DragStarted), RoutingStrategies.Bubble);

        /// <summary>
        /// Occurs when the user starts dragging the slider thumb.
        /// </summary>
        public event EventHandler<VectorEventArgs> DragStarted
        {
            add => AddHandler(DragStartedEvent, value);
            remove => RemoveHandler(DragStartedEvent, value);
        }

        /// <summary>
        /// Defines the <see cref="DragDelta"/> event.
        /// </summary>
        public static readonly RoutedEvent<VectorEventArgs> DragDeltaEvent =
            RoutedEvent.Register<CompareSlider, VectorEventArgs>(nameof(DragDelta), RoutingStrategies.Bubble);

        /// <summary>
        /// Occurs when the user drags the slider thumb.
        /// </summary>
        public event EventHandler<VectorEventArgs> DragDelta
        {
            add => AddHandler(DragDeltaEvent, value);
            remove => RemoveHandler(DragDeltaEvent, value);
        }

        /// <summary>
        /// Defines the <see cref="DragCompleted"/> event.
        /// </summary>
        public static readonly RoutedEvent<VectorEventArgs> DragCompletedEvent =
            RoutedEvent.Register<CompareSlider, VectorEventArgs>(nameof(DragCompleted), RoutingStrategies.Bubble);

        /// <summary>
        /// Occurs when the user stops dragging the slider thumb.
        /// </summary>
        public event EventHandler<VectorEventArgs> DragCompleted
        {
            add => AddHandler(DragCompletedEvent, value);
            remove => RemoveHandler(DragCompletedEvent, value);
        }
        
        // Define Direct Property for IsDragging to be ReadOnly
        private bool _isDragging;

        /// <summary>
        /// Defines the <see cref="IsDragging"/> property.
        /// </summary>
        public static readonly DirectProperty<CompareSlider, bool> IsDraggingProperty =
            AvaloniaProperty.RegisterDirect<CompareSlider, bool>(nameof(IsDragging), o => o.IsDragging);

        /// <summary>
        /// Gets a value indicating whether the slider thumb is currently being dragged.
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

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new CompareSliderAutomationPeer(this);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            // Unsubscribe from previous thumb if exists
            if (_thumb != null)
            {
                _thumb.DragStarted -= OnThumbDragStarted;
                _thumb.DragDelta -= OnThumbDragDelta;
                _thumb.DragCompleted -= OnThumbDragCompleted;
            }

            _container = e.NameScope.Find<Grid>("PART_Container");
            _beforePanel = e.NameScope.Find<Panel>("PART_BeforePanel");
            _afterPanel = e.NameScope.Find<Panel>("PART_AfterPanel");
            _track = e.NameScope.Find<Canvas>("PART_Track");
            _divider = e.NameScope.Find<Line>("PART_Divider");
            _thumb = e.NameScope.Find<Thumb>("PART_Thumb");

            if (_thumb != null)
            {
                _thumb.DragStarted += OnThumbDragStarted;
                _thumb.DragDelta += OnThumbDragDelta;
                _thumb.DragCompleted += OnThumbDragCompleted;
            }

            UpdateClipping();
        }

        /// <inheritdoc />
        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);

            // Cancel any running animation
            _animationCts?.Cancel();
            _animationCts?.Dispose();
            _animationCts = null;

            // Unsubscribe from thumb events
            if (_thumb != null)
            {
                _thumb.DragStarted -= OnThumbDragStarted;
                _thumb.DragDelta -= OnThumbDragDelta;
                _thumb.DragCompleted -= OnThumbDragCompleted;
            }
        }

        private void OnThumbDragStarted(object? sender, VectorEventArgs e)
        {
            IsDragging = true;
            RaiseEvent(new VectorEventArgs { RoutedEvent = DragStartedEvent, Vector = e.Vector });
        }

        private void OnThumbDragDelta(object? sender, VectorEventArgs e)
        {
            if (_thumb == null) return;

            double change = 0;
            if (Orientation == Orientation.Horizontal)
            {
                change = e.Vector.X / Bounds.Width;
            }
            else
            {
                change = e.Vector.Y / Bounds.Height;
            }
            
            if (IsDirectionReversed)
            {
                 change = -change;
            }

            Value = Math.Clamp(Value + change, Minimum, Maximum);
            
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

            var point = e.GetPosition(this);
            var targetValue = Orientation == Orientation.Horizontal
                ? point.X / Bounds.Width
                : point.Y / Bounds.Height;

            if (IsDirectionReversed)
            {
                targetValue = 1.0 - targetValue;
            }

            Value = Math.Clamp(targetValue, Minimum, Maximum);
            e.Handled = true;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            
            if (e.Handled) return;

            double change = 0;
            // Adjust keys based on orientation if needed, but standard logic usually:
            // Right/Up = Increase
            // Left/Down = Decrease
            // But spec says: Left/Down Decrease, Right/Up Increase.
            
            switch (e.Key)
            {
                case Key.Left:
                case Key.Down:
                    change = -SmallChange;
                    break;
                case Key.Right:
                case Key.Up:
                    change = SmallChange;
                    break;
                case Key.PageDown:
                    change = -LargeChange;
                    break;
                case Key.PageUp:
                    change = LargeChange;
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

            var val = Value;
            if (IsDirectionReversed) val = 1.0 - val;
            
            // Ensure visual consistency for 0 and 1
            val = Math.Clamp(val, 0.0, 1.0);

            if (Orientation == Orientation.Horizontal)
            {
                var position = val * Bounds.Width;

                _beforePanel.Clip = new RectangleGeometry(new Rect(0, 0, position, Bounds.Height));
                _afterPanel.Clip = new RectangleGeometry(new Rect(position, 0, Bounds.Width - position, Bounds.Height));

                Canvas.SetLeft(_divider, position);
                // Center thumb on position
                Canvas.SetLeft(_thumb, position - (_thumb.Bounds.Width / 2));
                
                // Reset Vertical properties
                Canvas.SetTop(_divider, 0);
                Canvas.SetTop(_thumb, (Bounds.Height / 2) - (_thumb.Bounds.Height / 2)); 
                
                // Adjust divider line
                _divider.StartPoint = new Point(0, 0);
                _divider.EndPoint = new Point(0, Bounds.Height);
            }
            else
            {
                var position = val * Bounds.Height;

                _beforePanel.Clip = new RectangleGeometry(new Rect(0, 0, Bounds.Width, position));
                _afterPanel.Clip = new RectangleGeometry(new Rect(0, position, Bounds.Width, Bounds.Height - position));

                Canvas.SetTop(_divider, position);
                Canvas.SetTop(_thumb, position - (_thumb.Bounds.Height / 2));
                
                 // Reset Horizontal properties
                Canvas.SetLeft(_divider, 0);
                Canvas.SetLeft(_thumb, (Bounds.Width / 2) - (_thumb.Bounds.Width / 2));

                // Adjust divider line
                _divider.StartPoint = new Point(0, 0);
                _divider.EndPoint = new Point(Bounds.Width, 0);
            }
        }
        
        /// <summary>
        /// Animates the slider value to the specified target position.
        /// </summary>
        /// <param name="value">The target value between Minimum and Maximum.</param>
        /// <param name="duration">Optional duration for the animation. Default is 300ms.</param>
        /// <param name="cancellationToken">Optional cancellation token to cancel the animation.</param>
        /// <returns>A task representing the animation operation.</returns>
        public async Task AnimateTo(double value, TimeSpan? duration = null, CancellationToken cancellationToken = default)
        {
            // Cancel any previous animation
            _animationCts?.Cancel();
            _animationCts?.Dispose();
            _animationCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var token = _animationCts.Token;

            try
            {
                var start = Value;
                var end = Math.Clamp(value, Minimum, Maximum);
                var time = duration ?? TimeSpan.FromMilliseconds(300);
                var refreshRate = TimeSpan.FromMilliseconds(16);
                var totalSteps = time.TotalMilliseconds / refreshRate.TotalMilliseconds;
                var stepSize = (end - start) / totalSteps;

                for (int i = 0; i < totalSteps; i++)
                {
                    token.ThrowIfCancellationRequested();
                    Value += stepSize;
                    await Task.Delay(refreshRate, token);
                }
                Value = end;
            }
            catch (OperationCanceledException)
            {
                // Animation was cancelled, which is expected behavior
            }
        }

        /// <summary>
        /// Resets the slider value to the center (0.5).
        /// </summary>
        /// <param name="animate">Whether to animate the change.</param>
        public void Reset(bool animate = true)
        {
            if (animate)
            {
                _ = AnimateTo(0.5);
            }
            else
            {
                Value = 0.5;
            }
        }
    }
}
