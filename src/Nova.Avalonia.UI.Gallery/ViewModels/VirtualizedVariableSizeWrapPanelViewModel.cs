using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Media;

namespace Nova.Avalonia.UI.Gallery.ViewModels;

public partial class VirtualizedVariableSizeWrapPanelViewModel : PageViewModel
{
    [ObservableProperty]
    private double _tileSize = 80;

    [ObservableProperty]
    private double _spacing = 6;

    [ObservableProperty]
    private int _columns = 4;

    public ObservableCollection<TileItem> Items { get; } = new();

    private static readonly string[] Colors = 
    {
        "#3498DB", "#2ECC71", "#E74C3C", "#9B59B6", "#F39C12", 
        "#1ABC9C", "#E91E63", "#00BCD4", "#FF5722", "#795548",
        "#607D8B", "#8BC34A", "#CDDC39", "#FFC107", "#FF9800"
    };

    public VirtualizedVariableSizeWrapPanelViewModel() : base("VirtualizedVariableSizeWrapPanel")
    {
        // Generate 500 items with varying spans for virtualization testing
        GenerateItems(500);
    }

    [RelayCommand]
    private void AddItems()
    {
        GenerateItems(50);
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
            string color = Colors[index % Colors.Length];
            
            // Randomly assign spans (1-2 for column, 1-2 for row)
            int colSpan = random.Next(1, 3); // 1 or 2
            int rowSpan = random.Next(1, 3); // 1 or 2
            
            // Make large tiles less frequent
            if (random.Next(100) < 70) // 70% chance to be 1x1
            {
                colSpan = 1;
                rowSpan = 1;
            }
            
            Items.Add(new TileItem($"{index + 1}", color, colSpan, rowSpan));
        }
    }
}

public record TileItem(string Title, string ColorHex, int ColumnSpan, int RowSpan)
{
    public IBrush Brush => new SolidColorBrush(Color.Parse(ColorHex));
}
