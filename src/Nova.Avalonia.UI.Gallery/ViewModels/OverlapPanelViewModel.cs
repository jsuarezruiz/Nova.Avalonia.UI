using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Nova.Avalonia.UI.Gallery.ViewModels;

public partial class OverlapPanelViewModel : PageViewModel
{
    [ObservableProperty] private double _offsetX = 20;
    [ObservableProperty] private double _offsetY = 20;
    [ObservableProperty] private bool _reverseZIndex = false;

    public ObservableCollection<DynamicItem> Items { get; } = new();

    public OverlapPanelViewModel() : base("OverlapPanel")
    {
        // Add initial items
        for (int i = 0; i < 5; i++)
        {
            AddItem();
        }
    }

    [RelayCommand]
    private void AddItem()
    {
        var colors = new[] 
        { 
            "#FF6B6B", "#4ECDC4", "#45B7D1", "#FFA07A", "#98D8C8", 
            "#F7DC6F", "#BB8FCE", "#85C1E9", "#E74C3C", "#3498DB" 
        };
        Items.Add(new DynamicItem($"Card {Items.Count + 1}", colors[Items.Count % colors.Length]));
    }

    [RelayCommand]
    private void RemoveItem()
    {
        if (Items.Count > 0)
        {
            Items.RemoveAt(Items.Count - 1);
        }
    }
}
