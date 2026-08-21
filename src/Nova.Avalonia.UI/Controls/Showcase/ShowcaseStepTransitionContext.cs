using System.Threading;
using System.Threading.Tasks;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Provides context for asynchronous showcase step hooks.
/// </summary>
public sealed class ShowcaseStepTransitionContext
{
    /// <summary>
    /// Creates a new transition context.
    /// </summary>
    public ShowcaseStepTransitionContext(
        Showcase showcase,
        ShowcaseStep? previousStep,
        ShowcaseStep nextStep,
        int? previousIndex,
        int nextIndex,
        ShowcaseStepTransitionReason reason)
    {
        Showcase = showcase;
        PreviousStep = previousStep;
        NextStep = nextStep;
        PreviousIndex = previousIndex;
        NextIndex = nextIndex;
        Reason = reason;
    }

    /// <summary>
    /// Gets the showcase performing the transition.
    /// </summary>
    public Showcase Showcase { get; }

    /// <summary>
    /// Gets the previous step, if any.
    /// </summary>
    public ShowcaseStep? PreviousStep { get; }

    /// <summary>
    /// Gets the next step that will become active.
    /// </summary>
    public ShowcaseStep NextStep { get; }

    /// <summary>
    /// Gets the previous step index, if any.
    /// </summary>
    public int? PreviousIndex { get; }

    /// <summary>
    /// Gets the index of the next active step.
    /// </summary>
    public int NextIndex { get; }

    /// <summary>
    /// Gets the reason for the transition.
    /// </summary>
    public ShowcaseStepTransitionReason Reason { get; }
}
