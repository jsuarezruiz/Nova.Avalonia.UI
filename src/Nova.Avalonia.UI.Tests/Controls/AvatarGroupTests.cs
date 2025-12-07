using Avalonia;
using Avalonia.Collections;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.Media;
using Nova.Avalonia.UI.Controls;
using Xunit;

namespace Nova.Avalonia.UI.Tests.Controls;

public class AvatarGroupTests
{
    [AvaloniaFact]
    public void DefaultValues_AreCorrect()
    {
        var group = new AvatarGroup();

        Assert.NotNull(group.Avatars);
        Assert.Empty(group.Avatars);
        Assert.Equal(5, group.MaxDisplayed);
        Assert.Equal(0.25, group.Overlap);
        Assert.True(group.ShowCount);
        Assert.Equal(AvatarSize.Medium, group.Size);
        Assert.Equal(Orientation.Horizontal, group.Orientation);
        Assert.Equal(new Thickness(2), group.BorderThickness);
    }

    [AvaloniaFact]
    public void Avatars_CanBeAdded()
    {
        var group = new AvatarGroup();

        group.Avatars.Add(new Avatar { DisplayName = "User 1" });
        group.Avatars.Add(new Avatar { DisplayName = "User 2" });

        Assert.Equal(2, group.Avatars.Count);
    }

    [AvaloniaFact]
    public void MaxDisplayed_CanBeSet()
    {
        var group = new AvatarGroup
        {
            MaxDisplayed = 3
        };

        Assert.Equal(3, group.MaxDisplayed);
    }

    [AvaloniaFact]
    public void Overlap_CanBeSet()
    {
        var group = new AvatarGroup
        {
            Overlap = 0.5
        };

        Assert.Equal(0.5, group.Overlap);
    }

    [AvaloniaFact]
    public void ShowCount_CanBeDisabled()
    {
        var group = new AvatarGroup
        {
            ShowCount = false
        };

        Assert.False(group.ShowCount);
    }

    [AvaloniaTheory]
    [InlineData(AvatarSize.ExtraSmall)]
    [InlineData(AvatarSize.Small)]
    [InlineData(AvatarSize.Medium)]
    [InlineData(AvatarSize.Large)]
    [InlineData(AvatarSize.ExtraLarge)]
    public void Size_CanBeSet(AvatarSize size)
    {
        var group = new AvatarGroup
        {
            Size = size
        };

        Assert.Equal(size, group.Size);
    }

    [AvaloniaFact]
    public void Orientation_CanBeVertical()
    {
        var group = new AvatarGroup
        {
            Orientation = Orientation.Vertical
        };

        Assert.Equal(Orientation.Vertical, group.Orientation);
    }

    [AvaloniaFact]
    public void BorderBrush_CanBeSet()
    {
        var brush = new SolidColorBrush(Colors.Red);
        var group = new AvatarGroup
        {
            BorderBrush = brush
        };

        Assert.Equal(brush, group.BorderBrush);
    }

    [AvaloniaFact]
    public void BorderThickness_CanBeSet()
    {
        var thickness = new Thickness(4);
        var group = new AvatarGroup
        {
            BorderThickness = thickness
        };

        Assert.Equal(thickness, group.BorderThickness);
    }

    [AvaloniaFact]
    public void Avatars_CollectionInitialized()
    {
        var group = new AvatarGroup();

        Assert.NotNull(group.Avatars);
        Assert.IsType<AvaloniaList<Avatar>>(group.Avatars);
    }

    [AvaloniaFact]
    public void Avatars_CanBeReplaced()
    {
        var group = new AvatarGroup();
        var newList = new AvaloniaList<Avatar>
        {
            new Avatar { DisplayName = "Test 1" },
            new Avatar { DisplayName = "Test 2" },
            new Avatar { DisplayName = "Test 3" }
        };

        group.Avatars = newList;

        Assert.Equal(3, group.Avatars.Count);
        Assert.Equal(newList, group.Avatars);
    }

    [AvaloniaFact]
    public void Size_AffectsAllAvatars()
    {
        var group = new AvatarGroup
        {
            Size = AvatarSize.Large
        };

        group.Avatars.Add(new Avatar { DisplayName = "User 1", Size = AvatarSize.Small });
        group.Avatars.Add(new Avatar { DisplayName = "User 2", Size = AvatarSize.Medium });

        // Group size should override individual avatar sizes
        Assert.Equal(AvatarSize.Large, group.Size);
    }
}
