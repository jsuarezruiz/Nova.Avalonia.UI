using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Provider;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
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

public class SegmentedSliderTests
{
    [AvaloniaFact]
    public void SegmentedSlider_DefaultValues_AreCorrect()
    {
        var slider = new SegmentedSlider();

        Assert.Equal(0, slider.Minimum);
        Assert.Equal(100, slider.Maximum);
        Assert.Equal(0, slider.Value);
        Assert.Equal(5, slider.SegmentCount);
        Assert.Empty(slider.Segments);
        Assert.Equal(4, slider.Spacing);
        Assert.Equal(SegmentTitleVisibility.AlwaysVisible, slider.TitleVisibility);
        Assert.False(slider.IsReadOnly);
        Assert.False(slider.IsSnapToSegmentEnabled);
        Assert.True(slider.Focusable);
    }

    [AvaloniaFact]
    public void SegmentedSlider_Value_UsesRangeBaseClamping()
    {
        var slider = new SegmentedSlider { Minimum = 10, Maximum = 50 };

        slider.Value = 5;
        Assert.Equal(10, slider.Value);

        slider.Value = 100;
        Assert.Equal(50, slider.Value);
    }

    [AvaloniaFact]
    public void SegmentedSlider_Value_ReclampsWhenMaximumDecreases()
    {
        var slider = new SegmentedSlider { Maximum = 100, Value = 80 };

        slider.Maximum = 50;

        Assert.Equal(50, slider.Value);
    }

    [AvaloniaFact]
    public void SegmentedSlider_LayoutProperties_CoerceInvalidValues()
    {
        var slider = new SegmentedSlider
        {
            SegmentCount = 0,
            Spacing = double.NaN,
            TitleVisibility = (SegmentTitleVisibility)999
        };

        Assert.Equal(1, slider.SegmentCount);
        Assert.Equal(0, slider.Spacing);
        Assert.Equal(SegmentTitleVisibility.AlwaysVisible, slider.TitleVisibility);
    }

    [AvaloniaFact]
    public void SegmentedSliderSegment_WidthRatio_CoercesInvalidValues()
    {
        var segment = new SegmentedSliderSegment { WidthRatio = double.NaN };
        Assert.Equal(0, segment.WidthRatio);

        segment.WidthRatio = -1;
        Assert.Equal(0, segment.WidthRatio);
    }

    [AvaloniaFact]
    public void SegmentedSlider_XamlSegments_AddToSegmentsCollection()
    {
        const string xaml = """
            <controls:SegmentedSlider
                xmlns="https://github.com/avaloniaui"
                xmlns:controls="clr-namespace:Nova.Avalonia.UI.Controls;assembly=Nova.Avalonia.UI">
                <controls:SegmentedSlider.Segments>
                    <controls:SegmentedSliderSegment Title="Low" />
                    <controls:SegmentedSliderSegment Title="Medium" WidthRatio="2" />
                    <controls:SegmentedSliderSegment Title="High" />
                </controls:SegmentedSlider.Segments>
            </controls:SegmentedSlider>
            """;

        var slider = AvaloniaRuntimeXamlLoader.Parse<SegmentedSlider>(xaml, typeof(SegmentedSlider).Assembly);

        Assert.Equal(3, slider.Segments.Count);
        Assert.Equal("Low", slider.Segments[0].Title);
        Assert.Equal(2, slider.Segments[1].WidthRatio);
    }

    [AvaloniaFact]
    public void SegmentedSlider_DirectXamlSegments_AddToSegmentsCollection()
    {
        const string xaml = """
            <controls:SegmentedSlider
                xmlns="https://github.com/avaloniaui"
                xmlns:controls="clr-namespace:Nova.Avalonia.UI.Controls;assembly=Nova.Avalonia.UI">
                <controls:SegmentedSliderSegment Title="Low" />
                <controls:SegmentedSliderSegment Title="Medium" WidthRatio="2" />
                <controls:SegmentedSliderSegment Title="High" />
            </controls:SegmentedSlider>
            """;

        var slider = AvaloniaRuntimeXamlLoader.Parse<SegmentedSlider>(xaml, typeof(SegmentedSlider).Assembly);

        Assert.Equal(3, slider.Segments.Count);
        Assert.Equal("Medium", slider.Segments[1].Title);
        Assert.Equal(2, slider.Segments[1].WidthRatio);
    }

    [AvaloniaFact]
    public void SegmentedSlider_ValueChanged_UsesRangeBaseEventArgs()
    {
        var slider = new SegmentedSlider { Value = 10 };
        double? oldValue = null;
        double? newValue = null;

        slider.ValueChanged += (_, args) =>
        {
            oldValue = args.OldValue;
            newValue = args.NewValue;
        };

        slider.Value = 42;

        Assert.Equal(10, oldValue);
        Assert.Equal(42, newValue);
    }

    [AvaloniaFact]
    public void SegmentedSlider_CalculateSegmentIndex_EqualSegments()
    {
        var slider = new SegmentedSlider { Maximum = 100, SegmentCount = 5 };

        Assert.Equal(0, slider.CalculateSegmentIndex(0));
        Assert.Equal(0, slider.CalculateSegmentIndex(10));
        Assert.Equal(1, slider.CalculateSegmentIndex(20));
        Assert.Equal(2, slider.CalculateSegmentIndex(50));
        Assert.Equal(4, slider.CalculateSegmentIndex(100));
    }

    [AvaloniaFact]
    public void SegmentedSlider_CalculateSegmentIndex_CustomSegments()
    {
        var slider = new SegmentedSlider
        {
            Maximum = 100,
            Segments = new ObservableCollection<SegmentedSliderSegment>
            {
                new() { Title = "A", WidthRatio = 1 },
                new() { Title = "B", WidthRatio = 3 },
                new() { Title = "C", WidthRatio = 1 }
            }
        };

        Assert.Equal(0, slider.CalculateSegmentIndex(0));
        Assert.Equal(0, slider.CalculateSegmentIndex(15));
        Assert.Equal(1, slider.CalculateSegmentIndex(50));
        Assert.Equal(2, slider.CalculateSegmentIndex(90));
        Assert.Equal(2, slider.CalculateSegmentIndex(100));
    }

    [AvaloniaFact]
    public void SegmentedSlider_SegmentChanged_FiresWithCustomSegment()
    {
        var slider = new SegmentedSlider
        {
            Maximum = 100,
            Segments = new ObservableCollection<SegmentedSliderSegment>
            {
                new() { Title = "Low" },
                new() { Title = "High" }
            }
        };
        SegmentChangedEventArgs? received = null;
        slider.SegmentChanged += (_, args) => received = args;

        slider.Value = 10;
        slider.Value = 60;

        Assert.NotNull(received);
        Assert.Equal(1, received.NewIndex);
        Assert.Equal("High", received.Segment?.Title);
    }

    [AvaloniaFact]
    public void SegmentedSlider_SegmentChanged_DoesNotFireOnVisualRebuild()
    {
        var slider = new SegmentedSlider
        {
            Width = 400,
            Maximum = 100,
            Value = 25,
            Segments = new ObservableCollection<SegmentedSliderSegment>
            {
                new() { Title = "Low" },
                new() { Title = "High" }
            }
        };
        var changes = 0;
        slider.SegmentChanged += (_, _) => changes++;
        var window = ShowInWindow(slider);

        try
        {
            Assert.Equal(0, changes);

            slider.Resources["SegmentedSliderTrackHeight"] = 12.0;
            slider.Spacing = 8;
            slider.CornerRadius = new CornerRadius(6);
            window.UpdateLayout();

            Assert.Equal(0, changes);

            slider.Value = 75;

            Assert.Equal(1, changes);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void SegmentedSlider_TrackPrimaryMousePress_UpdatesValueAndHandles()
    {
        var slider = new TestSegmentedSlider
        {
            Width = 400,
            Maximum = 100,
            Value = 20
        };
        var window = ShowInWindow(slider);

        try
        {
            var track = GetTemplatePart<Panel>(slider, "PART_Track");
            var pointer = new InputPointer(1, PointerType.Mouse, true);
            var args = slider.RaiseTrackPointerPressedForTest(
                track,
                pointer,
                new Point(track.Bounds.Width * 0.75, track.Bounds.Height / 2),
                RawInputModifiers.LeftMouseButton,
                PointerUpdateKind.LeftButtonPressed);

            Assert.True(args.Handled);
            Assert.True(slider.Value > 20);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void SegmentedSlider_TrackSecondaryMousePress_BubblesWithoutChangingValue()
    {
        var slider = new TestSegmentedSlider
        {
            Width = 400,
            Maximum = 100,
            Value = 20
        };
        var window = ShowInWindow(slider);

        try
        {
            var track = GetTemplatePart<Panel>(slider, "PART_Track");
            var pointer = new InputPointer(2, PointerType.Mouse, true);
            var args = slider.RaiseTrackPointerPressedForTest(
                track,
                pointer,
                new Point(track.Bounds.Width * 0.75, track.Bounds.Height / 2),
                RawInputModifiers.RightMouseButton,
                PointerUpdateKind.RightButtonPressed);

            Assert.False(args.Handled);
            Assert.Equal(20, slider.Value);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void SegmentedSlider_TrackTouchPress_BubblesWithoutChangingValue()
    {
        var slider = new TestSegmentedSlider
        {
            Width = 400,
            Maximum = 100,
            Value = 20
        };
        var window = ShowInWindow(slider);

        try
        {
            var track = GetTemplatePart<Panel>(slider, "PART_Track");
            var pointer = new InputPointer(3, PointerType.Touch, true);
            var args = slider.RaiseTrackPointerPressedForTest(
                track,
                pointer,
                new Point(track.Bounds.Width * 0.75, track.Bounds.Height / 2),
                RawInputModifiers.LeftMouseButton,
                PointerUpdateKind.LeftButtonPressed);

            Assert.False(args.Handled);
            Assert.Equal(20, slider.Value);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void SegmentedSlider_Template_RendersPartsAndSegmentTitles()
    {
        var slider = new SegmentedSlider
        {
            Width = 400,
            Segments = new ObservableCollection<SegmentedSliderSegment>
            {
                new() { Title = "Low" },
                new() { Title = "Medium" },
                new() { Title = "High" }
            }
        };
        var window = ShowInWindow(slider);

        try
        {
            Assert.NotNull(GetTemplatePart<Panel>(slider, "PART_Track"));
            Assert.NotNull(GetTemplatePart<Thumb>(slider, "PART_Thumb"));

            var titles = slider.GetVisualDescendants()
                .OfType<TextBlock>()
                .Select(textBlock => textBlock.Text)
                .Where(text => !string.IsNullOrWhiteSpace(text))
                .ToArray();

            Assert.Contains("Low", titles);
            Assert.Contains("Medium", titles);
            Assert.Contains("High", titles);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void SegmentedSlider_Template_MeasuresSegmentTitles()
    {
        var slider = new SegmentedSlider
        {
            Width = 400,
            Segments = new ObservableCollection<SegmentedSliderSegment>
            {
                new() { Title = "Low" },
                new() { Title = "Medium" },
                new() { Title = "High" }
            }
        };
        var window = ShowInWindow(slider);

        try
        {
            var track = GetTemplatePart<Panel>(slider, "PART_Track");
            var trackRectangle = slider.GetVisualDescendants().OfType<Rectangle>().First();
            var titleBlock = slider.GetVisualDescendants().OfType<TextBlock>().Single(textBlock => textBlock.Text == "Low");

            Assert.True(track.Bounds.Height > trackRectangle.Bounds.Height);
            Assert.True(track.Bounds.Height >= titleBlock.Bounds.Bottom);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void SegmentedSlider_ThumbCenter_AlignsWithSegmentFillEndpoint()
    {
        var slider = new SegmentedSlider
        {
            Width = 400,
            Maximum = 100,
            Value = 40,
            SegmentCount = 5,
            TitleVisibility = SegmentTitleVisibility.Collapsed,
            Background = Brushes.LightGray,
            Foreground = Brushes.DodgerBlue
        };
        var window = ShowInWindow(slider);

        try
        {
            var track = GetTemplatePart<Panel>(slider, "PART_Track");
            var thumb = GetTemplatePart<Thumb>(slider, "PART_Thumb");
            var transform = Assert.IsType<TranslateTransform>(thumb.RenderTransform);
            var trackRects = slider.GetVisualDescendants()
                .OfType<Rectangle>()
                .Where(rectangle => Equals(rectangle.Fill, Brushes.LightGray))
                .OrderBy(rectangle => rectangle.Bounds.X)
                .ToArray();

            Assert.Equal(5, trackRects.Length);

            var expectedCenterX = track.Margin.Left + trackRects[1].Bounds.Right;
            var actualCenterX = transform.X + thumb.Bounds.Width / 2;
            Assert.InRange(actualCenterX, expectedCenterX - 0.5, expectedCenterX + 0.5);

            slider.Value = 75;
            window.UpdateLayout();

            expectedCenterX = track.Margin.Left + trackRects[3].Bounds.X + trackRects[3].Bounds.Width * 0.75;
            actualCenterX = transform.X + thumb.Bounds.Width / 2;
            Assert.InRange(actualCenterX, expectedCenterX - 0.5, expectedCenterX + 0.5);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void SegmentedSlider_ThumbSizeResource_RepositionsThumb()
    {
        var slider = new SegmentedSlider
        {
            Width = 400,
            Maximum = 100,
            Value = 40,
            SegmentCount = 5,
            TitleVisibility = SegmentTitleVisibility.Collapsed,
            Background = Brushes.LightGray
        };
        var window = ShowInWindow(slider);

        try
        {
            var track = GetTemplatePart<Panel>(slider, "PART_Track");
            var thumb = GetTemplatePart<Thumb>(slider, "PART_Thumb");
            var transform = Assert.IsType<TranslateTransform>(thumb.RenderTransform);
            var trackRects = slider.GetVisualDescendants()
                .OfType<Rectangle>()
                .Where(rectangle => Equals(rectangle.Fill, Brushes.LightGray))
                .OrderBy(rectangle => rectangle.Bounds.X)
                .ToArray();
            var expectedCenterX = track.Margin.Left + trackRects[1].Bounds.Right;

            slider.Resources["SegmentedSliderThumbSize"] = 30.0;
            window.UpdateLayout();

            Assert.Equal(30, thumb.Bounds.Width, 1);
            var actualCenterX = transform.X + thumb.Bounds.Width / 2;
            Assert.InRange(actualCenterX, expectedCenterX - 0.5, expectedCenterX + 0.5);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void SegmentedSlider_TrackMarginResource_RepositionsThumbVertically()
    {
        var slider = new SegmentedSlider
        {
            Width = 400,
            Maximum = 100,
            Value = 40,
            SegmentCount = 5,
            TitleVisibility = SegmentTitleVisibility.Collapsed,
            Background = Brushes.LightGray
        };
        slider.Resources["SegmentedSliderTrackHeight"] = 8.0;
        slider.Resources["SegmentedSliderTrackMargin"] = new Thickness(10, 14, 10, 0);
        var window = ShowInWindow(slider);

        try
        {
            var track = GetTemplatePart<Panel>(slider, "PART_Track");
            var thumb = GetTemplatePart<Thumb>(slider, "PART_Thumb");
            var transform = Assert.IsType<TranslateTransform>(thumb.RenderTransform);
            var trackRectangle = slider.GetVisualDescendants()
                .OfType<Rectangle>()
                .First(rectangle => Equals(rectangle.Fill, Brushes.LightGray));

            var expectedCenterY = track.Bounds.Y + trackRectangle.Bounds.Y + trackRectangle.Bounds.Height / 2;
            var actualCenterY = transform.Y + thumb.Bounds.Height / 2;

            Assert.InRange(actualCenterY, expectedCenterY - 0.5, expectedCenterY + 0.5);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void SegmentedSlider_ThumbDrag_UsesArrangedTrackOffset()
    {
        var slider = new TestSegmentedSlider
        {
            Width = 400,
            Maximum = 100,
            Value = 40,
            SegmentCount = 5,
            TitleVisibility = SegmentTitleVisibility.Collapsed,
            Background = Brushes.LightGray
        };
        slider.Resources["SegmentedSliderTrackMargin"] = new Thickness(0);
        var window = ShowInWindow(slider);

        try
        {
            var track = GetTemplatePart<Panel>(slider, "PART_Track");
            var thumb = GetTemplatePart<Thumb>(slider, "PART_Thumb");
            var shiftedTrackX = 40.0;
            track.Arrange(new Rect(shiftedTrackX, track.Bounds.Y, track.Bounds.Width, track.Bounds.Height));
            Assert.Equal(shiftedTrackX, track.Bounds.X);
            Assert.Equal(0, track.Margin.Left);

            slider.Value = 20;

            Assert.IsType<TranslateTransform>(thumb.RenderTransform);

            slider.RaiseThumbDragDeltaForTest(new Vector(0, 0));

            Assert.Equal(20, slider.Value, 6);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void SegmentedSlider_InheritedStylingProperties_ApplyToGeneratedVisuals()
    {
        var slider = new SegmentedSlider
        {
            Width = 400,
            Background = Brushes.LightGray,
            Foreground = Brushes.SeaGreen,
            CornerRadius = new CornerRadius(6),
            FontSize = 15,
            Segments = new ObservableCollection<SegmentedSliderSegment>
            {
                new() { Title = "Low" },
                new() { Title = "High" }
            }
        };
        slider.Resources["SegmentedSliderTrackHeight"] = 10.0;
        var window = ShowInWindow(slider);

        try
        {
            var rectangles = slider.GetVisualDescendants().OfType<Rectangle>().ToArray();
            var trackRectangles = rectangles.Where(rectangle => Equals(rectangle.Fill, Brushes.LightGray)).ToArray();
            var titleBlock = slider.GetVisualDescendants().OfType<TextBlock>().Single(textBlock => textBlock.Text == "Low");

            Assert.NotEmpty(trackRectangles);
            Assert.Contains(rectangles, rectangle => Equals(rectangle.Fill, Brushes.SeaGreen));
            Assert.All(trackRectangles, rectangle => Assert.Equal(6, rectangle.RadiusX));
            Assert.All(trackRectangles, rectangle => Assert.Equal(10, rectangle.Height));
            Assert.Equal(15, titleBlock.FontSize);
            Assert.Equal(Brushes.SeaGreen, titleBlock.Foreground);

            slider.Resources["SegmentedSliderTrackHeight"] = 12.0;
            slider.UpdateLayout();

            rectangles = slider.GetVisualDescendants().OfType<Rectangle>().ToArray();
            trackRectangles = rectangles.Where(rectangle => Equals(rectangle.Fill, Brushes.LightGray)).ToArray();
            Assert.All(trackRectangles, rectangle => Assert.Equal(12, rectangle.Height));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void SegmentedSlider_SegmentBrushChanges_UpdateGeneratedVisuals()
    {
        var firstSegment = new SegmentedSliderSegment
        {
            Title = "Low",
            FillBrush = Brushes.DodgerBlue,
            TrackBrush = Brushes.LightGray
        };
        var slider = new SegmentedSlider
        {
            Width = 400,
            Maximum = 100,
            Value = 50,
            Segments = new ObservableCollection<SegmentedSliderSegment>
            {
                firstSegment,
                new() { Title = "High" }
            }
        };
        var window = ShowInWindow(slider);

        try
        {
            var trackRectangle = slider.GetVisualDescendants()
                .OfType<Rectangle>()
                .Single(rectangle => Equals(rectangle.Fill, Brushes.LightGray));
            var fillRectangle = slider.GetVisualDescendants()
                .OfType<Rectangle>()
                .Single(rectangle => Equals(rectangle.Fill, Brushes.DodgerBlue));

            firstSegment.TrackBrush = Brushes.Pink;
            firstSegment.FillBrush = Brushes.SeaGreen;

            Assert.Equal(Brushes.Pink, trackRectangle.Fill);
            Assert.Equal(Brushes.SeaGreen, fillRectangle.Fill);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void SegmentedSlider_TitleVisibility_ActiveSegmentOnly_HidesInactiveTitles()
    {
        var slider = new SegmentedSlider
        {
            Width = 400,
            Maximum = 100,
            Value = 60,
            TitleVisibility = SegmentTitleVisibility.ActiveSegmentOnly,
            Segments = new ObservableCollection<SegmentedSliderSegment>
            {
                new() { Title = "Low" },
                new() { Title = "High" }
            }
        };
        var window = ShowInWindow(slider);

        try
        {
            var titleBlocks = slider.GetVisualDescendants().OfType<TextBlock>().ToArray();

            Assert.False(titleBlocks.Single(textBlock => textBlock.Text == "Low").IsVisible);
            Assert.True(titleBlocks.Single(textBlock => textBlock.Text == "High").IsVisible);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void SegmentedSlider_SegmentsCollectionChanges_RebuildVisuals()
    {
        var segments = new ObservableCollection<SegmentedSliderSegment>
        {
            new() { Title = "Low" }
        };
        var slider = new SegmentedSlider
        {
            Width = 400,
            Segments = segments
        };
        var window = ShowInWindow(slider);

        try
        {
            segments.Add(new SegmentedSliderSegment { Title = "High" });

            var titles = slider.GetVisualDescendants().OfType<TextBlock>().Select(textBlock => textBlock.Text).ToArray();
            Assert.Contains("High", titles);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void SegmentedSlider_ReattachedControl_ObservesExistingSegmentsCollection()
    {
        var segments = new ObservableCollection<SegmentedSliderSegment>
        {
            new() { Title = "Low" }
        };
        var slider = new SegmentedSlider
        {
            Width = 400,
            Segments = segments
        };
        var window = ShowInWindow(slider);

        try
        {
            window.Content = null;
            window.UpdateLayout();
            window.Content = slider;
            window.UpdateLayout();

            segments.Add(new SegmentedSliderSegment { Title = "High" });
            window.UpdateLayout();

            var titles = slider.GetVisualDescendants().OfType<TextBlock>().Select(textBlock => textBlock.Text).ToArray();
            Assert.Contains("High", titles);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void SegmentedSlider_ReadOnly_SetsPseudoClass()
    {
        var slider = new SegmentedSlider { IsReadOnly = true };

        Assert.Contains(slider.Classes, cssClass => cssClass == ":readonly");
    }

    [AvaloniaFact]
    public void SegmentedSlider_AutomationPeer_ExposesRangeValueProvider()
    {
        var slider = new SegmentedSlider
        {
            Minimum = 0,
            Maximum = 100,
            Value = 25,
            SmallChange = 5,
            LargeChange = 20
        };
        var provider = Assert.IsAssignableFrom<IRangeValueProvider>(new SegmentedSliderAutomationPeer(slider));

        Assert.False(provider.IsReadOnly);
        Assert.Equal(0, provider.Minimum);
        Assert.Equal(100, provider.Maximum);
        Assert.Equal(25, provider.Value);
        Assert.Equal(5, provider.SmallChange);
        Assert.Equal(20, provider.LargeChange);

        provider.SetValue(75);
        Assert.Equal(75, slider.Value);

        slider.IsReadOnly = true;
        Assert.True(provider.IsReadOnly);
        provider.SetValue(20);
        Assert.Equal(75, slider.Value);

        slider.IsEnabled = false;
        Assert.Throws<ElementNotEnabledException>(() => provider.SetValue(20));
    }

    [AvaloniaFact]
    public void SegmentedSlider_AutomationPeer_UsesSegmentAndRangeInFallbackName()
    {
        var slider = new SegmentedSlider
        {
            Minimum = 0.5,
            Maximum = 2.5,
            Value = 1.25,
            Segments = new ObservableCollection<SegmentedSliderSegment>
            {
                new() { Title = "Low" },
                new() { Title = "High" }
            }
        };
        var peer = new SegmentedSliderAutomationPeer(slider);

        Assert.Equal($"Low ({1.25:G} from {0.5:G} to {2.5:G})", peer.GetName());
    }

    [AvaloniaFact]
    public void SegmentedSlider_PointerCaptureLost_CompletesDragAndSnaps()
    {
        var slider = new TestSegmentedSlider
        {
            Maximum = 100,
            SegmentCount = 5,
            Value = 34,
            IsSnapToSegmentEnabled = true
        };

        slider.IsDraggingForTest = true;

        slider.RaisePointerCaptureLostForTest();

        Assert.False(slider.IsDraggingForTest);
        Assert.Equal(30, slider.Value, 6);

        slider.RaisePointerCaptureLostForTest();

        Assert.Equal(30, slider.Value, 6);
    }

    [AvaloniaFact]
    public void SegmentedSlider_SnapToSegment_UsesNearestWeightedSegmentCenter()
    {
        var slider = new TestSegmentedSlider
        {
            Maximum = 100,
            Value = 77,
            IsSnapToSegmentEnabled = true,
            Segments = new ObservableCollection<SegmentedSliderSegment>
            {
                new() { WidthRatio = 1 },
                new() { WidthRatio = 3 },
                new() { WidthRatio = 1 }
            }
        };

        slider.IsDraggingForTest = true;

        slider.RaisePointerCaptureLostForTest();

        Assert.Equal(90, slider.Value, 6);
    }

    private static Window ShowInWindow(Control control)
    {
        var window = new Window
        {
            Width = 500,
            Height = 200,
            Content = control
        };

        window.Show();
        control.ApplyTemplate();
        control.UpdateLayout();

        return window;
    }

    private static T GetTemplatePart<T>(Control control, string name) where T : Control
    {
        return control.GetVisualDescendants().OfType<T>().Single(part => part.Name == name);
    }

    private sealed class TestSegmentedSlider : SegmentedSlider
    {
        private static readonly FieldInfo IsDraggingField =
            typeof(SegmentedSlider).GetField("_isDragging", BindingFlags.Instance | BindingFlags.NonPublic)!;
        private static readonly MethodInfo OnTrackPointerPressedMethod =
            typeof(SegmentedSlider).GetMethod("OnTrackPointerPressed", BindingFlags.Instance | BindingFlags.NonPublic)!;
        private static readonly MethodInfo OnThumbDragDeltaMethod =
            typeof(SegmentedSlider).GetMethod("OnThumbDragDelta", BindingFlags.Instance | BindingFlags.NonPublic)!;

        protected override Type StyleKeyOverride => typeof(SegmentedSlider);

        public bool IsDraggingForTest
        {
            get => (bool)IsDraggingField.GetValue(this)!;
            set => IsDraggingField.SetValue(this, value);
        }

        public void RaisePointerCaptureLostForTest()
        {
            OnPointerCaptureLost(new PointerCaptureLostEventArgs(this, null!));
        }

        public PointerPressedEventArgs RaiseTrackPointerPressedForTest(
            Control track,
            IPointer pointer,
            Point position,
            RawInputModifiers modifiers,
            PointerUpdateKind updateKind)
        {
            var args = new PointerPressedEventArgs(
                track,
                pointer,
                track,
                position,
                0,
                new PointerPointProperties(modifiers, updateKind),
                KeyModifiers.None);

            OnTrackPointerPressedMethod.Invoke(this, [track, args]);
            return args;
        }

        public void RaiseThumbDragDeltaForTest(Vector vector)
        {
            OnThumbDragDeltaMethod.Invoke(this, [this, new VectorEventArgs { Vector = vector }]);
        }
    }
}
