using System;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.Media;
using Nova.Avalonia.UI.Controls;
using Xunit;

namespace Nova.Avalonia.UI.Tests.Controls;

public class FortuneBarTests
{
    [AvaloniaFact]
    public void DefaultProperties_AreCorrect()
    {
        var bar = new FortuneBar();

        Assert.NotNull(bar.Items);
        Assert.Empty(bar.Items);
        Assert.Equal(0, bar.SelectedIndex);
        Assert.Equal(0.0, bar.ScrollOffset);
        Assert.Equal(Orientation.Horizontal, bar.Orientation);
        Assert.Equal(100.0, bar.ItemSize);
        Assert.Equal(TimeSpan.FromSeconds(3), bar.AnimationDuration);
        Assert.Equal(2, bar.MinimumCycles);
        Assert.False(bar.IsSpinning);
        Assert.True(bar.ShowIndicator);
        Assert.Equal(4.0, bar.IndicatorThickness);
    }

    [AvaloniaFact]
    public void Items_CanBePopulated()
    {
        var bar = new FortuneBar();
        bar.Items.Add(new FortuneItem("Cherry"));
        bar.Items.Add(new FortuneItem("Lemon"));
        bar.Items.Add(new FortuneItem("Orange"));

        Assert.Equal(3, bar.Items.Count);
        Assert.Equal("Cherry", bar.Items[0].Content);
    }

    [AvaloniaFact]
    public void SelectedIndex_CanBeSet()
    {
        var bar = new FortuneBar();
        bar.Items.Add(new FortuneItem("A"));
        bar.Items.Add(new FortuneItem("B"));

        bar.SelectedIndex = 1;

        Assert.Equal(1, bar.SelectedIndex);
    }

    [AvaloniaFact]
    public void Orientation_CanBeChanged()
    {
        var bar = new FortuneBar();

        bar.Orientation = Orientation.Vertical;

        Assert.Equal(Orientation.Vertical, bar.Orientation);
    }

    [AvaloniaFact]
    public void ItemSize_CanBeChanged()
    {
        var bar = new FortuneBar();

        bar.ItemSize = 150.0;

        Assert.Equal(150.0, bar.ItemSize);
    }

    [AvaloniaFact]
    public void StyleStrategy_DefaultsToAlternating()
    {
        var bar = new FortuneBar();

        Assert.IsType<AlternatingStyleStrategy>(bar.StyleStrategy);
    }

    [AvaloniaFact]
    public void StyleStrategy_CanBeChanged()
    {
        var bar = new FortuneBar();
        var gradient = new GradientStyleStrategy();

        bar.StyleStrategy = gradient;

        Assert.Same(gradient, bar.StyleStrategy);
    }

    [AvaloniaFact]
    public void AnimationDuration_CanBeChanged()
    {
        var bar = new FortuneBar();

        bar.AnimationDuration = TimeSpan.FromSeconds(5);

        Assert.Equal(TimeSpan.FromSeconds(5), bar.AnimationDuration);
    }

    [AvaloniaFact]
    public void MinimumCycles_CanBeChanged()
    {
        var bar = new FortuneBar();

        bar.MinimumCycles = 4;

        Assert.Equal(4, bar.MinimumCycles);
    }

    [AvaloniaFact]
    public void ScrollOffset_CanBeSet()
    {
        var bar = new FortuneBar();

        bar.ScrollOffset = 250.0;

        Assert.Equal(250.0, bar.ScrollOffset);
    }

    [AvaloniaFact]
    public void ShowIndicator_CanBeDisabled()
    {
        var bar = new FortuneBar();

        bar.ShowIndicator = false;

        Assert.False(bar.ShowIndicator);
    }

    [AvaloniaFact]
    public void IndicatorThickness_CanBeChanged()
    {
        var bar = new FortuneBar();

        bar.IndicatorThickness = 6.0;

        Assert.Equal(6.0, bar.IndicatorThickness);
    }

    [AvaloniaFact]
    public void IndicatorFill_CanBeChanged()
    {
        var bar = new FortuneBar();
        var brush = Brushes.Green;

        bar.IndicatorFill = brush;

        Assert.Same(brush, bar.IndicatorFill);
    }

    [AvaloniaTheory]
    [InlineData(Orientation.Horizontal)]
    [InlineData(Orientation.Vertical)]
    public void Orientation_AllValuesValid(Orientation orientation)
    {
        var bar = new FortuneBar();

        bar.Orientation = orientation;

        Assert.Equal(orientation, bar.Orientation);
    }

    [AvaloniaFact]
    public void MultipleBars_HaveSeparateItemCollections()
    {
        var bar1 = new FortuneBar();
        var bar2 = new FortuneBar();

        bar1.Items.Add(new FortuneItem("A"));

        Assert.Single(bar1.Items);
        Assert.Empty(bar2.Items);
    }

    [AvaloniaFact]
    public void Items_CanBeCleared()
    {
        var bar = new FortuneBar();
        bar.Items.Add(new FortuneItem("A"));
        bar.Items.Add(new FortuneItem("B"));

        bar.Items.Clear();

        Assert.Empty(bar.Items);
    }

    [AvaloniaFact]
    public void Items_CanBeRemoved()
    {
        var bar = new FortuneBar();
        var item = new FortuneItem("A");
        bar.Items.Add(item);
        bar.Items.Add(new FortuneItem("B"));

        bar.Items.Remove(item);

        Assert.Single(bar.Items);
        Assert.Equal("B", bar.Items[0].Content);
    }

    [AvaloniaTheory]
    [InlineData(50.0)]
    [InlineData(100.0)]
    [InlineData(200.0)]
    public void ItemSize_AcceptsVariousValues(double size)
    {
        var bar = new FortuneBar();

        bar.ItemSize = size;

        Assert.Equal(size, bar.ItemSize);
    }

    [AvaloniaTheory]
    [InlineData(0.0)]
    [InlineData(100.0)]
    [InlineData(-100.0)]
    [InlineData(1000.0)]
    public void ScrollOffset_AcceptsVariousValues(double offset)
    {
        var bar = new FortuneBar();

        bar.ScrollOffset = offset;

        Assert.Equal(offset, bar.ScrollOffset);
    }

    [AvaloniaFact]
    public void MinimumCycles_CanBeZero()
    {
        var bar = new FortuneBar();

        bar.MinimumCycles = 0;

        Assert.Equal(0, bar.MinimumCycles);
    }

    [AvaloniaFact]
    public void AnimationDuration_CanBeZero()
    {
        var bar = new FortuneBar();

        bar.AnimationDuration = TimeSpan.Zero;

        Assert.Equal(TimeSpan.Zero, bar.AnimationDuration);
    }

    [AvaloniaFact]
    public void IndicatorThickness_CanBeZero()
    {
        var bar = new FortuneBar();

        bar.IndicatorThickness = 0.0;

        Assert.Equal(0.0, bar.IndicatorThickness);
    }

    [AvaloniaFact]
    public void Items_WithWeights_AreStored()
    {
        var bar = new FortuneBar();
        bar.Items.Add(new FortuneItem("A") { Weight = 5.0 });
        bar.Items.Add(new FortuneItem("B") { Weight = 1.0 });

        Assert.Equal(5.0, bar.Items[0].Weight);
        Assert.Equal(1.0, bar.Items[1].Weight);
    }
}
