---
title: Gravatar
description: A control that displays a GitHub-style identicon avatar from an identifier.
ms.date: 2026-01-02
---

# Gravatar

The `Gravatar` control generates a unique, consistent identicon avatar based on an input string (like an email or username). It creates 5x5 pixel symmetric patterns similar to GitHub's default avatars, with colors derived from the input's hash.

## Basic Usage

```xaml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:nova="using:Nova.Avalonia.UI.Controls">

    <!-- Generate from email -->
    <nova:Gravatar Id="user@example.com" />
    
    <!-- Generate from username -->
    <nova:Gravatar Id="username123" />
    
</UserControl>
```

## Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Id` | `string` | `null` | The identifier (email, username) to generate the avatar from. |
| `Source` | `IImage` | `null` | A custom image source. If set, this overrides the generated avatar. |
| `Size` | `double` | `48` | The width and height of the avatar (circular). |
| `Generator` | `IGravatarGenerator` | `GithubGravatarGenerator` | The generator logic to use. |

## Sizing

The `Size` property controls both the width, height, and corner radius (to keep it circular).

```xaml
<!-- Small -->
<nova:Gravatar Id="user@example.com" Size="24" />

<!-- Large -->
<nova:Gravatar Id="user@example.com" Size="96" />
```

## Custom Image Fallback

You can use the `Source` property to display a specific image. This is useful if you want to show a user's uploaded profile picture if available, and fallback to `Id` generation otherwise (though currently `Source` takes precedence if both are set).

```xaml
<nova:Gravatar Id="fallback@user.com" Source="{Binding ProfilePicture}" />
```

## Custom Generator

Functionality can be customized by implementing `IGravatarGenerator`.

```csharp
public class MyCustomGenerator : IGravatarGenerator
{
    public object GenerateAvatar(string id)
    {
        // Return any Avalonia control or geometry
        return new TextBlock { Text = id[0].ToString() };
    }
}
```

```xaml
<nova:Gravatar Id="user@example.com">
    <nova:Gravatar.Generator>
        <local:MyCustomGenerator />
    </nova:Gravatar.Generator>
</nova:Gravatar>
```
