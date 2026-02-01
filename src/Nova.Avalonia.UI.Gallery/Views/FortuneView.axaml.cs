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
            vm.ImageBarSpinProvider = async () => await ImageSlotBar.SpinAsync();
            vm.VerticalBarSpinProvider = async () => await VerticalBar.SpinAsync();
            vm.EventsWheelSpinProvider = async () => await EventsWheel.SpinAsync();
        }
    }

    private string GetItemDisplay(FortuneItem? item)
    {
        if (item == null) return "Unknown";
        if (!string.IsNullOrEmpty(item.Name)) return item.Name;
        return item.Content?.ToString() ?? "Unknown";
    }

    private void OnWheelSpinCompleted(object? sender, FortuneSelectionEventArgs e)
    {
        if (DataContext is FortuneViewModel vm)
        {
            var prize = GetItemDisplay(e.SelectedItem);
            vm.WheelResult = $"You won: {prize}!";
        }
    }

    private void OnBarSpinCompleted(object? sender, FortuneSelectionEventArgs e)
    {
        if (DataContext is FortuneViewModel vm)
        {
            var result = GetItemDisplay(e.SelectedItem);
            vm.BarResult = $"Result: {result}";
        }
    }

    private void OnImageBarSpinCompleted(object? sender, FortuneSelectionEventArgs e)
    {
        if (DataContext is FortuneViewModel vm)
        {
            var result = GetItemDisplay(e.SelectedItem);
            vm.ImageBarResult = $"Result: {result}";
        }
    }

    private void OnVerticalBarSpinCompleted(object? sender, FortuneSelectionEventArgs e)
    {
        if (DataContext is FortuneViewModel vm)
        {
            var result = GetItemDisplay(e.SelectedItem);
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
            var prize = GetItemDisplay(e.SelectedItem);
            vm.EventCompleted = $"Completed: {prize}";
            vm.EventIsSpinning = false;
        }
    }
}
