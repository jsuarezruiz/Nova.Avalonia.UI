using System;
using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Nova.Avalonia.UI.Controls;
using Xunit;

namespace Nova.Avalonia.UI.Tests.Controls;

public class CircularSliderTests
{
    [AvaloniaFact]
    public void CircularSlider_DefaultValues_AreCorrect()
    {
        var slider = new CircularSlider();

        Assert.Equal(0, slider.MinValue);
        Assert.Equal(100, slider.MaxValue);
        Assert.Equal(0, slider.Value);
        Assert.Equal(-135, slider.StartAngle);
        Assert.Equal(135, slider.EndAngle);
        Assert.Equal(0, slider.StepFrequency);
        Assert.Equal("F0", slider.ValueFormat);
        Assert.Equal(12, slider.InactiveThickness);
        Assert.Equal(12, slider.ActiveThickness);
        Assert.Equal(20, slider.ThumbSize);
        Assert.True(slider.Focusable);
    }

    [AvaloniaFact]
    public void CircularSlider_Value_ClampedToMinMax()
    {
        var slider = new CircularSlider { MinValue = 10, MaxValue = 50 };

        slider.Value = 5;
        Assert.Equal(10, slider.Value);

        slider.Value = 100;
        Assert.Equal(50, slider.Value);

        slider.Value = 30;
        Assert.Equal(30, slider.Value);
    }

    [AvaloniaFact]
    public void CircularSlider_Value_ClampsWhenMinMaxChange()
    {
        var slider = new CircularSlider { Value = 50 };

        slider.MinValue = 60;
        Assert.Equal(60, slider.Value);
    }

    [AvaloniaFact]
    public void CircularSlider_AtMinimum_SetsPseudoClass()
    {
        var slider = new CircularSlider { MinValue = 0, MaxValue = 100, Value = 0 };

        Assert.Contains(slider.Classes, c => c == ":minimum");
        Assert.DoesNotContain(slider.Classes, c => c == ":maximum");
    }

    [AvaloniaFact]
    public void CircularSlider_AtMaximum_SetsPseudoClass()
    {
        var slider = new CircularSlider { MinValue = 0, MaxValue = 100, Value = 100 };

        Assert.Contains(slider.Classes, c => c == ":maximum");
        Assert.DoesNotContain(slider.Classes, c => c == ":minimum");
    }

    [AvaloniaFact]
    public void CircularSlider_MidValue_NoMinMaxPseudoClasses()
    {
        var slider = new CircularSlider { MinValue = 0, MaxValue = 100, Value = 50 };

        Assert.DoesNotContain(slider.Classes, c => c == ":minimum");
        Assert.DoesNotContain(slider.Classes, c => c == ":maximum");
    }

    [AvaloniaFact]
    public void CircularSlider_Brushes_CanBeSet()
    {
        var slider = new CircularSlider
        {
            ActiveBrush = Brushes.Red,
            InactiveBrush = Brushes.Gray,
            ThumbBrush = Brushes.Blue,
            InnerBackground = Brushes.White
        };

        Assert.Equal(Brushes.Red, slider.ActiveBrush);
        Assert.Equal(Brushes.Gray, slider.InactiveBrush);
        Assert.Equal(Brushes.Blue, slider.ThumbBrush);
        Assert.Equal(Brushes.White, slider.InnerBackground);
    }

    [AvaloniaFact]
    public void CircularSlider_AngleProperties_CanBeSet()
    {
        var slider = new CircularSlider
        {
            StartAngle = -180,
            EndAngle = 0
        };

        Assert.Equal(-180, slider.StartAngle);
        Assert.Equal(0, slider.EndAngle);
    }

    [AvaloniaFact]
    public void CircularSlider_ThicknessProperties_CanBeSet()
    {
        var slider = new CircularSlider
        {
            InactiveThickness = 8,
            ActiveThickness = 16,
            ThumbSize = 24
        };

        Assert.Equal(8, slider.InactiveThickness);
        Assert.Equal(16, slider.ActiveThickness);
        Assert.Equal(24, slider.ThumbSize);
    }

    [AvaloniaFact]
    public void CircularSlider_StepFrequency_CanBeSet()
    {
        var slider = new CircularSlider { StepFrequency = 5 };

        Assert.Equal(5, slider.StepFrequency);
    }

    [AvaloniaFact]
    public void CircularSlider_LineCapProperties_CanBeSet()
    {
        var slider = new CircularSlider
        {
            InactiveStrokeLineCap = PenLineCap.Square,
            ActiveStrokeLineCap = PenLineCap.Flat
        };

        Assert.Equal(PenLineCap.Square, slider.InactiveStrokeLineCap);
        Assert.Equal(PenLineCap.Flat, slider.ActiveStrokeLineCap);
    }

    [AvaloniaFact]
    public void CircularSlider_ActiveRadiusDelta_CanBeSet()
    {
        var slider = new CircularSlider { ActiveRadiusDelta = 5.0 };

        Assert.Equal(5.0, slider.ActiveRadiusDelta);
    }

    [AvaloniaFact]
    public void CircularSlider_TextProperties_CanBeSet()
    {
        var slider = new CircularSlider
        {
            TextBrush = Brushes.Black,
            TextFontSize = 32,
            TextFontWeight = FontWeight.Bold
        };

        Assert.Equal(Brushes.Black, slider.TextBrush);
        Assert.Equal(32, slider.TextFontSize);
        Assert.Equal(FontWeight.Bold, slider.TextFontWeight);
    }

    [AvaloniaFact]
    public void CircularSlider_ValueFormat_CanBeSet()
    {
        var slider = new CircularSlider { ValueFormat = "F2" };

        Assert.Equal("F2", slider.ValueFormat);
    }

    [AvaloniaFact]
    public void CircularSlider_CenterContent_CanBeSet()
    {
        var slider = new CircularSlider { CenterContent = "Test Content" };

        Assert.Equal("Test Content", slider.CenterContent);
    }

    [AvaloniaFact]
    public void CircularSlider_ThumbContent_CanBeSet()
    {
        var slider = new CircularSlider { ThumbContent = "T" };

        Assert.Equal("T", slider.ThumbContent);
    }

    [AvaloniaFact]
    public void CircularSlider_ValueChanged_FiresOnValueChange()
    {
        var slider = new CircularSlider { Value = 10 };
        double? oldValue = null;
        double? newValue = null;

        slider.ValueChanged += (_, args) =>
        {
            oldValue = args.OldValue;
            newValue = args.NewValue;
        };

        slider.Value = 50;

        Assert.Equal(10, oldValue);
        Assert.Equal(50, newValue);
    }

    [AvaloniaFact]
    public void CircularSlider_ValidMinMax_AcceptsRangeValues()
    {
        var slider = new CircularSlider { MinValue = -50, MaxValue = 50 };

        slider.Value = -25;
        Assert.Equal(-25, slider.Value);

        slider.Value = 25;
        Assert.Equal(25, slider.Value);
    }

    [AvaloniaFact]
    public void CircularSlider_FullCircle_AnglesWork()
    {
        var slider = new CircularSlider
        {
            StartAngle = 0,
            EndAngle = 359,
            Value = 50
        };

        Assert.Equal(0, slider.StartAngle);
        Assert.Equal(359, slider.EndAngle);
        Assert.Equal(50, slider.Value);
    }

    [AvaloniaFact]
    public void CircularSlider_Semicircle_AnglesWork()
    {
        var slider = new CircularSlider
        {
            StartAngle = -90,
            EndAngle = 90
        };

        Assert.Equal(-90, slider.StartAngle);
        Assert.Equal(90, slider.EndAngle);
    }

    [AvaloniaFact]
    public void CircularSlider_ZeroRange_HandlesGracefully()
    {
        var slider = new CircularSlider { MinValue = 50, MaxValue = 50 };

        slider.Value = 100;
        Assert.Equal(50, slider.Value);
    }

    [AvaloniaFact]
    public void CircularSlider_InvertedMinMax_ClampsToMin()
    {
        var slider = new CircularSlider { MinValue = 100, MaxValue = 0 };

        slider.Value = 50;
        Assert.Equal(100, slider.Value);
    }

    [AvaloniaFact]
    public void ValueChangedEventArgs_HasCorrectValues()
    {
        var args = new ValueChangedEventArgs(10, 20);

        Assert.Equal(10, args.OldValue);
        Assert.Equal(20, args.NewValue);
    }

    [AvaloniaFact]
    public void CircularSlider_DefaultLineCaps_AreRound()
    {
        var slider = new CircularSlider();

        Assert.Equal(PenLineCap.Round, slider.InactiveStrokeLineCap);
        Assert.Equal(PenLineCap.Round, slider.ActiveStrokeLineCap);
    }

    [AvaloniaFact]
    public void CircularSlider_NegativeValue_ClampsToMin()
    {
        var slider = new CircularSlider { MinValue = 0, MaxValue = 100 };

        slider.Value = -10;

        Assert.Equal(0, slider.Value);
    }

    [AvaloniaFact]
    public void CircularSlider_LargeValue_ClampsToMax()
    {
        var slider = new CircularSlider { MinValue = 0, MaxValue = 100 };

        slider.Value = 1000;

        Assert.Equal(100, slider.Value);
    }

    [AvaloniaFact]
    public void CircularSlider_DecimalValues_Work()
    {
        var slider = new CircularSlider { MinValue = 0, MaxValue = 1, Value = 0.5 };

        Assert.Equal(0.5, slider.Value);
    }
}
