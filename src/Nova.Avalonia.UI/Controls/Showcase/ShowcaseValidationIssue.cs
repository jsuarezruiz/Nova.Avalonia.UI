namespace Nova.Avalonia.UI.Controls;

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
