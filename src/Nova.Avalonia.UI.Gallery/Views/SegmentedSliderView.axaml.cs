using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Nova.Avalonia.UI.Controls;

namespace Nova.Avalonia.UI.Gallery.Views;

public partial class SegmentedSliderView : UserControl
{
    public SegmentedSliderView()
    {
        InitializeComponent();
        ConfigureSegments();
    }

    private void ConfigureSegments()
    {
        LabeledSlider.Segments = new List<SegmentedSliderSegment>
        {
            new() { Title = "Low" },
            new() { Title = "Medium" },
            new() { Title = "High" },
            new() { Title = "Critical" }
        };

        WeightedSlider.Segments = new List<SegmentedSliderSegment>
        {
            new() { Title = "Queued", WidthRatio = 1, FillBrush = Brushes.SteelBlue },
            new() { Title = "Running", WidthRatio = 2, FillBrush = Brushes.SeaGreen },
            new() { Title = "Review", WidthRatio = 1, FillBrush = Brushes.DarkOrange },
            new() { Title = "Done", WidthRatio = 1, FillBrush = Brushes.MediumVioletRed }
        };

        EventSlider.Segments = new List<SegmentedSliderSegment>
        {
            new() { Title = "Low" },
            new() { Title = "Medium" },
            new() { Title = "High" }
        };
    }

    private void OnEventSliderValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        EventValueText.Text = $"Value: {e.NewValue:F0}";
    }

    private void OnEventSliderSegmentChanged(object? sender, SegmentChangedEventArgs e)
    {
        var title = string.IsNullOrWhiteSpace(e.Segment?.Title) ? e.NewIndex.ToString() : e.Segment.Title;
        EventSegmentText.Text = $"Segment: {title}";
    }
}
