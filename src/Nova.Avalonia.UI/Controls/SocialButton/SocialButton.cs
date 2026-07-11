using System;
using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// A provider-branded sign-in button for social and identity providers.
/// </summary>
[TemplatePart("PART_Icon", typeof(ContentControl))]
[TemplatePart("PART_Text", typeof(TextBlock))]
[TemplatePart("PART_FocusRing", typeof(Border))]
[PseudoClasses(":brand", ":outline", ":gray", ":small", ":medium", ":large", ":icononly", ":noicon")]
public class SocialButton : Button
{
    private const string OutlineBackgroundResourceKey = "SocialButtonOutlineBackgroundBrush";
    private const string OutlineBorderResourceKey = "SocialButtonOutlineBorderBrush";
    private const string OutlineForegroundResourceKey = "SocialButtonOutlineForegroundBrush";
    private const string GrayBackgroundResourceKey = "SocialButtonGrayBackgroundBrush";
    private const string GrayBorderResourceKey = "SocialButtonGrayBorderBrush";
    private const string GrayForegroundResourceKey = "SocialButtonGrayForegroundBrush";

    private static readonly IBrush LightOutlineBackground = Brush.Parse("#FFFFFF");
    private static readonly IBrush LightOutlineBorder = Brush.Parse("#D0D5DD");
    private static readonly IBrush LightOutlineForeground = Brush.Parse("#101828");
    private static readonly IBrush LightGrayBackground = Brush.Parse("#F2F4F7");
    private static readonly IBrush LightGrayBorder = Brush.Parse("#EAECF0");
    private static readonly IBrush LightGrayForeground = Brush.Parse("#101828");
    private static readonly IBrush DarkOutlineBackground = Brush.Parse("#161B26");
    private static readonly IBrush DarkOutlineBorder = Brush.Parse("#333741");
    private static readonly IBrush DarkOutlineForeground = Brush.Parse("#F5F5F6");
    private static readonly IBrush DarkGrayBackground = Brush.Parse("#1F242F");
    private static readonly IBrush DarkGrayBorder = Brush.Parse("#333741");
    private static readonly IBrush DarkGrayForeground = Brush.Parse("#F5F5F6");
    private static readonly IBrush WhiteForeground = Brushes.White;
    private static readonly IBrush AccessibleBlackForeground = Brushes.Black;
    private static readonly IBrush BlackForeground = Brush.Parse("#101828");
    private static readonly IBrush NeutralDarkBackground = Brush.Parse("#101828");
    private static readonly IBrush GoogleBlue = Brush.Parse("#4285F4");
    private static readonly IBrush FacebookBlue = Brush.Parse("#1877F2");
    private static readonly IBrush AppleBlack = Brush.Parse("#000000");
    private static readonly IBrush XBlack = Brush.Parse("#0F1419");
    private static readonly IBrush GitHubBlack = Brush.Parse("#24292F");
    private static readonly IBrush MicrosoftBlue = Brush.Parse("#0078D4");
    private static readonly IBrush FigmaGreen = Brush.Parse("#0ACF83");
    private static readonly IBrush DribbblePink = Brush.Parse("#EA4C89");

    private static readonly IBrush GoogleRed = Brush.Parse("#EA4335");
    private static readonly IBrush GoogleYellow = Brush.Parse("#FBBC05");
    private static readonly IBrush GoogleGreen = Brush.Parse("#34A853");
    private static readonly IBrush MicrosoftRed = Brush.Parse("#F25022");
    private static readonly IBrush MicrosoftGreen = Brush.Parse("#7FBA00");
    private static readonly IBrush MicrosoftYellow = Brush.Parse("#FFB900");
    private static readonly IBrush FigmaOrange = Brush.Parse("#F24E1E");
    private static readonly IBrush FigmaCoral = Brush.Parse("#FF7262");
    private static readonly IBrush FigmaPurple = Brush.Parse("#A259FF");
    private static readonly IBrush FigmaCyan = Brush.Parse("#1ABCFE");

    private static readonly Geometry GoogleBlueIconData = Geometry.Parse("M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z");
    private static readonly Geometry GoogleGreenIconData = Geometry.Parse("M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C4 20.53 7.7 23 12 23z");
    private static readonly Geometry GoogleYellowIconData = Geometry.Parse("M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.16H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.84l3.66-2.75z");
    private static readonly Geometry GoogleRedIconData = Geometry.Parse("M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 4 3.47 2.18 7.16l3.66 2.84C6.71 7.4 9.14 5.38 12 5.38z");
    private static readonly Geometry FacebookIconData = Geometry.Parse("M24 12.07C24 5.41 18.63 0 12 0S0 5.41 0 12.07c0 6.02 4.39 11.01 10.13 11.93v-8.44H7.08v-3.49h3.05V9.41c0-3.02 1.79-4.69 4.53-4.69 1.31 0 2.68.24 2.68.24v2.96h-1.51c-1.49 0-1.96.93-1.96 1.88v2.26h3.33l-.53 3.49h-2.8V24C19.61 23.08 24 18.09 24 12.07z");
    private static readonly Geometry AppleIconData = Geometry.Parse("M16.37 12.3c-.02-2.1 1.72-3.11 1.8-3.16-.98-1.43-2.5-1.63-3.04-1.65-1.3-.13-2.53.76-3.19.76-.66 0-1.68-.74-2.76-.72-1.42.02-2.73.83-3.46 2.1-1.48 2.57-.38 6.38 1.06 8.47.71 1.03 1.53 2.18 2.62 2.14 1.05-.04 1.45-.69 2.73-.69 1.27 0 1.64.69 2.76.67 1.14-.02 1.86-1.05 2.55-2.09.82-1.2 1.16-2.36 1.18-2.42-.03-.01-2.25-.86-2.27-3.42z M14.28 6.12c.58-.7.97-1.68.86-2.65-.84.03-1.86.56-2.47 1.26-.54.63-1.02 1.64-.89 2.6.94.07 1.89-.48 2.5-1.21z");
    private static readonly Geometry XIconData = Geometry.Parse("M18.244 2.25h3.308l-7.227 8.26 8.502 11.24h-6.657l-5.214-6.817-5.966 6.817H1.68l7.73-8.835L1.254 2.25H8.08l4.713 6.231 5.45-6.231zm-1.161 17.52h1.833L7.084 4.126H5.117L17.083 19.77z");
    private static readonly Geometry GitHubIconData = Geometry.Parse("M12 .5C5.65.5.5 5.65.5 12c0 5.09 3.29 9.4 7.86 10.93.58.11.79-.25.79-.56v-2.16c-3.2.7-3.87-1.36-3.87-1.36-.52-1.33-1.28-1.69-1.28-1.69-1.05-.72.08-.71.08-.71 1.16.08 1.77 1.19 1.77 1.19 1.03 1.77 2.71 1.26 3.37.96.1-.75.4-1.26.73-1.55-2.55-.29-5.23-1.28-5.23-5.68 0-1.25.45-2.28 1.19-3.08-.12-.29-.52-1.46.11-3.04 0 0 .97-.31 3.17 1.18.92-.26 1.91-.38 2.9-.39.98 0 1.97.13 2.9.39 2.2-1.49 3.17-1.18 3.17-1.18.63 1.58.23 2.75.11 3.04.74.8 1.19 1.83 1.19 3.08 0 4.41-2.69 5.38-5.25 5.67.41.35.78 1.05.78 2.12v3.14c0 .31.21.67.8.56A11.51 11.51 0 0 0 23.5 12C23.5 5.65 18.35.5 12 .5z");
    private static readonly Geometry FigmaOrangeIconData = Geometry.Parse("M4 4a4 4 0 0 1 4-4h4v8H8a4 4 0 0 1-4-4z");
    private static readonly Geometry FigmaCoralIconData = Geometry.Parse("M12 0h4a4 4 0 0 1 0 8h-4V0z");
    private static readonly Geometry FigmaPurpleIconData = Geometry.Parse("M4 12a4 4 0 0 1 4-4h4v8H8a4 4 0 0 1-4-4z");
    private static readonly Geometry FigmaCyanIconData = Geometry.Parse("M12 12a4 4 0 1 1 8 0 4 4 0 0 1-8 0z");
    private static readonly Geometry FigmaGreenIconData = Geometry.Parse("M4 20a4 4 0 0 1 4-4h4v4a4 4 0 1 1-8 0z");
    private static readonly Geometry DribbbleIconData = Geometry.Parse("M12 2a10 10 0 1 0 0 20 10 10 0 0 0 0-20zm6.6 4.6a8.1 8.1 0 0 1 1.8 5.1c-1.9-.4-3.7-.4-5.3-.2-.2-.5-.4-.9-.6-1.4 2-.8 3.6-1.9 5-3.4zM12 3.7c2.1 0 4 .8 5.5 2.1-1.2 1.3-2.7 2.3-4.5 3.1a51 51 0 0 0-3.2-5 8.7 8.7 0 0 1 2.2-.3zM8.1 4.6c1.1 1.4 2.2 3 3.1 4.6-2.5.7-5.2.8-7.6.8a8.3 8.3 0 0 1 4.5-5.4zM3.5 12v-.3c2.8 0 5.9-.1 8.5-1 .2.4.4.8.6 1.3-3.2 1-5.4 3.1-6.8 5.6A8.2 8.2 0 0 1 3.5 12zm8.5 8.3c-1.9 0-3.7-.7-5.1-1.8 1.2-2.3 3.1-4.3 5.9-5.2.8 2.1 1.3 4.3 1.6 6.4-.8.2-1.6.3-2.4.3zm6-3.2a8.2 8.2 0 0 1-3 2.3c-.3-2-.8-4.1-1.5-6.1 1.5-.2 3.2-.1 5 .4-.1 1.2-.3 2.3-.5 3.4z");

    private string _displayText = string.Empty;
    private object? _resolvedIcon;

    /// <summary>
    /// Defines the <see cref="Provider"/> property.
    /// </summary>
    public static readonly StyledProperty<SocialButtonProvider> ProviderProperty =
        AvaloniaProperty.Register<SocialButton, SocialButtonProvider>(
            nameof(Provider),
            SocialButtonProvider.Google,
            coerce: static (_, value) => Enum.IsDefined(value) ? value : SocialButtonProvider.Google);

    /// <summary>
    /// Defines the <see cref="ProviderDisplayName"/> property.
    /// </summary>
    public static readonly StyledProperty<string?> ProviderDisplayNameProperty =
        AvaloniaProperty.Register<SocialButton, string?>(nameof(ProviderDisplayName));

    /// <summary>
    /// Defines the <see cref="Variant"/> property.
    /// </summary>
    public static readonly StyledProperty<SocialButtonVariant> VariantProperty =
        AvaloniaProperty.Register<SocialButton, SocialButtonVariant>(
            nameof(Variant),
            SocialButtonVariant.Outline,
            coerce: static (_, value) => Enum.IsDefined(value) ? value : SocialButtonVariant.Outline);

    /// <summary>
    /// Defines the <see cref="Size"/> property.
    /// </summary>
    public static readonly StyledProperty<SocialButtonSize> SizeProperty =
        AvaloniaProperty.Register<SocialButton, SocialButtonSize>(
            nameof(Size),
            SocialButtonSize.Medium,
            coerce: static (_, value) => Enum.IsDefined(value) ? value : SocialButtonSize.Medium);

    /// <summary>
    /// Defines the <see cref="Action"/> property.
    /// </summary>
    public static readonly StyledProperty<SocialButtonAction> ActionProperty =
        AvaloniaProperty.Register<SocialButton, SocialButtonAction>(
            nameof(Action),
            SocialButtonAction.Continue,
            coerce: static (_, value) => Enum.IsDefined(value) ? value : SocialButtonAction.Continue);

    /// <summary>
    /// Defines the <see cref="Text"/> property.
    /// </summary>
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<SocialButton, string?>(nameof(Text));

    /// <summary>
    /// Defines the <see cref="Icon"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<SocialButton, object?>(nameof(Icon));

    /// <summary>
    /// Defines the <see cref="IconForeground"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> IconForegroundProperty =
        AvaloniaProperty.Register<SocialButton, IBrush?>(nameof(IconForeground));

    /// <summary>
    /// Defines the <see cref="IsIconOnly"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsIconOnlyProperty =
        AvaloniaProperty.Register<SocialButton, bool>(nameof(IsIconOnly));

    /// <summary>
    /// Defines the <see cref="DisplayText"/> property.
    /// </summary>
    public static readonly DirectProperty<SocialButton, string> DisplayTextProperty =
        AvaloniaProperty.RegisterDirect<SocialButton, string>(
            nameof(DisplayText),
            owner => owner.DisplayText);

    /// <summary>
    /// Defines the <see cref="ResolvedIcon"/> property.
    /// </summary>
    public static readonly DirectProperty<SocialButton, object?> ResolvedIconProperty =
        AvaloniaProperty.RegisterDirect<SocialButton, object?>(
            nameof(ResolvedIcon),
            owner => owner.ResolvedIcon);

    /// <summary>
    /// Gets or sets the social or identity provider.
    /// </summary>
    public SocialButtonProvider Provider
    {
        get => GetValue(ProviderProperty);
        set => SetValue(ProviderProperty, value);
    }

    /// <summary>
    /// Gets or sets the displayed provider name. Built-in provider names are used when this is unset.
    /// </summary>
    public string? ProviderDisplayName
    {
        get => GetValue(ProviderDisplayNameProperty);
        set => SetValue(ProviderDisplayNameProperty, value);
    }

    /// <summary>
    /// Gets or sets the visual variant.
    /// </summary>
    public SocialButtonVariant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    /// <summary>
    /// Gets or sets the size preset.
    /// </summary>
    public SocialButtonSize Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>
    /// Gets or sets the generated action text.
    /// </summary>
    public SocialButtonAction Action
    {
        get => GetValue(ActionProperty);
        set => SetValue(ActionProperty, value);
    }

    /// <summary>
    /// Gets or sets custom button text. When unset, text is generated from <see cref="Action"/> and <see cref="Provider"/>.
    /// </summary>
    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// Gets or sets custom icon content. When unset, a built-in provider glyph is used.
    /// </summary>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// Gets or sets the icon foreground brush.
    /// </summary>
    public IBrush? IconForeground
    {
        get => GetValue(IconForegroundProperty);
        set => SetValue(IconForegroundProperty, value);
    }

    /// <summary>
    /// Gets or sets whether only the icon is shown.
    /// </summary>
    public bool IsIconOnly
    {
        get => GetValue(IsIconOnlyProperty);
        set => SetValue(IsIconOnlyProperty, value);
    }

    /// <summary>
    /// Gets the text displayed by the button.
    /// </summary>
    public string DisplayText
    {
        get => _displayText;
        private set => SetAndRaise(DisplayTextProperty, ref _displayText, value);
    }

    /// <summary>
    /// Gets the icon content displayed by the button.
    /// </summary>
    public object? ResolvedIcon
    {
        get => _resolvedIcon;
        private set => SetAndRaise(ResolvedIconProperty, ref _resolvedIcon, value);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SocialButton"/> class.
    /// </summary>
    public SocialButton()
    {
        ResourcesChanged += OnResourcesChanged;
        ActualThemeVariantChanged += OnActualThemeVariantChanged;
        UpdateResolvedState();
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        // Pseudo-classes and display text were already resolved in the constructor and do not
        // depend on the tree; only the themed brushes and icon can differ once attached.
        RefreshThemedAppearance();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        var property = change.Property;

        if (property == VariantProperty || property == SizeProperty || property == IsIconOnlyProperty)
        {
            UpdatePseudoClasses();
        }

        if (property == VariantProperty || property == ProviderProperty)
        {
            UpdateAppearance();
        }

        if (property == TextProperty || property == ActionProperty ||
            property == ProviderProperty || property == ProviderDisplayNameProperty)
        {
            UpdateDisplayText();
        }

        if (property == VariantProperty || property == ProviderProperty ||
            property == IconProperty || property == IconForegroundProperty ||
            property == SizeProperty)
        {
            UpdateResolvedIcon();
        }
    }

    /// <inheritdoc />
    protected override AutomationPeer OnCreateAutomationPeer() => new SocialButtonAutomationPeer(this);

    private void OnResourcesChanged(object? sender, ResourcesChangedEventArgs e)
    {
        RefreshThemedAppearance();
    }

    private void OnActualThemeVariantChanged(object? sender, EventArgs e)
    {
        RefreshThemedAppearance();
    }

    private void UpdateResolvedState()
    {
        UpdatePseudoClasses();
        UpdateAppearance();
        UpdateDisplayText();
        UpdateResolvedIcon();
    }

    private void RefreshThemedAppearance()
    {
        UpdateAppearance();
        UpdateResolvedIcon();
    }

    private void UpdateDisplayText()
    {
        DisplayText = CreateDisplayText();
    }

    private void UpdateResolvedIcon()
    {
        var icon = CreateResolvedIcon();
        ResolvedIcon = icon;
        PseudoClasses.Set(":noicon", icon is null);
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":brand", Variant == SocialButtonVariant.Brand);
        PseudoClasses.Set(":outline", Variant == SocialButtonVariant.Outline);
        PseudoClasses.Set(":gray", Variant == SocialButtonVariant.Gray);
        PseudoClasses.Set(":small", Size == SocialButtonSize.Small);
        PseudoClasses.Set(":medium", Size == SocialButtonSize.Medium);
        PseudoClasses.Set(":large", Size == SocialButtonSize.Large);
        PseudoClasses.Set(":icononly", IsIconOnly);
    }

    private void UpdateAppearance()
    {
        var background = GetBackgroundBrush();
        var foreground = GetForegroundBrush();
        var borderBrush = GetBorderBrush();

        SetCurrentValue(BackgroundProperty, background);
        SetCurrentValue(ForegroundProperty, foreground);
        SetCurrentValue(BorderBrushProperty, borderBrush);
    }

    private string CreateDisplayText()
    {
        if (!string.IsNullOrWhiteSpace(Text))
        {
            return Text!;
        }

        var action = GetActionText(Action);
        var providerName = GetProviderName();
        return string.IsNullOrWhiteSpace(providerName) ? action : $"{action} with {providerName}";
    }

    private object? CreateResolvedIcon()
    {
        if (Icon is not null)
        {
            return Icon;
        }

        return CreateProviderIcon();
    }

    private string GetProviderName()
    {
        if (!string.IsNullOrWhiteSpace(ProviderDisplayName))
        {
            return ProviderDisplayName!;
        }

        return Provider switch
        {
            SocialButtonProvider.Google => "Google",
            SocialButtonProvider.Facebook => "Facebook",
            SocialButtonProvider.Apple => "Apple",
            SocialButtonProvider.X => "X",
            SocialButtonProvider.GitHub => "GitHub",
            SocialButtonProvider.Microsoft => "Microsoft",
            SocialButtonProvider.Figma => "Figma",
            SocialButtonProvider.Dribbble => "Dribbble",
            _ => string.Empty
        };
    }

    private static string GetActionText(SocialButtonAction action) =>
        action switch
        {
            SocialButtonAction.SignIn => "Sign in",
            SocialButtonAction.SignUp => "Sign up",
            SocialButtonAction.Connect => "Connect",
            _ => "Continue"
        };

    private object? CreateProviderIcon()
    {
        var iconSize = GetIconSize();
        var iconForeground = IconForeground;
        var defaultIconForeground = GetIconForegroundBrush(GetForegroundBrush());
        var monochromeForeground = iconForeground ?? defaultIconForeground;

        return Provider switch
        {
            SocialButtonProvider.Google when iconForeground is not null => CreateIcon(iconSize,
                (GoogleBlueIconData, iconForeground),
                (GoogleGreenIconData, iconForeground),
                (GoogleYellowIconData, iconForeground),
                (GoogleRedIconData, iconForeground)),
            SocialButtonProvider.Google => CreateIcon(iconSize,
                (GoogleBlueIconData, GoogleBlue),
                (GoogleGreenIconData, GoogleGreen),
                (GoogleYellowIconData, GoogleYellow),
                (GoogleRedIconData, GoogleRed)),
            SocialButtonProvider.Facebook => CreateIcon(iconSize, (FacebookIconData, monochromeForeground)),
            SocialButtonProvider.Apple => CreateIcon(iconSize, (AppleIconData, monochromeForeground)),
            SocialButtonProvider.X => CreateIcon(iconSize, (XIconData, monochromeForeground)),
            SocialButtonProvider.GitHub => CreateIcon(iconSize, (GitHubIconData, monochromeForeground)),
            SocialButtonProvider.Microsoft when iconForeground is not null => CreateMicrosoftIcon(iconSize, iconForeground),
            SocialButtonProvider.Microsoft when Variant == SocialButtonVariant.Brand => CreateMicrosoftIcon(iconSize, monochromeForeground),
            SocialButtonProvider.Microsoft => CreateMicrosoftIcon(iconSize),
            SocialButtonProvider.Figma when iconForeground is not null => CreateIcon(iconSize,
                (FigmaOrangeIconData, iconForeground),
                (FigmaCoralIconData, iconForeground),
                (FigmaPurpleIconData, iconForeground),
                (FigmaCyanIconData, iconForeground),
                (FigmaGreenIconData, iconForeground)),
            SocialButtonProvider.Figma => CreateIcon(iconSize,
                (FigmaOrangeIconData, FigmaOrange),
                (FigmaCoralIconData, FigmaCoral),
                (FigmaPurpleIconData, FigmaPurple),
                (FigmaCyanIconData, FigmaCyan),
                (FigmaGreenIconData, FigmaGreen)),
            SocialButtonProvider.Dribbble => CreateIcon(iconSize, (DribbbleIconData, monochromeForeground)),
            _ => null
        };
    }

    private static Viewbox CreateIcon(double iconSize, params (Geometry Data, IBrush Fill)[] paths)
    {
        var canvas = new Canvas
        {
            Width = 24,
            Height = 24
        };

        foreach (var (data, fill) in paths)
        {
            canvas.Children.Add(new Path
            {
                Data = data,
                Fill = fill
            });
        }

        return new Viewbox
        {
            Width = iconSize,
            Height = iconSize,
            Stretch = Stretch.Uniform,
            Child = canvas
        };
    }

    private static Viewbox CreateMicrosoftIcon(double iconSize, IBrush? overrideFill = null)
    {
        var canvas = new Canvas
        {
            Width = 24,
            Height = 24
        };

        AddRectangle(canvas, 2, 2, 9, 9, overrideFill ?? MicrosoftRed);
        AddRectangle(canvas, 13, 2, 9, 9, overrideFill ?? MicrosoftGreen);
        AddRectangle(canvas, 2, 13, 9, 9, overrideFill ?? MicrosoftBlue);
        AddRectangle(canvas, 13, 13, 9, 9, overrideFill ?? MicrosoftYellow);

        return new Viewbox
        {
            Width = iconSize,
            Height = iconSize,
            Stretch = Stretch.Uniform,
            Child = canvas
        };
    }

    private static void AddRectangle(Canvas canvas, double left, double top, double width, double height, IBrush fill)
    {
        var rectangle = new Rectangle
        {
            Width = width,
            Height = height,
            Fill = fill
        };
        Canvas.SetLeft(rectangle, left);
        Canvas.SetTop(rectangle, top);
        canvas.Children.Add(rectangle);
    }

    private IBrush GetBackgroundBrush()
    {
        if (Variant == SocialButtonVariant.Outline)
        {
            return GetThemeBrush(OutlineBackgroundResourceKey, LightOutlineBackground, DarkOutlineBackground);
        }

        if (Variant == SocialButtonVariant.Gray)
        {
            return GetThemeBrush(GrayBackgroundResourceKey, LightGrayBackground, DarkGrayBackground);
        }

        return Provider switch
        {
            SocialButtonProvider.Google => LightOutlineBackground,
            SocialButtonProvider.Facebook => FacebookBlue,
            SocialButtonProvider.Apple => AppleBlack,
            SocialButtonProvider.X => XBlack,
            SocialButtonProvider.GitHub => GitHubBlack,
            SocialButtonProvider.Microsoft => MicrosoftBlue,
            SocialButtonProvider.Figma => FigmaGreen,
            SocialButtonProvider.Dribbble => DribbblePink,
            _ => NeutralDarkBackground
        };
    }

    private IBrush GetForegroundBrush()
    {
        if (Variant == SocialButtonVariant.Outline)
        {
            return GetThemeBrush(OutlineForegroundResourceKey, LightOutlineForeground, DarkOutlineForeground);
        }

        if (Variant == SocialButtonVariant.Gray)
        {
            return GetThemeBrush(GrayForegroundResourceKey, LightGrayForeground, DarkGrayForeground);
        }

        return Provider switch
        {
            SocialButtonProvider.Facebook => AccessibleBlackForeground,
            SocialButtonProvider.Figma => AccessibleBlackForeground,
            SocialButtonProvider.Dribbble => AccessibleBlackForeground,
            SocialButtonProvider.Google => BlackForeground,
            _ => WhiteForeground
        };
    }

    private IBrush GetBorderBrush()
    {
        if (Variant == SocialButtonVariant.Outline)
        {
            return GetThemeBrush(OutlineBorderResourceKey, LightOutlineBorder, DarkOutlineBorder);
        }

        if (Variant == SocialButtonVariant.Gray)
        {
            return GetThemeBrush(GrayBorderResourceKey, LightGrayBorder, DarkGrayBorder);
        }

        if (Variant == SocialButtonVariant.Brand && Provider != SocialButtonProvider.Google)
        {
            var background = GetBackgroundBrush();

            // A dark brand surface (Apple, X, GitHub, custom) blends into a dark app
            // background and loses its bounds. Delineate it with a subtle border in dark themes.
            if (IsDarkTheme && IsDarkBrush(background))
            {
                return GetThemeBrush(OutlineBorderResourceKey, LightOutlineBorder, DarkOutlineBorder);
            }

            return background;
        }

        return LightOutlineBorder;
    }

    private IBrush GetIconForegroundBrush(IBrush foreground)
    {
        if (Variant == SocialButtonVariant.Brand)
        {
            // The Microsoft brand background reuses the logo's blue, so render the glyph
            // in a single contrasting color to keep every quadrant visible.
            return Provider == SocialButtonProvider.Microsoft ? WhiteForeground : foreground;
        }

        return Provider switch
        {
            SocialButtonProvider.Facebook => FacebookBlue,
            SocialButtonProvider.Dribbble => DribbblePink,
            _ => foreground
        };
    }

    private IBrush GetThemeBrush(string resourceKey, IBrush lightFallback, IBrush darkFallback)
    {
        if (TryGetResource(resourceKey, ActualThemeVariant, out var resource) ||
            TryGetResource(resourceKey, null, out resource))
        {
            return resource switch
            {
                IBrush brush => brush,
                Color color => new SolidColorBrush(color),
                string value => Brush.Parse(value),
                _ => IsDarkTheme ? darkFallback : lightFallback
            };
        }

        return IsDarkTheme ? darkFallback : lightFallback;
    }

    private bool IsDarkTheme => ActualThemeVariant == ThemeVariant.Dark;

    private static bool IsDarkBrush(IBrush brush)
    {
        if (brush is ISolidColorBrush solid)
        {
            var color = solid.Color;
            var luminance = (0.299 * color.R) + (0.587 * color.G) + (0.114 * color.B);
            return luminance < 70.0;
        }

        return false;
    }

    private double GetIconSize() =>
        Size switch
        {
            SocialButtonSize.Small => 16.0,
            SocialButtonSize.Large => 20.0,
            _ => 18.0
        };
}
