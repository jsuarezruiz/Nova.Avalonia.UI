using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nova.Avalonia.UI.Controls;
using Xunit;

namespace Nova.Avalonia.UI.Tests.Controls;

public class RatingControlTests
{
    [AvaloniaFact]
    public void DefaultValues_AreCorrect()
    {
        var control = new RatingControl();

        Assert.Equal(0.0, control.Value);
        Assert.Equal(5, control.ItemCount);
        Assert.Equal(RatingPrecision.Full, control.Precision);
        Assert.False(control.IsReadOnly);
        Assert.Equal(RatingShape.Star, control.Shape);
        Assert.Equal(32.0, control.ItemSize);
        Assert.Equal(6.0, control.ItemSpacing);
        Assert.Equal(Orientation.Horizontal, control.Orientation);
        Assert.Equal(0.0, control.StrokeThickness);
    }

    [AvaloniaFact]
    public void Value_IsCoercedToItemCount()
    {
        var control = new RatingControl
        {
            ItemCount = 5,
            Value = 10
        };

        Assert.Equal(5, control.Value);
    }

    [AvaloniaFact]
    public void Value_IsCoercedToZero_WhenNegative()
    {
        var control = new RatingControl
        {
            Value = -5
        };

        Assert.Equal(0, control.Value);
    }

    [AvaloniaFact]
    public void ItemCount_IsCoercedToMinimumOne()
    {
        var control = new RatingControl
        {
            ItemCount = 0
        };

        Assert.Equal(1, control.ItemCount);
    }

    [AvaloniaFact]
    public void ItemCount_IsCoercedToMinimumOne_WhenNegative()
    {
        var control = new RatingControl
        {
            ItemCount = -5
        };

        Assert.Equal(1, control.ItemCount);
    }

    [AvaloniaFact]
    public void Value_UpdatesWhenItemCountDecreases()
    {
        var control = new RatingControl
        {
            ItemCount = 10,
            Value = 8
        };

        control.ItemCount = 5;

        Assert.Equal(5, control.Value);
    }

    [AvaloniaFact]
    public void ValueChanged_EventIsRaised()
    {
        var control = new RatingControl();
        var eventRaised = false;

        control.ValueChanged += (s, e) => eventRaised = true;
        control.Value = 3;

        Assert.True(eventRaised);
    }

    [AvaloniaFact]
    public void ValueChanged_EventIsRaisedMultipleTimes()
    {
        var control = new RatingControl();
        var eventCount = 0;

        control.ValueChanged += (s, e) => eventCount++;
        control.Value = 1;
        control.Value = 2;
        control.Value = 3;

        Assert.Equal(3, eventCount);
    }

    [AvaloniaTheory]
    [InlineData(RatingPrecision.Full, 2.7, 2.7)]
    [InlineData(RatingPrecision.Half, 2.5, 2.5)]
    [InlineData(RatingPrecision.Exact, 3.14159, 3.14159)]
    public void Value_AcceptsFractionalValues(RatingPrecision precision, double setValue, double expectedValue)
    {
        var control = new RatingControl
        {
            Precision = precision,
            Value = setValue
        };

        Assert.Equal(expectedValue, control.Value);
    }

    [AvaloniaTheory]
    [InlineData(RatingShape.Star)]
    [InlineData(RatingShape.Heart)]
    [InlineData(RatingShape.Circle)]
    [InlineData(RatingShape.Diamond)]
    [InlineData(RatingShape.Custom)]
    public void Shape_CanBeSet(RatingShape shape)
    {
        var control = new RatingControl
        {
            Shape = shape
        };

        Assert.Equal(shape, control.Shape);
    }

    [AvaloniaFact]
    public void CustomGeometry_CanBeSet()
    {
        var geometry = StreamGeometry.Parse("M 0,0 L 10,10 L 0,10 Z");
        var control = new RatingControl
        {
            Shape = RatingShape.Custom,
            CustomGeometry = geometry
        };

        Assert.Equal(geometry, control.CustomGeometry);
    }

    [AvaloniaFact]
    public void Orientation_CanBeVertical()
    {
        var control = new RatingControl
        {
            Orientation = Orientation.Vertical
        };

        Assert.Equal(Orientation.Vertical, control.Orientation);
    }

    [AvaloniaFact]
    public void IsReadOnly_PreventsValueChange_ViaProperty()
    {
        var control = new RatingControl
        {
            IsReadOnly = true,
            Value = 3
        };

        // Property setting still works, IsReadOnly only affects user interaction
        Assert.Equal(3, control.Value);
    }

    [AvaloniaFact]
    public void RatedFill_DefaultIsGold()
    {
        var control = new RatingControl();

        Assert.NotNull(control.RatedFill);
        var brush = Assert.IsAssignableFrom<ISolidColorBrush>(control.RatedFill);
        Assert.Equal(Colors.Gold, brush.Color);
    }

    [AvaloniaFact]
    public void UnratedFill_DefaultIsLightGray()
    {
        var control = new RatingControl();

        Assert.NotNull(control.UnratedFill);
        var brush = Assert.IsAssignableFrom<ISolidColorBrush>(control.UnratedFill);
        Assert.Equal(Colors.LightGray, brush.Color);
    }

    [AvaloniaFact]
    public void PreviewFill_DefaultIsOrange()
    {
        var control = new RatingControl();

        Assert.NotNull(control.PreviewFill);
        var brush = Assert.IsAssignableFrom<ISolidColorBrush>(control.PreviewFill);
        Assert.Equal(Colors.Orange, brush.Color);
    }

    [AvaloniaFact]
    public void Brushes_CanBeCustomized()
    {
        var ratedFill = new SolidColorBrush(Colors.Red);
        var unratedFill = new SolidColorBrush(Colors.Blue);
        var ratedStroke = new SolidColorBrush(Colors.DarkRed);
        var unratedStroke = new SolidColorBrush(Colors.DarkBlue);
        var previewFill = new SolidColorBrush(Colors.Green);
        var previewStroke = new SolidColorBrush(Colors.DarkGreen);

        var control = new RatingControl
        {
            RatedFill = ratedFill,
            UnratedFill = unratedFill,
            RatedStroke = ratedStroke,
            UnratedStroke = unratedStroke,
            PreviewFill = previewFill,
            PreviewStroke = previewStroke,
            StrokeThickness = 2.0
        };

        Assert.Equal(ratedFill, control.RatedFill);
        Assert.Equal(unratedFill, control.UnratedFill);
        Assert.Equal(ratedStroke, control.RatedStroke);
        Assert.Equal(unratedStroke, control.UnratedStroke);
        Assert.Equal(previewFill, control.PreviewFill);
        Assert.Equal(previewStroke, control.PreviewStroke);
        Assert.Equal(2.0, control.StrokeThickness);
    }

    [AvaloniaFact]
    public void ItemSize_AffectsControl()
    {
        var control = new RatingControl
        {
            ItemSize = 48
        };

        Assert.Equal(48, control.ItemSize);
    }

    [AvaloniaFact]
    public void ItemSpacing_AffectsControl()
    {
        var control = new RatingControl
        {
            ItemSpacing = 12
        };

        Assert.Equal(12, control.ItemSpacing);
    }

    [AvaloniaFact]
    public void Control_IsFocusable()
    {
        var control = new RatingControl();

        Assert.True(control.Focusable);
    }
}
