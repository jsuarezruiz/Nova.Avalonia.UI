using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// A specialized input control for PIN codes, OTP verification, and security codes.
/// Supports visual grouping, validation, masking, read-only display, and text normalization.
/// </summary>
[TemplatePart("PART_ItemsPanel", typeof(Panel))]
[TemplatePart("PART_InputTextBox", typeof(TextBox))]
[PseudoClasses(":readonly")]
public class PinBox : TemplatedControl
{
    private const double ShakeDistance = 8.0;

    private Panel? _itemsPanel;
    private TextBox? _inputTextBox;
    private readonly List<PinBoxItem> _items = new();
    private readonly List<TextBlock> _separators = new();
    private readonly HashSet<PinBoxTheme> _subscribedThemes = new();
    private string _lastText = string.Empty;
    private bool _syncingInputTextBox;
    private bool _isAttachedToVisualTree;

    /// <summary>
    /// Defines the <see cref="Text"/> property.
    /// </summary>
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<PinBox, string>(
            nameof(Text),
            defaultValue: string.Empty,
            defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Gets or sets the PIN text value.
    /// </summary>
    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="Length"/> property.
    /// </summary>
    public static readonly StyledProperty<int> LengthProperty =
        AvaloniaProperty.Register<PinBox, int>(
            nameof(Length),
            defaultValue: 6,
            coerce: (_, value) => Math.Clamp(value, 1, 12));

    /// <summary>
    /// Gets or sets the number of PIN characters.
    /// </summary>
    public int Length
    {
        get => GetValue(LengthProperty);
        set => SetValue(LengthProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="IsPassword"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsPasswordProperty =
        AvaloniaProperty.Register<PinBox, bool>(nameof(IsPassword), false);

    /// <summary>
    /// Gets or sets whether to obscure the input characters.
    /// </summary>
    public bool IsPassword
    {
        get => GetValue(IsPasswordProperty);
        set => SetValue(IsPasswordProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="PasswordChar"/> property.
    /// </summary>
    public static readonly StyledProperty<char> PasswordCharProperty =
        AvaloniaProperty.Register<PinBox, char>(nameof(PasswordChar), '●');

    /// <summary>
    /// Gets or sets the character to display when in password mode.
    /// </summary>
    public char PasswordChar
    {
        get => GetValue(PasswordCharProperty);
        set => SetValue(PasswordCharProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="DigitsOnly"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> DigitsOnlyProperty =
        AvaloniaProperty.Register<PinBox, bool>(nameof(DigitsOnly), true);

    /// <summary>
    /// Gets or sets whether only digit characters (0-9) are accepted.
    /// When false, letters and digits are allowed (useful for alphanumeric OTP codes).
    /// </summary>
    public bool DigitsOnly
    {
        get => GetValue(DigitsOnlyProperty);
        set => SetValue(DigitsOnlyProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="TextNormalizer"/> property.
    /// </summary>
    public static readonly StyledProperty<Func<string, string>?> TextNormalizerProperty =
        AvaloniaProperty.Register<PinBox, Func<string, string>?>(nameof(TextNormalizer));

    /// <summary>
    /// Gets or sets a function used to normalize text before filtering and length clamping.
    /// This is useful for pasted SMS or email codes that include spaces, dashes, or mixed casing.
    /// </summary>
    public Func<string, string>? TextNormalizer
    {
        get => GetValue(TextNormalizerProperty);
        set => SetValue(TextNormalizerProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="IsReadOnly"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        AvaloniaProperty.Register<PinBox, bool>(nameof(IsReadOnly), false);

    /// <summary>
    /// Gets or sets whether users can edit the PIN text.
    /// </summary>
    public bool IsReadOnly
    {
        get => GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="Spacing"/> property.
    /// </summary>
    public static readonly StyledProperty<double> SpacingProperty =
        AvaloniaProperty.Register<PinBox, double>(
            nameof(Spacing),
            8.0,
            coerce: (_, value) => double.IsFinite(value) ? Math.Max(0, value) : 0);

    /// <summary>
    /// Gets or sets the spacing between PIN boxes.
    /// </summary>
    public double Spacing
    {
        get => GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="GroupLength"/> property.
    /// </summary>
    public static readonly StyledProperty<int> GroupLengthProperty =
        AvaloniaProperty.Register<PinBox, int>(
            nameof(GroupLength),
            0,
            coerce: (_, value) => Math.Clamp(value, 0, 12));

    /// <summary>
    /// Gets or sets the number of boxes in each visual group. Set to 0 to disable grouping.
    /// </summary>
    public int GroupLength
    {
        get => GetValue(GroupLengthProperty);
        set => SetValue(GroupLengthProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="GroupLengths"/> property.
    /// </summary>
    public static readonly StyledProperty<IReadOnlyList<int>?> GroupLengthsProperty =
        AvaloniaProperty.Register<PinBox, IReadOnlyList<int>?>(nameof(GroupLengths));

    /// <summary>
    /// Gets or sets explicit visual group sizes. When set, this takes precedence over <see cref="GroupLength"/>.
    /// </summary>
    public IReadOnlyList<int>? GroupLengths
    {
        get => GetValue(GroupLengthsProperty);
        set => SetValue(GroupLengthsProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="Separator"/> property.
    /// </summary>
    public static readonly StyledProperty<string?> SeparatorProperty =
        AvaloniaProperty.Register<PinBox, string?>(nameof(Separator));

    /// <summary>
    /// Gets or sets the text displayed between visual groups.
    /// </summary>
    public string? Separator
    {
        get => GetValue(SeparatorProperty);
        set => SetValue(SeparatorProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="IsResponsive"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsResponsiveProperty =
        AvaloniaProperty.Register<PinBox, bool>(nameof(IsResponsive), true);

    /// <summary>
    /// Gets or sets whether boxes shrink when the available width is smaller than the preferred width.
    /// </summary>
    public bool IsResponsive
    {
        get => GetValue(IsResponsiveProperty);
        set => SetValue(IsResponsiveProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="MinItemWidth"/> property.
    /// </summary>
    public static readonly StyledProperty<double> MinItemWidthProperty =
        AvaloniaProperty.Register<PinBox, double>(
            nameof(MinItemWidth),
            32.0,
            coerce: (_, value) => double.IsFinite(value) ? Math.Max(1, value) : 1);

    /// <summary>
    /// Gets or sets the smallest width a responsive PIN box can shrink to.
    /// </summary>
    public double MinItemWidth
    {
        get => GetValue(MinItemWidthProperty);
        set => SetValue(MinItemWidthProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="ShowCursor"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> ShowCursorProperty =
        AvaloniaProperty.Register<PinBox, bool>(nameof(ShowCursor), true);

    /// <summary>
    /// Gets or sets whether to show the blinking cursor.
    /// </summary>
    public bool ShowCursor
    {
        get => GetValue(ShowCursorProperty);
        set => SetValue(ShowCursorProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="CursorBrush"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> CursorBrushProperty =
        AvaloniaProperty.Register<PinBox, IBrush?>(nameof(CursorBrush), Brushes.Black);

    /// <summary>
    /// Gets or sets the cursor brush.
    /// </summary>
    public IBrush? CursorBrush
    {
        get => GetValue(CursorBrushProperty);
        set => SetValue(CursorBrushProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="DefaultTheme"/> property.
    /// </summary>
    public static readonly StyledProperty<PinBoxTheme?> DefaultThemeProperty =
        AvaloniaProperty.Register<PinBox, PinBoxTheme?>(nameof(DefaultTheme));

    /// <summary>
    /// Gets or sets the default theme for empty boxes.
    /// </summary>
    public PinBoxTheme? DefaultTheme
    {
        get => GetValue(DefaultThemeProperty);
        set => SetValue(DefaultThemeProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="FocusedTheme"/> property.
    /// </summary>
    public static readonly StyledProperty<PinBoxTheme?> FocusedThemeProperty =
        AvaloniaProperty.Register<PinBox, PinBoxTheme?>(nameof(FocusedTheme));

    /// <summary>
    /// Gets or sets the theme for the focused box.
    /// </summary>
    public PinBoxTheme? FocusedTheme
    {
        get => GetValue(FocusedThemeProperty);
        set => SetValue(FocusedThemeProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="FilledTheme"/> property.
    /// </summary>
    public static readonly StyledProperty<PinBoxTheme?> FilledThemeProperty =
        AvaloniaProperty.Register<PinBox, PinBoxTheme?>(nameof(FilledTheme));

    /// <summary>
    /// Gets or sets the theme for filled boxes.
    /// </summary>
    public PinBoxTheme? FilledTheme
    {
        get => GetValue(FilledThemeProperty);
        set => SetValue(FilledThemeProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="ErrorTheme"/> property.
    /// </summary>
    public static readonly StyledProperty<PinBoxTheme?> ErrorThemeProperty =
        AvaloniaProperty.Register<PinBox, PinBoxTheme?>(nameof(ErrorTheme));

    /// <summary>
    /// Gets or sets the theme for boxes in error state.
    /// </summary>
    public PinBoxTheme? ErrorTheme
    {
        get => GetValue(ErrorThemeProperty);
        set => SetValue(ErrorThemeProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="Validator"/> property.
    /// </summary>
    public static readonly StyledProperty<Func<string, string?>?> ValidatorProperty =
        AvaloniaProperty.Register<PinBox, Func<string, string?>?>(nameof(Validator));

    /// <summary>
    /// Gets or sets the validation function.
    /// </summary>
    public Func<string, string?>? Validator
    {
        get => GetValue(ValidatorProperty);
        set => SetValue(ValidatorProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="ErrorText"/> property.
    /// </summary>
    public static readonly StyledProperty<string?> ErrorTextProperty =
        AvaloniaProperty.Register<PinBox, string?>(nameof(ErrorText));

    /// <summary>
    /// Gets the current error text from validation.
    /// </summary>
    public string? ErrorText
    {
        get => GetValue(ErrorTextProperty);
        private set => SetValue(ErrorTextProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="HasError"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> HasErrorProperty =
        AvaloniaProperty.Register<PinBox, bool>(nameof(HasError), false);

    /// <summary>
    /// Gets whether the PinBox has a validation error.
    /// </summary>
    public bool HasError
    {
        get => GetValue(HasErrorProperty);
        private set => SetValue(HasErrorProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="AnimationDuration"/> property.
    /// </summary>
    public static readonly StyledProperty<TimeSpan> AnimationDurationProperty =
        AvaloniaProperty.Register<PinBox, TimeSpan>(
            nameof(AnimationDuration),
            TimeSpan.FromMilliseconds(150),
            coerce: (_, value) => value < TimeSpan.Zero ? TimeSpan.Zero : value);

    /// <summary>
    /// Gets or sets the duration of animations.
    /// </summary>
    public TimeSpan AnimationDuration
    {
        get => GetValue(AnimationDurationProperty);
        set => SetValue(AnimationDurationProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="CompletedCommand"/> property.
    /// </summary>
    public static readonly StyledProperty<ICommand?> CompletedCommandProperty =
        AvaloniaProperty.Register<PinBox, ICommand?>(nameof(CompletedCommand));

    /// <summary>
    /// Gets or sets the command invoked when all PIN characters are entered.
    /// </summary>
    public ICommand? CompletedCommand
    {
        get => GetValue(CompletedCommandProperty);
        set => SetValue(CompletedCommandProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="CompletedCommandParameter"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> CompletedCommandParameterProperty =
        AvaloniaProperty.Register<PinBox, object?>(nameof(CompletedCommandParameter));

    /// <summary>
    /// Gets or sets the command parameter. When unset, the completed PIN text is used.
    /// </summary>
    public object? CompletedCommandParameter
    {
        get => GetValue(CompletedCommandParameterProperty);
        set => SetValue(CompletedCommandParameterProperty, value);
    }

    /// <summary>
    /// Occurs when all PIN characters are entered.
    /// </summary>
    public event EventHandler<PinBoxCompletedEventArgs>? Completed;

    /// <summary>
    /// Occurs when the PIN text changes.
    /// </summary>
    public event EventHandler<PinBoxTextChangedEventArgs>? TextChanged;

    static PinBox()
    {
        TextProperty.Changed.AddClassHandler<PinBox>((x, e) => x.OnTextPropertyChanged(e));
        LengthProperty.Changed.AddClassHandler<PinBox>((x, _) => x.OnLengthChanged());
        DigitsOnlyProperty.Changed.AddClassHandler<PinBox>((x, _) => x.OnInputFilterChanged());
        TextNormalizerProperty.Changed.AddClassHandler<PinBox>((x, _) => x.OnInputFilterChanged());
        IsReadOnlyProperty.Changed.AddClassHandler<PinBox>((x, _) => x.OnReadOnlyChanged());
        SpacingProperty.Changed.AddClassHandler<PinBox>((x, _) => x.OnLayoutPropertyChanged());
        GroupLengthProperty.Changed.AddClassHandler<PinBox>((x, _) => x.OnGroupingPropertyChanged());
        GroupLengthsProperty.Changed.AddClassHandler<PinBox>((x, _) => x.OnGroupingPropertyChanged());
        SeparatorProperty.Changed.AddClassHandler<PinBox>((x, _) => x.OnGroupingPropertyChanged());
        IsResponsiveProperty.Changed.AddClassHandler<PinBox>((x, _) => x.OnLayoutPropertyChanged());
        MinItemWidthProperty.Changed.AddClassHandler<PinBox>((x, _) => x.OnLayoutPropertyChanged());
        IsPasswordProperty.Changed.AddClassHandler<PinBox>((x, _) => x.OnPasswordPropertyChanged());
        PasswordCharProperty.Changed.AddClassHandler<PinBox>((x, _) => x.OnPasswordPropertyChanged());
        ShowCursorProperty.Changed.AddClassHandler<PinBox>((x, _) => x.UpdateItemsFromText());
        CursorBrushProperty.Changed.AddClassHandler<PinBox>((x, _) => x.OnThemePropertyChanged());
        DefaultThemeProperty.Changed.AddClassHandler<PinBox>((x, _) => x.OnThemePropertyChanged());
        FocusedThemeProperty.Changed.AddClassHandler<PinBox>((x, _) => x.OnThemePropertyChanged());
        FilledThemeProperty.Changed.AddClassHandler<PinBox>((x, _) => x.OnThemePropertyChanged());
        ErrorThemeProperty.Changed.AddClassHandler<PinBox>((x, _) => x.OnThemePropertyChanged());
        ValidatorProperty.Changed.AddClassHandler<PinBox>((x, _) => x.OnValidatorChanged());
        IsEnabledProperty.Changed.AddClassHandler<PinBox>((x, _) => x.SyncInputTextBox());
        IsEffectivelyEnabledProperty.Changed.AddClassHandler<PinBox>((x, _) => x.OnEffectiveEnabledChanged());
        IsKeyboardFocusWithinProperty.Changed.AddClassHandler<PinBox>((x, _) => x.UpdateItemsFromText());

        FocusableProperty.OverrideDefaultValue<PinBox>(true);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (_isAttachedToVisualTree && IsThemeProperty(change.Property))
        {
            SyncThemeSubscriptions();
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isAttachedToVisualTree = true;
        SyncThemeSubscriptions();
        OnThemePropertyChanged();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _isAttachedToVisualTree = false;
        ClearThemeSubscriptions();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_inputTextBox != null)
        {
            _inputTextBox.TextChanged -= OnInputTextBoxTextChanged;
            _inputTextBox.RemoveHandler(KeyDownEvent, OnInputTextBoxKeyDown);
        }

        _itemsPanel = e.NameScope.Find<Panel>("PART_ItemsPanel");
        _inputTextBox = e.NameScope.Find<TextBox>("PART_InputTextBox");

        if (_inputTextBox != null)
        {
            _inputTextBox.TextChanged += OnInputTextBoxTextChanged;
            _inputTextBox.AddHandler(
                KeyDownEvent,
                OnInputTextBoxKeyDown,
                RoutingStrategies.Tunnel,
                handledEventsToo: true);
            SyncInputTextBox();
        }

        if (_itemsPanel != null)
        {
            CreateItems();
        }
    }

    private void CreateItems()
    {
        if (_itemsPanel == null) return;

        _itemsPanel.ClipToBounds = false;
        _itemsPanel.Margin = new Thickness(ShakeDistance, 0);
        _itemsPanel.Children.Clear();
        _items.Clear();
        _separators.Clear();

        var defaultTheme = DefaultTheme ?? PinBoxTheme.Default;

        for (int i = 0; i < Length; i++)
        {
            if (ShouldInsertSeparatorBefore(i))
            {
                var separator = CreateSeparator(defaultTheme);
                _separators.Add(separator);
                _itemsPanel.Children.Add(separator);
            }

            var item = new PinBoxItem
            {
                BoxTheme = defaultTheme,
                IsPassword = IsPassword,
                PasswordChar = PasswordChar,
                ShowCursor = ShowCursor,
                CursorBrush = CursorBrush,
                Margin = GetItemMargin(i),
                MinWidth = MinItemWidth
            };

            _items.Add(item);
            _itemsPanel.Children.Add(item);
        }

        UpdateResponsiveItemWidths(Bounds.Width);
        UpdateItemsFromText();
    }

    private void UpdateItemsFromText()
    {
        if (_items.Count == 0)
        {
            return;
        }

        var text = Text ?? string.Empty;
        var isFocused = IsInputFocused;
        var isEnabled = IsEffectivelyEnabled;
        var defaultTheme = DefaultTheme ?? PinBoxTheme.Default;
        var focusedTheme = FocusedTheme ?? defaultTheme;
        var filledTheme = FilledTheme ?? defaultTheme;
        var errorTheme = ErrorTheme ?? defaultTheme;

        for (int i = 0; i < _items.Count; i++)
        {
            var item = _items[i];
            var charValue = i < text.Length ? text[i] : (char?)null;

            item.Character = charValue;
            item.State = GetItemState(i, text, charValue, isFocused, isEnabled);

            if (HasError)
            {
                item.BoxTheme = errorTheme;
            }
            else if (charValue.HasValue)
            {
                item.BoxTheme = filledTheme;
            }
            else if (item.State == PinBoxItemState.Focused)
            {
                item.BoxTheme = focusedTheme;
            }
            else
            {
                item.BoxTheme = defaultTheme;
            }

            item.IsPassword = IsPassword;
            item.PasswordChar = PasswordChar;
            item.ShowCursor = ShowCursor;
            item.CursorBrush = CursorBrush;
            item.InvalidateVisual();
        }
    }

    private PinBoxItemState GetItemState(int index, string text, char? charValue, bool isFocused, bool isEnabled)
    {
        if (!isEnabled)
        {
            return PinBoxItemState.Disabled;
        }

        if (HasError)
        {
            return PinBoxItemState.Error;
        }

        if (charValue.HasValue)
        {
            return PinBoxItemState.Filled;
        }

        if (IsReadOnly)
        {
            return PinBoxItemState.Default;
        }

        return index == text.Length && isFocused ? PinBoxItemState.Focused : PinBoxItemState.Default;
    }

    private bool IsInputFocused => IsFocused || IsKeyboardFocusWithin;

    private void UpdateItemMargins()
    {
        for (var i = 0; i < _items.Count; i++)
        {
            _items[i].Margin = GetItemMargin(i);
            _items[i].MinWidth = MinItemWidth;
        }

        foreach (var separator in _separators)
        {
            separator.Margin = GetSeparatorMargin();
        }
    }

    private Thickness GetItemMargin(int index)
    {
        return new Thickness(index == 0 ? 0 : Spacing, 0, 0, 0);
    }

    private Thickness GetSeparatorMargin()
    {
        return new Thickness(Spacing, 0, 0, 0);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        UpdateResponsiveItemWidths(availableSize.Width);
        return base.MeasureOverride(availableSize);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        UpdateResponsiveItemWidths(finalSize.Width);
        return base.ArrangeOverride(finalSize);
    }

    private void OnLayoutPropertyChanged()
    {
        UpdateItemMargins();
        UpdateResponsiveItemWidths(Bounds.Width);
        InvalidateMeasure();
    }

    private void OnThemePropertyChanged()
    {
        UpdateSeparators();
        UpdateItemsFromText();
        InvalidateItemMeasures();
        UpdateResponsiveItemWidths(Bounds.Width);
        InvalidateMeasure();
    }

    private void InvalidateItemMeasures()
    {
        foreach (var item in _items)
        {
            item.InvalidateMeasure();
        }
    }

    private void SyncThemeSubscriptions()
    {
        var assignedThemes = GetAssignedThemes().ToHashSet();

        foreach (var subscribedTheme in _subscribedThemes.ToArray())
        {
            if (assignedThemes.Contains(subscribedTheme))
            {
                continue;
            }

            subscribedTheme.PropertyChanged -= OnThemeObjectPropertyChanged;
            _subscribedThemes.Remove(subscribedTheme);
        }

        foreach (var assignedTheme in assignedThemes)
        {
            if (_subscribedThemes.Add(assignedTheme))
            {
                assignedTheme.PropertyChanged += OnThemeObjectPropertyChanged;
            }
        }
    }

    private void ClearThemeSubscriptions()
    {
        foreach (var subscribedTheme in _subscribedThemes)
        {
            subscribedTheme.PropertyChanged -= OnThemeObjectPropertyChanged;
        }

        _subscribedThemes.Clear();
    }

    private IEnumerable<PinBoxTheme> GetAssignedThemes()
    {
        if (DefaultTheme != null) yield return DefaultTheme;
        if (FocusedTheme != null) yield return FocusedTheme;
        if (FilledTheme != null) yield return FilledTheme;
        if (ErrorTheme != null) yield return ErrorTheme;
    }

    private void OnThemeObjectPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        OnThemePropertyChanged();
    }

    private static bool IsThemeProperty(AvaloniaProperty property)
    {
        return property == DefaultThemeProperty ||
               property == FocusedThemeProperty ||
               property == FilledThemeProperty ||
               property == ErrorThemeProperty;
    }

    private void OnGroupingPropertyChanged()
    {
        CreateItems();
        InvalidateMeasure();
    }

    private void OnReadOnlyChanged()
    {
        PseudoClasses.Set(":readonly", IsReadOnly);
        UpdateItemsFromText();
        SyncInputTextBox();
    }

    private void UpdateResponsiveItemWidths(double availableWidth)
    {
        if (_items.Count == 0)
        {
            return;
        }

        var availableItemsWidth = availableWidth - ShakeDistance * 2;

        if (!IsResponsive || !double.IsFinite(availableItemsWidth) || availableItemsWidth <= 0)
        {
            foreach (var item in _items)
            {
                item.Width = double.NaN;
            }

            return;
        }

        var preferredWidth = _items.Sum(item => (item.BoxTheme ?? DefaultTheme ?? PinBoxTheme.Default).Width);
        var separatorWidth = GetSeparatorWidth();
        var spacingWidth = Spacing * Math.Max(0, _items.Count + _separators.Count - 1);

        if (preferredWidth + separatorWidth + spacingWidth <= availableItemsWidth)
        {
            foreach (var item in _items)
            {
                item.Width = double.NaN;
            }

            return;
        }

        var itemWidth = Math.Max(MinItemWidth, (availableItemsWidth - separatorWidth - spacingWidth) / _items.Count);
        foreach (var item in _items)
        {
            item.Width = itemWidth;
        }
    }

    private double GetSeparatorWidth()
    {
        var width = 0.0;

        foreach (var separator in _separators)
        {
            separator.Measure(Size.Infinity);
            width += separator.DesiredSize.Width;
        }

        return width;
    }

    protected override void OnGotFocus(GotFocusEventArgs e)
    {
        base.OnGotFocus(e);
        if (ReferenceEquals(e.Source, this))
        {
            FocusInputTextBox();
        }

        UpdateItemsFromText();
    }

    protected override void OnLostFocus(global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        base.OnLostFocus(e);
        UpdateItemsFromText();
    }

    protected override void OnPointerPressed(global::Avalonia.Input.PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        FocusInputTextBox();
        e.Handled = true;
    }

    protected override void OnTextInput(TextInputEventArgs e)
    {
        base.OnTextInput(e);

        if (_inputTextBox?.IsKeyboardFocusWithin == true)
        {
            return;
        }

        var text = Text ?? string.Empty;
        if (!CanEditInput || string.IsNullOrEmpty(e.Text) || text.Length >= Length)
        {
            return;
        }

        var input = NormalizeText(text + e.Text);

        if (!string.Equals(input, text, StringComparison.Ordinal))
        {
            SetCurrentValue(TextProperty, input);
        }

        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Handled || !CanEditInput || _inputTextBox?.IsKeyboardFocusWithin == true)
        {
            return;
        }

        var text = Text ?? string.Empty;
        switch (e.Key)
        {
            case Key.Back when text.Length > 0:
                SetCurrentValue(TextProperty, text[..^1]);
                e.Handled = true;
                break;

            case Key.Delete when text.Length > 0:
                SetCurrentValue(TextProperty, text[..^1]);
                e.Handled = true;
                break;

            case Key.V when IsPasteShortcut(e.KeyModifiers):
                _ = HandlePasteAsync();
                e.Handled = true;
                break;
        }
    }

    private void OnInputTextBoxTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_syncingInputTextBox || _inputTextBox == null)
        {
            return;
        }

        SetCurrentValue(TextProperty, _inputTextBox.Text ?? string.Empty);
    }

    private void OnInputTextBoxKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Handled || !CanEditInput || _inputTextBox == null)
        {
            return;
        }

        var text = Text ?? string.Empty;
        switch (e.Key)
        {
            case Key.Back when text.Length > 0:
                SetCurrentValue(TextProperty, text[..^1]);
                e.Handled = true;
                break;

            case Key.Delete when text.Length > 0:
                SetCurrentValue(TextProperty, text[..^1]);
                e.Handled = true;
                break;

            case Key.Left:
            case Key.Right:
            case Key.Up:
            case Key.Down:
            case Key.Home:
            case Key.End:
                MoveInputCaretToEnd();
                e.Handled = true;
                break;
        }
    }

    private void FocusInputTextBox()
    {
        if (_inputTextBox?.IsEffectivelyEnabled == true)
        {
            _inputTextBox.Focus();
            MoveInputCaretToEnd();
            return;
        }

        Focus();
    }

    private void SyncInputTextBox()
    {
        if (_inputTextBox == null)
        {
            return;
        }

        var text = Text ?? string.Empty;
        _syncingInputTextBox = true;
        try
        {
            _inputTextBox.IsEnabled = IsEffectivelyEnabled;
            _inputTextBox.IsReadOnly = IsReadOnly;
            _inputTextBox.PasswordChar = IsPassword ? PasswordChar : '\0';

            if (!string.Equals(_inputTextBox.Text ?? string.Empty, text, StringComparison.Ordinal))
            {
                _inputTextBox.Text = text;
            }

            MoveInputCaretToEnd();
        }
        finally
        {
            _syncingInputTextBox = false;
        }
    }

    private void MoveInputCaretToEnd()
    {
        if (_inputTextBox == null)
        {
            return;
        }

        var caretIndex = (_inputTextBox.Text ?? string.Empty).Length;
        _inputTextBox.CaretIndex = caretIndex;
        _inputTextBox.SelectionStart = caretIndex;
        _inputTextBox.SelectionEnd = caretIndex;
    }

    private async Task HandlePasteAsync()
    {
        if (!CanEditInput) return;

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel?.Clipboard == null) return;

#pragma warning disable CS0618 // GetTextAsync is obsolete but TryGetTextAsync not available in all versions
        var text = await topLevel.Clipboard.GetTextAsync();
#pragma warning restore CS0618
        if (string.IsNullOrEmpty(text)) return;

        var validText = NormalizeText(text);

        if (!string.IsNullOrEmpty(validText))
        {
            SetCurrentValue(TextProperty, validText);
        }
    }

    private void OnTextPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        var rawNewText = e.NewValue as string;
        var normalizedNewText = NormalizeText(rawNewText);

        if (!string.Equals(rawNewText ?? string.Empty, normalizedNewText, StringComparison.Ordinal))
        {
            SetCurrentValue(TextProperty, normalizedNewText);
            return;
        }

        var oldText = _lastText;
        ValidateInput();
        UpdateItemsFromText();
        SyncInputTextBox();

        if (string.Equals(oldText, normalizedNewText, StringComparison.Ordinal))
        {
            return;
        }

        _lastText = normalizedNewText;
        TextChanged?.Invoke(this, new PinBoxTextChangedEventArgs(oldText, normalizedNewText));

        if (normalizedNewText.Length == Length && oldText.Length < Length)
        {
            OnCompleted(normalizedNewText);
        }
    }

    private void OnLengthChanged()
    {
        NormalizeCurrentText();
        CreateItems();
        ValidateInput();
        UpdateItemsFromText();
        SyncInputTextBox();
    }

    private void OnInputFilterChanged()
    {
        NormalizeCurrentText();
        ValidateInput();
        UpdateItemsFromText();
        SyncInputTextBox();
    }

    private void OnValidatorChanged()
    {
        ValidateInput();
        UpdateItemsFromText();
    }

    private void OnPasswordPropertyChanged()
    {
        UpdateItemsFromText();
        SyncInputTextBox();
    }

    private void OnEffectiveEnabledChanged()
    {
        UpdateItemsFromText();
        SyncInputTextBox();
    }

    private void ValidateInput()
    {
        if (Validator == null)
        {
            HasError = false;
            ErrorText = null;
            return;
        }

        var error = Validator(Text ?? string.Empty);
        var previousHasError = HasError;
        HasError = error != null;
        ErrorText = error;

        if (HasError && !previousHasError)
        {
            _ = ShakeAsync();
        }
    }

    private bool NormalizeCurrentText()
    {
        var currentText = Text ?? string.Empty;
        var normalizedText = NormalizeText(currentText);

        if (string.Equals(currentText, normalizedText, StringComparison.Ordinal))
        {
            return false;
        }

        SetCurrentValue(TextProperty, normalizedText);
        return true;
    }

    private string NormalizeText(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        var normalizedText = TextNormalizer?.Invoke(text) ?? text;
        return new string(normalizedText.Where(IsValidCharacter).Take(Length).ToArray());
    }

    private bool CanEditInput => IsEffectivelyEnabled && !IsReadOnly;

    private TextBlock CreateSeparator(PinBoxTheme theme)
    {
        return new TextBlock
        {
            Text = Separator,
            FontSize = theme.FontSize,
            FontWeight = theme.FontWeight,
            Foreground = theme.Foreground ?? CursorBrush,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = GetSeparatorMargin()
        };
    }

    private void UpdateSeparators()
    {
        var theme = DefaultTheme ?? PinBoxTheme.Default;

        foreach (var separator in _separators)
        {
            separator.Text = Separator;
            separator.FontSize = theme.FontSize;
            separator.FontWeight = theme.FontWeight;
            separator.Foreground = theme.Foreground ?? CursorBrush;
            separator.Margin = GetSeparatorMargin();
        }
    }

    private bool ShouldInsertSeparatorBefore(int index)
    {
        if (index <= 0 || string.IsNullOrEmpty(Separator))
        {
            return false;
        }

        var groupLengths = GroupLengths;
        if (groupLengths is { Count: > 0 })
        {
            var boundary = 0;

            foreach (var groupLength in groupLengths)
            {
                if (groupLength <= 0)
                {
                    continue;
                }

                boundary += groupLength;
                if (boundary == index && boundary < Length)
                {
                    return true;
                }

                if (boundary >= index)
                {
                    return false;
                }
            }

            return false;
        }

        var fixedGroupLength = GroupLength;
        return fixedGroupLength > 0 && index % fixedGroupLength == 0;
    }

    private bool IsValidCharacter(char character)
    {
        return DigitsOnly ? IsAsciiDigit(character) : IsAsciiLetterOrDigit(character);
    }

    private static bool IsAsciiDigit(char character)
    {
        return character is >= '0' and <= '9';
    }

    private static bool IsAsciiLetterOrDigit(char character)
    {
        return IsAsciiDigit(character) ||
               character is >= 'A' and <= 'Z' ||
               character is >= 'a' and <= 'z';
    }

    private static bool IsPasteShortcut(KeyModifiers keyModifiers)
    {
        return keyModifiers.HasFlag(KeyModifiers.Control) || keyModifiers.HasFlag(KeyModifiers.Meta);
    }

    /// <summary>
    /// Plays a shake animation to indicate an error.
    /// </summary>
    public async Task ShakeAsync()
    {
        if (_itemsPanel == null) return;

        var translateTransform = new TranslateTransform(0, 0);
        _itemsPanel.RenderTransform = translateTransform;

        var shakeDuration = AnimationDuration <= TimeSpan.Zero
            ? TimeSpan.Zero
            : TimeSpan.FromTicks(AnimationDuration.Ticks / 6);

        for (var i = 0; i < 3; i++)
        {
            translateTransform.X = ShakeDistance;
            await Task.Delay(shakeDuration);
            translateTransform.X = -ShakeDistance;
            await Task.Delay(shakeDuration);
        }

        translateTransform.X = 0;
    }

    private void OnCompleted(string completedText)
    {
        var args = new PinBoxCompletedEventArgs(completedText);
        Completed?.Invoke(this, args);

        var command = CompletedCommand;
        var parameter = CompletedCommandParameter ?? completedText;
        if (command?.CanExecute(parameter) == true)
        {
            command.Execute(parameter);
        }
    }

    /// <summary>
    /// Clears all entered characters.
    /// </summary>
    public void Clear()
    {
        SetCurrentValue(TextProperty, string.Empty);
    }

    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new PinBoxAutomationPeer(this);
    }
}
