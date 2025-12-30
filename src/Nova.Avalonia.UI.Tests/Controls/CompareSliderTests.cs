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
    public void Reset_SetsValueToCenter()
    {
        var control = new CompareSlider { Value = 0.2 };
        control.Reset(animate: false);
        Assert.Equal(0.5, control.Value);
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
    
    [AvaloniaTheory]
    [InlineData(Key.Left, 0.5, 0.49)] // 0.5 - 0.01
    [InlineData(Key.Right, 0.5, 0.51)] // 0.5 + 0.01
    [InlineData(Key.Down, 0.5, 0.49)] // 0.5 - 0.01
    [InlineData(Key.Up, 0.5, 0.51)] // 0.5 + 0.01
    [InlineData(Key.PageDown, 0.5, 0.4)] // 0.5 - 0.1
    [InlineData(Key.PageUp, 0.5, 0.6)] // 0.5 + 0.1
    [InlineData(Key.Home, 0.5, 0.0)]
    [InlineData(Key.End, 0.5, 1.0)]
    public void KeyNavigation_ChangesValue(Key key, double startValue, double expectedValue)
    {
        var control = new TestableCompareSlider { Value = startValue };
        
        // Simulate key press
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

        // Right key -> change = +SmallChange -> Inverted -> -SmallChange
        control.SimulateKeyDown(Key.Right);
        Assert.Equal(0.49, control.Value, 4);
        
        // Left key -> change = -SmallChange -> Inverted -> +SmallChange
        control.SimulateKeyDown(Key.Left);
        Assert.Equal(0.50, control.Value, 4); // Back to 0.5
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
    
    // Testable subclass to access protected members
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