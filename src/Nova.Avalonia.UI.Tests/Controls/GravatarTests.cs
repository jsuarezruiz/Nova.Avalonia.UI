using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Nova.Avalonia.UI.Controls;
using Xunit;

namespace Nova.Avalonia.UI.Tests.Controls;

public class GravatarTests
{
    [AvaloniaFact]
    public void Gravatar_Defaults()
    {
        var gravatar = new Gravatar();

        Assert.Null(gravatar.Id);
        Assert.Null(gravatar.Source);
        Assert.NotNull(gravatar.Generator);
        Assert.IsType<GithubGravatarGenerator>(gravatar.Generator);
        Assert.Equal(48.0, gravatar.Size);
    }

    [AvaloniaFact]
    public void Gravatar_Sets_Id()
    {
        var gravatar = new Gravatar { Id = "test@example.com" };
        Assert.Equal("test@example.com", gravatar.Id);
    }

    [AvaloniaFact]
    public void Gravatar_Sets_Size()
    {
        var gravatar = new Gravatar { Size = 100 };
        Assert.Equal(100.0, gravatar.Size);
    }

    [AvaloniaFact]
    public void GithubGravatarGenerator_Returns_Null_For_Empty_Id()
    {
        var generator = new GithubGravatarGenerator();
        Assert.Null(generator.GenerateAvatar(null));
        Assert.Null(generator.GenerateAvatar(""));
    }

    [AvaloniaFact]
    public void GithubGravatarGenerator_Generates_Avatar()
    {
        var generator = new GithubGravatarGenerator();
        var avatar = generator.GenerateAvatar("test@example.com");

        Assert.NotNull(avatar);
        // It returns a Path with GeometryGroup
        Assert.IsType<global::Avalonia.Controls.Shapes.Path>(avatar);
    }

    [AvaloniaFact]
    public void GithubGravatarGenerator_Consistent_Output()
    {
        var generator = new GithubGravatarGenerator();
        var id = "consistent@test.com";
        
        var avatar1 = generator.GenerateAvatar(id);
        var avatar2 = generator.GenerateAvatar(id);

        Assert.NotNull(avatar1);
        Assert.NotNull(avatar2);
        
        // We can't easily compare visual objects for equality, but we can check properties
        var path1 = (global::Avalonia.Controls.Shapes.Path)avatar1!;
        var path2 = (global::Avalonia.Controls.Shapes.Path)avatar2!;

        var brush1 = (SolidColorBrush)path1.Fill!;
        var brush2 = (SolidColorBrush)path2.Fill!;

        Assert.Equal(brush1.Color, brush2.Color);
    }

    [AvaloniaFact]
    public void GithubGravatarGenerator_Is_Case_Insensitive()
    {
        var generator = new GithubGravatarGenerator();
        var id1 = "test@example.com";
        var id2 = "Test@Example.COM";
        var id3 = "  test@example.com  ";

        var avatar1 = generator.GenerateAvatar(id1);
        var avatar2 = generator.GenerateAvatar(id2);
        var avatar3 = generator.GenerateAvatar(id3);

        var color1 = ((SolidColorBrush)((global::Avalonia.Controls.Shapes.Path)avatar1!).Fill!).Color;
        var color2 = ((SolidColorBrush)((global::Avalonia.Controls.Shapes.Path)avatar2!).Fill!).Color;
        var color3 = ((SolidColorBrush)((global::Avalonia.Controls.Shapes.Path)avatar3!).Fill!).Color;

        Assert.Equal(color1, color2);
        Assert.Equal(color1, color3);
    }

    [AvaloniaFact]
    public void Gravatar_AutomationPeer_Returns_Id_As_Name()
    {
        var gravatar = new Gravatar { Id = "accessible@test.com" };
        var peer = new GravatarAutomationPeer(gravatar);

        Assert.Equal("accessible@test.com", peer.GetName());
        Assert.Equal(global::Avalonia.Automation.Peers.AutomationControlType.Image, peer.GetAutomationControlType());
        Assert.Equal("Gravatar", peer.GetClassName());
    }
}
