namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Describes the severity of a showcase validation issue.
/// </summary>
public enum ShowcaseValidationSeverity
{
    /// <summary>
    /// The issue is informational and does not block the showcase.
    /// </summary>
    Info,

    /// <summary>
    /// The issue indicates a degraded experience but the showcase may still run.
    /// </summary>
    Warning,

    /// <summary>
    /// The issue prevents the showcase from running correctly.
    /// </summary>
    Error
}
