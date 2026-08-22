using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Avalonia.Themes.Simple;

namespace Nova.Avalonia.UI.Gallery.Views;

public partial class MainView : UserControl
{
    private const double SidebarBreakpoint = 880;
    private bool _settingsReady;

    public MainView()
    {
        InitializeComponent();

        var variant = Application.Current?.RequestedThemeVariant;
        LightChip.IsChecked = variant == ThemeVariant.Light;
        DarkChip.IsChecked = variant == ThemeVariant.Dark;
        SystemChip.IsChecked = variant != ThemeVariant.Light && variant != ThemeVariant.Dark;

        var isSimple = Application.Current?.Styles.FirstOrDefault() is SimpleTheme;
        SimpleChip.IsChecked = isSimple;
        FluentChip.IsChecked = !isSimple;
        _settingsReady = true;

        SizeChanged += (_, e) => UpdateShellLayout(e.NewSize.Width);
    }

    private void UpdateShellLayout(double width)
    {
        var isWide = width >= SidebarBreakpoint;
        Shell.DisplayMode = isWide ? SplitViewDisplayMode.Inline : SplitViewDisplayMode.Overlay;
        Shell.IsPaneOpen = isWide;
        MenuButton.IsVisible = !isWide;
        PageHost.Margin = isWide ? default : new Thickness(0, 46, 0, 0);
    }

    private void OnToggleMenu(object? sender, RoutedEventArgs e)
    {
        Shell.IsPaneOpen = !Shell.IsPaneOpen;
    }

    private void OnNavigationPicked(object? sender, RoutedEventArgs e)
    {
        CloseOverlayPane();
    }

    private void OnNavigationSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is ListBox { SelectedItem: not null })
        {
            CloseOverlayPane();
        }
    }

    private void CloseOverlayPane()
    {
        if (Shell.DisplayMode == SplitViewDisplayMode.Overlay)
        {
            Shell.IsPaneOpen = false;
        }
    }

    private void OnVariantChanged(object? sender, RoutedEventArgs e)
    {
        if (!_settingsReady || Application.Current is not { } application ||
            sender is not RadioButton { IsChecked: true, Tag: string variant })
        {
            return;
        }

        application.RequestedThemeVariant = variant switch
        {
            "Light" => ThemeVariant.Light,
            "Dark" => ThemeVariant.Dark,
            _ => ThemeVariant.Default,
        };
    }

    private void OnBaseThemeChanged(object? sender, RoutedEventArgs e)
    {
        if (!_settingsReady || sender is not RadioButton { IsChecked: true, Tag: string baseTheme } ||
            !App.SetBaseTheme(useSimple: baseTheme == "Simple"))
        {
            return;
        }

        var replacement = new MainView { DataContext = DataContext };
        switch (Application.Current?.ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime { MainWindow: { } window }:
                window.Content = replacement;
                break;
            case ISingleViewApplicationLifetime singleView:
                singleView.MainView = replacement;
                break;
        }
    }
}
