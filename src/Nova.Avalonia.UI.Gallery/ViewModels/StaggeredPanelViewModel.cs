using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Nova.Avalonia.UI.Gallery.ViewModels;

public partial class StaggeredPanelViewModel : PageViewModel
{
    // Section 2: Layout Configuration
    [ObservableProperty]
    private ObservableCollection<StaggeredItem> _configItems = new();

    [ObservableProperty]
    private double _configDesiredColumnWidth = 150;

    [ObservableProperty]
    private double _configColumnSpacing = 5;

    [ObservableProperty]
    private double _configRowSpacing = 5;

    [ObservableProperty]
    private Thickness _configPadding = new Thickness(10);

    // Section 3: Dynamic Interaction
    [ObservableProperty]
    private ObservableCollection<StaggeredItem> _dynamicItems = new();

    public StaggeredPanelViewModel() : base("StaggeredPanel")
    {
        InitializeCollections();
    }

    private void InitializeCollections()
    {
        var random = new Random();

        // Populate Config Items (Fixed set)
        for (int i = 0; i < 10; i++)
        {
            ConfigItems.Add(CreateRandomItem(random, i));
        }

        // Populate Dynamic Items (Initial set)
        for (int i = 0; i < 5; i++)
        {
            DynamicItems.Add(CreateRandomItem(random, i));
        }
    }

    [RelayCommand]
    private void AddDynamicItem()
    {
        var random = new Random();
        DynamicItems.Add(CreateRandomItem(random, DynamicItems.Count));
    }

    [RelayCommand]
    private void RemoveDynamicItem()
    {
        if (DynamicItems.Any())
        {
            DynamicItems.RemoveAt(DynamicItems.Count - 1);
        }
    }

    private StaggeredItem CreateRandomItem(Random random, int index)
    {
        return new StaggeredItem(
            Title: $"Item {index + 1}",
            Height: random.Next(100, 300),
            Color: Color.FromRgb((byte)random.Next(100, 255), (byte)random.Next(100, 255), (byte)random.Next(100, 255))
        );
    }
}

public record StaggeredItem(string Title, double Height, Color Color)
{
    public SolidColorBrush Brush => new SolidColorBrush(Color);
}
