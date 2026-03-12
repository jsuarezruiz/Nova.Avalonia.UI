using System;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Nova.Avalonia.UI.Controls;
using Xunit;

namespace Nova.Avalonia.UI.Tests.Controls;

public class FortuneWheelTests
{
    [AvaloniaFact]
    public void DefaultProperties_AreCorrect()
    {
        var wheel = new FortuneWheel();

        Assert.NotNull(wheel.Items);
        Assert.Empty(wheel.Items);
        Assert.Equal(0, wheel.SelectedIndex);
        Assert.Equal(0.0, wheel.RotationAngle);
        Assert.Equal(TimeSpan.FromSeconds(3), wheel.AnimationDuration);
        Assert.Equal(3, wheel.MinimumSpins);
        Assert.False(wheel.IsSpinning);
        Assert.True(wheel.ShowIndicator);
        Assert.Equal(IndicatorPosition.Top, wheel.IndicatorPosition);
        Assert.Equal(24.0, wheel.IndicatorSize);
        Assert.Equal(30.0, wheel.CenterRadius);
    }

    [AvaloniaFact]
    public void Items_CanBePopulated()
    {
        var wheel = new FortuneWheel();
        wheel.Items.Add(new FortuneItem("A"));
        wheel.Items.Add(new FortuneItem("B"));
        wheel.Items.Add(new FortuneItem("C"));

        Assert.Equal(3, wheel.Items.Count);
        Assert.Equal("A", wheel.Items[0].Content);
    }

    [AvaloniaFact]
    public void SelectedIndex_CanBeSet()
    {
        var wheel = new FortuneWheel();
        wheel.Items.Add(new FortuneItem("A"));
        wheel.Items.Add(new FortuneItem("B"));

        wheel.SelectedIndex = 1;

        Assert.Equal(1, wheel.SelectedIndex);
    }

    [AvaloniaFact]
    public void StyleStrategy_DefaultsToAlternating()
    {
        var wheel = new FortuneWheel();

        Assert.IsType<AlternatingStyleStrategy>(wheel.StyleStrategy);
    }

    [AvaloniaFact]
    public void StyleStrategy_CanBeChanged()
    {
        var wheel = new FortuneWheel();
        var gradient = new GradientStyleStrategy();

        wheel.StyleStrategy = gradient;

        Assert.Same(gradient, wheel.StyleStrategy);
    }

    [AvaloniaFact]
    public void IndicatorPosition_CanBeChanged()
    {
        var wheel = new FortuneWheel();

        wheel.IndicatorPosition = IndicatorPosition.Right;

        Assert.Equal(IndicatorPosition.Right, wheel.IndicatorPosition);
    }

    [AvaloniaFact]
    public void AnimationDuration_CanBeChanged()
    {
        var wheel = new FortuneWheel();

        wheel.AnimationDuration = TimeSpan.FromSeconds(5);

        Assert.Equal(TimeSpan.FromSeconds(5), wheel.AnimationDuration);
    }

    [AvaloniaFact]
    public void MinimumSpins_CanBeChanged()
    {
        var wheel = new FortuneWheel();

        wheel.MinimumSpins = 5;

        Assert.Equal(5, wheel.MinimumSpins);
    }

    [AvaloniaFact]
    public void CenterRadius_CanBeChanged()
    {
        var wheel = new FortuneWheel();

        wheel.CenterRadius = 50.0;

        Assert.Equal(50.0, wheel.CenterRadius);
    }

    [AvaloniaFact]
    public void ShowIndicator_CanBeDisabled()
    {
        var wheel = new FortuneWheel();

        wheel.ShowIndicator = false;

        Assert.False(wheel.ShowIndicator);
    }

    [AvaloniaFact]
    public void RotationAngle_CanBeSet()
    {
        var wheel = new FortuneWheel();

        wheel.RotationAngle = 45.0;

        Assert.Equal(45.0, wheel.RotationAngle);
    }

    [AvaloniaFact]
    public void IndicatorSize_CanBeChanged()
    {
        var wheel = new FortuneWheel();

        wheel.IndicatorSize = 32.0;

        Assert.Equal(32.0, wheel.IndicatorSize);
    }

    [AvaloniaFact]
    public void IndicatorFill_CanBeChanged()
    {
        var wheel = new FortuneWheel();
        var brush = Brushes.Blue;

        wheel.IndicatorFill = brush;

        Assert.Same(brush, wheel.IndicatorFill);
    }

    [AvaloniaFact]
    public void CenterFill_CanBeChanged()
    {
        var wheel = new FortuneWheel();
        var brush = Brushes.Gold;

        wheel.CenterFill = brush;

        Assert.Same(brush, wheel.CenterFill);
    }

    [AvaloniaTheory]
    [InlineData(IndicatorPosition.Top)]
    [InlineData(IndicatorPosition.Bottom)]
    [InlineData(IndicatorPosition.Left)]
    [InlineData(IndicatorPosition.Right)]
    public void IndicatorPosition_AllValuesValid(IndicatorPosition position)
    {
        var wheel = new FortuneWheel();

        wheel.IndicatorPosition = position;

        Assert.Equal(position, wheel.IndicatorPosition);
    }

    [AvaloniaFact]
    public void MultipleWheels_HaveSeparateItemCollections()
    {
        var wheel1 = new FortuneWheel();
        var wheel2 = new FortuneWheel();

        wheel1.Items.Add(new FortuneItem("A"));

        Assert.Single(wheel1.Items);
        Assert.Empty(wheel2.Items);
    }

    [AvaloniaFact]
    public void Items_CanBeCleared()
    {
        var wheel = new FortuneWheel();
        wheel.Items.Add(new FortuneItem("A"));
        wheel.Items.Add(new FortuneItem("B"));

        wheel.Items.Clear();

        Assert.Empty(wheel.Items);
    }

    [AvaloniaFact]
    public void Items_CanBeRemoved()
    {
        var wheel = new FortuneWheel();
        var item = new FortuneItem("A");
        wheel.Items.Add(item);
        wheel.Items.Add(new FortuneItem("B"));

        wheel.Items.Remove(item);

        Assert.Single(wheel.Items);
        Assert.Equal("B", wheel.Items[0].Content);
    }
}

public class FortuneItemTests
{
    [AvaloniaFact]
    public void Constructor_SetsContent()
    {
        var item = new FortuneItem("Test");

        Assert.Equal("Test", item.Content);
    }

    [AvaloniaFact]
    public void Weight_DefaultsToOne()
    {
        var item = new FortuneItem("Test");

        Assert.Equal(1.0, item.Weight);
    }

    [AvaloniaFact]
    public void Weight_CanBeChanged()
    {
        var item = new FortuneItem("Test");

        item.Weight = 2.5;

        Assert.Equal(2.5, item.Weight);
    }

    [AvaloniaFact]
    public void Style_DefaultsToNull()
    {
        var item = new FortuneItem("Test");

        Assert.Null(item.Style);
    }

    [AvaloniaFact]
    public void Style_CanBeSet()
    {
        var item = new FortuneItem("Test");
        var style = new FortuneItemStyle { Background = Brushes.Red };

        item.Style = style;

        Assert.Same(style, item.Style);
    }

    [AvaloniaFact]
    public void ContentTemplate_DefaultsToNull()
    {
        var item = new FortuneItem("Test");

        Assert.Null(item.ContentTemplate);
    }

    [AvaloniaTheory]
    [InlineData(0.0)]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(10.0)]
    public void Weight_AcceptsVariousValues(double weight)
    {
        var item = new FortuneItem("Test") { Weight = weight };

        Assert.Equal(weight, item.Weight);
    }

    [AvaloniaFact]
    public void Content_CanBeNull()
    {
        var item = new FortuneItem(null!);

        Assert.Null(item.Content);
    }
}

public class FortuneItemStyleTests
{
    [AvaloniaFact]
    public void Background_CanBeSet()
    {
        var style = new FortuneItemStyle();

        style.Background = Brushes.Red;

        Assert.Same(Brushes.Red, style.Background);
    }

    [AvaloniaFact]
    public void Foreground_CanBeSet()
    {
        var style = new FortuneItemStyle();

        style.Foreground = Brushes.White;

        Assert.Same(Brushes.White, style.Foreground);
    }

    [AvaloniaFact]
    public void BorderBrush_CanBeSet()
    {
        var style = new FortuneItemStyle();

        style.BorderBrush = Brushes.Black;

        Assert.Same(Brushes.Black, style.BorderBrush);
    }

    [AvaloniaFact]
    public void BorderThickness_CanBeSet()
    {
        var style = new FortuneItemStyle();

        style.BorderThickness = 3.0;

        Assert.Equal(3.0, style.BorderThickness);
    }

    [AvaloniaFact]
    public void AllProperties_CanBeSetTogether()
    {
        var style = new FortuneItemStyle
        {
            Background = Brushes.Blue,
            Foreground = Brushes.White,
            BorderBrush = Brushes.DarkBlue,
            BorderThickness = 2.0
        };

        Assert.Same(Brushes.Blue, style.Background);
        Assert.Same(Brushes.White, style.Foreground);
        Assert.Same(Brushes.DarkBlue, style.BorderBrush);
        Assert.Equal(2.0, style.BorderThickness);
    }
}

public class AlternatingStyleStrategyTests
{
    [AvaloniaFact]
    public void GetStyle_ReturnsItemStyleWhenProvided()
    {
        var strategy = new AlternatingStyleStrategy();
        var itemStyle = new FortuneItemStyle { Background = Brushes.Green };

        var result = strategy.GetStyle(0, 4, itemStyle);

        Assert.Same(itemStyle, result);
    }

    [AvaloniaFact]
    public void GetStyle_ReturnsPrimaryForEvenIndex()
    {
        var strategy = new AlternatingStyleStrategy
        {
            PrimaryBackground = Brushes.Red,
            SecondaryBackground = Brushes.Blue
        };

        var result = strategy.GetStyle(0, 4, null);

        Assert.Same(Brushes.Red, result.Background);
    }

    [AvaloniaFact]
    public void GetStyle_ReturnsSecondaryForOddIndex()
    {
        var strategy = new AlternatingStyleStrategy
        {
            PrimaryBackground = Brushes.Red,
            SecondaryBackground = Brushes.Blue
        };

        var result = strategy.GetStyle(1, 4, null);

        Assert.Same(Brushes.Blue, result.Background);
    }

    [AvaloniaFact]
    public void GetStyle_AlternatesCorrectly()
    {
        var strategy = new AlternatingStyleStrategy
        {
            PrimaryBackground = Brushes.Red,
            SecondaryBackground = Brushes.Blue
        };

        var even = strategy.GetStyle(2, 4, null);
        var odd = strategy.GetStyle(3, 4, null);

        Assert.Same(Brushes.Red, even.Background);
        Assert.Same(Brushes.Blue, odd.Background);
    }

    [AvaloniaFact]
    public void DefaultColors_AreSet()
    {
        var strategy = new AlternatingStyleStrategy();

        Assert.NotNull(strategy.PrimaryBackground);
        Assert.NotNull(strategy.SecondaryBackground);
        Assert.NotNull(strategy.BorderBrush);
        Assert.NotNull(strategy.Foreground);
    }
}

public class GradientStyleStrategyTests
{
    [AvaloniaFact]
    public void GetStyle_ReturnsItemStyleWhenProvided()
    {
        var strategy = new GradientStyleStrategy();
        var itemStyle = new FortuneItemStyle { Background = Brushes.Green };

        var result = strategy.GetStyle(0, 4, itemStyle);

        Assert.Same(itemStyle, result);
    }

    [AvaloniaFact]
    public void GetStyle_ReturnsStartColorForFirstItem()
    {
        var strategy = new GradientStyleStrategy
        {
            StartColor = Colors.Red,
            EndColor = Colors.Blue
        };

        var result = strategy.GetStyle(0, 4, null);

        Assert.NotNull(result.Background);
    }

    [AvaloniaFact]
    public void GetStyle_ReturnsEndColorForLastItem()
    {
        var strategy = new GradientStyleStrategy
        {
            StartColor = Colors.Red,
            EndColor = Colors.Blue
        };

        var result = strategy.GetStyle(3, 4, null);

        Assert.NotNull(result.Background);
    }

    [AvaloniaFact]
    public void GetStyle_ReturnsSameColorForSingleItem()
    {
        var strategy = new GradientStyleStrategy
        {
            StartColor = Colors.Red,
            EndColor = Colors.Blue
        };

        var result = strategy.GetStyle(0, 1, null);

        Assert.NotNull(result.Background);
    }

    [AvaloniaFact]
    public void DefaultColors_AreSet()
    {
        var strategy = new GradientStyleStrategy();

        Assert.NotEqual(default, strategy.StartColor);
        Assert.NotEqual(default, strategy.EndColor);
        Assert.NotNull(strategy.BorderBrush);
        Assert.NotNull(strategy.Foreground);
    }
}

public class FortuneSelectionEventArgsTests
{
    [AvaloniaFact]
    public void Constructor_SetsProperties()
    {
        var item = new FortuneItem("Test");
        var args = new FortuneSelectionEventArgs(2, item);

        Assert.Equal(2, args.SelectedIndex);
        Assert.Same(item, args.SelectedItem);
    }

    [AvaloniaFact]
    public void SelectedIndex_CanBeZero()
    {
        var item = new FortuneItem("First");
        var args = new FortuneSelectionEventArgs(0, item);

        Assert.Equal(0, args.SelectedIndex);
    }

    [AvaloniaFact]
    public void SelectedItem_CanBeNull()
    {
        var args = new FortuneSelectionEventArgs(0, null!);

        Assert.Null(args.SelectedItem);
    }
}
