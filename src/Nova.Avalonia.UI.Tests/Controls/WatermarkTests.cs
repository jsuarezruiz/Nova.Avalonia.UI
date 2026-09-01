using System.Linq;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Automation.Peers;
using Avalonia.Platform;
using Avalonia.VisualTree;
using Nova.Avalonia.UI.Controls;
using Xunit;

namespace Nova.Avalonia.UI.Tests.Controls;

public class WatermarkTests
{
    [AvaloniaFact]
    public void Watermark_Defaults()
    {
        var watermark = new Watermark();
        
        Assert.Null(watermark.Text);
        Assert.Null(watermark.Source);
        Assert.Equal(-30.0, watermark.Angle);
        Assert.Equal(50.0, watermark.HorizontalSpacing);
        Assert.Equal(30.0, watermark.VerticalSpacing);
        Assert.Equal(0.15, watermark.WatermarkOpacity);
        Assert.Equal(14.0, watermark.WatermarkFontSize);
        Assert.Equal(FontFamily.Default, watermark.WatermarkFontFamily);
        Assert.Equal(FontWeight.Normal, watermark.WatermarkFontWeight);
        Assert.Equal(FontStyle.Normal, watermark.WatermarkFontStyle);
    }

    [AvaloniaFact]
    public void Watermark_Sets_Text()
    {
        var watermark = new Watermark { Text = "CONFIDENTIAL" };
        Assert.Equal("CONFIDENTIAL", watermark.Text);
    }

    [AvaloniaFact]
    public void Watermark_Sets_Angle()
    {
        var watermark = new Watermark { Angle = -45 };
        Assert.Equal(-45.0, watermark.Angle);
    }

    [AvaloniaFact]
    public void Watermark_Sets_Angle_Positive()
    {
        var watermark = new Watermark { Angle = 30 };
        Assert.Equal(30.0, watermark.Angle);
    }

    [AvaloniaFact]
    public void Watermark_Sets_HorizontalSpacing()
    {
        var watermark = new Watermark { HorizontalSpacing = 100 };
        Assert.Equal(100.0, watermark.HorizontalSpacing);
    }

    [AvaloniaFact]
    public void Watermark_Sets_VerticalSpacing()
    {
        var watermark = new Watermark { VerticalSpacing = 60 };
        Assert.Equal(60.0, watermark.VerticalSpacing);
    }

    [AvaloniaFact]
    public void Watermark_Sets_WatermarkOpacity()
    {
        var watermark = new Watermark { WatermarkOpacity = 0.25 };
        Assert.Equal(0.25, watermark.WatermarkOpacity);
    }

    [AvaloniaFact]
    public void Watermark_Opacity_Coerced_Above_One()
    {
        var watermark = new Watermark { WatermarkOpacity = 1.5 };
        Assert.Equal(1.0, watermark.WatermarkOpacity);
    }

    [AvaloniaFact]
    public void Watermark_Opacity_Coerced_Below_Zero()
    {
        var watermark = new Watermark { WatermarkOpacity = -0.5 };
        Assert.Equal(0.0, watermark.WatermarkOpacity);
    }

    [AvaloniaFact]
    public void Watermark_Sets_WatermarkFontSize()
    {
        var watermark = new Watermark { WatermarkFontSize = 20 };
        Assert.Equal(20.0, watermark.WatermarkFontSize);
    }

    [AvaloniaFact]
    public void Watermark_Sets_WatermarkFontFamily()
    {
        var watermark = new Watermark { WatermarkFontFamily = new FontFamily("Arial") };
        Assert.Equal("Arial", watermark.WatermarkFontFamily.Name);
    }

    [AvaloniaFact]
    public void Watermark_Sets_WatermarkFontWeight()
    {
        var watermark = new Watermark { WatermarkFontWeight = FontWeight.Bold };
        Assert.Equal(FontWeight.Bold, watermark.WatermarkFontWeight);
    }

    [AvaloniaFact]
    public void Watermark_Sets_WatermarkFontStyle()
    {
        var watermark = new Watermark { WatermarkFontStyle = FontStyle.Italic };
        Assert.Equal(FontStyle.Italic, watermark.WatermarkFontStyle);
    }

    [AvaloniaFact]
    public void Watermark_Sets_WatermarkForeground()
    {
        var watermark = new Watermark { WatermarkForeground = Brushes.Red };
        Assert.Equal(Brushes.Red, watermark.WatermarkForeground);
    }

    [AvaloniaFact]
    public void Watermark_Content_Can_Be_Set()
    {
        var watermark = new Watermark { Content = "Child Content" };
        Assert.Equal("Child Content", watermark.Content);
    }

    [AvaloniaFact]
    public void Watermark_Defaults_FlowDirection_LeftToRight()
    {
        var watermark = new Watermark();
        Assert.Equal(FlowDirection.LeftToRight, watermark.WatermarkFlowDirection);
    }

    [AvaloniaFact]
    public void Watermark_Sets_FlowDirection_RightToLeft()
    {
        var watermark = new Watermark { WatermarkFlowDirection = FlowDirection.RightToLeft };
        Assert.Equal(FlowDirection.RightToLeft, watermark.WatermarkFlowDirection);
    }

    [AvaloniaFact]
    public void Watermark_Creates_AutomationPeer()
    {
        var watermark = new Watermark();
        var peer = ControlAutomationPeer.CreatePeerForElement(watermark);
        
        Assert.IsType<WatermarkAutomationPeer>(peer);
    }

    [AvaloniaFact]
    public void Watermark_AutomationPeer_Exposes_Text()
    {
        var watermark = new Watermark { Text = "SECRET" };
        var peer = ControlAutomationPeer.CreatePeerForElement(watermark);
        
        Assert.Equal("SECRET", peer.GetName());
    }

    [AvaloniaFact]
    public void Watermark_AutomationPeer_Exposes_Source_Name_When_No_Text()
    {
        var watermark = new Watermark { Source = new DrawingImage() };
        var peer = ControlAutomationPeer.CreatePeerForElement(watermark);
        
        Assert.Equal("Watermark image", peer.GetName());
    }

    [AvaloniaFact]
    public void Watermark_Renders_Above_Opaque_Content()
    {
        AssertOverlayChangesPixels(new Watermark
        {
            Text = "VISIBLE",
            Angle = 0,
            HorizontalSpacing = 20,
            VerticalSpacing = 10,
            WatermarkFontSize = 24,
            WatermarkForeground = Brushes.Black
        });
    }

    [AvaloniaFact]
    public void Image_Watermark_Renders_Above_Opaque_Content()
    {
        AssertOverlayChangesPixels(new Watermark
        {
            Source = new DrawingImage
            {
                Drawing = new GeometryDrawing
                {
                    Brush = Brushes.Black,
                    Geometry = new RectangleGeometry(new Rect(0, 0, 20, 20))
                }
            },
            Angle = 0,
            HorizontalSpacing = 20,
            VerticalSpacing = 10
        });
    }

    private static void AssertOverlayChangesPixels(Watermark watermark)
    {
        watermark.WatermarkOpacity = 0;
        watermark.Content = new Border { Background = Brushes.White };

        var window = new Window
        {
            Width = 240,
            Height = 140,
            Content = watermark
        };
        window.Show();

        try
        {
            var overlay = Assert.Single(watermark.GetVisualDescendants().OfType<WatermarkOverlayPresenter>());
            Assert.Same(watermark, overlay.Owner);
            Assert.True(overlay.Bounds.Width > 0);
            Assert.True(overlay.Bounds.Height > 0);

            var withoutOverlay = CapturePixels(window);

            watermark.WatermarkOpacity = 1;
            var withOverlay = CapturePixels(window);

            Assert.NotEqual(withoutOverlay, withOverlay);
        }
        finally
        {
            window.Close();
        }
    }

    private static byte[] CapturePixels(Window window)
    {
        using var bitmap = window.CaptureRenderedFrame();
        Assert.NotNull(bitmap);

        using var copy = new WriteableBitmap(
            bitmap.PixelSize,
            bitmap.Dpi,
            PixelFormat.Bgra8888,
            AlphaFormat.Premul);
        using var framebuffer = copy.Lock();
        bitmap.CopyPixels(framebuffer);

        var pixels = new byte[framebuffer.RowBytes * framebuffer.Size.Height];
        Marshal.Copy(framebuffer.Address, pixels, 0, pixels.Length);
        return pixels;
    }
}
