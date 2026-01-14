using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Media;

namespace Nova.Avalonia.UI.Gallery.ViewModels;

public partial class RadialPanelViewModel : PageViewModel
{
    [ObservableProperty] private double _radius = 150;
    [ObservableProperty] private double _startAngle = 0;
    [ObservableProperty] private double _sweepAngle = 360;
    [ObservableProperty] private double _itemAngle = 0;
    [ObservableProperty] private bool _rotateItems = true;

    public ObservableCollection<DynamicItem> Items { get; } = new();

    public RadialPanelViewModel() : base("RadialPanel")
    {
        // Add initial items
        AddItem();
        AddItem();
        AddItem();
        AddItem();
        AddItem();
        AddItem();
        AddItem();
        AddItem();
    }

    [RelayCommand]
    private void AddItem()
    {
        Items.Add(new DynamicItem($"Item {Items.Count + 1}", GetRandomColor()));
    }

    [RelayCommand]
    private void RemoveItem()
    {
        if (Items.Count > 0)
        {
            Items.RemoveAt(Items.Count - 1);
        }
    }

    private string GetRandomColor()
    {
        var colors = new[] 
        { 
            "#FF6B6B", "#4ECDC4", "#45B7D1", "#FFA07A", "#98D8C8", 
            "#F7DC6F", "#BB8FCE", "#85C1E9", "#E74C3C", "#3498DB",
            "#2ECC71", "#F39C12", "#9B59B6", "#1ABC9C", "#E91E63" 
        };
        return colors[Items.Count % colors.Length];
    }
}
