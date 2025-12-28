using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// A specialized input control for PIN codes, OTP verification, and security codes.
/// </summary>
public class PinBox : TemplatedControl
{
    private Panel? _itemsPanel;
    private readonly List<PinBoxItem> _items = new();

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
    /// Gets or sets the number of PIN digits.
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
    /// Defines the <see cref="Spacing"/> property.
    /// </summary>
    public static readonly StyledProperty<double> SpacingProperty =
        AvaloniaProperty.Register<PinBox, double>(nameof(Spacing), 8.0);

    /// <summary>
    /// Gets or sets the spacing between PIN boxes.
    /// </summary>
    public double Spacing
    {
        get => GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
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
        AvaloniaProperty.Register<PinBox, TimeSpan>(nameof(AnimationDuration), TimeSpan.FromMilliseconds(150));

    /// <summary>
    /// Gets or sets the duration of animations.
    /// </summary>
    public TimeSpan AnimationDuration
    {
        get => GetValue(AnimationDurationProperty);
        set => SetValue(AnimationDurationProperty, value);
    }



    /// <summary>
    /// Occurs when all PIN digits are entered.
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

        FocusableProperty.OverrideDefaultValue<PinBox>(true);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _itemsPanel = e.NameScope.Find<Panel>("PART_ItemsPanel");

        if (_itemsPanel != null)
        {
            CreateItems();
        }


    }

    private void CreateItems()
    {
        if (_itemsPanel == null) return;

        _itemsPanel.Children.Clear();
        _items.Clear();

        var defaultTheme = DefaultTheme ?? PinBoxTheme.Default;

        for (int i = 0; i < Length; i++)
        {
            var item = new PinBoxItem
            {
                BoxTheme = defaultTheme,
                IsPassword = IsPassword,
                PasswordChar = PasswordChar,
                ShowCursor = ShowCursor,
                CursorBrush = CursorBrush,
                Margin = new Thickness(i == 0 ? 0 : Spacing, 0, 0, 0)
            };

            _items.Add(item);
            _itemsPanel.Children.Add(item);
        }

        UpdateItemsFromText();
    }

    private void UpdateItemsFromText()
    {
        var isFocused = IsFocused;
        var defaultTheme = DefaultTheme ?? PinBoxTheme.Default;
        var focusedTheme = FocusedTheme ?? defaultTheme;
        var filledTheme = FilledTheme ?? defaultTheme;

        for (int i = 0; i < _items.Count; i++)
        {
            var item = _items[i];
            var charValue = i < Text.Length ? Text[i] : (char?)null;

            item.Character = charValue;
            item.State = i == Text.Length && isFocused ? PinBoxItemState.Focused : PinBoxItemState.Default;

            if (HasError && ErrorTheme != null)
            {
                item.BoxTheme = ErrorTheme;
                item.State = PinBoxItemState.Error;
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

    protected override void OnGotFocus(GotFocusEventArgs e)
    {
        base.OnGotFocus(e);
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
        Focus();
        e.Handled = true;
    }

    protected override void OnTextInput(TextInputEventArgs e)
    {
        base.OnTextInput(e);

        if (string.IsNullOrEmpty(e.Text) || Text.Length >= Length)
            return;

        var inputChar = e.Text[0];
        var isValidChar = DigitsOnly ? char.IsDigit(inputChar) : char.IsLetterOrDigit(inputChar);

        if (e.Text.Length == 1 && isValidChar)
        {
            var newText = Text + e.Text;
            SetCurrentValue(TextProperty, newText);

            if (newText.Length == Length)
            {
                OnCompleted(newText);
            }
        }

        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        switch (e.Key)
        {
            case Key.Back when Text.Length > 0:
                SetCurrentValue(TextProperty, Text[..^1]);
                e.Handled = true;
                break;

            case Key.Delete when Text.Length > 0:
                SetCurrentValue(TextProperty, Text[..^1]);
                e.Handled = true;
                break;

            case Key.V when e.KeyModifiers.HasFlag(KeyModifiers.Control):
                _ = HandlePasteAsync();
                e.Handled = true;
                break;
        }
    }

    private async Task HandlePasteAsync()
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel?.Clipboard == null) return;

#pragma warning disable CS0618 // GetTextAsync is obsolete but TryGetTextAsync not available in all versions
        var text = await topLevel.Clipboard.GetTextAsync();
#pragma warning restore CS0618
        if (string.IsNullOrEmpty(text)) return;

        Func<char, bool> charFilter = DigitsOnly ? char.IsDigit : char.IsLetterOrDigit;
        var validText = new string(text.Where(charFilter).Take(Length).ToArray());

        if (!string.IsNullOrEmpty(validText))
        {
            SetCurrentValue(TextProperty, validText);

            if (validText.Length == Length)
            {
                OnCompleted(validText);
            }
        }
    }

    private void OnTextPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        var oldText = e.OldValue as string ?? string.Empty;
        var newText = e.NewValue as string ?? string.Empty;

        // Enforce length limit
        if (newText.Length > Length)
        {
            newText = newText[..Length];
            SetCurrentValue(TextProperty, newText);
            return;
        }

        UpdateItemsFromText();
        ValidateInput();

        TextChanged?.Invoke(this, new PinBoxTextChangedEventArgs(oldText, newText));

        // Raise Completed if text reaches full length
        if (newText.Length == Length && oldText.Length < Length)
        {
            OnCompleted(newText);
        }
    }

    private void OnLengthChanged()
    {
        CreateItems();
    }



    private void ValidateInput()
    {
        if (Validator == null)
        {
            HasError = false;
            ErrorText = null;
            return;
        }

        var error = Validator(Text);
        var previousHasError = HasError;
        HasError = error != null;
        ErrorText = error;

        if (HasError && ErrorTheme != null)
        {
            foreach (var item in _items.Where(i => i.Character.HasValue))
            {
                item.State = PinBoxItemState.Error;
                item.BoxTheme = ErrorTheme;
                item.InvalidateVisual();
            }

            // Trigger shake animation on new error
            if (!previousHasError)
            {
                _ = ShakeAsync();
            }
        }
    }

    /// <summary>
    /// Plays a shake animation to indicate an error.
    /// </summary>
    public async Task ShakeAsync()
    {
        if (_itemsPanel == null) return;

        var translateTransform = new TranslateTransform(0, 0);
        _itemsPanel.RenderTransform = translateTransform;

        var shakeDistance = 8.0;
        var shakeDuration = TimeSpan.FromMilliseconds(50);

        for (var i = 0; i < 3; i++)
        {
            translateTransform.X = shakeDistance;
            await Task.Delay(shakeDuration);
            translateTransform.X = -shakeDistance;
            await Task.Delay(shakeDuration);
        }

        translateTransform.X = 0;
    }

    private void OnCompleted(string pin)
    {
        Completed?.Invoke(this, new PinBoxCompletedEventArgs(pin));
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
