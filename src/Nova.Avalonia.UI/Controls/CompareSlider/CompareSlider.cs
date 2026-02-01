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
        private readonly RectangleGeometry _beforeClip = new();
        private readonly RectangleGeometry _afterClip = new();

        static CompareSlider()
        {
            ValueProperty.Changed.AddClassHandler<CompareSlider>((x, e) => x.UpdateClipping());
            BoundsProperty.Changed.AddClassHandler<CompareSlider>((x, e) => x.UpdateClipping());
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

        public static readonly StyledProperty<object?> BeforeContentProperty =
            AvaloniaProperty.Register<CompareSlider, object?>(nameof(BeforeContent));

        public static readonly StyledProperty<IDataTemplate?> BeforeContentTemplateProperty =
            AvaloniaProperty.Register<CompareSlider, IDataTemplate?>(nameof(BeforeContentTemplate));

        public static readonly StyledProperty<object?> AfterContentProperty =
            AvaloniaProperty.Register<CompareSlider, object?>(nameof(AfterContent));

        public static readonly StyledProperty<IDataTemplate?> AfterContentTemplateProperty =
            AvaloniaProperty.Register<CompareSlider, IDataTemplate?>(nameof(AfterContentTemplate));

        public static readonly StyledProperty<Orientation> OrientationProperty =
            AvaloniaProperty.Register<CompareSlider, Orientation>(nameof(Orientation), Orientation.Horizontal);

        public static readonly StyledProperty<bool> IsDirectionReversedProperty =
            AvaloniaProperty.Register<CompareSlider, bool>(nameof(IsDirectionReversed));

        public static readonly StyledProperty<bool> IsMoveToPointEnabledProperty =
            AvaloniaProperty.Register<CompareSlider, bool>(nameof(IsMoveToPointEnabled), true);

        public static readonly DirectProperty<CompareSlider, bool> IsDraggingProperty =
            AvaloniaProperty.RegisterDirect<CompareSlider, bool>(nameof(IsDragging), o => o.IsDragging);

        public static readonly RoutedEvent<VectorEventArgs> DragStartedEvent =
            RoutedEvent.Register<CompareSlider, VectorEventArgs>(nameof(DragStarted), RoutingStrategies.Bubble);

        public static readonly RoutedEvent<VectorEventArgs> DragDeltaEvent =
            RoutedEvent.Register<CompareSlider, VectorEventArgs>(nameof(DragDelta), RoutingStrategies.Bubble);

        public static readonly RoutedEvent<VectorEventArgs> DragCompletedEvent =
            RoutedEvent.Register<CompareSlider, VectorEventArgs>(nameof(DragCompleted), RoutingStrategies.Bubble);

        public object? BeforeContent
        {
            get => GetValue(BeforeContentProperty);
            set => SetValue(BeforeContentProperty, value);
        }

        public IDataTemplate? BeforeContentTemplate
        {
            get => GetValue(BeforeContentTemplateProperty);
            set => SetValue(BeforeContentTemplateProperty, value);
        }

        public object? AfterContent
        {
            get => GetValue(AfterContentProperty);
            set => SetValue(AfterContentProperty, value);
        }

        public IDataTemplate? AfterContentTemplate
        {
            get => GetValue(AfterContentTemplateProperty);
            set => SetValue(AfterContentTemplateProperty, value);
        }

        public Orientation Orientation
        {
            get => GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        public bool IsDirectionReversed
        {
            get => GetValue(IsDirectionReversedProperty);
            set => SetValue(IsDirectionReversedProperty, value);
        }

        public bool IsMoveToPointEnabled
        {
            get => GetValue(IsMoveToPointEnabledProperty);
            set => SetValue(IsMoveToPointEnabledProperty, value);
        }

        private bool _isDragging;

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

        public event EventHandler<VectorEventArgs> DragStarted
        {
            add => AddHandler(DragStartedEvent, value);
            remove => RemoveHandler(DragStartedEvent, value);
        }

        public event EventHandler<VectorEventArgs> DragDelta
        {
            add => AddHandler(DragDeltaEvent, value);
            remove => RemoveHandler(DragDeltaEvent, value);
        }

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
            
            val = Math.Clamp(val, 0.0, 1.0);

            if (Orientation == Orientation.Horizontal)
            {
                var position = val * Bounds.Width;

                _beforeClip.Rect = new Rect(0, 0, position, Bounds.Height);
                _afterClip.Rect = new Rect(position, 0, Bounds.Width - position, Bounds.Height);

                Canvas.SetLeft(_divider, position);
                Canvas.SetLeft(_thumb, position - (_thumb.Bounds.Width / 2));
                
                Canvas.SetTop(_divider, 0);
                Canvas.SetTop(_thumb, (Bounds.Height / 2) - (_thumb.Bounds.Height / 2)); 
                
                _divider.StartPoint = new Point(0, 0);
                _divider.EndPoint = new Point(0, Bounds.Height);
            }
            else
            {
                var position = val * Bounds.Height;

                _beforeClip.Rect = new Rect(0, 0, Bounds.Width, position);
                _afterClip.Rect = new Rect(0, position, Bounds.Width, Bounds.Height - position);

                Canvas.SetTop(_divider, position);
                Canvas.SetTop(_thumb, position - (_thumb.Bounds.Height / 2));
                
                Canvas.SetLeft(_divider, 0);
                Canvas.SetLeft(_thumb, (Bounds.Width / 2) - (_thumb.Bounds.Width / 2));

                _divider.StartPoint = new Point(0, 0);
                _divider.EndPoint = new Point(Bounds.Width, 0);
            }

            // Force clip update by re-assigning (some Avalonia versions need this for Geometry mutations)
            _beforePanel.Clip = null;
            _beforePanel.Clip = _beforeClip;
            _afterPanel.Clip = null;
            _afterPanel.Clip = _afterClip;

            _beforePanel.InvalidateVisual();
            _afterPanel.InvalidateVisual();
        }
        
        /// <summary>
        /// Animates the slider value to the specified target position.
        /// </summary>
        public async Task AnimateTo(double value, TimeSpan? duration = null, CancellationToken cancellationToken = default)
        {
            _animationCts?.Cancel();
            _animationCts?.Dispose();
            _animationCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var token = _animationCts.Token;

            try
            {
                var start = Value;
                var end = Math.Clamp(value, Minimum, Maximum);
                var time = duration ?? TimeSpan.FromMilliseconds(300);
                var startTime = DateTime.UtcNow;
                
                while (true)
                {
                    token.ThrowIfCancellationRequested();
                    
                    var elapsed = DateTime.UtcNow - startTime;
                    var progress = Math.Min(1.0, elapsed.TotalMilliseconds / time.TotalMilliseconds);
                    
                    // EaseOutCubic
                    var factor = 1.0 - Math.Pow(1.0 - progress, 3);
                    
                    Value = start + (end - start) * factor;

                    if (progress >= 1.0) break;
                    
                    await Task.Delay(16, token);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        /// <summary>
        /// Resets the slider value to the center (0.5).
        /// </summary>
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
