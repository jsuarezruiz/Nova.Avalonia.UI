using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nova.Avalonia.UI.Controls;

namespace Nova.Avalonia.UI.Gallery.ViewModels;

public partial class FortuneViewModel : PageViewModel
{
    public ObservableCollection<FortuneItem> WheelItems { get; } = new();
    public ObservableCollection<FortuneItem> SmallWheelItems { get; } = new();
    public ObservableCollection<FortuneItem> BarItems { get; } = new();
    public ObservableCollection<FortuneItem> ImageBarItems { get; } = new();

    [ObservableProperty]
    private int _wheelSelectedIndex;

    [ObservableProperty]
    private int _barSelectedIndex;

    [ObservableProperty]
    private int _imageBarSelectedIndex;

    [ObservableProperty]
    private string _wheelResult = "Click to spin!";

    [ObservableProperty]
    private string _barResult = "Click to spin!";

    [ObservableProperty]
    private string _imageBarResult = "Click to spin!";

    [ObservableProperty]
    private string _verticalBarResult = "Click to spin!";

    [ObservableProperty]
    private string _eventStarted = "Started: -";

    [ObservableProperty]
    private string _eventCompleted = "Completed: -";

    [ObservableProperty]
    private bool _eventIsSpinning;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSpin))]
    [NotifyCanExecuteChangedFor(nameof(SpinWheelCommand))]
    [NotifyCanExecuteChangedFor(nameof(SpinBarCommand))]
    [NotifyCanExecuteChangedFor(nameof(SpinImageBarCommand))]
    [NotifyCanExecuteChangedFor(nameof(SpinVerticalBarCommand))]
    [NotifyCanExecuteChangedFor(nameof(SpinEventsWheelCommand))]
    private bool _isSpinning;

    public bool CanSpin => !IsSpinning;

    public Func<Task>? WheelSpinProvider { get; set; }
    public Func<Task>? BarSpinProvider { get; set; }
    public Func<Task>? ImageBarSpinProvider { get; set; }
    public Func<Task>? VerticalBarSpinProvider { get; set; }
    public Func<Task>? EventsWheelSpinProvider { get; set; }

    [RelayCommand(CanExecute = nameof(CanSpin))]
    private async Task SpinWheel()
    {
        if (WheelSpinProvider != null)
        {
            try
            {
                IsSpinning = true;
                await WheelSpinProvider();
            }
            finally
            {
                IsSpinning = false;
            }
        }
    }

    [RelayCommand(CanExecute = nameof(CanSpin))]
    private async Task SpinBar()
    {
        if (BarSpinProvider != null)
        {
            try
            {
                IsSpinning = true;
                await BarSpinProvider();
            }
            finally
            {
                IsSpinning = false;
            }
        }
    }

    [RelayCommand(CanExecute = nameof(CanSpin))]
    private async Task SpinVerticalBar()
    {
        if (VerticalBarSpinProvider != null)
        {
            try
            {
                IsSpinning = true;
                await VerticalBarSpinProvider();
            }
            finally
            {
                IsSpinning = false;
            }
        }
    }

    [RelayCommand(CanExecute = nameof(CanSpin))]
    private async Task SpinEventsWheel()
    {
        if (EventsWheelSpinProvider != null)
        {
            try
            {
                IsSpinning = true;
                await EventsWheelSpinProvider();
            }
            finally
            {
                IsSpinning = false;
            }
        }
    }

    [RelayCommand(CanExecute = nameof(CanSpin))]
    private async Task SpinImageBar()
    {
        if (ImageBarSpinProvider != null)
        {
            try
            {
                IsSpinning = true;
                await ImageBarSpinProvider();
            }
            finally
            {
                IsSpinning = false;
            }
        }
    }

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

        // Image slot machine items
        try
        {
            var cherryMsg = LoadBitmap("icon_cherry.png");
            var lemonImg = LoadBitmap("icon_lemon.png");
            var sevenImg = LoadBitmap("icon_seven.png");
            var diamondImg = LoadBitmap("icon_diamond.png");

            ImageBarItems.Add(new FortuneItem(cherryMsg) { Name = "Cherry" });
            ImageBarItems.Add(new FortuneItem(lemonImg) { Name = "Lemon" });
            ImageBarItems.Add(new FortuneItem(sevenImg) { Name = "Seven" });
            ImageBarItems.Add(new FortuneItem(diamondImg) { Name = "Diamond" });
            ImageBarItems.Add(new FortuneItem(cherryMsg) { Name = "Cherry" }); 
            ImageBarItems.Add(new FortuneItem(lemonImg) { Name = "Lemon" });
            ImageBarItems.Add(new FortuneItem(sevenImg) { Name = "Seven" });
        }
        catch
        {
            // Fallback if assets not found
            ImageBarItems.Add(new FortuneItem("Cherry (Img)"));
            ImageBarItems.Add(new FortuneItem("Lemon (Img)"));
            ImageBarItems.Add(new FortuneItem("Seven (Img)"));
            ImageBarItems.Add(new FortuneItem("Diamond (Img)"));
        }
    }

    private static Bitmap LoadBitmap(string name)
    {
        var uri = new Uri($"avares://Nova.Avalonia.UI.Gallery/Assets/{name}");
        return new Bitmap(AssetLoader.Open(uri));
    }
}
