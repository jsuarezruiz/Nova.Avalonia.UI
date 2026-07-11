using System;
using System.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Nova.Avalonia.UI.Controls;
using Xunit;

namespace Nova.Avalonia.UI.Tests.Controls;

public class SocialButtonTests
{
    [AvaloniaFact]
    public void Defaults_AreCorrect()
    {
        var button = new SocialButton();

        Assert.Equal(SocialButtonProvider.Google, button.Provider);
        Assert.Equal(SocialButtonVariant.Outline, button.Variant);
        Assert.Equal(SocialButtonSize.Medium, button.Size);
        Assert.Equal(SocialButtonAction.Continue, button.Action);
        Assert.Null(button.Text);
        Assert.Null(button.Icon);
        Assert.False(button.IsIconOnly);
        Assert.Equal("Continue with Google", button.DisplayText);
        Assert.NotNull(button.ResolvedIcon);
    }

    [AvaloniaFact]
    public void Action_UpdatesGeneratedText()
    {
        var button = new SocialButton
        {
            Provider = SocialButtonProvider.Apple,
            Action = SocialButtonAction.SignIn
        };

        Assert.Equal("Sign in with Apple", button.DisplayText);

        button.Action = SocialButtonAction.SignUp;

        Assert.Equal("Sign up with Apple", button.DisplayText);
    }

    [AvaloniaFact]
    public void Text_OverridesGeneratedText()
    {
        var button = new SocialButton
        {
            Provider = SocialButtonProvider.Google,
            Text = "Use Google"
        };

        Assert.Equal("Use Google", button.DisplayText);
    }

    [AvaloniaFact]
    public void ProviderDisplayName_OverridesBuiltInProviderName()
    {
        var button = new SocialButton
        {
            Provider = SocialButtonProvider.Google,
            ProviderDisplayName = "Workspace"
        };

        Assert.Equal("Continue with Workspace", button.DisplayText);
    }

    [AvaloniaFact]
    public void CustomProvider_UsesActionText_WhenProviderNameIsMissing()
    {
        var button = new SocialButton
        {
            Provider = SocialButtonProvider.Custom,
            Action = SocialButtonAction.Connect
        };

        Assert.Equal("Connect", button.DisplayText);
    }

    [AvaloniaFact]
    public void CustomProvider_UsesProviderDisplayName()
    {
        var button = new SocialButton
        {
            Provider = SocialButtonProvider.Custom,
            ProviderDisplayName = "Contoso",
            Action = SocialButtonAction.Continue
        };

        Assert.Equal("Continue with Contoso", button.DisplayText);
    }

    [AvaloniaFact]
    public void Icon_OverridesBuiltInIcon()
    {
        var icon = new TextBlock { Text = "C" };
        var button = new SocialButton
        {
            Icon = icon
        };

        Assert.Same(icon, button.ResolvedIcon);
    }

    [AvaloniaFact]
    public void BuiltInProviderIcon_IsViewbox()
    {
        var button = new SocialButton
        {
            Provider = SocialButtonProvider.GitHub
        };

        Assert.IsType<Viewbox>(button.ResolvedIcon);
    }

    [AvaloniaFact]
    public void GoogleIcon_UsesFourColoredPaths()
    {
        var button = new SocialButton
        {
            Provider = SocialButtonProvider.Google
        };

        var canvas = GetIconCanvas(button);
        var paths = canvas.Children.OfType<Path>().ToArray();

        Assert.Equal(4, paths.Length);
        Assert.Equal(4, paths.Select(path => path.Fill).Distinct().Count());
    }

    [AvaloniaFact]
    public void MicrosoftIcon_UsesFourColoredSquares()
    {
        var button = new SocialButton
        {
            Provider = SocialButtonProvider.Microsoft
        };

        var canvas = GetIconCanvas(button);
        var rectangles = canvas.Children.OfType<Rectangle>().ToArray();

        Assert.Equal(4, rectangles.Length);
        Assert.Equal(4, rectangles.Select(rectangle => rectangle.Fill).Distinct().Count());
    }

    [AvaloniaFact]
    public void FigmaIcon_UsesFiveColoredPaths()
    {
        var button = new SocialButton
        {
            Provider = SocialButtonProvider.Figma
        };

        var canvas = GetIconCanvas(button);
        var paths = canvas.Children.OfType<Path>().ToArray();

        Assert.Equal(5, paths.Length);
        Assert.Equal(5, paths.Select(path => path.Fill).Distinct().Count());
    }

    [AvaloniaFact]
    public void IconForeground_OverridesGoogleIconColors()
    {
        var button = new SocialButton
        {
            Provider = SocialButtonProvider.Google,
            IconForeground = Brushes.Red
        };

        var canvas = GetIconCanvas(button);
        var paths = canvas.Children.OfType<Path>().ToArray();

        Assert.Equal(4, paths.Length);
        Assert.All(paths, path => Assert.Same(Brushes.Red, path.Fill));
    }

    [AvaloniaFact]
    public void BrandMicrosoftIcon_UsesContrastingMonochromeFill()
    {
        var button = new SocialButton
        {
            Provider = SocialButtonProvider.Microsoft,
            Variant = SocialButtonVariant.Brand
        };

        var canvas = GetIconCanvas(button);
        var rectangles = canvas.Children.OfType<Rectangle>().ToArray();

        Assert.Equal(4, rectangles.Length);
        Assert.Single(rectangles.Select(rectangle => rectangle.Fill).Distinct());
        Assert.All(rectangles, rectangle =>
            Assert.NotEqual(GetSolidColor(button.Background), GetSolidColor(rectangle.Fill)));
    }

    [AvaloniaFact]
    public void IconForeground_OverridesMicrosoftIconColors()
    {
        var button = new SocialButton
        {
            Provider = SocialButtonProvider.Microsoft,
            IconForeground = Brushes.Red
        };

        var canvas = GetIconCanvas(button);
        var rectangles = canvas.Children.OfType<Rectangle>().ToArray();

        Assert.Equal(4, rectangles.Length);
        Assert.All(rectangles, rectangle => Assert.Same(Brushes.Red, rectangle.Fill));
    }

    [AvaloniaTheory]
    [InlineData(SocialButtonProvider.Facebook)]
    [InlineData(SocialButtonProvider.Figma)]
    [InlineData(SocialButtonProvider.Dribbble)]
    public void BrandVariant_UsesContrastSafeForeground(SocialButtonProvider provider)
    {
        var button = new SocialButton
        {
            Provider = provider,
            Variant = SocialButtonVariant.Brand
        };

        Assert.Same(Brushes.Black, button.Foreground);
    }

    [AvaloniaTheory]
    [InlineData(SocialButtonProvider.Custom)]
    [InlineData(SocialButtonProvider.Google)]
    [InlineData(SocialButtonProvider.Facebook)]
    [InlineData(SocialButtonProvider.Apple)]
    [InlineData(SocialButtonProvider.X)]
    [InlineData(SocialButtonProvider.GitHub)]
    [InlineData(SocialButtonProvider.Microsoft)]
    [InlineData(SocialButtonProvider.Figma)]
    [InlineData(SocialButtonProvider.Dribbble)]
    public void BrandVariant_TextContrastMeetsWcagAA(SocialButtonProvider provider)
    {
        var button = new SocialButton
        {
            Provider = provider,
            Variant = SocialButtonVariant.Brand
        };

        var ratio = GetContrastRatio(GetSolidColor(button.Foreground), GetSolidColor(button.Background));

        Assert.True(ratio >= 4.5, $"{provider} contrast ratio was {ratio:0.00}.");
    }

    [AvaloniaFact]
    public void OutlineVariant_UsesDarkThemeResources()
    {
        var button = new SocialButton
        {
            Provider = SocialButtonProvider.Apple
        };
        var window = ShowInWindow(button, ThemeVariant.Dark);

        try
        {
            Assert.Equal(Color.Parse("#161B26"), GetSolidColor(button.Background));
            Assert.Equal(Color.Parse("#333741"), GetSolidColor(button.BorderBrush));
            Assert.Equal(Color.Parse("#F5F5F6"), GetSolidColor(button.Foreground));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void GrayVariant_UsesDarkThemeResources()
    {
        var button = new SocialButton
        {
            Provider = SocialButtonProvider.Apple,
            Variant = SocialButtonVariant.Gray
        };
        var window = ShowInWindow(button, ThemeVariant.Dark);

        try
        {
            Assert.Equal(Color.Parse("#1F242F"), GetSolidColor(button.Background));
            Assert.Equal(Color.Parse("#333741"), GetSolidColor(button.BorderBrush));
            Assert.Equal(Color.Parse("#F5F5F6"), GetSolidColor(button.Foreground));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void ThemeChange_RefreshesNeutralAppearance()
    {
        var button = new SocialButton
        {
            Provider = SocialButtonProvider.Apple
        };
        var window = ShowInWindow(button, ThemeVariant.Light);

        try
        {
            Assert.Equal(Color.Parse("#FFFFFF"), GetSolidColor(button.Background));

            window.RequestedThemeVariant = ThemeVariant.Dark;

            Assert.Equal(Color.Parse("#161B26"), GetSolidColor(button.Background));
            Assert.Equal(Color.Parse("#F5F5F6"), GetSolidColor(button.Foreground));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTheory]
    [InlineData(SocialButtonProvider.Apple)]
    [InlineData(SocialButtonProvider.X)]
    [InlineData(SocialButtonProvider.GitHub)]
    public void DarkTheme_UsesThemeForegroundForMonochromeNeutralIcons(SocialButtonProvider provider)
    {
        var button = new SocialButton
        {
            Provider = provider
        };
        var window = ShowInWindow(button, ThemeVariant.Dark);

        try
        {
            var canvas = GetIconCanvas(button);
            var path = Assert.Single(canvas.Children.OfType<Path>());

            Assert.Equal(GetSolidColor(button.Foreground), GetSolidColor(path.Fill));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTheory]
    [InlineData(SocialButtonProvider.Apple)]
    [InlineData(SocialButtonProvider.X)]
    [InlineData(SocialButtonProvider.GitHub)]
    public void BrandDarkSurface_GetsDelineatingBorderInDarkTheme(SocialButtonProvider provider)
    {
        var button = new SocialButton
        {
            Provider = provider,
            Variant = SocialButtonVariant.Brand
        };
        var window = ShowInWindow(button, ThemeVariant.Dark);

        try
        {
            Assert.NotEqual(GetSolidColor(button.Background), GetSolidColor(button.BorderBrush));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void BrandDarkSurface_StaysBorderlessInLightTheme()
    {
        var button = new SocialButton
        {
            Provider = SocialButtonProvider.Apple,
            Variant = SocialButtonVariant.Brand
        };
        var window = ShowInWindow(button, ThemeVariant.Light);

        try
        {
            Assert.Equal(GetSolidColor(button.Background), GetSolidColor(button.BorderBrush));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTheory]
    [InlineData(SocialButtonProvider.Facebook)]
    [InlineData(SocialButtonProvider.Microsoft)]
    [InlineData(SocialButtonProvider.Dribbble)]
    public void BrandBrightSurface_StaysBorderlessInDarkTheme(SocialButtonProvider provider)
    {
        var button = new SocialButton
        {
            Provider = provider,
            Variant = SocialButtonVariant.Brand
        };
        var window = ShowInWindow(button, ThemeVariant.Dark);

        try
        {
            Assert.Equal(GetSolidColor(button.Background), GetSolidColor(button.BorderBrush));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Provider_IsCoerced_WhenInvalid()
    {
        var button = new SocialButton();

        button.SetValue(SocialButton.ProviderProperty, (SocialButtonProvider)999);

        Assert.Equal(SocialButtonProvider.Google, button.Provider);
    }

    [AvaloniaFact]
    public void Variant_IsCoerced_WhenInvalid()
    {
        var button = new SocialButton();

        button.SetValue(SocialButton.VariantProperty, (SocialButtonVariant)999);

        Assert.Equal(SocialButtonVariant.Outline, button.Variant);
    }

    [AvaloniaFact]
    public void Size_IsCoerced_WhenInvalid()
    {
        var button = new SocialButton();

        button.SetValue(SocialButton.SizeProperty, (SocialButtonSize)999);

        Assert.Equal(SocialButtonSize.Medium, button.Size);
    }

    [AvaloniaFact]
    public void Action_IsCoerced_WhenInvalid()
    {
        var button = new SocialButton();

        button.SetValue(SocialButton.ActionProperty, (SocialButtonAction)999);

        Assert.Equal(SocialButtonAction.Continue, button.Action);
    }

    [AvaloniaFact]
    public void Variant_SetsPseudoClass()
    {
        var button = new SocialButton
        {
            Variant = SocialButtonVariant.Brand
        };

        Assert.Contains(":brand", button.Classes);
        Assert.DoesNotContain(":outline", button.Classes);
    }

    [AvaloniaFact]
    public void Size_SetsPseudoClass()
    {
        var button = new SocialButton
        {
            Size = SocialButtonSize.Large
        };

        Assert.Contains(":large", button.Classes);
        Assert.DoesNotContain(":medium", button.Classes);
    }

    [AvaloniaFact]
    public void IsIconOnly_SetsPseudoClass()
    {
        var button = new SocialButton
        {
            IsIconOnly = true
        };

        Assert.Contains(":icononly", button.Classes);
    }

    [AvaloniaFact]
    public void AutomationPeer_Returns_Button_ControlType()
    {
        var button = new SocialButton();
        var peer = new SocialButtonAutomationPeer(button);

        Assert.Equal(AutomationControlType.Button, peer.GetAutomationControlType());
    }

    [AvaloniaFact]
    public void AutomationPeer_Returns_ClassName()
    {
        var button = new SocialButton();
        var peer = new SocialButtonAutomationPeer(button);

        Assert.Equal("SocialButton", peer.GetClassName());
    }

    [AvaloniaFact]
    public void AutomationPeer_UsesDisplayTextFallbackName()
    {
        var button = new SocialButton
        {
            Provider = SocialButtonProvider.Facebook,
            Action = SocialButtonAction.SignIn
        };
        var peer = new SocialButtonAutomationPeer(button);

        Assert.Equal("Sign in with Facebook", peer.GetName());
    }

    [AvaloniaFact]
    public void AutomationPeer_RespectsExplicitName()
    {
        var button = new SocialButton();
        AutomationProperties.SetName(button, "External sign-in");
        var peer = new SocialButtonAutomationPeer(button);

        Assert.Equal("External sign-in", peer.GetName());
    }

    [AvaloniaFact]
    public void AutomationPeer_Invoke_RaisesClick()
    {
        var button = new SocialButton();
        var peer = new SocialButtonAutomationPeer(button);
        var clicked = false;
        button.Click += (_, _) => clicked = true;

        peer.GetProvider<IInvokeProvider>()?.Invoke();

        Assert.True(clicked);
    }

    [AvaloniaFact]
    public void Command_IsExecuted_WhenInvoked()
    {
        var command = new TestCommand();
        var button = new SocialButton
        {
            Command = command,
            CommandParameter = "google"
        };
        var peer = new SocialButtonAutomationPeer(button);

        peer.GetProvider<IInvokeProvider>()?.Invoke();

        Assert.Equal("google", command.Parameter);
        Assert.Equal(1, command.ExecuteCount);
    }

    [AvaloniaFact]
    public void Template_RendersTextAndIcon()
    {
        var button = new SocialButton
        {
            Provider = SocialButtonProvider.Google,
            Action = SocialButtonAction.SignIn
        };
        var window = ShowInWindow(button);

        try
        {
            var icon = GetTemplateChild<ContentControl>(button, "PART_Icon");
            var text = GetTemplateChild<TextBlock>(button, "PART_Text");

            Assert.NotNull(icon.Content);
            Assert.Equal("Sign in with Google", text.Text);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Template_UsesCustomIcon()
    {
        var customIcon = new TextBlock { Text = "C" };
        var button = new SocialButton
        {
            Icon = customIcon
        };
        var window = ShowInWindow(button);

        try
        {
            var icon = GetTemplateChild<ContentControl>(button, "PART_Icon");

            Assert.Same(customIcon, icon.Content);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Template_MarksProviderIconAsRawAutomation()
    {
        var button = new SocialButton();
        var window = ShowInWindow(button);

        try
        {
            var icon = GetTemplateChild<ContentControl>(button, "PART_Icon");

            Assert.Equal(AccessibilityView.Raw, AutomationProperties.GetAccessibilityView(icon));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Template_HasHiddenFocusRingByDefault()
    {
        var button = new SocialButton();
        var window = ShowInWindow(button);

        try
        {
            var focusRing = GetTemplateChild<Border>(button, "PART_FocusRing");

            Assert.False(focusRing.IsVisible);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void IconOnly_Template_HidesTextAndRemovesColumnSpacing()
    {
        var button = new SocialButton
        {
            IsIconOnly = true
        };
        var window = ShowInWindow(button);

        try
        {
            var grid = GetTemplateChild<Grid>(button, "PART_ContentGrid");
            var text = GetTemplateChild<TextBlock>(button, "PART_Text");

            Assert.False(text.IsVisible);
            Assert.Equal(0, grid.ColumnSpacing);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void CustomProviderWithoutIcon_SetsNoIconPseudoClass()
    {
        var button = new SocialButton
        {
            Provider = SocialButtonProvider.Custom,
            Text = "Continue"
        };

        Assert.Null(button.ResolvedIcon);
        Assert.Contains(":noicon", button.Classes);
    }

    [AvaloniaFact]
    public void CustomProviderWithoutIcon_CollapsesIconColumnSpacing()
    {
        var button = new SocialButton
        {
            Provider = SocialButtonProvider.Custom,
            Text = "Continue"
        };
        var window = ShowInWindow(button);

        try
        {
            var grid = GetTemplateChild<Grid>(button, "PART_ContentGrid");
            var icon = GetTemplateChild<ContentControl>(button, "PART_Icon");

            Assert.False(icon.IsVisible);
            Assert.Equal(0, grid.ColumnSpacing);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void SettingIcon_ClearsNoIconPseudoClass()
    {
        var button = new SocialButton
        {
            Provider = SocialButtonProvider.Custom
        };

        Assert.Contains(":noicon", button.Classes);

        button.Icon = new TextBlock { Text = "C" };

        Assert.DoesNotContain(":noicon", button.Classes);
    }

    [AvaloniaFact]
    public void ChangingText_DoesNotRebuildIcon()
    {
        var button = new SocialButton
        {
            Provider = SocialButtonProvider.Google
        };
        var icon = button.ResolvedIcon;

        button.Text = "Use Google";

        Assert.Same(icon, button.ResolvedIcon);
    }

    [AvaloniaFact]
    public void ChangingAction_DoesNotRebuildIcon()
    {
        var button = new SocialButton
        {
            Provider = SocialButtonProvider.Google
        };
        var icon = button.ResolvedIcon;

        button.Action = SocialButtonAction.SignIn;

        Assert.Same(icon, button.ResolvedIcon);
    }

    [AvaloniaFact]
    public void IconOnly_WithoutResolvedIcon_KeepsTextVisible()
    {
        var button = new SocialButton
        {
            Provider = SocialButtonProvider.Custom,
            Text = "Continue with Contoso",
            IsIconOnly = true
        };
        var window = ShowInWindow(button);

        try
        {
            var icon = GetTemplateChild<ContentControl>(button, "PART_Icon");
            var text = GetTemplateChild<TextBlock>(button, "PART_Text");

            Assert.Null(button.ResolvedIcon);
            Assert.False(icon.IsVisible);
            Assert.True(text.IsVisible);
        }
        finally
        {
            window.Close();
        }
    }

    private static Window ShowInWindow(SocialButton button, ThemeVariant? themeVariant = null)
    {
        var window = new Window
        {
            Width = 360,
            Height = 120,
            Content = button
        };

        if (themeVariant is not null)
        {
            window.RequestedThemeVariant = themeVariant;
        }

        window.Show();
        button.ApplyTemplate();
        button.Measure(new Size(360, 120));
        button.Arrange(new Rect(0, 0, 360, 120));
        return window;
    }

    private static T GetTemplateChild<T>(SocialButton button, string name)
        where T : Control
    {
        return button.GetVisualDescendants()
            .OfType<T>()
            .Single(control => control.Name == name);
    }

    private static Canvas GetIconCanvas(SocialButton button)
    {
        var viewbox = Assert.IsType<Viewbox>(button.ResolvedIcon);
        return Assert.IsType<Canvas>(viewbox.Child);
    }

    private static Color GetSolidColor(IBrush? brush)
    {
        var solidBrush = Assert.IsAssignableFrom<ISolidColorBrush>(brush);
        return solidBrush.Color;
    }

    private static double GetContrastRatio(Color first, Color second)
    {
        var firstLuminance = GetRelativeLuminance(first);
        var secondLuminance = GetRelativeLuminance(second);
        var lighter = Math.Max(firstLuminance, secondLuminance);
        var darker = Math.Min(firstLuminance, secondLuminance);

        return (lighter + 0.05) / (darker + 0.05);
    }

    private static double GetRelativeLuminance(Color color)
    {
        var red = GetLinearChannel(color.R / 255.0);
        var green = GetLinearChannel(color.G / 255.0);
        var blue = GetLinearChannel(color.B / 255.0);

        return (0.2126 * red) + (0.7152 * green) + (0.0722 * blue);
    }

    private static double GetLinearChannel(double channel) =>
        channel <= 0.03928
            ? channel / 12.92
            : Math.Pow((channel + 0.055) / 1.055, 2.4);

    private sealed class TestCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public int ExecuteCount { get; private set; }

        public object? Parameter { get; private set; }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            ExecuteCount++;
            Parameter = parameter;
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
