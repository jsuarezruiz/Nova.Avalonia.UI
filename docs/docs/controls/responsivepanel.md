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
    <Grid nova:ResponsivePanel.Condition="Normal | Wide">
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

You can combine them using utility syntax if supported, or by standard piping in code-behind. In XAML, the parser usually supports comma-separated values for flags in some frameworks, but here you typically specify one. If you need multiple, you can use:

```xml
<!-- Example if flag parsing is supported by XAML compiler for this enum -->
<Border nova:ResponsivePanel.Condition="Narrow, Normal" ... />
```

## Lazy Layout & Performance

`ResponsivePanel` uses a **Lazy Layout** strategy:
- Hidden views have `IsVisible` set to `false`.
- They cost **zero** layout / render time.
- They **retain state** (e.g., text in a TextBox is preserved when switching views).
- They are **eagerly loaded** (created in memory), offering a simpler syntax than DataTemplates.
