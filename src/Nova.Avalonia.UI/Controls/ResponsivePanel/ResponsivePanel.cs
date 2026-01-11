using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// A panel that selectively shows its children based on the current width breakpoint.
/// </summary>
public class ResponsivePanel : Panel
{
    /// <summary>
    /// Defines the <see cref="NarrowBreakpoint"/> property.
    /// </summary>
    public static readonly StyledProperty<double> NarrowBreakpointProperty =
        AvaloniaProperty.Register<ResponsivePanel, double>(nameof(NarrowBreakpoint), 600);

    /// <summary>
    /// Defines the <see cref="WideBreakpoint"/> property.
    /// </summary>
    public static readonly StyledProperty<double> WideBreakpointProperty =
        AvaloniaProperty.Register<ResponsivePanel, double>(nameof(WideBreakpoint), 900);

    /// <summary>
    /// Defines the <see cref="Condition"/> attached property.
    /// </summary>
    public static readonly AttachedProperty<ResponsiveBreakpoint> ConditionProperty =
        AvaloniaProperty.RegisterAttached<ResponsivePanel, Control, ResponsiveBreakpoint>("Condition", ResponsiveBreakpoint.All);

    /// <summary>
    /// Defines the <see cref="Transition"/> property.
    /// </summary>
    public static readonly StyledProperty<IPageTransition?> TransitionProperty =
        AvaloniaProperty.Register<ResponsivePanel, IPageTransition?>(nameof(Transition));

    /// <summary>
    /// Gets or sets the transition to use when switching between visible children.
    /// </summary>
    public IPageTransition? Transition
    {
        get => GetValue(TransitionProperty);
        set => SetValue(TransitionProperty, value);
    }

    private ResponsiveBreakpoint _lastBreakpoint = ResponsiveBreakpoint.None;
    private CancellationTokenSource? _transitionCts;

    /// <summary>
    /// Gets or sets the threshold for the narrow layout class.
    /// </summary>
    public double NarrowBreakpoint
    {
        get => GetValue(NarrowBreakpointProperty);
        set => SetValue(NarrowBreakpointProperty, value);
    }

    /// <summary>
    /// Gets or sets the threshold for the wide layout class.
    /// </summary>
    public double WideBreakpoint
    {
        get => GetValue(WideBreakpointProperty);
        set => SetValue(WideBreakpointProperty, value);
    }

    /// <summary>
    /// Gets the <see cref="ConditionProperty"/> for an element.
    /// </summary>
    public static ResponsiveBreakpoint GetCondition(Control element) => element.GetValue(ConditionProperty);

    /// <summary>
    /// Sets the <see cref="ConditionProperty"/> for an element.
    /// </summary>
    public static void SetCondition(Control element, ResponsiveBreakpoint value) => element.SetValue(ConditionProperty, value);

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize)
    {
        var currentBreakpoint = GetCurrentBreakpoint(availableSize.Width);
        
        // If breakpoint changed, handle transition
        if (currentBreakpoint != _lastBreakpoint)
        {
            var isFirstLoad = _lastBreakpoint == ResponsiveBreakpoint.None;
            UpdateVisibility(currentBreakpoint, !isFirstLoad);
            _lastBreakpoint = currentBreakpoint;
        }

        var maxDesiredWidth = 0.0;
        var maxDesiredHeight = 0.0;

        foreach (var child in Children)
        {
            if (child.IsVisible)
            {
                child.Measure(availableSize);
                maxDesiredWidth = Math.Max(maxDesiredWidth, child.DesiredSize.Width);
                maxDesiredHeight = Math.Max(maxDesiredHeight, child.DesiredSize.Height);
            }
        }

        return new Size(maxDesiredWidth, maxDesiredHeight);
    }

    private async void UpdateVisibility(ResponsiveBreakpoint newBreakpoint, bool animate = true)
    {
        var transition = Transition;
        
        // Identify Incoming/Outgoing
        var incoming = new List<Control>();
        var outgoing = new List<Control>();

        foreach (var child in Children)
        {
            var condition = GetCondition(child);
            var matchesNew = condition.HasFlag(newBreakpoint);
            var matchesOld = condition.HasFlag(_lastBreakpoint);

            // If _lastBreakpoint is None (First Load), we consider nothing as "Old Matching" 
            // to avoid confusing logic. Usually matchesOld would be true for everything if None=0.
            if (_lastBreakpoint == ResponsiveBreakpoint.None) matchesOld = false;

            if (matchesNew && !matchesOld) incoming.Add(child);
            else if (!matchesNew && matchesOld) outgoing.Add(child);
            // Else: Persisting or remaining hidden
        }

        // Cancel previous transition
        _transitionCts?.Cancel();
        _transitionCts = new CancellationTokenSource();
        var token = _transitionCts.Token;

        if (animate && transition != null && (incoming.Count > 0 || outgoing.Count > 0))
        {
            // Prepare for animation
            foreach (var view in incoming) view.IsVisible = true;
            // Outgoing are already visible

            try
            {
                // Start transition between outgoing and incoming views
                Control? from = outgoing.Count > 0 ? outgoing[0] : null;
                Control? to = incoming.Count > 0 ? incoming[0] : null;
                
                await transition.Start(from, to, true, token);
            }
            catch (OperationCanceledException)
            {
                // Ignored
            }
            finally
            {
                if (!token.IsCancellationRequested)
                {
                   // Cleanup
                   foreach (var view in outgoing) view.IsVisible = false;
                }
            }
        }
        else
        {
            // Instant Toggle
            foreach (var child in Children)
            {
                var condition = GetCondition(child);
                child.IsVisible = condition.HasFlag(newBreakpoint);
            }
        }
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
        var currentBreakpoint = GetCurrentBreakpoint(finalSize.Width);

        foreach (var child in Children)
        {
            if (child.IsVisible)
            {
                child.Arrange(new Rect(finalSize));
            }
        }

        return finalSize;
    }

    private ResponsiveBreakpoint GetCurrentBreakpoint(double width)
    {
        if (double.IsInfinity(width)) return ResponsiveBreakpoint.Wide; // Fallback for infinite width

        if (width < NarrowBreakpoint) return ResponsiveBreakpoint.Narrow;
        if (width < WideBreakpoint) return ResponsiveBreakpoint.Normal;
        return ResponsiveBreakpoint.Wide;
    }
}
