using System;
using Avalonia;
using Avalonia.Headless;
using Avalonia.Markup.Xaml.Styling;

[assembly: AvaloniaTestApplication(typeof(Nova.Avalonia.UI.Tests.TestApp))]

namespace Nova.Avalonia.UI.Tests;

public class TestApp : Application
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<TestApp>()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions());

    public override void Initialize()
    {
        Styles.Add(new StyleInclude(new Uri("avares://Nova.Avalonia.UI.Tests"))
        {
            Source = new Uri("avares://Nova.Avalonia.UI/Themes/Controls.axaml")
        });
    }
}
