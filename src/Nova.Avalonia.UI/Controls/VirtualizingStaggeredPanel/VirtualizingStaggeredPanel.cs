using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.VisualTree;

namespace Nova.Avalonia.UI.Controls
{
    /// <summary>
    /// A virtualizing panel that arranges children in a staggered grid (masonry) layout.
    /// Items are placed in the shortest column, creating an efficient Pinterest-style layout.
    /// Only items within the visible viewport (plus buffer) are materialized for optimal performance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This panel virtualizes items by only creating UI containers for visible items.
    /// Off-screen items are recycled and reused when scrolling, minimizing memory usage
    /// and improving scroll performance for large datasets.
    /// </para>
    /// <para>
    /// The panel automatically calculates the number of columns based on the available
    /// width and the <see cref="DesiredColumnWidth"/> property.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// &lt;ItemsControl ItemsSource="{Binding Items}"&gt;
    ///     &lt;ItemsControl.ItemsPanel&gt;
    ///         &lt;ItemsPanelTemplate&gt;
    ///             &lt;controls:VirtualizingStaggeredPanel DesiredColumnWidth="200" ColumnSpacing="8" RowSpacing="8"/&gt;
    ///         &lt;/ItemsPanelTemplate&gt;
    ///     &lt;/ItemsControl.ItemsPanel&gt;
    /// &lt;/ItemsControl&gt;
    /// </code>
    /// </example>
    public class VirtualizingStaggeredPanel : VirtualizingPanel
    {
        private static readonly AttachedProperty<object?> RecycleKeyProperty =
            AvaloniaProperty.RegisterAttached<VirtualizingStaggeredPanel, Control, object?>("RecycleKey");

        private static readonly object s_itemIsItsOwnContainer = new();

        /// <summary>
        /// Defines the <see cref="DesiredColumnWidth"/> property.
        /// </summary>
        public static readonly StyledProperty<double> DesiredColumnWidthProperty =
            AvaloniaProperty.Register<VirtualizingStaggeredPanel, double>(nameof(DesiredColumnWidth), 250);

        /// <summary>
        /// Defines the <see cref="ColumnSpacing"/> property.
        /// </summary>
        public static readonly StyledProperty<double> ColumnSpacingProperty =
            AvaloniaProperty.Register<VirtualizingStaggeredPanel, double>(nameof(ColumnSpacing), 0);

        /// <summary>
        /// Defines the <see cref="RowSpacing"/> property.
        /// </summary>
        public static readonly StyledProperty<double> RowSpacingProperty =
            AvaloniaProperty.Register<VirtualizingStaggeredPanel, double>(nameof(RowSpacing), 0);

        // Pre-allocated delegate to avoid closure allocations
        private readonly Action<Control, int> _recycleElement;

        // Container tracking, Dictionary supports sparse visibility patterns in staggered layouts
        private readonly Dictionary<int, Control> _indexToContainer = new();
        private readonly Dictionary<Control, int> _containerToIndex = new();

        // Recycle pool, hidden containers ready for reuse (avoids Remove/Add visual tree operations)
        private Dictionary<object, Stack<Control>>? _recyclePool;

        // Scroll anchor support
        private IScrollAnchorProvider? _scrollAnchorProvider;

        // Layout state, reused across measure passes
        private Rect[] _itemBoundsCache = Array.Empty<Rect>();
        private int _itemBoundsCacheCount;
        private double[] _columnNextY = Array.Empty<double>();
        private Rect _viewport;
        private double _lastEstimatedItemHeight = 100;
        private double _lastMeasureWidth = -1;
        private double _lastMaxHeight = -1;
        private bool _isInLayout;

        // Reusable collections to avoid allocations in hot path
        private readonly HashSet<int> _neededIndices = new();
        private readonly List<int> _toRecycle = new();
        private const int MaxPoolSize = 20;

        /// <summary>
        /// Gets or sets the desired width of each column.
        /// The actual column width may vary slightly to fill the available space evenly.
        /// </summary>
        /// <value>The desired column width in device-independent pixels. Default is 250.</value>
        public double DesiredColumnWidth
        {
            get => GetValue(DesiredColumnWidthProperty);
            set => SetValue(DesiredColumnWidthProperty, value);
        }

        /// <summary>
        /// Gets or sets the horizontal spacing between columns.
        /// </summary>
        /// <value>The horizontal spacing in device-independent pixels. Default is 0.</value>
        public double ColumnSpacing
        {
            get => GetValue(ColumnSpacingProperty);
            set => SetValue(ColumnSpacingProperty, value);
        }

        /// <summary>
        /// Gets or sets the vertical spacing between items in the same column.
        /// </summary>
        /// <value>The vertical spacing in device-independent pixels. Default is 0.</value>
        public double RowSpacing
        {
            get => GetValue(RowSpacingProperty);
            set => SetValue(RowSpacingProperty, value);
        }

        static VirtualizingStaggeredPanel()
        {
            AffectsMeasure<VirtualizingStaggeredPanel>(DesiredColumnWidthProperty, ColumnSpacingProperty, RowSpacingProperty);
            AffectsArrange<VirtualizingStaggeredPanel>(ColumnSpacingProperty, RowSpacingProperty);
        }

        public VirtualizingStaggeredPanel()
        {
            _recycleElement = RecycleElement;
            EffectiveViewportChanged += OnEffectiveViewportChanged;
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            _scrollAnchorProvider = this.FindAncestorOfType<IScrollAnchorProvider>();
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            _scrollAnchorProvider = null;
        }

        private void OnEffectiveViewportChanged(object? sender, EffectiveViewportChangedEventArgs e)
        {
            var effectiveViewport = e.EffectiveViewport;
            
            // Always store the viewport, even if it seems invalid
            // The effective viewport can have negative coordinates when scrolled
            _viewport = effectiveViewport;
            
            // Always invalidate, let MeasureOverride handle edge cases
            InvalidateMeasure();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == DesiredColumnWidthProperty ||
                change.Property == ColumnSpacingProperty ||
                change.Property == RowSpacingProperty)
            {
                _lastMeasureWidth = -1;
            }
        }

        protected override void OnItemsChanged(IReadOnlyList<object?> items, NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(items, e);

            bool isAppend = e.Action == NotifyCollectionChangedAction.Add && 
                            e.NewStartingIndex >= _itemBoundsCacheCount;

            if (e.Action == NotifyCollectionChangedAction.Reset ||
                e.Action == NotifyCollectionChangedAction.Remove ||
                e.Action == NotifyCollectionChangedAction.Replace ||
                e.Action == NotifyCollectionChangedAction.Move ||
                (e.Action == NotifyCollectionChangedAction.Add && !isAppend))
            {
                RecycleAllContainers();
                ClearBoundsCache();
            }

            _lastMeasureWidth = -1;
            InvalidateMeasure();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var items = Items;
            int itemCount = items.Count;

            // Non-virtualized mode: children added directly
            if (itemCount == 0 && Children.Count > 0)
            {
                return MeasureNonVirtualized(availableSize);
            }

            if (itemCount == 0)
            {
                RecycleAllContainers();
                return default;
            }

            var generator = ItemContainerGenerator;
            if (generator == null)
            {
                return MeasureNonVirtualized(availableSize);
            }

            _isInLayout = true;

            try
            {
                // Calculate column configuration
                double totalWidth = double.IsInfinity(availableSize.Width)
                    ? (_viewport.Width > 0 ? _viewport.Width : 800)
                    : availableSize.Width;

                double columnSpacing = ColumnSpacing;
                double rowSpacing = RowSpacing;
                int columnCount = CalculateColumnCount(totalWidth, DesiredColumnWidth, columnSpacing);
                double actualColumnWidth = CalculateActualColumnWidth(totalWidth, columnCount, columnSpacing);

                EnsureColumnArrays(columnCount);
                EnsureBoundsCache(itemCount);

                // Calculate extended viewport with generous buffer for smooth scrolling
                // Use Bounds height or fallback for initial estimation
                double viewportHeight = _viewport.Height > 0 ? _viewport.Height : 
                                       (Bounds.Height > 0 ? Bounds.Height : 600);
                double viewportY = _viewport.Height > 0 ? _viewport.Y : 0;
                
                // Use 2x viewport as buffer for smooth bidirectional scrolling
                double bufferSize = viewportHeight * 2;
                double viewportTop = Math.Max(0, viewportY - bufferSize);
                double viewportBottom = viewportY + viewportHeight + bufferSize;

                // Reset column heights using Span for efficient array access
                _columnNextY.AsSpan(0, columnCount).Clear();

                // Track which containers are still needed (reuse collection)
                _neededIndices.Clear();

                // Cache measure constraint outside loop to avoid allocations
                var measureConstraint = new Size(actualColumnWidth, double.PositiveInfinity);

                double maxHeight = 0;
                int totalMeasured = 0;
                double totalMeasuredHeight = 0;
                bool needsFullLayout = true;

                // Optimistically reuse cached bounds if width hasn't changed
                if (totalWidth == _lastMeasureWidth && _itemBoundsCacheCount == itemCount)
                {
                    needsFullLayout = false;
                    for (int i = 0; i < itemCount; i++)
                    {
                        var rect = _itemBoundsCache[i];
                        bool isVisible = rect.Bottom >= viewportTop && rect.Y <= viewportBottom;
                        
                        if (isVisible)
                        {
                            var container = GetOrCreateContainer(items, i);
                            if (container != null)
                            {
                                container.Measure(measureConstraint);
                                double itemHeight = container.DesiredSize.Height;
                                
                                // Re-measure if height changed
                                if (Math.Abs(itemHeight - rect.Height) > 0.1)
                                {
                                    needsFullLayout = true;
                                    break;
                                }

                                _neededIndices.Add(i);
                            }
                        }
                    }

                    if (!needsFullLayout)
                    {
                        maxHeight = _lastMaxHeight;
                    }
                }

                if (needsFullLayout)
                {
                    // Clear and prepare for full layout
                    _neededIndices.Clear();
                    _columnNextY.AsSpan(0, columnCount).Clear();

                    for (int i = 0; i < itemCount; i++)
                    {
                    int columnIndex = GetShortestColumn(columnCount);
                    double x = columnIndex * (actualColumnWidth + columnSpacing);
                    double y = _columnNextY[columnIndex];

                    // Use cached or estimated height
                    double estimatedHeight = _itemBoundsCache[i].Height > 0
                        ? _itemBoundsCache[i].Height
                        : _lastEstimatedItemHeight;

                    double bottom = y + estimatedHeight;
                    bool isVisible = bottom >= viewportTop && y <= viewportBottom;

                    double itemHeight;

                    if (isVisible)
                    {
                        _neededIndices.Add(i);
                        var container = GetOrCreateContainer(items, i);

                        if (container != null)
                        {
                            container.Measure(measureConstraint);
                            itemHeight = container.DesiredSize.Height;

                            // Update average
                            totalMeasured++;
                            totalMeasuredHeight += itemHeight;
                        }
                        else
                        {
                            itemHeight = estimatedHeight;
                        }
                    }
                    else
                    {
                        itemHeight = estimatedHeight;
                    }

                    _itemBoundsCache[i] = new Rect(x, y, actualColumnWidth, itemHeight);
                    _columnNextY[columnIndex] = y + itemHeight + rowSpacing;

                    double columnHeight = _columnNextY[columnIndex] - rowSpacing;
                    if (columnHeight > maxHeight)
                        maxHeight = columnHeight;
                }

                }

                // Recycle containers that are no longer visible
                RecycleUnneededContainers();

                // Update estimated height
                if (totalMeasured > 0)
                {
                    _lastEstimatedItemHeight = totalMeasuredHeight / totalMeasured;
                }

                _itemBoundsCacheCount = itemCount;
                _lastMeasureWidth = totalWidth;
                _lastMaxHeight = maxHeight;

                return new Size(totalWidth, maxHeight);
            }
            finally
            {
                _isInLayout = false;
            }
        }

        private Size MeasureNonVirtualized(Size availableSize)
        {
            double totalWidth = double.IsInfinity(availableSize.Width) ? 800 : availableSize.Width;
            double columnSpacing = ColumnSpacing;
            double rowSpacing = RowSpacing;
            int columnCount = CalculateColumnCount(totalWidth, DesiredColumnWidth, columnSpacing);
            double actualColumnWidth = CalculateActualColumnWidth(totalWidth, columnCount, columnSpacing);

            EnsureColumnArrays(columnCount);
            Array.Clear(_columnNextY, 0, columnCount);

            EnsureBoundsCache(Children.Count);
            _itemBoundsCacheCount = 0;

            int index = 0;
            foreach (var child in Children)
            {
                if (child is not Control control) continue;

                int columnIndex = GetShortestColumn(columnCount);
                double x = columnIndex * (actualColumnWidth + columnSpacing);
                double y = _columnNextY[columnIndex];

                control.Measure(new Size(actualColumnWidth, double.PositiveInfinity));
                double itemHeight = control.DesiredSize.Height;

                _itemBoundsCache[index] = new Rect(x, y, actualColumnWidth, itemHeight);
                _columnNextY[columnIndex] = y + itemHeight + rowSpacing;
                index++;
            }

            _itemBoundsCacheCount = index;

            double maxHeight = 0;
            for (int i = 0; i < columnCount; i++)
            {
                double colHeight = _columnNextY[i] - rowSpacing;
                if (colHeight > maxHeight)
                    maxHeight = colHeight;
            }

            return new Size(totalWidth, Math.Max(0, maxHeight));
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            _isInLayout = true;

            try
            {
                foreach (var kvp in _indexToContainer)
                {
                    int index = kvp.Key;
                    var container = kvp.Value;

                    if (index >= 0 && index < _itemBoundsCacheCount)
                    {
                        var rect = _itemBoundsCache[index];
                        container.Arrange(rect);

                        if (_viewport.Intersects(rect))
                        {
                            _scrollAnchorProvider?.RegisterAnchorCandidate(container);
                        }
                    }
                }

                // Non-virtualized mode fallback
                if (_indexToContainer.Count == 0 && _itemBoundsCacheCount > 0)
                {
                    int index = 0;
                    foreach (var child in Children)
                    {
                        if (child is Control control && index < _itemBoundsCacheCount)
                        {
                            control.Arrange(_itemBoundsCache[index]);
                            index++;
                        }
                    }
                }

                return finalSize;
            }
            finally
            {
                _isInLayout = false;
            }
        }

        private Control? GetOrCreateContainer(IReadOnlyList<object?> items, int index)
        {
            if (ItemContainerGenerator is not { } generator)
                return null;

            // Return existing container
            if (_indexToContainer.TryGetValue(index, out var existing))
            {
                existing.SetCurrentValue(Visual.IsVisibleProperty, true);
                return existing;
            }

            if (index < 0 || index >= items.Count)
                return null;

            var item = items[index];

            if (generator.NeedsContainer(item, index, out var recycleKey))
            {
                return GetRecycledContainer(item, index, recycleKey) ??
                       CreateContainer(item, index, recycleKey);
            }
            else
            {
                return GetItemAsOwnContainer(item, index);
            }
        }

        private Control GetItemAsOwnContainer(object? item, int index)
        {
            var generator = ItemContainerGenerator!;
            var controlItem = (Control)item!;

            if (!controlItem.IsSet(RecycleKeyProperty))
            {
                generator.PrepareItemContainer(controlItem, controlItem, index);
                AddInternalChild(controlItem);
                controlItem.SetValue(RecycleKeyProperty, s_itemIsItsOwnContainer);
                generator.ItemContainerPrepared(controlItem, item, index);
            }

            _indexToContainer[index] = controlItem;
            _containerToIndex[controlItem] = index;

            controlItem.SetCurrentValue(Visual.IsVisibleProperty, true);
            return controlItem;
        }

        private Control? GetRecycledContainer(object? item, int index, object? recycleKey)
        {
            if (recycleKey is null)
                return null;

            if (_recyclePool?.TryGetValue(recycleKey, out var pool) == true && pool.Count > 0)
            {
                var generator = ItemContainerGenerator!;
                var recycled = pool.Pop();

                _indexToContainer[index] = recycled;
                _containerToIndex[recycled] = index;

                recycled.SetCurrentValue(Visual.IsVisibleProperty, true);
                generator.PrepareItemContainer(recycled, item, index);
                generator.ItemContainerPrepared(recycled, item, index);
                return recycled;
            }

            return null;
        }

        private Control CreateContainer(object? item, int index, object? recycleKey)
        {
            var generator = ItemContainerGenerator!;
            var container = generator.CreateContainer(item, index, recycleKey);

            container.SetValue(RecycleKeyProperty, recycleKey);
            generator.PrepareItemContainer(container, item, index);
            AddInternalChild(container);
            generator.ItemContainerPrepared(container, item, index);

            _indexToContainer[index] = container;
            _containerToIndex[container] = index;

            return container;
        }

        private void RecycleElement(Control element, int index)
        {
            _scrollAnchorProvider?.UnregisterAnchorCandidate(element);

            _indexToContainer.Remove(index);
            _containerToIndex.Remove(element);

            var recycleKey = element.GetValue(RecycleKeyProperty);

            if (recycleKey is null)
            {
                RemoveInternalChild(element);
            }
            else if (recycleKey == s_itemIsItsOwnContainer)
            {
                element.SetCurrentValue(Visual.IsVisibleProperty, false);
            }
            else
            {
                ItemContainerGenerator?.ClearItemContainer(element);
                PushToRecyclePool(recycleKey, element);
                element.SetCurrentValue(Visual.IsVisibleProperty, false);
            }
        }

        private void RecycleUnneededContainers()
        {
            // Find containers to recycle (reuse list to avoid allocations)
            _toRecycle.Clear();
            foreach (var index in _indexToContainer.Keys)
            {
                if (!_neededIndices.Contains(index))
                {
                    _toRecycle.Add(index);
                }
            }

            // Recycle them
            foreach (var index in _toRecycle)
            {
                if (_indexToContainer.TryGetValue(index, out var container))
                {
                    RecycleElement(container, index);
                }
            }
        }

        private void RecycleAllContainers()
        {
            _toRecycle.Clear();
            _toRecycle.AddRange(_indexToContainer.Keys);
            foreach (var index in _toRecycle)
            {
                if (_indexToContainer.TryGetValue(index, out var container))
                {
                    RecycleElement(container, index);
                }
            }
        }

        private void PushToRecyclePool(object recycleKey, Control element)
        {
            _recyclePool ??= new Dictionary<object, Stack<Control>>();

            if (!_recyclePool.TryGetValue(recycleKey, out var pool))
            {
                pool = new Stack<Control>();
                _recyclePool.Add(recycleKey, pool);
            }

            // Limit pool size to prevent unbounded memory growth
            if (pool.Count < MaxPoolSize)
            {
                pool.Push(element);
            }
            else
            {
                // Remove from visual tree if pool is full
                RemoveInternalChild(element);
            }
        }

        private int CalculateColumnCount(double totalWidth, double desiredColumnWidth, double columnSpacing)
        {
            double denominator = desiredColumnWidth + columnSpacing;
            if (denominator <= 0) return 1;
            int count = (int)Math.Floor((totalWidth + columnSpacing) / denominator);
            return Math.Max(1, count);
        }

        private double CalculateActualColumnWidth(double totalWidth, int columnCount, double columnSpacing)
        {
            return (totalWidth - (columnCount - 1) * columnSpacing) / columnCount;
        }

        private void EnsureColumnArrays(int columnCount)
        {
            if (_columnNextY.Length < columnCount)
            {
                _columnNextY = new double[columnCount];
            }
        }

        private void EnsureBoundsCache(int itemCount)
        {
            if (_itemBoundsCache.Length < itemCount)
            {
                int newSize = Math.Max(itemCount, _itemBoundsCache.Length * 2);
                Array.Resize(ref _itemBoundsCache, newSize);
            }
        }

        private void ClearBoundsCache()
        {
            Array.Clear(_itemBoundsCache, 0, _itemBoundsCacheCount);
            _itemBoundsCacheCount = 0;
        }

        private int GetShortestColumn(int columnCount)
        {
            var columns = _columnNextY.AsSpan(0, columnCount);
            int shortestIndex = 0;
            double minHeight = columns[0];

            for (int i = 1; i < columnCount; i++)
            {
                if (columns[i] < minHeight)
                {
                    minHeight = columns[i];
                    shortestIndex = i;
                }
            }

            return shortestIndex;
        }

        protected override Control? ContainerFromIndex(int index)
        {
            _indexToContainer.TryGetValue(index, out var container);
            return container;
        }

        protected override int IndexFromContainer(Control container)
        {
            return _containerToIndex.TryGetValue(container, out var index) ? index : -1;
        }

        protected override IEnumerable<Control>? GetRealizedContainers()
        {
            return _indexToContainer.Values;
        }

        protected override Control? GetControl(NavigationDirection direction, IInputElement? from, bool wrap)
        {
            int count = Items.Count;
            var fromControl = from as Control;

            if (count == 0 || (fromControl is null && direction is not NavigationDirection.First and not NavigationDirection.Last))
                return null;

            int fromIndex = fromControl != null ? IndexFromContainer(fromControl) : -1;
            int toIndex = fromIndex;

            switch (direction)
            {
                case NavigationDirection.First:
                    toIndex = 0;
                    break;
                case NavigationDirection.Last:
                    toIndex = count - 1;
                    break;
                case NavigationDirection.Next:
                case NavigationDirection.Down:
                    toIndex++;
                    break;
                case NavigationDirection.Previous:
                case NavigationDirection.Up:
                    toIndex--;
                    break;
            }

            if (fromIndex == toIndex)
                return fromControl;

            if (wrap)
            {
                if (toIndex < 0)
                    toIndex = count - 1;
                else if (toIndex >= count)
                    toIndex = 0;
            }

            return ScrollIntoView(toIndex);
        }

        protected override Control? ScrollIntoView(int index)
        {
            if (_isInLayout || index < 0 || index >= Items.Count)
                return null;

            if (index < _itemBoundsCacheCount)
            {
                var element = GetOrCreateContainer(Items, index);
                element?.BringIntoView();
                return element;
            }

            return null;
        }
    }
}