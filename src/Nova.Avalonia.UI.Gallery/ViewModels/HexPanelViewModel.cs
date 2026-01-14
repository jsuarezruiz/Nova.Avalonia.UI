using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace Nova.Avalonia.UI.Gallery.ViewModels;

public partial class HexPanelViewModel : PageViewModel
{
    [ObservableProperty]
    private int _columnCount = 3;

    [ObservableProperty]
    private int _rowCount = 3;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Orientation))]
    private bool _isHorizontal = false;

    public Orientation Orientation => IsHorizontal ? Orientation.Horizontal : Orientation.Vertical;

    public ObservableCollection<HexItem> Items { get; } = new();

    public HexPanelViewModel() : base("HexPanel")
    {
        var colors = new[] { "#3498DB", "#2ECC71", "#E74C3C", "#9B59B6", "#F39C12", "#1ABC9C" };
        for (int i = 0; i < 6; i++)
        {
            Items.Add(new HexItem(colors[i]));
        }
        ReflowItems();
    }



    [RelayCommand]
    private void AddItem()
    {
        var colors = new[] { "#3498DB", "#2ECC71", "#E74C3C", "#9B59B6", "#F39C12", "#1ABC9C", "#E91E63", "#00BCD4" };
        Items.Add(new HexItem(colors[Items.Count % colors.Length]));
        ReflowItems();
    }

    [RelayCommand]
    private void RemoveItem()
    {
        if (Items.Count > 0)
        {
            Items.RemoveAt(Items.Count - 1);
            ReflowItems();
        }
    }

    private void ReflowItems()
    {
        const int dynamicColumnCount = 4;
        for (int i = 0; i < Items.Count; i++)
        {
            var item = Items[i];
            item.Row = i / dynamicColumnCount;
            item.Column = i % dynamicColumnCount;
        }
    }
}

public partial class HexItem : ObservableObject
{
    [ObservableProperty] private string _colorHex;
    [ObservableProperty] private int _row;
    [ObservableProperty] private int _column;

    public IBrush Brush => new SolidColorBrush(Color.Parse(ColorHex));

    public HexItem(string colorHex)
    {
        _colorHex = colorHex;
    }
}
