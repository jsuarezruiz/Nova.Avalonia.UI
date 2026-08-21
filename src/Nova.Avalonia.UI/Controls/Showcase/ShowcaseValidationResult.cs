using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nova.Avalonia.UI.Controls;

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
