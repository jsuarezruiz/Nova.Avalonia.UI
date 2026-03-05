using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Layout;
using Nova.Avalonia.UI.Controls;
using Xunit;

namespace Nova.Avalonia.UI.Tests.Controls;

public class CompareSliderTests
{
    [AvaloniaFact]
    public void DefaultValues_AreCorrect()
    {
        var control = new CompareSlider();

        Assert.Equal(0.5, control.Value);
        Assert.Equal(0.0, control.Minimum);
        Assert.Equal(1.0, control.Maximum);
        Assert.Equal(Orientation.Horizontal, control.Orientation);
        Assert.Equal(0.01, control.SmallChange);
        Assert.Equal(0.1, control.LargeChange);
        Assert.False(control.IsDirectionReversed);
        Assert.True(control.IsMoveToPointEnabled);
        Assert.Null(control.BeforeContent);
        Assert.Null(control.AfterContent);
        Assert.Null(control.BeforeContentTemplate);
        Assert.Null(control.AfterContentTemplate);
    }

    [AvaloniaTheory]
    [InlineData(0.5, 0.5)]
    [InlineData(0.0, 0.0)]
    [InlineData(1.0, 1.0)]
    [InlineData(-0.1, 0.0)]
    [InlineData(1.1, 1.0)]
    [InlineData(100.0, 1.0)]
    [InlineData(-100.0, 0.0)]
    public void Value_IsClamped_BetweenMinAndMax(double input, double expected)
    {
        var control = new CompareSlider();
        control.Value = input;
        Assert.Equal(expected, control.Value);
    }

    [AvaloniaFact]
    public void Reset_NonAnimated_SetsValueToCenter()
    {
        var control = new CompareSlider { Value = 0.2 };
        control.Reset(animate: false);
        Assert.Equal(0.5, control.Value);
    }

    [AvaloniaFact]
    public void Reset_ReturnsCompletedTask_WhenNotAnimated()
    {
        var control = new CompareSlider { Value = 0.2 };
        var task = control.Reset(animate: false);
        Assert.True(task.IsCompletedSuccessfully);
    }

    [AvaloniaFact]
    public void Reset_ReturnsTask_WhenAnimated()
    {
        var control = new CompareSlider { Value = 0.2 };
        var task = control.Reset(animate: true);
        Assert.NotNull(task);
    }

    [AvaloniaTheory]
    [InlineData(Orientation.Horizontal)]
    [InlineData(Orientation.Vertical)]
    public void Orientation_CanBeChanged(Orientation orientation)
    {
        var control = new CompareSlider { Orientation = orientation };
        Assert.Equal(orientation, control.Orientation);
    }

    [AvaloniaFact]
    public void SettingContent_UpdatesProperties()
    {
        var control = new CompareSlider();
        var content1 = "Before";
        var content2 = "After";

        control.BeforeContent = content1;
        control.AfterContent = content2;

        Assert.Equal(content1, control.BeforeContent);
        Assert.Equal(content2, control.AfterContent);
    }

    [AvaloniaFact]
    public void Templates_CanBeSet()
    {
        var control = new CompareSlider();
        var template = new FuncDataTemplate<object>((_, _) => new TextBlock(), true);

        control.BeforeContentTemplate = template;
        control.AfterContentTemplate = template;

        Assert.NotNull(control.BeforeContentTemplate);
        Assert.NotNull(control.AfterContentTemplate);
        Assert.Equal(template, control.BeforeContentTemplate);
    }

    // Horizontal key navigation (default orientation)
    [AvaloniaTheory]
    [InlineData(Key.Left, 0.5, 0.49)]
    [InlineData(Key.Right, 0.5, 0.51)]
    [InlineData(Key.Down, 0.5, 0.49)]
    [InlineData(Key.Up, 0.5, 0.51)]
    [InlineData(Key.PageDown, 0.5, 0.4)]
    [InlineData(Key.PageUp, 0.5, 0.6)]
    [InlineData(Key.Home, 0.5, 0.0)]
    [InlineData(Key.End, 0.5, 1.0)]
    public void KeyNavigation_Horizontal_ChangesValue(Key key, double startValue, double expectedValue)
    {
        var control = new TestableCompareSlider { Value = startValue, Orientation = Orientation.Horizontal };

        control.SimulateKeyDown(key);

        Assert.Equal(expectedValue, control.Value, 4);
    }

    // Vertical key navigation: Up/Down are spatially mapped to the divider position.
    // Up → divider moves up → value decreases; Down → divider moves down → value increases.
    [AvaloniaTheory]
    [InlineData(Key.Up, 0.5, 0.49)]
    [InlineData(Key.Down, 0.5, 0.51)]
    [InlineData(Key.PageUp, 0.5, 0.4)]
    [InlineData(Key.PageDown, 0.5, 0.6)]
    [InlineData(Key.Left, 0.5, 0.49)]
    [InlineData(Key.Right, 0.5, 0.51)]
    [InlineData(Key.Home, 0.5, 0.0)]
    [InlineData(Key.End, 0.5, 1.0)]
    public void KeyNavigation_Vertical_ChangesValue(Key key, double startValue, double expectedValue)
    {
        var control = new TestableCompareSlider { Value = startValue, Orientation = Orientation.Vertical };

        control.SimulateKeyDown(key);

        Assert.Equal(expectedValue, control.Value, 4);
    }

    [AvaloniaFact]
    public void KeyNavigation_DirectionReversed()
    {
        var control = new TestableCompareSlider
        {
            Value = 0.5,
            IsDirectionReversed = true
        };

        control.SimulateKeyDown(Key.Right);
        Assert.Equal(0.49, control.Value, 4);

        control.SimulateKeyDown(Key.Left);
        Assert.Equal(0.50, control.Value, 4);
    }

    [AvaloniaFact]
    public void ChangeEvents_AreRaised()
    {
        var control = new CompareSlider();
        bool changed = false;
        control.ValueChanged += (s, e) => changed = true;

        control.Value = 0.8;
        Assert.True(changed);
    }

    [AvaloniaFact]
    public void CustomRange_ValueMapping_IsCorrect()
    {
        var control = new CompareSlider
        {
            Minimum = 100,
            Maximum = 200,
            Value = 150
        };

        Assert.Equal(150, control.Value);
    }

    [AvaloniaTheory]
    [InlineData(100, 200, 150)]
    [InlineData(0, 100, 50)]
    [InlineData(-100, 100, 0)]
    public void CustomRange_Reset_SetsValueToCenter(double min, double max, double expected)
    {
        var control = new CompareSlider
        {
            Minimum = min,
            Maximum = max,
            Value = max
        };

        control.Reset(animate: false);
        Assert.Equal(expected, control.Value, 4);
    }

    [AvaloniaFact]
    public void OrientationChange_UpdatesPseudoClasses()
    {
        var control = new CompareSlider { Orientation = Orientation.Horizontal };

        Assert.Contains(":horizontal", control.Classes);
        Assert.DoesNotContain(":vertical", control.Classes);

        control.Orientation = Orientation.Vertical;

        Assert.DoesNotContain(":horizontal", control.Classes);
        Assert.Contains(":vertical", control.Classes);
    }

    [AvaloniaFact]
    public async Task AnimateTo_SetsValueToTarget()
    {
        var control = new CompareSlider { Value = 0.0 };

        await control.AnimateTo(1.0, TimeSpan.FromMilliseconds(1));

        Assert.Equal(1.0, control.Value);
    }

    [AvaloniaFact]
    public async Task AnimateTo_ClampsValueToRange()
    {
        var control = new CompareSlider { Value = 0.5 };

        await control.AnimateTo(2.0, TimeSpan.FromMilliseconds(1));

        Assert.Equal(1.0, control.Value);
    }

    [AvaloniaFact]
    public async Task AnimateTo_Cancellation_LeavesValueBelowTarget()
    {
        var control = new CompareSlider { Value = 0.0 };
        using var cts = new CancellationTokenSource();

        var task = control.AnimateTo(1.0, TimeSpan.FromMilliseconds(500), cts.Token);
        cts.Cancel();
        await task;

        Assert.True(control.Value < 1.0);
    }

    [AvaloniaFact]
    public async Task AnimateTo_SubsequentCall_CancelsPrevious()
    {
        var control = new CompareSlider { Value = 0.0 };

        // Start a long animation, then immediately override with a short one
        var first = control.AnimateTo(1.0, TimeSpan.FromMilliseconds(2000));
        await control.AnimateTo(0.2, TimeSpan.FromMilliseconds(1));

        // Second animation wins
        Assert.Equal(0.2, control.Value, 4);
        await first; // should have completed (cancelled) without throwing
    }

    [AvaloniaFact]
    public void IsMoveToPointEnabled_DefaultsToTrue()
    {
        var control = new CompareSlider();
        Assert.True(control.IsMoveToPointEnabled);
    }

    [AvaloniaFact]
    public void IsMoveToPointEnabled_CanBeDisabled()
    {
        var control = new CompareSlider { IsMoveToPointEnabled = false };
        Assert.False(control.IsMoveToPointEnabled);
    }

    [AvaloniaFact]
    public void IsDragging_DefaultsToFalse()
    {
        var control = new CompareSlider();
        Assert.False(control.IsDragging);
    }

    private class TestableCompareSlider : CompareSlider
    {
        public void SimulateKeyDown(Key key)
        {
            OnKeyDown(new KeyEventArgs
            {
                RoutedEvent = KeyDownEvent,
                Key = key,
                Source = this
            });
        }
    }
}
