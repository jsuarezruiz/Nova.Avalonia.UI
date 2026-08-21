---
title: PinBox
description: A specialized input control for PIN codes, OTP verification, and security codes.
ms.date: 2025-12-27
---

# PinBox

The `PinBox` control provides a specialized input field for PIN codes, OTP verification, and security codes. It features individual character boxes, smooth animations, validation support, and customizable themes.

## Create a PinBox

Add a `PinBox` control to your view. The default length is 6 digits.

```xaml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:controls="using:Nova.Avalonia.UI.Controls">

    <controls:PinBox Length="4" />
</UserControl>
```

## Binding to Text

Bind the `Text` property for two-way data binding with your ViewModel.

```xaml
<controls:PinBox Length="4" Text="{Binding Pin}" />
```

## Password Mode

Set `IsPassword="True"` to obscure entered characters. Customize the mask character with `PasswordChar`.

```xaml
<!-- Default dot mask -->
<controls:PinBox Length="4" IsPassword="True" />

<!-- Custom asterisk mask -->
<controls:PinBox Length="4" IsPassword="True" PasswordChar="*" />
```

## Theme Variants

The control includes several built-in theme presets:

```xaml
<!-- Rounded circles -->
<controls:PinBox Length="4" Classes="rounded" />

<!-- Underline style -->
<controls:PinBox Length="4" Classes="underline" />

<!-- Material Design -->
<controls:PinBox Length="4" Classes="material" />

<!-- iOS style -->
<controls:PinBox Length="6" Classes="ios" />
```

## Validation

Provide a `Validator` function to validate input. The function receives the current text and returns an error message (or null if valid).

```xaml
<controls:PinBox Length="4"
                Text="{Binding Pin}"
                Validator="{Binding ValidatePinFunc}" />
```

```csharp
public Func<string, string?> ValidatePinFunc => ValidatePin;

private string? ValidatePin(string text)
{
    if (text.Length == 4 && text != "1234")
        return "Invalid PIN";
    return null;
}
```

When validation fails, the control displays an error message and applies the `ErrorTheme`.

## Events

Handle the `Completed` event when all digits are entered, and `TextChanged` for real-time updates.

```xaml
<controls:PinBox Length="4"
                Completed="OnPinCompleted"
                TextChanged="OnPinTextChanged" />
```

```csharp
private void OnPinCompleted(object sender, PinBoxCompletedEventArgs e)
{
    string enteredPin = e.Pin;
    // Verify PIN...
}
```

## Programmatic Access

Use the `Text` property and `Clear()` method for programmatic control:

```csharp
// Set value directly
pinBox.Text = "1234";

// Clear all digits
pinBox.Clear();
```

## Customization

### Spacing

Adjust the gap between boxes with the `Spacing` property.

```xaml
<controls:PinBox Length="4" Spacing="16" />
```

### Custom Themes

Provide custom themes for different states:

```xaml
<controls:PinBox Length="4">
    <controls:PinBox.DefaultTheme>
        <controls:PinBoxTheme Width="50" Height="50"
                             Background="#F5F5F5"
                             BorderBrush="#CCCCCC"
                             BorderThickness="1"
                             CornerRadius="8"
                             FontSize="24"
                             Foreground="#333333" />
    </controls:PinBox.DefaultTheme>
    <controls:PinBox.FocusedTheme>
        <controls:PinBoxTheme Width="50" Height="50"
                             Background="White"
                             BorderBrush="#2196F3"
                             BorderThickness="2"
                             CornerRadius="8"
                             FontSize="24"
                             Foreground="#333333" />
    </controls:PinBox.FocusedTheme>
</controls:PinBox>
```

## Properties Reference

| Property | Type | Description |
|----------|------|-------------|
| `Text` | `string` | The current PIN value (two-way bindable). |
| `Length` | `int` | Number of digits (1-12). Default is 6. |
| `DigitsOnly` | `bool` | Filter to digits only. Default is `true`. |
| `IsPassword` | `bool` | Whether to obscure characters. Default is `false`. |
| `PasswordChar` | `char` | Character used to mask input. Default is `●`. |
| `Spacing` | `double` | Gap between boxes in pixels. Default is 8. |
| `ShowCursor` | `bool` | Show blinking cursor. Default is `true`. |
| `CursorBrush` | `IBrush` | Cursor color. |
| `DefaultTheme` | `PinBoxTheme` | Theme for empty boxes. |
| `FocusedTheme` | `PinBoxTheme` | Theme for the focused box. |
| `FilledTheme` | `PinBoxTheme` | Theme for filled boxes. |
| `ErrorTheme` | `PinBoxTheme` | Theme for error state. |
| `Validator` | `Func<string, string?>` | Validation function. |
| `HasError` | `bool` | Whether validation has failed (read-only). |
| `ErrorText` | `string` | Current validation error message (read-only). |

## Events Reference

| Event | Description |
|-------|-------------|
| `Completed` | Raised when all digits are entered. |
| `TextChanged` | Raised when the text value changes. |
