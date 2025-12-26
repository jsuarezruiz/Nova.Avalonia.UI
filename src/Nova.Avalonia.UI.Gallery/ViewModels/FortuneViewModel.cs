using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nova.Avalonia.UI.Controls;

namespace Nova.Avalonia.UI.Gallery.ViewModels;

public partial class FortuneViewModel : PageViewModel
{
    public ObservableCollection<FortuneItem> WheelItems { get; } = new();
    public ObservableCollection<FortuneItem> SmallWheelItems { get; } = new();
    public ObservableCollection<FortuneItem> BarItems { get; } = new();

    [ObservableProperty]
    private int _wheelSelectedIndex;

    [ObservableProperty]
    private int _barSelectedIndex;

    [ObservableProperty]
    private string _lastResult = "Spin to play!";

    [ObservableProperty]
    private bool _isSpinning;

    public FortuneViewModel() : base("Fortune")
    {
        // Prize wheel items (8 items for larger wheels)
        WheelItems.Add(new FortuneItem("$100"));
        WheelItems.Add(new FortuneItem("$50"));
        WheelItems.Add(new FortuneItem("$25"));
        WheelItems.Add(new FortuneItem("$10"));
        WheelItems.Add(new FortuneItem("Retry"));
        WheelItems.Add(new FortuneItem("$5"));
        WheelItems.Add(new FortuneItem("$75"));
        WheelItems.Add(new FortuneItem("Jackpot"));

        // Smaller wheel items (4-5 items for smaller wheels)
        SmallWheelItems.Add(new FortuneItem("Win"));
        SmallWheelItems.Add(new FortuneItem("Lose"));
        SmallWheelItems.Add(new FortuneItem("Retry"));
        SmallWheelItems.Add(new FortuneItem("Bonus"));

        // Slot machine items
        BarItems.Add(new FortuneItem("Cherry"));
        BarItems.Add(new FortuneItem("Lemon"));
        BarItems.Add(new FortuneItem("Orange"));
        BarItems.Add(new FortuneItem("Plum"));
        BarItems.Add(new FortuneItem("Bell"));
        BarItems.Add(new FortuneItem("Bar"));
        BarItems.Add(new FortuneItem("Seven"));
    }
}
