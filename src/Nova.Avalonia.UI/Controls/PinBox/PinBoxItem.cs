using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Represents an individual digit box within a PinBox control.
/// </summary>
internal class PinBoxItem : Control
{
    private bool _showCursor;
    private DispatcherTimer? _cursorTimer;

    /// <summary>
    /// Defines the <see cref="Character"/> property.
    /// </summary>
    public static readonly StyledProperty<char?> CharacterProperty =
        AvaloniaProperty.Register<PinBoxItem, char?>(nameof(Character));

    /// <summary>
    /// Gets or sets the character displayed in this box.
    /// </summary>
    public char? Character
    {
        get => GetValue(CharacterProperty);
        set => SetValue(CharacterProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="State"/> property.
    /// </summary>
    public static readonly StyledProperty<PinBoxItemState> StateProperty =
        AvaloniaProperty.Register<PinBoxItem, PinBoxItemState>(
            nameof(State),
            defaultValue: PinBoxItemState.Default);

    /// <summary>
    /// Gets or sets the state of this box.
    /// </summary>
    public PinBoxItemState State
    {
        get => GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="BoxTheme"/> property.
    /// </summary>
    public static readonly StyledProperty<PinBoxTheme?> BoxThemeProperty =
        AvaloniaProperty.Register<PinBoxItem, PinBoxTheme?>(nameof(BoxTheme));

    /// <summary>
    /// Gets or sets the theme for this box.
    /// </summary>
    public PinBoxTheme? BoxTheme
    {
        get => GetValue(BoxThemeProperty);
        set => SetValue(BoxThemeProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="IsPassword"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsPasswordProperty =
        AvaloniaProperty.Register<PinBoxItem, bool>(nameof(IsPassword));

    /// <summary>
    /// Gets or sets whether to obscure the character.
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
        AvaloniaProperty.Register<PinBoxItem, char>(nameof(PasswordChar), '●');

    /// <summary>
    /// Gets or sets the character to display when in password mode.
    /// </summary>
    public char PasswordChar
    {
        get => GetValue(PasswordCharProperty);
        set => SetValue(PasswordCharProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="ShowCursor"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> ShowCursorProperty =
        AvaloniaProperty.Register<PinBoxItem, bool>(nameof(ShowCursor), true);

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
        AvaloniaProperty.Register<PinBoxItem, IBrush?>(nameof(CursorBrush), Brushes.Black);

    /// <summary>
    /// Gets or sets the cursor brush.
    /// </summary>
    public IBrush? CursorBrush
    {
        get => GetValue(CursorBrushProperty);
        set => SetValue(CursorBrushProperty, value);
    }

    public PinBoxItem()
    {
        _cursorTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(530)
        };
        _cursorTimer.Tick += OnCursorTimerTick;
    }

    private void OnCursorTimerTick(object? sender, EventArgs e)
    {
        _showCursor = !_showCursor;
        InvalidateVisual();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (State == PinBoxItemState.Focused)
        {
            _cursorTimer?.Start();
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _cursorTimer?.Stop();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == StateProperty)
        {
            if (State == PinBoxItemState.Focused)
            {
                _showCursor = true;
                _cursorTimer?.Start();
            }
            else
            {
                _cursorTimer?.Stop();
                _showCursor = false;
            }
            InvalidateVisual();
        }
        else if (change.Property == CharacterProperty ||
                 change.Property == BoxThemeProperty ||
                 change.Property == IsPasswordProperty)
        {
            InvalidateVisual();
        }
    }


    public override void Render(DrawingContext context)
    {
        var theme = BoxTheme ?? PinBoxTheme.Default;
        var bounds = new Rect(0, 0, theme.Width, theme.Height);

        // Draw background (skip for underline style)
        if (theme.Background != null && !theme.IsUnderline)
        {
            var backgroundGeometry = CreateRoundedRect(bounds, theme.CornerRadius);
            context.DrawGeometry(theme.Background, null, backgroundGeometry);
        }

        // Draw border
        if (theme.BorderBrush != null && theme.BorderThickness > 0)
        {
            var pen = new Pen(theme.BorderBrush, theme.BorderThickness);
            
            if (theme.IsUnderline)
            {
                // Draw only bottom line for underline style
                var y = bounds.Bottom - theme.BorderThickness / 2;
                context.DrawLine(pen, new Point(bounds.Left, y), new Point(bounds.Right, y));
            }
            else
            {
                // Draw full border
                var borderRect = bounds.Deflate(theme.BorderThickness / 2);
                var borderGeometry = CreateRoundedRect(borderRect, theme.CornerRadius);
                context.DrawGeometry(null, pen, borderGeometry);
            }
        }

        // Draw character or cursor
        if (Character.HasValue)
        {
            DrawCharacter(context, bounds, theme);
        }
        else if (State == PinBoxItemState.Focused && ShowCursor && _showCursor)
        {
            DrawCursor(context, bounds);
        }
    }

    private void DrawCharacter(DrawingContext context, Rect bounds, PinBoxTheme theme)
    {
        var displayChar = IsPassword ? PasswordChar : Character!.Value;
        var text = displayChar.ToString();

        var typeface = new Typeface(
            FontFamily.Default,
            FontStyle.Normal,
            theme.FontWeight);

        var formattedText = new FormattedText(
            text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            typeface,
            theme.FontSize,
            theme.Foreground ?? Brushes.Black);

        var textPosition = new Point(
            (bounds.Width - formattedText.Width) / 2,
            (bounds.Height - formattedText.Height) / 2);

        context.DrawText(formattedText, textPosition);
    }

    private void DrawCursor(DrawingContext context, Rect bounds)
    {
        var cursorBrush = CursorBrush ?? Brushes.Black;
        var cursorHeight = bounds.Height * 0.5;
        var cursorWidth = 2.0;

        var cursorRect = new Rect(
            (bounds.Width - cursorWidth) / 2,
            (bounds.Height - cursorHeight) / 2,
            cursorWidth,
            cursorHeight);

        context.FillRectangle(cursorBrush, cursorRect);
    }

    private static StreamGeometry CreateRoundedRect(Rect rect, CornerRadius radius)
    {
        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            var r = Math.Min(radius.TopLeft, Math.Min(rect.Width, rect.Height) / 2);

            ctx.BeginFigure(new Point(rect.Left + r, rect.Top), true);
            ctx.LineTo(new Point(rect.Right - r, rect.Top));
            ctx.ArcTo(new Point(rect.Right, rect.Top + r), new Size(r, r), 0, false, SweepDirection.Clockwise);
            ctx.LineTo(new Point(rect.Right, rect.Bottom - r));
            ctx.ArcTo(new Point(rect.Right - r, rect.Bottom), new Size(r, r), 0, false, SweepDirection.Clockwise);
            ctx.LineTo(new Point(rect.Left + r, rect.Bottom));
            ctx.ArcTo(new Point(rect.Left, rect.Bottom - r), new Size(r, r), 0, false, SweepDirection.Clockwise);
            ctx.LineTo(new Point(rect.Left, rect.Top + r));
            ctx.ArcTo(new Point(rect.Left + r, rect.Top), new Size(r, r), 0, false, SweepDirection.Clockwise);
            ctx.EndFigure(true);
        }
        return geometry;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var theme = BoxTheme ?? PinBoxTheme.Default;
        return new Size(theme.Width, theme.Height);
    }
}
