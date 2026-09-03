---
title: Welcome to Nova.Avalonia.UI
description: Overview of the Nova.Avalonia.UI control suite and how to get started.
ms.date: 2026-02-01
---

# Welcome to Nova.Avalonia.UI

**Nova.Avalonia.UI** is a comprehensive suite of controls for [Avalonia UI](https://avaloniaui.net/). The library bridges the gap between standard framework controls and the complex requirements of modern applications, providing components that are production-ready, fully themeable, and accessibility-first.


## Overview

### Controls

- **[Avatar](docs/controls/avatar.md)**: Identity control supporting images, initials, icons, and status indicators with auto-generated backgrounds.
- **[Badge](docs/controls/badge.md)**: Notification indicator for counts, status, or dot indicators with configurable placement and overflow handling.
- **[BarcodeGenerator](docs/controls/barcodegenerator.md)**: Generates QR codes, Data Matrix, Code 128, and other barcode formats with customizable colors and logo support.
- **[CodeViewer](docs/controls/codeviewer.md)**: Displays read-only source code inline or opens several source files from a compact button.
- **[CompareSlider](docs/controls/compareslider.md)**: A control that allows side-by-side comparison of two pieces of content with a draggable divider.
- **[FortuneBar](xref:Nova.Avalonia.UI.Controls.FortuneBar)**: Slot machine style horizontal or vertical scrolling bar for random selection.
- **[FortuneWheel](xref:Nova.Avalonia.UI.Controls.FortuneWheel)**: Circular prize wheel with smooth spin animations, weighted selection, and customizable styling strategies.
- **[Gravatar](docs/controls/gravatar.md)**: GitHub-style identicon generator.
- **[RatingControl](docs/controls/ratingcontrol.md)**: Allows users to view and set ratings using interactive items such as stars, hearts, or custom shapes. It supports multiple precision levels, customizable appearance, and full keyboard and pointer interaction.
- **[Shimmer](docs/controls/shimmer.md)**: Skeleton loading effect that detects your content layout to create matching placeholders.
- **[Scratcher](docs/controls/scratcher.md)**: Interactive control that hides content beneath a scratchable overlay.
- **[Shield](docs/controls/shield.md)**: Badge-like control for displaying status, version, or metadata in a compact two-part format (subject + status).
- **[Watermark](docs/controls/watermark.md)**: Tiled text or image overlay for marking documents as confidential, draft, or adding branding.

### Panels

- **[ArcPanel](docs/controls/arcpanel.md)**: Arranges items along an arc (partial circle) for semi-circular menus or dial interfaces.
- **[AutoLayout](docs/controls/autolayout.md)**: A layout panel inspired by Figma's Auto Layout with Orientation, Spacing, Padding, and Alignment.
- **[AvatarGroup](xref:Nova.Avalonia.UI.Controls.AvatarGroup)**: Layout container for stacking multiple avatars with configurable overlap and overflow handling.
- **[AvatarStackPanel](xref:Nova.Avalonia.UI.Controls.AvatarStackPanel)**: A specialized panel for laying out avatars with overlap.
- **[BubblePanel](docs/controls/bubblepanel.md)**: Packs circular items using a circle packing algorithm for dense, organic layouts.
- **[CircularPanel](docs/controls/circularpanel.md)**: Arranges items evenly around a circle, perfect for radial menus or clock faces.
- **[HexPanel](docs/controls/hexpanel.md)**: Layout panel that arranges items in a honeycomb hexagonal grid.
- **[LoopPanel](docs/controls/looppanel.md)**: Infinite scrolling panel with looping, inertia, and snap-to-item behavior.
- **[OrbitPanel](docs/controls/orbitpanel.md)**: Arranges child elements in concentric orbit rings around a center point.
- **[OverlapPanel](docs/controls/overlappanel.md)**: Stacks children with configurable X/Y offsets for card pile effects.
- **[RadialPanel](docs/controls/radialpanel.md)**: Arranges items in a circular or spiral fan arrangement.
- **[ResponsivePanel](docs/controls/responsivepanel.md)**: Adaptive panel that toggles visibility of children based on breakpoints.
- **[StaggeredPanel](docs/controls/staggeredpanel.md)**: Positions items in a staggered grid, creating a masonry-like effect.
- **[TimelinePanel](docs/controls/timelinepanel.md)**: Arranges items in a timeline/step flow with connecting lines.
- **[VariableSizeWrapPanel](docs/controls/variablesizewrappanel.md)**: Arranges varying-sized items in a wrapping grid.
- **[VirtualizingStaggeredPanel](docs/controls/staggeredpanel.md)**: High-performance staggered grid for large datasets.
- **[VirtualizingVariableSizeWrapPanel](docs/controls/variablesizewrappanel.md)**: High-performance variable-size tile grid for large datasets.

### Input

- **[PinBox](xref:Nova.Avalonia.UI.Controls.PinBox)**: Specialized input control for PIN codes, security codes, and OTP entry with validation, masking, and individual character boxes.
- **[SegmentedSlider](docs/controls/segmentedslider.md)**: Range input divided into named segments with configurable titles, colors, and selection behavior.

## Requirements

- Avalonia UI 12.1.1 or later in the Avalonia 12 release line
- .NET 8 or later for desktop and browser applications
- .NET 10 for Android and iOS applications
