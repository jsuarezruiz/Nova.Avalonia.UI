using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Nova.Avalonia.UI.Controls;
using Xunit;

namespace Nova.Avalonia.UI.Tests.Controls;

public class AvatarTests
{
    [AvaloniaFact]
    public void DefaultValues_AreCorrect()
    {
        var avatar = new Avatar();

        Assert.Equal(string.Empty, avatar.DisplayName);
        Assert.Null(avatar.ImageSource);
        Assert.Null(avatar.Icon);
        Assert.Null(avatar.Content);
        Assert.Equal(AvatarDisplayMode.Auto, avatar.DisplayMode);
        Assert.Equal(AvatarShape.Circle, avatar.Shape);
        Assert.Equal(AvatarSize.Medium, avatar.Size);
        Assert.Equal(48.0, avatar.CustomSize);
        Assert.True(avatar.AutoGenerateBackground);
        Assert.Equal(AvatarStatus.None, avatar.Status);
        Assert.Equal(string.Empty, avatar.Initials);
        Assert.Equal(2, avatar.MaxInitials);
        Assert.True(avatar.ShowTooltip);
    }

    [AvaloniaFact]
    public void DisplayName_CanBeSet()
    {
        var avatar = new Avatar
        {
            DisplayName = "John Doe"
        };

        Assert.Equal("John Doe", avatar.DisplayName);
    }

    [AvaloniaTheory]
    [InlineData(AvatarSize.ExtraSmall)]
    [InlineData(AvatarSize.Small)]
    [InlineData(AvatarSize.Medium)]
    [InlineData(AvatarSize.Large)]
    [InlineData(AvatarSize.ExtraLarge)]
    [InlineData(AvatarSize.Custom)]
    public void Size_CanBeSet(AvatarSize size)
    {
        var avatar = new Avatar
        {
            Size = size
        };

        Assert.Equal(size, avatar.Size);
    }

    [AvaloniaFact]
    public void CustomSize_IsUsedWhenSizeIsCustom()
    {
        var avatar = new Avatar
        {
            Size = AvatarSize.Custom,
            CustomSize = 100
        };

        Assert.Equal(AvatarSize.Custom, avatar.Size);
        Assert.Equal(100, avatar.CustomSize);
    }

    [AvaloniaTheory]
    [InlineData(AvatarShape.Circle)]
    [InlineData(AvatarShape.Square)]
    [InlineData(AvatarShape.Rectangle)]
    public void Shape_CanBeSet(AvatarShape shape)
    {
        var avatar = new Avatar
        {
            Shape = shape
        };

        Assert.Equal(shape, avatar.Shape);
    }

    [AvaloniaTheory]
    [InlineData(AvatarDisplayMode.Auto)]
    [InlineData(AvatarDisplayMode.Image)]
    [InlineData(AvatarDisplayMode.Initials)]
    [InlineData(AvatarDisplayMode.Icon)]
    [InlineData(AvatarDisplayMode.Content)]
    public void DisplayMode_CanBeSet(AvatarDisplayMode mode)
    {
        var avatar = new Avatar
        {
            DisplayMode = mode
        };

        Assert.Equal(mode, avatar.DisplayMode);
    }

    [AvaloniaTheory]
    [InlineData(AvatarStatus.None)]
    [InlineData(AvatarStatus.Online)]
    [InlineData(AvatarStatus.Offline)]
    [InlineData(AvatarStatus.Away)]
    [InlineData(AvatarStatus.Busy)]
    [InlineData(AvatarStatus.DoNotDisturb)]
    public void Status_CanBeSet(AvatarStatus status)
    {
        var avatar = new Avatar
        {
            Status = status
        };

        Assert.Equal(status, avatar.Status);
    }

    [AvaloniaFact]
    public void StatusColor_CanBeCustomized()
    {
        var customColor = new SolidColorBrush(Colors.Purple);
        var avatar = new Avatar
        {
            Status = AvatarStatus.Online,
            StatusColor = customColor
        };

        Assert.Equal(customColor, avatar.StatusColor);
    }

    [AvaloniaFact]
    public void Initials_CanBeSetDirectly()
    {
        var avatar = new Avatar
        {
            Initials = "AB"
        };

        Assert.Equal("AB", avatar.Initials);
    }

    [AvaloniaFact]
    public void MaxInitials_CanBeSet()
    {
        var avatar = new Avatar
        {
            MaxInitials = 3
        };

        Assert.Equal(3, avatar.MaxInitials);
    }

    [AvaloniaFact]
    public void AutoGenerateBackground_CanBeDisabled()
    {
        var avatar = new Avatar
        {
            AutoGenerateBackground = false
        };

        Assert.False(avatar.AutoGenerateBackground);
    }

    [AvaloniaFact]
    public void BackgroundColor_CanBeSet()
    {
        var customBrush = new SolidColorBrush(Colors.Red);
        var avatar = new Avatar
        {
            AutoGenerateBackground = false,
            BackgroundColor = customBrush
        };

        Assert.Equal(customBrush, avatar.BackgroundColor);
    }

    [AvaloniaFact]
    public void ForegroundColor_CanBeSet()
    {
        var customBrush = new SolidColorBrush(Colors.Black);
        var avatar = new Avatar
        {
            ForegroundColor = customBrush
        };

        Assert.Equal(customBrush, avatar.ForegroundColor);
    }

    [AvaloniaFact]
    public void ForegroundColor_DefaultIsWhite()
    {
        var avatar = new Avatar();

        Assert.Equal(Brushes.White, avatar.ForegroundColor);
    }

    [AvaloniaFact]
    public void ShowTooltip_CanBeDisabled()
    {
        var avatar = new Avatar
        {
            ShowTooltip = false
        };

        Assert.False(avatar.ShowTooltip);
    }

    [AvaloniaFact]
    public void Icon_CanBeSet()
    {
        var icon = new object();
        var avatar = new Avatar
        {
            Icon = icon
        };

        Assert.Equal(icon, avatar.Icon);
    }

    [AvaloniaFact]
    public void Content_CanBeSet()
    {
        var content = new object();
        var avatar = new Avatar
        {
            Content = content
        };

        Assert.Equal(content, avatar.Content);
    }
}
