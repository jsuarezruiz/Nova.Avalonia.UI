---
title: Gallery
description: Explore Nova.Avalonia.UI controls and samples in the WebAssembly gallery.
ms.date: 2026-09-08
---

# Gallery

The Nova.Avalonia.UI gallery lets you try the controls and layout panels directly in your browser.

**[Open the WebAssembly gallery](https://jsuarezruiz.github.io/Nova.Avalonia.UI/gallery/)**

## Run locally

Install the WebAssembly tools once, then run the browser host:

```bash
dotnet workload install wasm-tools
dotnet run --project src/Nova.Avalonia.UI.Gallery.Browser
```

Publishing produces a static site at `src/Nova.Avalonia.UI.Gallery.Browser/bin/Release/net10.0-browser/publish/wwwroot`. The documentation workflow deploys it under `/gallery/` alongside the project documentation whenever changes reach `main`.

Desktop, Android, and iOS gallery hosts are also available under `src/`.
