using System;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// Provides animation helpers for the Showcase control.
/// </summary>
public static class ShowcaseAnimations
{
    /// <summary>
    /// Creates a fade-in and scale animation.
    /// </summary>
    public static Animation CreateShowAnimation(TimeSpan? duration = null)
    {
        return new Animation
        {
            Duration = duration ?? TimeSpan.FromMilliseconds(300),
            Easing = new CubicEaseOut(),
            Children =
            {
                new KeyFrame
                {
                    Setters =
                    {
                        new Setter(Visual.OpacityProperty, 0d),
                        new Setter(ScaleTransform.ScaleXProperty, 0.9d),
                        new Setter(ScaleTransform.ScaleYProperty, 0.9d)
                    },
                    Cue = new Cue(0)
                },
                new KeyFrame
                {
                    Setters =
                    {
                        new Setter(Visual.OpacityProperty, 1d),
                        new Setter(ScaleTransform.ScaleXProperty, 1d),
                        new Setter(ScaleTransform.ScaleYProperty, 1d)
                    },
                    Cue = new Cue(1)
                }
            }
        };
    }
    
    /// <summary>
    /// Creates a fade-out and scale animation.
    /// </summary>
    public static Animation CreateHideAnimation(TimeSpan? duration = null)
    {
        return new Animation
        {
            Duration = duration ?? TimeSpan.FromMilliseconds(200),
            Easing = new CubicEaseIn(),
            Children =
            {
                new KeyFrame
                {
                    Setters =
                    {
                        new Setter(Visual.OpacityProperty, 1d),
                        new Setter(ScaleTransform.ScaleXProperty, 1d),
                        new Setter(ScaleTransform.ScaleYProperty, 1d)
                    },
                    Cue = new Cue(0)
                },
                new KeyFrame
                {
                    Setters =
                    {
                        new Setter(Visual.OpacityProperty, 0d),
                        new Setter(ScaleTransform.ScaleXProperty, 0.9d),
                        new Setter(ScaleTransform.ScaleYProperty, 0.9d)
                    },
                    Cue = new Cue(1)
                }
            }
        };
    }
    
    /// <summary>
    /// Creates a subtle pulse animation.
    /// </summary>
    public static Animation CreatePulseAnimation()
    {
        return new Animation
        {
            Duration = TimeSpan.FromMilliseconds(1500),
            Easing = new SineEaseInOut(),
            IterationCount = IterationCount.Infinite,
            Children =
            {
                new KeyFrame
                {
                    Setters = { new Setter(Visual.OpacityProperty, 1d) },
                    Cue = new Cue(0)
                },
                new KeyFrame
                {
                    Setters = { new Setter(Visual.OpacityProperty, 0.6d) },
                    Cue = new Cue(0.5)
                },
                new KeyFrame
                {
                    Setters = { new Setter(Visual.OpacityProperty, 1d) },
                    Cue = new Cue(1)
                }
            }
        };
    }
}
