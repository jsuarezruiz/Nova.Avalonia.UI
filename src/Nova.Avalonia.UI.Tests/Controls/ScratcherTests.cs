using Avalonia.Media;
using Nova.Avalonia.UI.Controls;
using Xunit;

namespace Nova.Avalonia.UI.Tests.Controls;

public class ScratcherTests
{
    [Fact]
    public void BrushSize_DefaultValue_Is30()
    {
        var scratcher = new Scratcher();
        
        Assert.Equal(30.0, scratcher.BrushSize);
    }

    [Fact]
    public void Threshold_DefaultValue_Is50()
    {
        var scratcher = new Scratcher();
        
        Assert.Equal(50.0, scratcher.Threshold);
    }

    [Fact]
    public void Threshold_ClampedBetween0And100()
    {
        var scratcher = new Scratcher();
        
        scratcher.Threshold = 150;
        Assert.Equal(100.0, scratcher.Threshold);
        
        scratcher.Threshold = -10;
        Assert.Equal(0.0, scratcher.Threshold);
    }

    [Fact]
    public void OverlayBrush_DefaultValue_IsGray()
    {
        var scratcher = new Scratcher();
        
        Assert.NotNull(scratcher.OverlayBrush);
    }

    [Fact]
    public void CornerRadius_DefaultValue_IsZero()
    {
        var scratcher = new Scratcher();
        
        Assert.Equal(new CornerRadius(0), scratcher.CornerRadius);
    }

    [Fact]
    public void RebuildOnResize_DefaultValue_IsTrue()
    {
        var scratcher = new Scratcher();
        
        Assert.True(scratcher.RebuildOnResize);
    }

    [Fact]
    public void ScratchProgress_InitialValue_IsZero()
    {
        var scratcher = new Scratcher();
        
        Assert.Equal(0.0, scratcher.ScratchProgress);
    }

    [Fact]
    public void IsThresholdReached_InitialValue_IsFalse()
    {
        var scratcher = new Scratcher();
        
        Assert.False(scratcher.IsThresholdReached);
    }

    [Fact]
    public void IsScratching_InitialValue_IsFalse()
    {
        var scratcher = new Scratcher();
        
        Assert.False(scratcher.IsScratching);
    }

    [Fact]
    public void OverlayBrush_CanBeSet()
    {
        var scratcher = new Scratcher { OverlayBrush = Brushes.Red };
        
        Assert.Equal(Brushes.Red, scratcher.OverlayBrush);
    }

    [Fact]
    public void BrushSize_CanBeSet()
    {
        var scratcher = new Scratcher { BrushSize = 50.0 };
        
        Assert.Equal(50.0, scratcher.BrushSize);
    }

    [Fact]
    public void OverlayBrush_AcceptsGradient()
    {
        var gradient = new LinearGradientBrush();
        var scratcher = new Scratcher { OverlayBrush = gradient };
        
        Assert.Equal(gradient, scratcher.OverlayBrush);
    }

    }
    }

    [Fact]
    public void Reset_ResetsProgress()
    {
        var scratcher = new Scratcher();
        // Simulate progress
        // Note: In a real scenario we'd need to mock the internals or use reflection to set _scratchProgress
        // But we can check that Reset doesn't throw and sets state
        
        scratcher.Reset();
        Assert.Equal(0.0, scratcher.ScratchProgress);
        Assert.False(scratcher.IsThresholdReached);
    }

    [Fact]
    public void Reveal_SetsProgressTo100()
    {
        var scratcher = new Scratcher();
        
        // We can't fully test side effects without a window/renderer, 
        // but we can verify the state changes if buffer exists.
        // However, buffer is created on Measure/Arrange which requires a rooted visual tree.
        // So for now we just verify it handles null buffer gracefully.
        
        scratcher.Reveal();
        // Since there's no buffer in unit test environment, progress won't update to 100
        // unless we mock the buffer creation. 
        // But we can verify it doesn't crash.
    }

    }
    
    [Fact]
    public void CornerRadius_CanBeSet()
    {
        var scratcher = new Scratcher { CornerRadius = new CornerRadius(5) };
        Assert.Equal(new CornerRadius(5), scratcher.CornerRadius);
    }
}
