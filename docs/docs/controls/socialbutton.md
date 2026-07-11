---
title: SocialButton
description: Provider-branded sign-in buttons for social and identity providers.
ms.date: 2026-06-09
---

# SocialButton

`SocialButton` is a provider-branded button for social and identity sign-in actions. It derives from `Button`, so it supports click events, commands, keyboard activation, disabled state, and button automation behavior.

`SocialButton` is visual only. It does not start an OAuth or sign-in flow by itself.

## Create a social button

```xaml
<controls:SocialButton Provider="Google"
                       Action="Continue"
                       Command="{Binding ContinueWithGoogleCommand}" />
```

## Providers

Built-in providers include `Google`, `Facebook`, `Apple`, `X`, `GitHub`, `Microsoft`, `Figma`, and `Dribbble`. Use `Custom` with `ProviderDisplayName`, `Text`, and `Icon` for other providers.

```xaml
<controls:SocialButton Provider="Custom"
                       ProviderDisplayName="Contoso"
                       Text="Continue with Contoso">
    <controls:SocialButton.Icon>
        <TextBlock Text="C" FontWeight="Bold" />
    </controls:SocialButton.Icon>
</controls:SocialButton>
```

## Variants and sizes

```xaml
<StackPanel Spacing="8">
    <controls:SocialButton Provider="Google" Variant="Outline" />
    <controls:SocialButton Provider="Facebook" Variant="Brand" />
    <controls:SocialButton Provider="Apple" Variant="Gray" />
</StackPanel>
```

`Size` supports `Small`, `Medium`, and `Large`.

`Outline` and `Gray` use theme resources for light and dark mode. Provider-branded `Brand` buttons keep provider colors while still using contrast-safe foregrounds.

## Icon-only buttons

Set `IsIconOnly="True"` to hide the visible text while preserving the generated accessible name. You can still set `AutomationProperties.Name` explicitly.

If no icon can be resolved, such as a `Custom` provider without `Icon`, the control keeps the text visible so the button does not render as an empty hit target.

```xaml
<controls:SocialButton Provider="GitHub"
                       Variant="Brand"
                       IsIconOnly="True"
                       AutomationProperties.Name="Continue with GitHub" />
```

## Localization

Generated text (`Continue with Google`, `Sign in with Apple`, and so on) is English and uses a fixed
`{action} with {provider}` word order. For other languages or word orders, set `Text` explicitly to
supply the full, already-localized label:

```xaml
<controls:SocialButton Provider="Google" Text="Continuer avec Google" />
```

## Accessibility

`SocialButton` exposes `AutomationControlType.Button`. Its accessible name defaults to the generated display text, such as `Continue with Google`, and explicit `AutomationProperties.Name` values are respected.

The built-in provider icon is decorative and is kept out of the control automation view. Icon-only buttons keep the same generated accessible name unless an explicit name is set.

Use `IconForeground` to force a monochrome provider icon. When it is unset, built-in multi-color marks such as Google, Microsoft, and Figma keep their provider colors. The exception is Microsoft `Brand`, which uses a contrasting monochrome glyph because the brand background reuses Microsoft blue.

Neutral monochrome icons such as Apple, X, and GitHub follow the theme foreground so they remain visible in both light and dark themes.

When using provider buttons in production, verify each provider's current brand requirements. Some providers restrict color, icon-only usage, title text, spacing, and relative prominence.

## Content model

`SocialButton` derives from `Button`, but its template is driven by `Text` and `Icon`, not inherited `Content`. Set `Text` for the visible label and `Icon` for custom icon content.

## Properties reference

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Provider` | `SocialButtonProvider` | `Google` | Built-in provider used for generated text and icon |
| `ProviderDisplayName` | `string` | `null` | Provider name override, especially for `Custom` |
| `Variant` | `SocialButtonVariant` | `Outline` | Visual style: `Brand`, `Outline`, or `Gray` |
| `Size` | `SocialButtonSize` | `Medium` | Size preset |
| `Action` | `SocialButtonAction` | `Continue` | Generated action text |
| `Text` | `string` | `null` | Custom visible text |
| `Icon` | `object` | Provider icon | Custom icon content |
| `IconForeground` | `IBrush` | `null` | Optional brush that overrides the built-in provider icon colors |
| `IsIconOnly` | `bool` | `false` | Hides visible text and keeps icon-only layout |
| `DisplayText` | `string` | `Continue with Google` | Read-only generated or custom display text |
| `ResolvedIcon` | `object` | Provider icon | Read-only icon content used by the template |
