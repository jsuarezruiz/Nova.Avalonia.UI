using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Media;

namespace Nova.Avalonia.UI.Gallery.ViewModels;

public partial class ArcPanelViewModel : PageViewModel
{
    [ObservableProperty]
    private double _radius = 100;

    [ObservableProperty]
    private double _startAngle = -90;

    [ObservableProperty]
    private double _sweepAngle = 180;

    public ObservableCollection<DynamicItem> Items { get; } = new();

    public ArcPanelViewModel() : base("ArcPanel")
    {
        var colors = new[] { "#FF6B6B", "#4ECDC4", "#45B7D1", "#FFA07A", "#98D8C8", "#F7DC6F" };
        for (int i = 0; i < 6; i++)
        {
            Items.Add(new DynamicItem($"{i + 1}", colors[i % colors.Length]));
        }
    }

    [RelayCommand]
    private void AddItem()
    {
        var colors = new[] { "#FF6B6B", "#4ECDC4", "#45B7D1", "#FFA07A", "#98D8C8", "#F7DC6F", "#BB8FCE", "#85C1E9" };
        Items.Add(new DynamicItem($"{Items.Count + 1}", colors[Items.Count % colors.Length]));
    }

    [RelayCommand]
    private void RemoveItem()
    {
        if (Items.Count > 0)
            Items.RemoveAt(Items.Count - 1);
    }
}
