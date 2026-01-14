# AutoLayout

`AutoLayout` is a specialized panel that replicates the behavior of Figma's Auto Layout feature. It provides an intuitive way to stack items, control spacing, and manage alignment without the complexity of a Grid.

## Basic Usage

The API is designed to be familiar to designers and developers used to Figma.

```xml
<nova:AutoLayout Orientation="Horizontal" Spacing="10" Padding="20">
    <Button Content="Item 1"/>
    <Button Content="Item 2"/>
</nova:AutoLayout>
```

## Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Orientation` | `Orientation` | `Vertical` | Direction to stack items (`Horizontal` or `Vertical`). |
| `Spacing` | `double` | `0` | Uniform distance between items. |
| `Padding` | `Thickness` | `0` | Padding around the content. |
| `Justification` | `AutoLayoutJustify` | `Packed` | Controls distribution (`Packed` vs `SpaceBetween`). |
| `HorizontalContentAlignment` | `HorizontalAlignment` | `Left` | Aligns the content horizontally within the panel. |
| `VerticalContentAlignment` | `VerticalAlignment` | `Top` | Aligns the content vertically within the panel. |
| `IsReverseZIndex` | `bool` | `false` | If true, reverses the visual stacking order (last item on bottom). |

## Alignment & Distribution

### Alignment Matrix
Use `HorizontalContentAlignment` and `VerticalContentAlignment` to align the entire cluster of items within the panel. This eliminates the need to set alignment on individual children (unless you want to override it).

```xml
<!-- Centers items in the middle of the panel -->
<nova:AutoLayout HorizontalContentAlignment="Center" VerticalContentAlignment="Center">
   ...
</nova:AutoLayout>
```

### Justification
- **Packed (Default)**: Items are bundled together separated by `Spacing`. They adhere to the ContentAlignment properties.
- **SpaceBetween**: Items are distributed evenly to fill the available space. The `Spacing` property is ignored (spacing is calculated dynamically).

```xml
<nova:AutoLayout Justification="SpaceBetween">
   <TextBlock Text="Left"/>
   <TextBlock Text="Right"/>
</nova:AutoLayout>
```

## Absolute Positioning
You can exclude items from the auto-layout flow (e.g., for notification badges or overlays) using the attached property `AutoLayout.IsAbsolute="True"`.

```xml
<nova:AutoLayout>
    <Button Content="Message"/>
    
    <!-- Floats on top at top-right corner -->
    <Border nova:AutoLayout.IsAbsolute="True" 
            HorizontalAlignment="Right" VerticalAlignment="Top">
        <TextBlock Text="1"/>
    </Border>
</nova:AutoLayout>
```

## Figma Mapping

| Figma Concept | AutoLayout Property |
|---------------|---------------------|
| **Direction** (Arrow) | `Orientation` |
| **Resizing**: Hug Contents | `HorizontalAlignment="Left/Center/Right"` (on Container) |
| **Resizing**: Fill Container | `HorizontalAlignment="Stretch"` (on Container) |
| **Spacing Mode**: Packed | `Justification="Packed"` |
| **Spacing Mode**: Auto | `Justification="SpaceBetween"` |
| **Absolute Position** | `AutoLayout.IsAbsolute="True"` |
