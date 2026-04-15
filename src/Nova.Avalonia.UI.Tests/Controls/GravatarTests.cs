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

        var path1 = (global::Avalonia.Controls.Shapes.Path)avatar1!;
        var path2 = (global::Avalonia.Controls.Shapes.Path)avatar2!;

        var brush1 = (SolidColorBrush)path1.Fill!;
        var brush2 = (SolidColorBrush)path2.Fill!;
        Assert.Equal(brush1.Color, brush2.Color);

        var geo1 = (GeometryGroup)path1.Data!;
        var geo2 = (GeometryGroup)path2.Data!;
        Assert.Equal(geo1.Children.Count, geo2.Children.Count);
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

    [AvaloniaFact]
    public void GithubGravatarGenerator_Handles_Unicode_Identifiers()
    {
        var generator = new GithubGravatarGenerator();

        var avatar1 = generator.GenerateAvatar("josé@email.com");
        var avatar2 = generator.GenerateAvatar("jos?@email.com");

        Assert.NotNull(avatar1);
        Assert.NotNull(avatar2);

        var color1 = ((SolidColorBrush)((global::Avalonia.Controls.Shapes.Path)avatar1!).Fill!).Color;
        var color2 = ((SolidColorBrush)((global::Avalonia.Controls.Shapes.Path)avatar2!).Fill!).Color;

        Assert.NotEqual(color1, color2);
    }

    [AvaloniaFact]
    public void GithubGravatarGenerator_Different_Ids_Produce_Different_Avatars()
    {
        var generator = new GithubGravatarGenerator();

        var avatar1 = generator.GenerateAvatar("alice@example.com");
        var avatar2 = generator.GenerateAvatar("bob@example.com");

        var color1 = ((SolidColorBrush)((global::Avalonia.Controls.Shapes.Path)avatar1!).Fill!).Color;
        var color2 = ((SolidColorBrush)((global::Avalonia.Controls.Shapes.Path)avatar2!).Fill!).Color;

        Assert.NotEqual(color1, color2);
    }

    [AvaloniaFact]
    public void GithubGravatarGenerator_Produces_Symmetric_Pattern()
    {
        var generator = new GithubGravatarGenerator();
        var avatar = generator.GenerateAvatar("symmetry@test.com");

        var path = (global::Avalonia.Controls.Shapes.Path)avatar!;
        var geo = (GeometryGroup)path.Data!;

        var grid = new bool[5, 5];
        foreach (var child in geo.Children)
        {
            var rect = ((RectangleGeometry)child).Rect;
            grid[(int)rect.X, (int)rect.Y] = true;
        }

        for (var row = 0; row < 5; row++)
        {
            Assert.Equal(grid[0, row], grid[4, row]);
            Assert.Equal(grid[1, row], grid[3, row]);
        }
    }

    [AvaloniaFact]
    public void GithubGravatarGenerator_Only_Creates_Visible_Cells()
    {
        var generator = new GithubGravatarGenerator();
        var avatar = generator.GenerateAvatar("test@example.com");

        var path = (global::Avalonia.Controls.Shapes.Path)avatar!;
        var geo = (GeometryGroup)path.Data!;

        foreach (var child in geo.Children)
        {
            var rect = ((RectangleGeometry)child).Rect;
            Assert.True(rect.Width > 0);
            Assert.True(rect.Height > 0);
        }

        Assert.True(geo.Children.Count <= 25);
        Assert.True(geo.Children.Count > 0);
    }

    [AvaloniaFact]
    public void Gravatar_Source_Property_Can_Be_Set()
    {
        var gravatar = new Gravatar();
        Assert.Null(gravatar.Source);

        gravatar.Source = null;
        Assert.Null(gravatar.Source);
    }

    [AvaloniaFact]
    public void Gravatar_Generator_Property_Can_Be_Changed()
    {
        var gravatar = new Gravatar();
        var customGenerator = new GithubGravatarGenerator();

        gravatar.Generator = customGenerator;
        Assert.Same(customGenerator, gravatar.Generator);
    }

    [AvaloniaFact]
    public void GithubGravatarGenerator_Handles_Long_Identifiers()
    {
        var generator = new GithubGravatarGenerator();
        var longId = new string('a', 10000) + "@example.com";

        var avatar = generator.GenerateAvatar(longId);
        Assert.NotNull(avatar);
        Assert.IsType<global::Avalonia.Controls.Shapes.Path>(avatar);
    }
}
