using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Nova.Avalonia.UI.Controls;

namespace Nova.Avalonia.UI.Gallery.Views;

public partial class ShowcaseView : UserControl
{
    private readonly ShowcaseController _controller;

    public ShowcaseView()
    {
        InitializeComponent();

        _controller = CreateController();
        _controller.StepChanged += OnStepChanged;
        _controller.Completed += OnCompleted;
        _controller.Skipped += OnSkipped;
        _controller.TransitionFailed += OnTransitionFailed;

        InteractionModeComboBox.ItemsSource = Enum.GetValues<ShowcaseInteractionMode>();
        InteractionModeComboBox.SelectedItem = ShowcaseInteractionMode.Modal;

        ShowcaseControl.Controller = _controller;
        ShowcaseControl.InteractionMode = GetSelectedInteractionMode();

        StartButton.Click += OnStartButtonClick;
        ValidateButton.Click += OnValidateButtonClick;
        SearchBox.TextChanged += OnSearchBoxTextChanged;
        InteractionModeComboBox.SelectionChanged += OnInteractionModeSelectionChanged;
    }

    private static ShowcaseController CreateController()
    {
        return new ShowcaseController(
        [
            new ShowcaseStep
            {
                Key = "WelcomeTitle",
                Title = "Welcome",
                Description = "This tour highlights the main screen elements. Use the interaction mode selector to change how the overlay blocks input.",
                TooltipPosition = ShowcaseTooltipPosition.Bottom
            },
            new ShowcaseStep
            {
                Key = "ValidationButton",
                Title = "Validate First",
                Description = "Call Validate() before starting to check that all target keys resolve correctly.",
                TooltipPosition = ShowcaseTooltipPosition.Bottom,
                HighlightShape = ShowcaseHighlightShape.Rectangle
            },
            new ShowcaseStep
            {
                Key = "InteractionModeSelector",
                Title = "Interaction Mode",
                Description = "Switch between Modal, TargetOnly, and Passthrough to see how the overlay behaves on each step.",
                TooltipPosition = ShowcaseTooltipPosition.Right,
                HighlightShape = ShowcaseHighlightShape.Circle
            },
            new ShowcaseStep
            {
                Key = "SearchCard",
                Title = "Search Preview",
                Description = "This card shows a live preview of the search text typed below.",
                TooltipPosition = ShowcaseTooltipPosition.Bottom
            },
            new ShowcaseStep
            {
                Key = "SearchBox",
                Title = "Try Typing",
                Description = "Type something here and watch the preview card update above.",
                TooltipPosition = ShowcaseTooltipPosition.Top
            }
        ]);
    }

    private async void OnStartButtonClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            await ShowcaseControl.StartAsync();
            UpdateCurrentStepStatus("Started tour");
        }
        catch (InvalidOperationException ex)
        {
            UpdateStatus($"Unable to start the tour: {ex.Message}");
        }
    }

    private void OnValidateButtonClick(object? sender, RoutedEventArgs e)
    {
        UpdateStatus(FormatValidationResult(ShowcaseControl.Validate()));
    }

    private void OnSearchBoxTextChanged(object? sender, TextChangedEventArgs e)
    {
        SearchPreviewTextBlock.Text = string.IsNullOrWhiteSpace(SearchBox.Text)
            ? "Type in the search box below to see a live preview here."
            : SearchBox.Text;
    }

    private void OnInteractionModeSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        ShowcaseControl.InteractionMode = GetSelectedInteractionMode();

        if (_controller.IsActive)
        {
            UpdateCurrentStepStatus("Updated default interaction mode");
            return;
        }

        UpdateStatus($"Default interaction mode set to {ShowcaseControl.InteractionMode}.");
    }

    private void OnCompleted(object? sender, EventArgs e) => UpdateStatus("Tour completed.");

    private void OnSkipped(object? sender, EventArgs e) => UpdateStatus("Tour skipped.");

    private void OnTransitionFailed(object? sender, ShowcaseTransitionFailedEventArgs args) =>
        UpdateStatus($"Tour transition failed during {args.Action}: {args.Exception.Message}");

    private void OnStepChanged(object? sender, ShowcaseStepChangedEventArgs e)
    {
        UpdateCurrentStepStatus("Now showing");
    }

    private ShowcaseInteractionMode GetSelectedInteractionMode()
    {
        if (InteractionModeComboBox.SelectedItem is ShowcaseInteractionMode mode)
        {
            return mode;
        }

        return ShowcaseInteractionMode.Modal;
    }

    private void UpdateCurrentStepStatus(string prefix)
    {
        if (_controller.CurrentStep == null)
        {
            UpdateStatus(prefix);
            return;
        }

        var effectiveMode = _controller.CurrentStep.InteractionMode ?? ShowcaseControl.InteractionMode;
        UpdateStatus(
            $"{prefix}: step {_controller.CurrentIndex + 1}/{_controller.Steps.Count} " +
            $"\"{_controller.CurrentStep.Title}\" ({effectiveMode}).");
    }

    private static string FormatValidationResult(ShowcaseValidationResult result)
    {
        if (result.IsValid && !result.HasWarnings)
        {
            return "Validation passed. All showcase targets are ready.";
        }

        var issues = result.Issues
            .Select(issue => issue.Message)
            .Take(3)
            .ToArray();

        var summary = string.Join(" ", issues);
        var remaining = result.Issues.Count - issues.Length;
        var suffix = remaining > 0 ? $" (+{remaining} more issues)" : string.Empty;
        var prefix = result.IsValid ? "Validation passed with warnings" : "Validation failed";
        return $"{prefix}: {summary}{suffix}";
    }

    private void UpdateStatus(string message)
    {
        StatusTextBlock.Text = message;
    }
}
