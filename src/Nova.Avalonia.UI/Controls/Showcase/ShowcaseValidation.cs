using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

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

/// <summary>
/// Identifies a showcase validation issue type.
/// </summary>
public enum ShowcaseValidationIssueCode
{
    /// <summary>
    /// No controller is bound to the showcase.
    /// </summary>
    NoController,

    /// <summary>
    /// The controller does not define any steps.
    /// </summary>
    NoSteps,

    /// <summary>
    /// A step is missing its target key.
    /// </summary>
    EmptyStepKey,

    /// <summary>
    /// A step has no visible body content.
    /// </summary>
    EmptyStepContent,

    /// <summary>
    /// Target resolution could not be evaluated because the showcase is not attached to a visual root.
    /// </summary>
    VisualRootUnavailable,

    /// <summary>
    /// A step key does not resolve to any target control.
    /// </summary>
    MissingTarget,

    /// <summary>
    /// A target key resolves to multiple controls.
    /// </summary>
    DuplicateTargetKey,

    /// <summary>
    /// A target exists but is currently not laid out or visible.
    /// </summary>
    TargetUnavailable
}

/// <summary>
/// Represents a single showcase validation issue.
/// </summary>
public sealed class ShowcaseValidationIssue
{
    /// <summary>
    /// Creates a new issue instance.
    /// </summary>
    public ShowcaseValidationIssue(
        ShowcaseValidationIssueCode code,
        ShowcaseValidationSeverity severity,
        string message,
        int? stepIndex = null,
        string? key = null)
    {
        Code = code;
        Severity = severity;
        Message = message;
        StepIndex = stepIndex;
        Key = key;
    }

    /// <summary>
    /// Gets the issue code.
    /// </summary>
    public ShowcaseValidationIssueCode Code { get; }

    /// <summary>
    /// Gets the issue severity.
    /// </summary>
    public ShowcaseValidationSeverity Severity { get; }

    /// <summary>
    /// Gets the human-readable issue message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the related step index, if any.
    /// </summary>
    public int? StepIndex { get; }

    /// <summary>
    /// Gets the related showcase key, if any.
    /// </summary>
    public string? Key { get; }

    /// <inheritdoc />
    public override string ToString() => $"[{Severity}] {Message}";
}

/// <summary>
/// Contains the results of showcase validation.
/// </summary>
public sealed class ShowcaseValidationResult
{
    /// <summary>
    /// Creates a new validation result.
    /// </summary>
    public ShowcaseValidationResult(IEnumerable<ShowcaseValidationIssue> issues)
    {
        var materialized = issues.ToList();
        Issues = new ReadOnlyCollection<ShowcaseValidationIssue>(materialized);
    }

    /// <summary>
    /// Gets all validation issues.
    /// </summary>
    public IReadOnlyList<ShowcaseValidationIssue> Issues { get; }

    /// <summary>
    /// Gets whether the showcase has no validation errors.
    /// </summary>
    public bool IsValid => !Issues.Any(x => x.Severity == ShowcaseValidationSeverity.Error);

    /// <summary>
    /// Gets whether the showcase has at least one warning.
    /// </summary>
    public bool HasWarnings => Issues.Any(x => x.Severity == ShowcaseValidationSeverity.Warning);

    /// <inheritdoc />
    public override string ToString()
    {
        if (Issues.Count == 0)
        {
            return "Valid";
        }

        var errors = Issues.Count(x => x.Severity == ShowcaseValidationSeverity.Error);
        var warnings = Issues.Count(x => x.Severity == ShowcaseValidationSeverity.Warning);

        return (errors, warnings) switch
        {
            (0, 0) => "Valid",
            (0, _) => $"Valid with {warnings} warning(s)",
            (_, 0) => $"Invalid: {errors} error(s)",
            _ => $"Invalid: {errors} error(s), {warnings} warning(s)"
        };
    }
}
