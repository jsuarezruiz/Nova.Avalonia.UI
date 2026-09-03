---
title: ResponsivePanel
description: Switch between narrow, normal, and wide layouts based on the available width.
ms.date: 2026-09-02
---

# ResponsivePanel

`ResponsivePanel` is an adaptive layout panel that selectively displays its children based on the available width and specified breakpoints. It enables simplified "Mobile vs Desktop" layout switching directly in XAML.

## Basic Usage

The control works by attaching a `Condition` to its children. Only children matching the current size class are visible; others are hidden (collapsed) and do not participate in layout.

```xml
<nova:ResponsivePanel>
    
    <!-- Visible on Mobile (< 600px) -->
    <StackPanel nova:ResponsivePanel.Condition="Narrow">
        <TextBlock Text="Mobile View"/>
    </StackPanel>

    <!-- Visible on Tablet/Desktop (>= 600px) -->
    <Grid nova:ResponsivePanel.Condition="Normal, Wide">
        <TextBlock Text="Desktop View"/>
    </Grid>

</nova:ResponsivePanel>
```

## Breakpoints

You can customize the breakpoints using the `NarrowBreakpoint` and `WideBreakpoint` properties on the panel.

| Property | Default | Description |
|----------|---------|-------------|
| `NarrowBreakpoint` | `600` | Width below this is considered `Narrow`. |
| `WideBreakpoint` | `900` | Width above this is considered `Wide`. Width between Narrow and Wide is `Normal`. |

## Conditions

The `ResponsivePanel.Condition` attached property accepts a flag enum `ResponsiveBreakpoint`:

- `Narrow`
- `Normal`
- `Wide`
- `All` (Default)

Use comma-separated values to match more than one breakpoint in XAML:

```xml
<Border nova:ResponsivePanel.Condition="Narrow, Normal" ... />
```

In code, combine values with the bitwise OR operator:

```csharp
ResponsivePanel.SetCondition(
    content,
    ResponsiveBreakpoint.Narrow | ResponsiveBreakpoint.Normal);
```

## Lazy Layout & Performance

`ResponsivePanel` uses a **Lazy Layout** strategy:
- Hidden views have `IsVisible` set to `false`.
- They cost **zero** layout / render time.
- They **retain state** (e.g., text in a TextBox is preserved when switching views).
- They are **eagerly loaded** (created in memory), offering a simpler syntax than DataTemplates.
