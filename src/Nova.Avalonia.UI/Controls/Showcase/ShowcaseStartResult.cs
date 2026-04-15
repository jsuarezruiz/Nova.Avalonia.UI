using System;
using System.Linq;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Represents the outcome of a validated showcase start attempt.
/// </summary>
public sealed class ShowcaseStartResult
{
    /// <summary>
    /// Creates a new start result.
    /// </summary>
    public ShowcaseStartResult(bool started, ShowcaseValidationResult validationResult)
    {
        Started = started;
        ValidationResult = validationResult;
    }

    /// <summary>
    /// Gets whether the showcase was started.
    /// </summary>
    public bool Started { get; }

    /// <summary>
    /// Gets the validation result produced for the start attempt.
    /// </summary>
    public ShowcaseValidationResult ValidationResult { get; }

    /// <summary>
    /// Throws an exception if the showcase did not start because validation failed.
    /// </summary>
    public void EnsureStarted()
    {
        if (Started)
        {
            return;
        }

        var message = ValidationResult.Issues.Count == 0
            ? "The showcase could not be started."
            : string.Join(Environment.NewLine, ValidationResult.Issues.Select(x => x.Message));

        throw new InvalidOperationException(message);
    }

    /// <inheritdoc />
    public override string ToString() =>
        Started ? "Started" : $"Not started ({ValidationResult})";
}
