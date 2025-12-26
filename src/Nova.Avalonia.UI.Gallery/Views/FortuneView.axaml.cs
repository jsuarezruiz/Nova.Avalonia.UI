using Avalonia.Controls;
using Avalonia.Interactivity;
using Nova.Avalonia.UI.Controls;

namespace Nova.Avalonia.UI.Gallery.Views;

public partial class FortuneView : UserControl
{
    public FortuneView()
    {
        InitializeComponent();
    }

    private async void OnSpinWheelClick(object? sender, RoutedEventArgs e)
    {
        await PrizeWheel.SpinAsync();
    }

    private void OnWheelSpinCompleted(object? sender, FortuneSelectionEventArgs e)
    {
        var prize = e.SelectedItem?.Content?.ToString() ?? "Unknown";
        WheelResultText.Text = $"You won: {prize}!";
    }

    private async void OnSpinBarClick(object? sender, RoutedEventArgs e)
    {
        await SlotBar.SpinAsync();
    }

    private void OnBarSpinCompleted(object? sender, FortuneSelectionEventArgs e)
    {
        var result = e.SelectedItem?.Content?.ToString() ?? "Unknown";
        BarResultText.Text = $"Result: {result}";
    }

    private async void OnSpinVerticalClick(object? sender, RoutedEventArgs e)
    {
        await VerticalBar.SpinAsync();
    }

    private async void OnSpinEventsClick(object? sender, RoutedEventArgs e)
    {
        await EventsWheel.SpinAsync();
    }

    private void OnEventsWheelSpinStarted(object? sender, FortuneSelectionEventArgs e)
    {
        EventStartedText.Text = $"Started: #{e.SelectedIndex}";
        EventIsSpinningText.Text = "IsSpinning: True";
    }

    private void OnEventsWheelSpinCompleted(object? sender, FortuneSelectionEventArgs e)
    {
        var prize = e.SelectedItem?.Content?.ToString() ?? "Unknown";
        EventCompletedText.Text = $"Completed: {prize}";
        EventIsSpinningText.Text = "IsSpinning: False";
    }
}
