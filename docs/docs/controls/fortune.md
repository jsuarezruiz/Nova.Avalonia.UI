---
title: Fortune
description: Interactive spin-to-win controls for games and random selection.
ms.date: 2025-12-26
---

# Fortune

The Fortune controls provide interactive spin-to-win functionality for games, raffles, and random selection scenarios. The library includes two main controls: `FortuneWheel` for circular prize wheels and `FortuneBar` for slot machine style scrolling bars.

## Create a FortuneWheel

Declare a `FortuneWheel` and populate its `Items` collection with `FortuneItem` objects.

```xaml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:nova="clr-namespace:Nova.Avalonia.UI.Controls;assembly=Nova.Avalonia.UI">

    <nova:FortuneWheel x:Name="PrizeWheel" Width="300" Height="300">
        <nova:FortuneWheel.Items>
            <nova:FortuneItem Content="$100" />
            <nova:FortuneItem Content="$50" />
            <!-- You can also use Images -->
            <nova:FortuneItem>
                <nova:FortuneItem.Content>
                    <Image Source="/Assets/jackpot.png" />
                </nova:FortuneItem.Content>
            </nova:FortuneItem>
            <nova:FortuneItem Content="Try Again" />
        </nova:FortuneWheel.Items>
    </nova:FortuneWheel>
</UserControl>
```

## Spin the wheel

Call `SpinAsync()` to spin to a random item, or `SpinToAsync(index)` to spin to a specific item.

```csharp
private async void OnSpinClick(object sender, RoutedEventArgs e)
{
    await PrizeWheel.SpinAsync();
}
```

## Handle spin events

Subscribe to `SpinStarted` and `SpinCompleted` to respond to spin activities.

```xaml
<nova:FortuneWheel SpinStarted="OnSpinStarted" 
                   SpinCompleted="OnSpinCompleted" />
```

```csharp
private void OnSpinCompleted(object sender, FortuneSelectionEventArgs e)
{
    var result = $"You won: {e.SelectedItem.Content}";
    ResultText.Text = result;
}
```

## Style strategies

Control the visual appearance of wheel slices using style strategies.

### Alternating colors

```xaml
<nova:FortuneWheel>
    <nova:FortuneWheel.StyleStrategy>
        <nova:AlternatingStyleStrategy 
            PrimaryBackground="#1E88E5"
            SecondaryBackground="#42A5F5" />
    </nova:FortuneWheel.StyleStrategy>
</nova:FortuneWheel>
```

### Gradient colors

```xaml
<nova:FortuneWheel>
    <nova:FortuneWheel.StyleStrategy>
        <nova:GradientStyleStrategy 
            StartColor="#E91E63"
            EndColor="#9C27B0" />
    </nova:FortuneWheel.StyleStrategy>
</nova:FortuneWheel>
```

## Customize the indicator

Position the indicator at any edge using `IndicatorPosition`. Customize its appearance with `IndicatorFill` and `IndicatorSize`.

```xaml
<nova:FortuneWheel IndicatorPosition="Right"
                   IndicatorFill="#4CAF50"
                   IndicatorSize="30" />
```

Set `ShowIndicator="False"` to hide the indicator entirely.

## Customize the center

Adjust the center circle with `CenterRadius` and `CenterFill`.

```xaml
<nova:FortuneWheel CenterRadius="25"
                   CenterFill="#FFD700" />
```

## Weighted selection

Assign weights to items to control selection probability. Items with higher weights are more likely to be selected.

```xaml
<nova:FortuneWheel.Items>
    <nova:FortuneItem Content="Common" Weight="5" />
    <nova:FortuneItem Content="Rare" Weight="2" />
    <nova:FortuneItem Content="Jackpot!" Weight="0.5" />
</nova:FortuneWheel.Items>
```

## FortuneBar

Use `FortuneBar` for a slot machine style scrolling experience.

```xaml
<nova:FortuneBar x:Name="SlotBar" 
                 Width="350" 
                 Height="80"
                 ItemSize="100">
    <nova:FortuneBar.Items>
        <nova:FortuneItem Content="Cherry" />
        <nova:FortuneItem Content="Lemon" />
        <nova:FortuneItem Content="Orange" />
        <nova:FortuneItem Content="Seven" />
    </nova:FortuneBar.Items>
</nova:FortuneBar>
```

### Vertical orientation

```xaml
<nova:FortuneBar Orientation="Vertical"
                 Width="100"
                 Height="200"
                 ItemSize="60" />
```

## Animation settings

Control the spin animation with `AnimationDuration` and `MinimumSpins` (for wheel) or `MinimumCycles` (for bar).

```xaml
<nova:FortuneWheel AnimationDuration="0:0:5"
                   MinimumSpins="5" />

<nova:FortuneBar AnimationDuration="0:0:3"
                 MinimumCycles="3" />
```

## Properties reference

### FortuneWheel

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Items` | `ObservableCollection<FortuneItem>` | Empty | Items displayed on the wheel |
| `SelectedIndex` | `int` | 0 | Index of the selected item (two-way) |
| `StyleStrategy` | `IStyleStrategy` | `AlternatingStyleStrategy` | Strategy for styling items |
| `AnimationDuration` | `TimeSpan` | 3 seconds | Duration of spin animation |
| `MinimumSpins` | `int` | 3 | Minimum full rotations |
| `ShowIndicator` | `bool` | true | Whether to show the indicator |
| `IndicatorPosition` | `IndicatorPosition` | Top | Position of the indicator |
| `IndicatorFill` | `IBrush` | Red | Indicator color |
| `IndicatorSize` | `double` | 24 | Size of the indicator |
| `CenterRadius` | `double` | 30 | Radius of center circle |
| `CenterFill` | `IBrush` | White | Center circle color |
| `IsSpinning` | `bool` | false | Read-only spinning state |

### FortuneBar

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Items` | `ObservableCollection<FortuneItem>` | Empty | Items displayed on the bar |
| `SelectedIndex` | `int` | 0 | Index of the selected item (two-way) |
| `Orientation` | `Orientation` | Horizontal | Bar orientation |
| `ItemSize` | `double` | 100 | Size of each item |
| `StyleStrategy` | `IStyleStrategy` | `AlternatingStyleStrategy` | Strategy for styling items |
| `AnimationDuration` | `TimeSpan` | 3 seconds | Duration of spin animation |
| `MinimumCycles` | `int` | 2 | Minimum full scroll cycles |
| `ShowIndicator` | `bool` | true | Whether to show the indicator |
| `IndicatorFill` | `IBrush` | Red | Indicator color |
| `IndicatorThickness` | `double` | 4 | Thickness of indicator line |
| `IsSpinning` | `bool` | false | Read-only spinning state |

## Events

| Event | Description |
|-------|-------------|
| `SpinStarted` | Fired when a spin animation begins |
| `SpinCompleted` | Fired when a spin animation completes with the selected item |
