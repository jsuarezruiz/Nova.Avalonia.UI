using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Media;

namespace Nova.Avalonia.UI.Gallery.ViewModels;

public partial class TimelinePanelViewModel : PageViewModel
{
    [ObservableProperty]
    private double _spacing = 20;

    [ObservableProperty]
    private double _connectorWidth = 40;

    [ObservableProperty]
    private bool _alternateItems = false;

    public ObservableCollection<DynamicItem> Items { get; } = new();

    public TimelinePanelViewModel() : base("TimelinePanel")
    {
        var colors = new[] { "#3498DB", "#2ECC71", "#E74C3C", "#F39C12" };
        for (int i = 0; i < 4; i++)
        {
            Items.Add(new DynamicItem($"Step {i + 1}", colors[i]));
        }
    }

    [RelayCommand]
    private void AddItem()
    {
        var colors = new[] { "#3498DB", "#2ECC71", "#E74C3C", "#9B59B6", "#F39C12", "#1ABC9C", "#E91E63", "#00BCD4" };
        Items.Add(new DynamicItem($"Step {Items.Count + 1}", colors[Items.Count % colors.Length]));
    }

    [RelayCommand]
    private void RemoveItem()
    {
        if (Items.Count > 0)
            Items.RemoveAt(Items.Count - 1);
    }
}
