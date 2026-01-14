using System;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Defines the size classes (breakpoints) for responsive layouts.
/// </summary>
[Flags]
public enum ResponsiveBreakpoint
{
    /// <summary>
    /// No breakpoint specified.
    /// </summary>
    None = 0,

    /// <summary>
    /// Narrow layout (e.g., mobile devices).
    /// </summary>
    Narrow = 1,

    /// <summary>
    /// Normal layout (e.g., tablets).
    /// </summary>
    Normal = 2,

    /// <summary>
    /// Wide layout (e.g., desktop monitors).
    /// </summary>
    Wide = 4,

    /// <summary>
    /// Matches all breakpoints.
    /// </summary>
    All = Narrow | Normal | Wide
}