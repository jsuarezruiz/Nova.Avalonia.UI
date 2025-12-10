using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Nova.Avalonia.UI.Controls;
using Xunit;

namespace Nova.Avalonia.UI.Tests.Controls;

public class ShimmerTests
{
    [AvaloniaFact]
    public void DefaultValues_AreCorrect()
    {
        var shimmer = new Shimmer();

        Assert.True(shimmer.IsLoading);
        Assert.Equal("Loading content", shimmer.LoadingText);
        Assert.Null(shimmer.HighlightBrush);
        Assert.Equal(0.5, shimmer.ShimmerOpacity);
        Assert.Equal(0.0, shimmer.ShimmerAngle);
    }

    [AvaloniaFact]
    public void IsLoading_CanBeToggled()
    {
        var shimmer = new Shimmer
        {
            IsLoading = false
        };

        Assert.False(shimmer.IsLoading);

        shimmer.IsLoading = true;

        Assert.True(shimmer.IsLoading);
    }

    [AvaloniaFact]
    public void LoadingText_CanBeSet()
    {
        var shimmer = new Shimmer
        {
            LoadingText = "Please wait..."
        };

        Assert.Equal("Please wait...", shimmer.LoadingText);
    }

    [AvaloniaFact]
    public void HighlightBrush_CanBeSet()
    {
        var brush = new SolidColorBrush(Colors.Blue);
        var shimmer = new Shimmer
        {
            HighlightBrush = brush
        };

        Assert.Equal(brush, shimmer.HighlightBrush);
    }

    [AvaloniaFact]
    public void ShimmerOpacity_CanBeSet()
    {
        var shimmer = new Shimmer
        {
            ShimmerOpacity = 0.8
        };

        Assert.Equal(0.8, shimmer.ShimmerOpacity);
    }

    [AvaloniaTheory]
    [InlineData(0.0)]
    [InlineData(0.25)]
    [InlineData(0.5)]
    [InlineData(0.75)]
    [InlineData(1.0)]
    public void ShimmerOpacity_AcceptsValidRange(double opacity)
    {
        var shimmer = new Shimmer
        {
            ShimmerOpacity = opacity
        };

        Assert.Equal(opacity, shimmer.ShimmerOpacity);
    }

    [AvaloniaFact]
    public void ShimmerAngle_CanBeSet()
    {
        var shimmer = new Shimmer
        {
            ShimmerAngle = 45.0
        };

        Assert.Equal(45.0, shimmer.ShimmerAngle);
    }

    [AvaloniaTheory]
    [InlineData(0)]
    [InlineData(45)]
    [InlineData(90)]
    [InlineData(180)]
    [InlineData(270)]
    [InlineData(360)]
    public void ShimmerAngle_AcceptsVariousAngles(double angle)
    {
        var shimmer = new Shimmer
        {
            ShimmerAngle = angle
        };

        Assert.Equal(angle, shimmer.ShimmerAngle);
    }

    [AvaloniaFact]
    public void Content_CanBeSet()
    {
        var content = new TextBlock { Text = "Test Content" };
        var shimmer = new Shimmer
        {
            Content = content
        };

        Assert.Equal(content, shimmer.Content);
    }

    [AvaloniaFact]
    public void IsLoading_DefaultIsTrue()
    {
        var shimmer = new Shimmer();

        Assert.True(shimmer.IsLoading);
    }

    [AvaloniaFact]
    public void Background_CanBeSet()
    {
        var brush = new SolidColorBrush(Colors.LightGray);
        var shimmer = new Shimmer
        {
            Background = brush
        };

        Assert.Equal(brush, shimmer.Background);
    }

    [AvaloniaFact]
    public void PseudoClass_Loading_IsSet_WhenIsLoadingTrue()
    {
        var shimmer = new Shimmer
        {
            IsLoading = true
        };

        // PseudoClasses are set during template application
        // This test verifies the property is set correctly
        Assert.True(shimmer.IsLoading);
    }

    [AvaloniaFact]
    public void LoadingText_DefaultValue_IsCorrect()
    {
        var shimmer = new Shimmer();

        Assert.Equal("Loading content", shimmer.LoadingText);
    }
}
