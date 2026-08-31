using System;
using Avalonia;
using Avalonia.Headless;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Themes.Fluent;

[assembly: AvaloniaTestApplication(typeof(Nova.Avalonia.UI.Tests.TestApp))]

namespace Nova.Avalonia.UI.Tests;

public class TestApp : Application
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<TestApp>()
        .UseSkia()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions
        {
            UseHeadlessDrawing = false
        });

    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
        Styles.Add(new StyleInclude(new Uri("avares://Nova.Avalonia.UI.Tests"))
        {
            Source = new Uri("avares://Nova.Avalonia.UI/Themes/Controls.axaml")
        });
        Styles.Add(new StyleInclude(new Uri("avares://Nova.Avalonia.UI.Tests"))
        {
            Source = new Uri("avares://Nova.Avalonia.UI.CodeViewer/Themes/Controls.axaml")
        });
    }
}
