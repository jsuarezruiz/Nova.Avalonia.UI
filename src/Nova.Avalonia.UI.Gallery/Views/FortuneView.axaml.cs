using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Nova.Avalonia.UI.Controls;
using Nova.Avalonia.UI.Gallery.ViewModels;

namespace Nova.Avalonia.UI.Gallery.Views;

public partial class FortuneView : UserControl
{
    public FortuneView()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is FortuneViewModel vm)
        {
            vm.WheelSpinProvider = async () => await PrizeWheel.SpinAsync();
            vm.BarSpinProvider = async () => await SlotBar.SpinAsync();
            vm.VerticalBarSpinProvider = async () => await VerticalBar.SpinAsync();
            vm.EventsWheelSpinProvider = async () => await EventsWheel.SpinAsync();
        }
    }

    private void OnWheelSpinCompleted(object? sender, FortuneSelectionEventArgs e)
    {
        if (DataContext is FortuneViewModel vm)
        {
            var prize = e.SelectedItem?.Content?.ToString() ?? "Unknown";
            vm.WheelResult = $"You won: {prize}!";
        }
    }

    private void OnBarSpinCompleted(object? sender, FortuneSelectionEventArgs e)
    {
        if (DataContext is FortuneViewModel vm)
        {
            var result = e.SelectedItem?.Content?.ToString() ?? "Unknown";
            vm.BarResult = $"Result: {result}";
        }
    }

    private void OnVerticalBarSpinCompleted(object? sender, FortuneSelectionEventArgs e)
    {
        if (DataContext is FortuneViewModel vm)
        {
            var result = e.SelectedItem?.Content?.ToString() ?? "Unknown";
            vm.VerticalBarResult = $"Result: {result}";
        }
    }

    private void OnEventsWheelSpinStarted(object? sender, FortuneSelectionEventArgs e)
    {
        if (DataContext is FortuneViewModel vm)
        {
            vm.EventStarted = $"Started: #{e.SelectedIndex}";
            vm.EventIsSpinning = true;
        }
    }

    private void OnEventsWheelSpinCompleted(object? sender, FortuneSelectionEventArgs e)
    {
        if (DataContext is FortuneViewModel vm)
        {
            var prize = e.SelectedItem?.Content?.ToString() ?? "Unknown";
            vm.EventCompleted = $"Completed: {prize}";
            vm.EventIsSpinning = false;
        }
    }
}
