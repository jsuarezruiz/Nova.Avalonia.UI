using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Media;

namespace Nova.Avalonia.UI.Gallery.ViewModels;

public partial class VirtualizedStaggeredPanelViewModel : PageViewModel
{
    [ObservableProperty]
    private double _desiredColumnWidth = 150;

    [ObservableProperty]
    private double _columnSpacing = 8;

    [ObservableProperty]
    private double _rowSpacing = 8;

    public ObservableCollection<VirtualizedStaggeredItem> Items { get; } = new();

    private static readonly string[] Colors = 
    {
        "#3498DB", "#2ECC71", "#E74C3C", "#9B59B6", "#F39C12", 
        "#1ABC9C", "#E91E63", "#00BCD4", "#FF5722", "#795548",
        "#607D8B", "#8BC34A", "#CDDC39", "#FFC107", "#FF9800"
    };

    public VirtualizedStaggeredPanelViewModel() : base("VirtualizedStaggeredPanel")
    {
        // Generate 1000 items with varying heights for virtualization testing
        GenerateItems(1000);
    }

    [RelayCommand]
    private void AddItems()
    {
        GenerateItems(100);
    }

    [RelayCommand]
    private void ClearItems()
    {
        Items.Clear();
    }

    private void GenerateItems(int count)
    {
        var random = new System.Random();
        int startIndex = Items.Count;
        
        for (int i = 0; i < count; i++)
        {
            int index = startIndex + i;
            // Random heights between 80 and 250 for staggered effect
            int height = random.Next(80, 251);
            string color = Colors[index % Colors.Length];
            Items.Add(new VirtualizedStaggeredItem($"Item {index + 1}", color, height));
        }
    }
}

public record VirtualizedStaggeredItem(string Title, string ColorHex, int Height)
{
    public IBrush Brush => new SolidColorBrush(Color.Parse(ColorHex));
}
