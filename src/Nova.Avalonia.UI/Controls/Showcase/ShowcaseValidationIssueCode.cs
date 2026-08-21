using System;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Identifies a showcase validation issue type.
/// </summary>
public enum ShowcaseValidationIssueCode
{
    /// <summary>
    /// A legacy showcase did not have a controller assigned.
    /// </summary>
    [Obsolete("Showcase now owns its navigation state and no longer requires a controller.")]
    NoController = 0,

    /// <summary>
    /// The showcase does not define any steps.
    /// </summary>
    NoSteps = 1,

    /// <summary>
    /// A step is missing its target key.
    /// </summary>
    EmptyStepKey = 2,

    /// <summary>
    /// A step has no visible body content.
    /// </summary>
    EmptyStepContent = 3,

    /// <summary>
    /// Target checks were skipped because the showcase has no visual root.
    /// </summary>
    VisualRootUnavailable = 4,

    /// <summary>
    /// A step key does not resolve to any target control.
    /// </summary>
    MissingTarget = 5,

    /// <summary>
    /// A target key resolves to multiple controls.
    /// </summary>
    DuplicateTargetKey = 6,

    /// <summary>
    /// A target exists but is not currently laid out or visible.
    /// </summary>
    TargetUnavailable = 7,

    /// <summary>
    /// Persisted progress cannot uniquely identify a showcase step.
    /// </summary>
    AmbiguousStepIdentity = 8
}
