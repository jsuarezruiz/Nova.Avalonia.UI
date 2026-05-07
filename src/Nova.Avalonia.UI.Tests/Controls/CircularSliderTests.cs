using System;
using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Provider;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.VisualTree;
using Nova.Avalonia.UI.Controls;
using Xunit;
using InputPointer = Avalonia.Input.Pointer;

namespace Nova.Avalonia.UI.Tests.Controls;

public class CircularSliderTests
{
    private static readonly FieldInfo TrackThicknessField =
        typeof(CircularSlider).GetField("_trackThickness", BindingFlags.Instance | BindingFlags.NonPublic)!;

    [AvaloniaFact]
    public void CircularSlider_DefaultValues_AreCorrect()
    {
        var slider = new CircularSlider();

        Assert.Equal(0, slider.Minimum);
        Assert.Equal(100, slider.Maximum);
        Assert.Equal(0, slider.Value);
        Assert.Equal(-135, slider.StartAngle);
        Assert.Equal(135, slider.EndAngle);
        Assert.Equal(1, slider.TickFrequency);
        Assert.False(slider.IsSnapToTickEnabled);
        Assert.Equal("F0", slider.ValueStringFormat);
        Assert.True(slider.Focusable);
    }

    [AvaloniaFact]
    public void CircularSlider_Value_ClampedToMinMax()
    {
        var slider = new CircularSlider { Minimum = 10, Maximum = 50 };

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

        slider.Minimum = 60;
        Assert.Equal(60, slider.Value);
    }

    [AvaloniaFact]
    public void CircularSlider_AtMinimum_SetsPseudoClass()
    {
        var slider = new CircularSlider { Minimum = 0, Maximum = 100, Value = 0 };

        Assert.Contains(slider.Classes, c => c == ":minimum");
        Assert.DoesNotContain(slider.Classes, c => c == ":maximum");
    }

    [AvaloniaFact]
    public void CircularSlider_AtMaximum_SetsPseudoClass()
    {
        var slider = new CircularSlider { Minimum = 0, Maximum = 100, Value = 100 };

        Assert.Contains(slider.Classes, c => c == ":maximum");
        Assert.DoesNotContain(slider.Classes, c => c == ":minimum");
    }

    [AvaloniaFact]
    public void CircularSlider_MidValue_NoMinMaxPseudoClasses()
    {
        var slider = new CircularSlider { Minimum = 0, Maximum = 100, Value = 50 };

        Assert.DoesNotContain(slider.Classes, c => c == ":minimum");
        Assert.DoesNotContain(slider.Classes, c => c == ":maximum");
    }

    [AvaloniaFact]
    public void CircularSlider_AppearanceBrushes_UseInheritedAppearanceProperties()
    {
        var slider = new CircularSlider
        {
            Foreground = Brushes.Red,
            Background = Brushes.Gray
        };

        Assert.Equal(Brushes.Red, slider.Foreground);
        Assert.Equal(Brushes.Gray, slider.Background);
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
    public void CircularSlider_TrackThickness_UsesThemeResource()
    {
        var slider = new CircularSlider();
        slider.Resources["CircularSliderTrackThickness"] = 16.0;
        var window = ShowInWindow(slider);

        try
        {
            Assert.Equal(16, (double)TrackThicknessField.GetValue(slider)!);

            slider.Resources["CircularSliderTrackThickness"] = 20.0;

            Assert.Equal(20, (double)TrackThicknessField.GetValue(slider)!);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void CircularSlider_ThumbSizeResource_RepositionsThumb()
    {
        var slider = new CircularSlider
        {
            Width = 200,
            Height = 200,
            StartAngle = 90,
            EndAngle = 180,
            Value = 0
        };
        slider.Resources["CircularSliderThumbSize"] = 40.0;
        var window = ShowInWindow(slider);

        try
        {
            window.UpdateLayout();
            var thumb = GetTemplatePart<Border>(slider, "PART_Thumb");

            Assert.Equal(40, thumb.Bounds.Width, 1);
            Assert.Equal(160, Canvas.GetLeft(thumb), 1);

            slider.Resources["CircularSliderThumbSize"] = 20.0;
            window.UpdateLayout();

            Assert.Equal(20, thumb.Bounds.Width, 1);
            Assert.Equal(180, Canvas.GetLeft(thumb), 1);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void CircularSlider_TickFrequency_CanBeSet()
    {
        var slider = new CircularSlider { TickFrequency = 5 };

        Assert.Equal(5, slider.TickFrequency);
    }

    [AvaloniaFact]
    public void CircularSlider_TextAppearance_UsesInheritedControlProperties()
    {
        var slider = new CircularSlider
        {
            Foreground = Brushes.Black,
            FontSize = 32,
            FontWeight = FontWeight.Bold
        };

        Assert.Equal(Brushes.Black, slider.Foreground);
        Assert.Equal(32, slider.FontSize);
        Assert.Equal(FontWeight.Bold, slider.FontWeight);
    }

    [AvaloniaFact]
    public void CircularSlider_ValueStringFormat_CanBeSet()
    {
        var slider = new CircularSlider { ValueStringFormat = "F2" };

        Assert.Equal("F2", slider.ValueStringFormat);
    }

    [AvaloniaFact]
    public void CircularSlider_Content_CanBeSet()
    {
        var slider = new CircularSlider { Content = "Test Content" };

        Assert.Equal("Test Content", slider.Content);
    }

    [AvaloniaFact]
    public void CircularSlider_XamlContent_SetsContent()
    {
        const string xaml = """
            <controls:CircularSlider
                xmlns="https://github.com/avaloniaui"
                xmlns:controls="clr-namespace:Nova.Avalonia.UI.Controls;assembly=Nova.Avalonia.UI">
                Test Content
            </controls:CircularSlider>
            """;

        var slider = AvaloniaRuntimeXamlLoader.Parse<CircularSlider>(xaml, typeof(CircularSlider).Assembly);

        Assert.Equal("Test Content", slider.Content);
    }

    [AvaloniaFact]
    public void CircularSlider_Template_UpdatesDefaultAndCustomContent()
    {
        var slider = new CircularSlider
        {
            Width = 200,
            Height = 200,
            Value = 12.3
        };
        var window = ShowInWindow(slider);

        try
        {
            var presenter = GetTemplatePart<ContentPresenter>(slider, "PART_CenterContent");
            var defaultText = GetTemplatePart<TextBlock>(slider, "PART_DefaultCenterText");

            Assert.False(presenter.IsVisible);
            Assert.True(defaultText.IsVisible);
            Assert.Equal("12", defaultText.Text);

            slider.Content = "Custom";
            Assert.True(presenter.IsVisible);
            Assert.False(defaultText.IsVisible);
            Assert.Equal("Custom", presenter.Content);

            slider.Content = null;
            Assert.False(presenter.IsVisible);
            Assert.True(defaultText.IsVisible);
            Assert.Equal("12", defaultText.Text);

            slider.ValueStringFormat = "F2";
            Assert.Equal(slider.Value.ToString("F2"), defaultText.Text);

            slider.Value = 25.5;
            Assert.Equal(slider.Value.ToString("F2"), defaultText.Text);
        }
        finally
        {
            window.Close();
        }
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
        var slider = new CircularSlider { Minimum = -50, Maximum = 50 };

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
        var slider = new CircularSlider { Minimum = 50, Maximum = 50 };

        slider.Value = 100;
        Assert.Equal(50, slider.Value);
    }

    [AvaloniaFact]
    public void CircularSlider_InvertedMinMax_ClampsToMin()
    {
        var slider = new CircularSlider { Minimum = 100, Maximum = 0 };

        slider.Value = 50;
        Assert.Equal(100, slider.Value);
    }

    [AvaloniaFact]
    public void CircularSlider_NegativeValue_ClampsToMin()
    {
        var slider = new CircularSlider { Minimum = 0, Maximum = 100 };

        slider.Value = -10;

        Assert.Equal(0, slider.Value);
    }

    [AvaloniaFact]
    public void CircularSlider_LargeValue_ClampsToMax()
    {
        var slider = new CircularSlider { Minimum = 0, Maximum = 100 };

        slider.Value = 1000;

        Assert.Equal(100, slider.Value);
    }

    [AvaloniaFact]
    public void CircularSlider_DecimalValues_Work()
    {
        var slider = new CircularSlider { Minimum = 0, Maximum = 1, Value = 0.5 };

        Assert.Equal(0.5, slider.Value);
    }

    [AvaloniaFact]
    public void CircularSlider_AutomationPeer_ExposesRangeValueProvider()
    {
        var slider = new CircularSlider
        {
            Minimum = 0,
            Maximum = 0.5,
            Value = 0.25,
            SmallChange = 0.005,
            LargeChange = 0.05
        };
        var provider = Assert.IsAssignableFrom<IRangeValueProvider>(new CircularSliderAutomationPeer(slider));

        Assert.False(provider.IsReadOnly);
        Assert.Equal(0, provider.Minimum);
        Assert.Equal(0.5, provider.Maximum);
        Assert.Equal(0.25, provider.Value);
        Assert.Equal(0.005, provider.SmallChange, 6);
        Assert.Equal(0.05, provider.LargeChange, 6);

        provider.SetValue(0.4);

        Assert.Equal(0.4, slider.Value);

        slider.IsEnabled = false;

        Assert.True(provider.IsReadOnly);
        Assert.Throws<ElementNotEnabledException>(() => provider.SetValue(0.3));
    }

    [AvaloniaFact]
    public void CircularSlider_AutomationPeer_UsesComputedFallbackChanges_AndHelpfulName()
    {
        var slider = new CircularSlider
        {
            Minimum = 0,
            Maximum = 100,
            Value = 25,
            ValueStringFormat = "F0"
        };
        var peer = new CircularSliderAutomationPeer(slider);
        var provider = Assert.IsAssignableFrom<IRangeValueProvider>(peer);

        Assert.Equal(1, provider.SmallChange);
        Assert.Equal(10, provider.LargeChange);
        Assert.Equal("Value 25 (0 to 100)", peer.GetName());
    }

    [AvaloniaFact]
    public void CircularSlider_PointerCaptureLost_CompletesDrag()
    {
        var slider = new TestCircularSlider();

        slider.IsDraggingForTest = true;

        slider.RaisePointerCaptureLostForTest();

        Assert.False(slider.IsDraggingForTest);

        slider.RaisePointerCaptureLostForTest();

        Assert.False(slider.IsDraggingForTest);
    }

    [AvaloniaFact]
    public void CircularSlider_MousePressOnArc_StartsDragImmediately()
    {
        var slider = new TestCircularSlider
        {
            Width = 200,
            Height = 200,
            Minimum = 0,
            Maximum = 100,
            Value = 50
        };
        var window = ShowInWindow(slider);

        try
        {
            window.UpdateLayout();
            var pointer = new InputPointer(1, PointerType.Mouse, true);
            var arcPoint = GetArcPoint(slider, 90);

            var args = slider.RaisePointerPressedForTest(pointer, arcPoint);

            Assert.True(args.Handled);
            Assert.True(slider.IsDraggingForTest);
            Assert.Same(slider, pointer.Captured);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void CircularSlider_MousePressInCenter_DoesNotStartDrag()
    {
        var slider = new TestCircularSlider
        {
            Width = 200,
            Height = 200,
            Minimum = 0,
            Maximum = 100,
            Value = 50
        };
        var window = ShowInWindow(slider);

        try
        {
            window.UpdateLayout();
            var pointer = new InputPointer(2, PointerType.Mouse, true);

            var args = slider.RaisePointerPressedForTest(pointer, new Point(100, 100));

            Assert.False(args.Handled);
            Assert.False(slider.IsDraggingForTest);
            Assert.Null(pointer.Captured);
            Assert.Equal(50, slider.Value);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void CircularSlider_TouchPressOnArc_BubblesToParentScroll()
    {
        var slider = new TestCircularSlider
        {
            Width = 200,
            Height = 200,
            Minimum = 0,
            Maximum = 100,
            Value = 50
        };
        var window = ShowInWindow(slider);

        try
        {
            window.UpdateLayout();
            var pointer = new InputPointer(3, PointerType.Touch, true);
            var arcPoint = GetArcPoint(slider, 90);

            var args = slider.RaisePointerPressedForTest(pointer, arcPoint);

            Assert.False(args.Handled);
            Assert.False(slider.IsDraggingForTest);
            Assert.Null(pointer.Captured);
            Assert.Equal(50, slider.Value);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void CircularSlider_TouchPressOnThumb_StartsDragImmediately()
    {
        var slider = new TestCircularSlider
        {
            Width = 200,
            Height = 200,
            Minimum = 0,
            Maximum = 100,
            Value = 50
        };
        var window = ShowInWindow(slider);

        try
        {
            window.UpdateLayout();
            var pointer = new InputPointer(4, PointerType.Touch, true);
            var thumbCenter = GetThumbCenter(slider);

            var args = slider.RaisePointerPressedForTest(pointer, thumbCenter);

            Assert.True(args.Handled);
            Assert.True(slider.IsDraggingForTest);
            Assert.Same(slider, pointer.Captured);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void CircularSlider_TouchDragFromThumb_UpdatesValue()
    {
        var slider = new TestCircularSlider
        {
            Width = 200,
            Height = 200,
            Minimum = 0,
            Maximum = 100,
            Value = 50
        };
        var window = ShowInWindow(slider);

        try
        {
            window.UpdateLayout();
            var pointer = new InputPointer(5, PointerType.Touch, true);
            var thumbCenter = GetThumbCenter(slider);

            slider.RaisePointerPressedForTest(pointer, thumbCenter);
            var args = slider.RaisePointerMovedForTest(pointer, new Point(190, 100));

            Assert.True(args.Handled);
            Assert.True(slider.IsDraggingForTest);
            Assert.Same(slider, pointer.Captured);
            Assert.NotEqual(50, slider.Value);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void CircularSlider_WheelWithoutFocus_BubblesToParentScroll()
    {
        var slider = new TestCircularSlider
        {
            Minimum = 0,
            Maximum = 100,
            Value = 50
        };

        var args = slider.RaisePointerWheelForTest(1);

        Assert.Equal(50, slider.Value);
        Assert.False(slider.IsDraggingForTest);
        Assert.False(args.Handled);
    }

    [AvaloniaFact]
    public void CircularSlider_WheelWithFocus_BubblesToParentScroll()
    {
        var slider = new TestCircularSlider
        {
            Minimum = 0,
            Maximum = 100,
            Value = 50,
            SmallChange = 5
        };
        var window = ShowInWindow(slider);

        try
        {
            slider.Focus();

            var args = slider.RaisePointerWheelForTest(1);

            Assert.Equal(50, slider.Value);
            Assert.False(args.Handled);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void CircularSlider_WheelDuringDrag_BubblesToParentScroll()
    {
        var slider = new TestCircularSlider
        {
            Minimum = 0,
            Maximum = 100,
            Value = 50,
            SmallChange = 5
        };

        slider.IsDraggingForTest = true;

        var args = slider.RaisePointerWheelForTest(1);

        Assert.Equal(50, slider.Value);
        Assert.False(slider.IsDraggingForTest);
        Assert.False(args.Handled);
    }

    private static Window ShowInWindow(Control control)
    {
        var window = new Window
        {
            Width = 300,
            Height = 300,
            Content = control
        };

        window.Show();
        control.ApplyTemplate();

        return window;
    }

    private static T GetTemplatePart<T>(Control control, string name) where T : Control
    {
        return control.GetVisualDescendants().OfType<T>().Single(part => part.Name == name);
    }

    private static Point GetThumbCenter(Control control)
    {
        var thumb = GetTemplatePart<Border>(control, "PART_Thumb");
        return new Point(
            thumb.Bounds.X + thumb.Bounds.Width / 2,
            thumb.Bounds.Y + thumb.Bounds.Height / 2);
    }

    private static Point GetArcPoint(Control control, double angleDegrees)
    {
        var thumb = GetTemplatePart<Border>(control, "PART_Thumb");
        var center = new Point(control.Bounds.Width / 2, control.Bounds.Height / 2);
        var radius = (Math.Min(control.Bounds.Width, control.Bounds.Height) - Math.Max(thumb.Bounds.Width, thumb.Bounds.Height)) / 2;
        var angleRad = (angleDegrees - 90) * Math.PI / 180.0;

        return new Point(
            center.X + radius * Math.Cos(angleRad),
            center.Y + radius * Math.Sin(angleRad));
    }

    private sealed class TestCircularSlider : CircularSlider
    {
        private static readonly FieldInfo IsDraggingField =
            typeof(CircularSlider).GetField("_isDragging", BindingFlags.Instance | BindingFlags.NonPublic)!;

        protected override Type StyleKeyOverride => typeof(CircularSlider);

        public bool IsDraggingForTest
        {
            get => (bool)IsDraggingField.GetValue(this)!;
            set => IsDraggingField.SetValue(this, value);
        }

        public void RaisePointerCaptureLostForTest()
        {
            OnPointerCaptureLost(new PointerCaptureLostEventArgs(this, null!));
        }

        public PointerPressedEventArgs RaisePointerPressedForTest(IPointer pointer, Point position)
        {
            var args = new PointerPressedEventArgs(
                this,
                pointer,
                this,
                position,
                0,
                new PointerPointProperties(RawInputModifiers.LeftMouseButton, PointerUpdateKind.LeftButtonPressed),
                KeyModifiers.None);

            OnPointerPressed(args);
            return args;
        }

        public PointerEventArgs RaisePointerMovedForTest(IPointer pointer, Point position)
        {
            var args = new PointerEventArgs(
                InputElement.PointerMovedEvent,
                this,
                pointer,
                this,
                position,
                0,
                new PointerPointProperties(RawInputModifiers.LeftMouseButton, PointerUpdateKind.Other),
                KeyModifiers.None);

            OnPointerMoved(args);
            return args;
        }

        public PointerWheelEventArgs RaisePointerWheelForTest(double deltaY)
        {
            var args = new PointerWheelEventArgs(
                this,
                null!,
                this,
                new Point(),
                0,
                new PointerPointProperties(),
                KeyModifiers.None,
                new Vector(0, deltaY));

            OnPointerWheelChanged(args);
            return args;
        }
    }

}
