using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Media;

namespace Nova.Avalonia.UI.Gallery.ViewModels;

public partial class BubblePanelViewModel : PageViewModel
{
    [ObservableProperty]
    private double _spacing = 4;

    public ObservableCollection<DynamicItem> Items { get; } = new();

    public BubblePanelViewModel() : base("BubblePanel")
    {
        var colors = new[] { "#3498DB", "#2ECC71", "#E74C3C", "#9B59B6", "#F39C12", "#1ABC9C" };
        for (int i = 0; i < 6; i++)
        {
            Items.Add(new DynamicItem($"{i + 1}", colors[i % colors.Length]));
        }
    }

    [RelayCommand]
    private void AddItem()
    {
        var colors = new[] { "#3498DB", "#2ECC71", "#E74C3C", "#9B59B6", "#F39C12", "#1ABC9C", "#E91E63", "#00BCD4" };
        Items.Add(new DynamicItem($"{Items.Count + 1}", colors[Items.Count % colors.Length]));
    }

    [RelayCommand]
    private void RemoveItem()
    {
        if (Items.Count > 0)
            Items.RemoveAt(Items.Count - 1);
    }
}
