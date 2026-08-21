namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Identifies why a showcase step transition occurred.
/// </summary>
public enum ShowcaseStepTransitionReason
{
    /// <summary>
    /// The showcase started from the beginning.
    /// </summary>
    Start,

    /// <summary>
    /// The showcase resumed from persisted progress.
    /// </summary>
    Resume,

    /// <summary>
    /// The showcase advanced to the next step.
    /// </summary>
    Next,

    /// <summary>
    /// The showcase moved back to the previous step.
    /// </summary>
    Previous
}
