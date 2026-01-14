using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.VisualTree;

namespace Nova.Avalonia.UI.Controls
{
    /// <summary>
    /// A virtualizing panel that arranges children in a variable-size grid layout.
    /// Items can span multiple columns and rows, creating flexible tile-based layouts.
    /// Only items within the visible viewport (plus buffer) are materialized for optimal performance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This panel virtualizes items by only creating UI containers for visible items.
    /// Off-screen items are recycled and reused when scrolling, minimizing memory usage
    /// and improving scroll performance for large datasets.
    /// </para>
    /// <para>
    /// Use the <see cref="ColumnSpanProperty"/> and <see cref="RowSpanProperty"/> attached
    /// properties on child elements to control their size in the grid.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// &lt;ItemsControl ItemsSource="{Binding Items}"&gt;
    ///     &lt;ItemsControl.ItemsPanel&gt;
    ///         &lt;ItemsPanelTemplate&gt;
    ///             &lt;controls:VirtualizingVariableSizeWrapPanel Columns="4" TileSize="100" Spacing="8"/&gt;
    ///         &lt;/ItemsPanelTemplate&gt;
    ///     &lt;/ItemsControl.ItemsPanel&gt;
    /// &lt;/ItemsControl&gt;
    /// </code>
    /// </example>
    public class VirtualizingVariableSizeWrapPanel : VirtualizingPanel
    {
        private static readonly AttachedProperty<object?> RecycleKeyProperty =
            AvaloniaProperty.RegisterAttached<VirtualizingVariableSizeWrapPanel, Control, object?>("RecycleKey");

        private static readonly object s_itemIsItsOwnContainer = new();

        /// <summary>
        /// Defines the <see cref="TileSize"/> property.
        /// </summary>
        public static readonly StyledProperty<double> TileSizeProperty =
            AvaloniaProperty.Register<VirtualizingVariableSizeWrapPanel, double>(nameof(TileSize), 100);

        /// <summary>
        /// Defines the <see cref="Spacing"/> property.
        /// </summary>
        public static readonly StyledProperty<double> SpacingProperty =
            AvaloniaProperty.Register<VirtualizingVariableSizeWrapPanel, double>(nameof(Spacing), 8);

        /// <summary>
        /// Defines the <see cref="Columns"/> property.
        /// </summary>
        public static readonly StyledProperty<int> ColumnsProperty =
            AvaloniaProperty.Register<VirtualizingVariableSizeWrapPanel, int>(nameof(Columns), 4);

        /// <summary>
        /// Defines the ColumnSpan attached property.
        /// </summary>
        public static readonly AttachedProperty<int> ColumnSpanProperty =
            AvaloniaProperty.RegisterAttached<VirtualizingVariableSizeWrapPanel, Control, int>("ColumnSpan", 1);

        /// <summary>
        /// Defines the RowSpan attached property.
        /// </summary>
        public static readonly AttachedProperty<int> RowSpanProperty =
            AvaloniaProperty.RegisterAttached<VirtualizingVariableSizeWrapPanel, Control, int>("RowSpan", 1);

        // Pre-allocated delegate to avoid closure allocations
        private readonly Action<Control, int> _recycleElement;

        // Container tracking, Dictionary supports sparse visibility patterns
        private readonly Dictionary<int, Control> _indexToContainer = new();
        private readonly Dictionary<Control, int> _containerToIndex = new();

        // Recycle pool, hidden containers ready for reuse
        private Dictionary<object, Stack<Control>>? _recyclePool;

        // Scroll anchor support
        private IScrollAnchorProvider? _scrollAnchorProvider;

        // Layout state, reused across measure passes
        private Rect[] _itemBoundsCache = Array.Empty<Rect>();
        private int[] _cachedColSpans = Array.Empty<int>();
        private int[] _cachedRowSpans = Array.Empty<int>();
        private int _itemCacheCount;

        // Occupied grid, reusable array for position finding
        private bool[] _occupiedGrid = Array.Empty<bool>();
        private int[] _rowOccupiedCount = Array.Empty<int>();
        private int _occupiedGridRows;
        private int _firstFreeRow;
        private int _maxDirtyIndex = -1;

        private Rect _viewport;
        private double _lastMeasureWidth = -1;
        private double _lastMaxHeight;
        private bool _isInLayout;

        // Reusable collections to avoid allocations in hot path
        private readonly HashSet<int> _neededIndices = new();
        private readonly List<int> _toRecycle = new();
        private const int MaxPoolSize = 20;

        /// <summary>
        /// Gets or sets the size of a single tile (both width and height for a 1x1 item).
        /// </summary>
        /// <value>The tile size in device-independent pixels. Default is 100.</value>
        public double TileSize
        {
            get => GetValue(TileSizeProperty);
            set => SetValue(TileSizeProperty, value);
        }

        /// <summary>
        /// Gets or sets the spacing between tiles in both directions.
        /// </summary>
        /// <value>The spacing in device-independent pixels. Default is 8.</value>
        public double Spacing
        {
            get => GetValue(SpacingProperty);
            set => SetValue(SpacingProperty, value);
        }

        /// <summary>
        /// Gets or sets the number of columns in the grid.
        /// </summary>
        /// <value>The number of columns. Default is 4.</value>
        public int Columns
        {
            get => GetValue(ColumnsProperty);
            set => SetValue(ColumnsProperty, value);
        }

        /// <summary>
        /// Gets the number of columns an element should span.
        /// </summary>
        /// <param name="element">The element to get the column span for.</param>
        /// <returns>The number of columns the element spans.</returns>
        public static int GetColumnSpan(Control element) => element.GetValue(ColumnSpanProperty);

        /// <summary>
        /// Sets the number of columns an element should span.
        /// </summary>
        /// <param name="element">The element to set the column span for.</param>
        /// <param name="value">The number of columns to span.</param>
        public static void SetColumnSpan(Control element, int value) => element.SetValue(ColumnSpanProperty, value);

        /// <summary>
        /// Gets the number of rows an element should span.
        /// </summary>
        /// <param name="element">The element to get the row span for.</param>
        /// <returns>The number of rows the element spans.</returns>
        public static int GetRowSpan(Control element) => element.GetValue(RowSpanProperty);

        /// <summary>
        /// Sets the number of rows an element should span.
        /// </summary>
        /// <param name="element">The element to set the row span for.</param>
        /// <param name="value">The number of rows to span.</param>
        public static void SetRowSpan(Control element, int value) => element.SetValue(RowSpanProperty, value);

        static VirtualizingVariableSizeWrapPanel()
        {
            AffectsMeasure<VirtualizingVariableSizeWrapPanel>(TileSizeProperty, SpacingProperty, ColumnsProperty);
            AffectsArrange<VirtualizingVariableSizeWrapPanel>(TileSizeProperty, SpacingProperty);
            AffectsMeasure<VirtualizingVariableSizeWrapPanel>(ColumnSpanProperty, RowSpanProperty);
        }

        public VirtualizingVariableSizeWrapPanel()
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
            if (change.Property == TileSizeProperty ||
                change.Property == SpacingProperty ||
                change.Property == ColumnsProperty)
            {
                _lastMeasureWidth = -1;
            }
        }

        protected override void OnItemsChanged(IReadOnlyList<object?> items, NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(items, e);

            bool isAppend = e.Action == NotifyCollectionChangedAction.Add && 
                            e.NewStartingIndex >= _itemCacheCount;

            if (e.Action == NotifyCollectionChangedAction.Reset ||
                e.Action == NotifyCollectionChangedAction.Remove ||
                e.Action == NotifyCollectionChangedAction.Replace ||
                e.Action == NotifyCollectionChangedAction.Move ||
                (e.Action == NotifyCollectionChangedAction.Add && !isAppend))
            {
                RecycleAllContainers();
                ClearCaches();
            }

            _lastMeasureWidth = -1;
            InvalidateMeasure();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var items = Items;
            int itemCount = items.Count;

            // Non-virtualized mode
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
                int columns = Columns;
                double tileSize = TileSize;
                double spacing = Spacing;

                // Calculate extended viewport with generous buffer for smooth scrolling
                // Use Bounds height or fallback for initial estimation
                double viewportHeight = _viewport.Height > 0 ? _viewport.Height : 
                                       (Bounds.Height > 0 ? Bounds.Height : 600);
                double viewportY = _viewport.Height > 0 ? _viewport.Y : 0;
                
                // Use 2x viewport as buffer for smooth bidirectional scrolling
                double bufferSize = viewportHeight * 2;
                double viewportTop = Math.Max(0, viewportY - bufferSize);
                double viewportBottom = viewportY + viewportHeight + bufferSize;

                ClearOccupiedGrid();
                _firstFreeRow = 0;

                EnsureCaches(itemCount);

                _neededIndices.Clear();
                double maxPanelHeight = 0;
                bool needsFullLayout = true;

                if (availableSize.Width == _lastMeasureWidth && _itemCacheCount == itemCount)
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
                                // Verify spans match cache (critical for correctness)
                                int currentColSpan = GetColumnSpan(container);
                                int currentRowSpan = GetRowSpan(container);
                                if (currentColSpan < 1) currentColSpan = 1;
                                if (currentRowSpan < 1) currentRowSpan = 1;

                                if (currentColSpan != _cachedColSpans[i] || currentRowSpan != _cachedRowSpans[i])
                                {
                                    needsFullLayout = true;
                                    break;
                                }

                                _neededIndices.Add(i);
                                container.Measure(rect.Size);
                            }
                        }
                    }

                    if (!needsFullLayout)
                    {
                        maxPanelHeight = _lastMaxHeight;
                    }
                }

                if (needsFullLayout)
                {
                    // Clear needed indices as we will repopulate them
                    _neededIndices.Clear();

                    for (int i = 0; i < itemCount; i++)
                    {
                    // Get spans
                    int colSpan = _cachedColSpans[i];
                    int rowSpan = _cachedRowSpans[i];

                    if (colSpan == 0)
                    {
                        var container = GetOrCreateContainer(items, i);
                        if (container != null)
                        {
                            colSpan = GetColumnSpan(container);
                            rowSpan = GetRowSpan(container);
                            if (colSpan < 1) colSpan = 1;
                            if (rowSpan < 1) rowSpan = 1;
                            _cachedColSpans[i] = colSpan;
                            _cachedRowSpans[i] = rowSpan;
                            _neededIndices.Add(i);
                        }
                        else
                        {
                            colSpan = 1;
                            rowSpan = 1;
                        }
                    }

                    if (colSpan > columns) colSpan = columns;

                    FindPosition(columns, colSpan, rowSpan, out int startRow, out int startCol);
                    MarkOccupied(columns, startRow, startCol, rowSpan, colSpan);

                    // Calculate bounds
                    double x = startCol * (tileSize + spacing);
                    double y = startRow * (tileSize + spacing);
                    double w = colSpan * tileSize + (colSpan - 1) * spacing;
                    double h = rowSpan * tileSize + (rowSpan - 1) * spacing;

                    var rect = new Rect(x, y, w, h);
                    _itemBoundsCache[i] = rect;

                    if (rect.Bottom > maxPanelHeight)
                        maxPanelHeight = rect.Bottom;

                    // Virtualization
                    bool isVisible = rect.Bottom >= viewportTop && rect.Y <= viewportBottom;

                    if (isVisible)
                    {
                        _neededIndices.Add(i);
                        var container = GetOrCreateContainer(items, i);
                        container?.Measure(new Size(w, h));
                    }
                }
                }

                // Recycle unneeded containers
                RecycleUnneededContainers();

                _itemCacheCount = itemCount;
                _lastMaxHeight = maxPanelHeight;
                _lastMeasureWidth = availableSize.Width;

                return new Size(availableSize.Width, maxPanelHeight);
            }
            finally
            {
                _isInLayout = false;
            }
        }

        private Size MeasureNonVirtualized(Size availableSize)
        {
            int columns = Columns;
            double tileSize = TileSize;
            double spacing = Spacing;

            ClearOccupiedGrid();
            EnsureCaches(Children.Count);
            _itemCacheCount = 0;

            double maxPanelHeight = 0;
            int index = 0;

            foreach (var child in Children)
            {
                if (child is not Control control) continue;

                int colSpan = GetColumnSpan(control);
                int rowSpan = GetRowSpan(control);
                if (colSpan < 1) colSpan = 1;
                if (rowSpan < 1) rowSpan = 1;
                if (colSpan > columns) colSpan = columns;

                FindPosition(columns, colSpan, rowSpan, out int startRow, out int startCol);
                MarkOccupied(columns, startRow, startCol, rowSpan, colSpan);

                double x = startCol * (tileSize + spacing);
                double y = startRow * (tileSize + spacing);
                double w = colSpan * tileSize + (colSpan - 1) * spacing;
                double h = rowSpan * tileSize + (rowSpan - 1) * spacing;

                child.Measure(new Size(w, h));

                var rect = new Rect(x, y, w, h);
                _itemBoundsCache[index] = rect;

                if (rect.Bottom > maxPanelHeight)
                    maxPanelHeight = rect.Bottom;

                index++;
            }

            _itemCacheCount = index;
            return new Size(availableSize.Width, maxPanelHeight);
        }

        private void FindPosition(int columns, int colSpan, int rowSpan, out int startRow, out int startCol)
        {
            startRow = 0;
            startCol = 0;
            int currentRow = _firstFreeRow;

            while (true)
            {
                EnsureOccupiedGridRows(currentRow + rowSpan, columns);

                for (int c = 0; c <= columns - colSpan; c++)
                {
                    bool fits = true;
                    for (int r = 0; r < rowSpan && fits; r++)
                    {
                        for (int k = 0; k < colSpan && fits; k++)
                        {
                            int idx = (currentRow + r) * columns + (c + k);
                            if (_occupiedGrid[idx])
                            {
                                fits = false;
                            }
                        }
                    }

                    if (fits)
                    {
                        startRow = currentRow;
                        startCol = c;
                        return;
                    }
                }
                currentRow++;
            }
        }

        private void MarkOccupied(int columns, int startRow, int startCol, int rowSpan, int colSpan)
        {
            EnsureOccupiedGridRows(startRow + rowSpan, columns);

            for (int r = 0; r < rowSpan; r++)
            {
                for (int c = 0; c < colSpan; c++)
                {
                    int rIndex = startRow + r;
                    int idx = rIndex * columns + (startCol + c);
                    _occupiedGrid[idx] = true;
                    if (idx > _maxDirtyIndex) _maxDirtyIndex = idx;
                    _rowOccupiedCount[rIndex]++;
                }
            }


            while (_firstFreeRow < _occupiedGridRows && _rowOccupiedCount[_firstFreeRow] >= columns)
            {
                _firstFreeRow++;
            }
        }

        private void ClearOccupiedGrid()
        {
            if (_maxDirtyIndex >= 0)
            {
                int lengthToClear = Math.Min(_maxDirtyIndex + 1, _occupiedGrid.Length);
                _occupiedGrid.AsSpan(0, lengthToClear).Clear();
                _maxDirtyIndex = -1;
            }

            if (_occupiedGridRows > 0)
            {
                Array.Clear(_rowOccupiedCount, 0, _occupiedGridRows);
            }
            _occupiedGridRows = 0;
        }

        private void EnsureOccupiedGridRows(int rows, int columns)
        {
            if (rows <= _occupiedGridRows)
                return;

            int required = rows * columns;
            if (_occupiedGrid.Length < required)
            {
                int newSize = Math.Max(required, _occupiedGrid.Length * 2);
                Array.Resize(ref _occupiedGrid, newSize);
                // Also resize row counts
                Array.Resize(ref _rowOccupiedCount, newSize / columns + 1);
            }
            // Ensure row count array is large enough even if grid was large enough
            if (_rowOccupiedCount.Length < rows)
            {
                 Array.Resize(ref _rowOccupiedCount, Math.Max(rows, _rowOccupiedCount.Length * 2));
            }

            // Clear new rows only
            int startIdx = _occupiedGridRows * columns;
            int endIdx = rows * columns;
            if (endIdx > startIdx)
            {
                _occupiedGrid.AsSpan(startIdx, endIdx - startIdx).Clear();
                int rowsToClear = rows - _occupiedGridRows;
                if (rowsToClear > 0)
                    Array.Clear(_rowOccupiedCount, _occupiedGridRows, rowsToClear);
            }
            _occupiedGridRows = rows;
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

                    if (index >= 0 && index < _itemCacheCount)
                    {
                        var rect = _itemBoundsCache[index];
                        container.Arrange(rect);

                        if (_viewport.Intersects(rect))
                        {
                            _scrollAnchorProvider?.RegisterAnchorCandidate(container);
                        }
                    }
                }

                if (_indexToContainer.Count == 0 && _itemCacheCount > 0)
                {
                    int index = 0;
                    foreach (var child in Children)
                    {
                        if (child is Control control && index < _itemCacheCount)
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
            _toRecycle.Clear();
            foreach (var index in _indexToContainer.Keys)
            {
                if (!_neededIndices.Contains(index))
                {
                    _toRecycle.Add(index);
                }
            }

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

        private void EnsureCaches(int itemCount)
        {
            if (_itemBoundsCache.Length < itemCount)
            {
                int newSize = Math.Max(itemCount, _itemBoundsCache.Length * 2);
                Array.Resize(ref _itemBoundsCache, newSize);
                Array.Resize(ref _cachedColSpans, newSize);
                Array.Resize(ref _cachedRowSpans, newSize);
            }
        }

        private void ClearCaches()
        {
            Array.Clear(_itemBoundsCache, 0, _itemCacheCount);
            Array.Clear(_cachedColSpans, 0, _itemCacheCount);
            Array.Clear(_cachedRowSpans, 0, _itemCacheCount);
            _itemCacheCount = 0;
            _lastMeasureWidth = -1;
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
            return null;
        }

        protected override Control? ScrollIntoView(int index)
        {
            if (_isInLayout || index < 0 || index >= Items.Count)
                return null;

            if (index < _itemCacheCount)
            {
                var element = GetOrCreateContainer(Items, index);
                element?.BringIntoView();
                return element;
            }

            return null;
        }
    }
}
