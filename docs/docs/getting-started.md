---
title: Getting Started
description: Install Nova.Avalonia.UI and add the control styles to your Avalonia app.
ms.date: 2026-08-22
---

# Getting Started

Follow these steps to install Nova.Avalonia.UI, register its styles, and place your first control in a view.

## Prerequisites

- Avalonia 12.1.1 or later in the Avalonia 12 release line
- .NET 8 or later

Android and iOS applications require .NET 10, matching Avalonia 12's platform support.

## Install the NuGet package

From your application project, install the library:

```bash
dotnet add package Nova.Avalonia.UI
```

## Register the control styles

Add the Nova styles to your `Application.Styles` so the controls pick up their templates. Keep your base theme (for example, `FluentTheme`) before the style include.

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="MyApp.App">
    <Application.Styles>
        <FluentTheme />
        <StyleInclude Source="avares://Nova.Avalonia.UI/Themes/Controls.axaml" />
    </Application.Styles>
</Application>
```

## Use the controls in XAML

Declare the namespace for the controls and drop them into your layout. This example shows a shimmer placeholder wrapping content and a simple avatar.

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:nova="clr-namespace:Nova.Avalonia.UI.Controls;assembly=Nova.Avalonia.UI">

    <StackPanel Spacing="16">
        <nova:Shimmer IsLoading="True" LoadingText="Loading profile">
            <StackPanel Spacing="8">
                <TextBlock FontSize="18" Text="Profile" />
                <Border Height="120" CornerRadius="12" Background="#1F1F1F" />
            </StackPanel>
        </nova:Shimmer>

        <nova:Avatar DisplayName="Avery Patel" Status="Online" />
    </StackPanel>
</UserControl>
```

Next, explore the individual control pages to see customization options and platform-specific notes.
