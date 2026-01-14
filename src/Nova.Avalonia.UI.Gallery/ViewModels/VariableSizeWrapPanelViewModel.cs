using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Media;

namespace Nova.Avalonia.UI.Gallery.ViewModels;

public partial class VariableSizeWrapPanelViewModel : PageViewModel
{
    [ObservableProperty]
    private double _tileSize = 80;

    [ObservableProperty]
    private double _spacing = 8;

    [ObservableProperty]
    private int _columns = 4;

    public ObservableCollection<MetroItem> Items { get; } = new();

    public VariableSizeWrapPanelViewModel() : base("VariableSizeWrapPanel")
    {
        var colors = new[] { "#3498DB", "#2ECC71", "#E74C3C", "#9B59B6", "#F39C12", "#1ABC9C" };
        for (int i = 0; i < 6; i++)
        {
            Items.Add(new MetroItem(colors[i % colors.Length], 1, 1));
        }
    }

    [RelayCommand]
    private void AddItem()
    {
        var colors = new[] { "#3498DB", "#2ECC71", "#E74C3C", "#9B59B6", "#F39C12", "#1ABC9C", "#E91E63", "#00BCD4" };
        Items.Add(new MetroItem(colors[Items.Count % colors.Length], 1, 1));
    }

    [RelayCommand]
    private void RemoveItem()
    {
        if (Items.Count > 0)
            Items.RemoveAt(Items.Count - 1);
    }
}

public record MetroItem(string ColorHex, int ColumnSpan, int RowSpan)
{
    public IBrush Brush => new SolidColorBrush(Color.Parse(ColorHex));
}
