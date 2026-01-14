using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Media;

namespace Nova.Avalonia.UI.Gallery.ViewModels;

public partial class OrbitPanelViewModel : PageViewModel
{
    [ObservableProperty]
    private double _orbitSpacing = 60;

    [ObservableProperty]
    private double _innerRadius = 50;

    [ObservableProperty]
    private double _startAngle = 0;

    public ObservableCollection<OrbitItem> Items { get; } = new();

    public OrbitPanelViewModel() : base("OrbitPanel")
    {
        // Initialize with items in different orbits
        Items.Add(new OrbitItem("#E74C3C", 0)); // Center
        Items.Add(new OrbitItem("#3498DB", 1));
        Items.Add(new OrbitItem("#2ECC71", 1));
        Items.Add(new OrbitItem("#F39C12", 1));
        Items.Add(new OrbitItem("#9B59B6", 2));
        Items.Add(new OrbitItem("#1ABC9C", 2));
    }

    [RelayCommand]
    private void AddItem()
    {
        var colors = new[] { "#3498DB", "#2ECC71", "#E74C3C", "#9B59B6", "#F39C12", "#1ABC9C", "#E91E63", "#00BCD4" };
        int orbit = Items.Count < 2 ? 0 : (Items.Count < 5 ? 1 : 2);
        Items.Add(new OrbitItem(colors[Items.Count % colors.Length], orbit));
    }

    [RelayCommand]
    private void RemoveItem()
    {
        if (Items.Count > 0)
            Items.RemoveAt(Items.Count - 1);
    }
}

public record OrbitItem(string ColorHex, int Orbit)
{
    public IBrush Brush => new SolidColorBrush(Color.Parse(ColorHex));
}
