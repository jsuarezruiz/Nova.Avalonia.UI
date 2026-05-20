using System.Linq;
using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.VisualTree;
using Nova.Avalonia.UI.Controls;
using Xunit;

namespace Nova.Avalonia.UI.Tests.Controls;

public class ShieldTests
{
    [AvaloniaFact]
    public void Shield_Should_Have_Default_Values()
    {
        var shield = new Shield();

        Assert.Null(shield.Subject);
        Assert.Null(shield.Status);
        Assert.Null(shield.Color);
        Assert.Null(shield.SubjectBackground);
    }

    [AvaloniaFact]
    public void Shield_Should_Set_Subject_Property()
    {
        var shield = new Shield { Subject = "Build" };

        Assert.Equal("Build", shield.Subject);
    }

    [AvaloniaFact]
    public void Shield_Should_Set_Status_Property()
    {
        var shield = new Shield { Status = "passing" };

        Assert.Equal("passing", shield.Status);
    }

    [AvaloniaFact]
    public void Shield_Should_Set_Color_Property()
    {
        var shield = new Shield { Color = Brushes.Green };

        Assert.Equal(Brushes.Green, shield.Color);
    }

    [AvaloniaFact]
    public void Shield_Should_Set_SubjectBackground_Property()
    {
        var shield = new Shield { SubjectBackground = Brushes.DarkGray };

        Assert.Equal(Brushes.DarkGray, shield.SubjectBackground);
    }

    [AvaloniaFact]
    public void Shield_Should_Set_Multiple_Properties()
    {
        var shield = new Shield
        {
            Subject = "Test Subject",
            Status = "Test Status",
            Color = Brushes.Red,
            SubjectBackground = Brushes.Black
        };

        Assert.Equal("Test Subject", shield.Subject);
        Assert.Equal("Test Status", shield.Status);
        Assert.Equal(Brushes.Red, shield.Color);
        Assert.Equal(Brushes.Black, shield.SubjectBackground);
    }

    [AvaloniaFact]
    public void Shield_Should_Accept_Complex_Content_As_Subject()
    {
        var complexContent = new TextBlock { Text = "Complex" };
        var shield = new Shield { Subject = complexContent };

        Assert.Same(complexContent, shield.Subject);
    }

    [AvaloniaFact]
    public void Shield_Should_Accept_Complex_Content_As_Status()
    {
        var complexContent = new StackPanel();
        var shield = new Shield { Status = complexContent };

        Assert.Same(complexContent, shield.Status);
    }

    [AvaloniaFact]
    public void Shield_AutomationPeer_Should_Return_Correct_Name()
    {
        var shield = new Shield { Subject = "Build", Status = "passing" };
        var peer = new ShieldAutomationPeer(shield);

        var name = peer.GetName();

        Assert.Equal("Build: passing", name);
    }

    [AvaloniaFact]
    public void Shield_AutomationPeer_Should_Return_Text_From_Custom_Content()
    {
        var shield = new Shield
        {
            Subject = new TextBlock { Text = "Users" },
            Status = new StackPanel
            {
                Children =
                {
                    new TextBlock { Text = "1.2k" }
                }
            }
        };
        var peer = new ShieldAutomationPeer(shield);

        var name = peer.GetName();

        Assert.Equal("Users: 1.2k", name);
    }

    [AvaloniaFact]
    public void Shield_AutomationPeer_Should_Respect_Explicit_Automation_Name()
    {
        var shield = new Shield { Subject = "Build", Status = "passing" };
        global::Avalonia.Automation.AutomationProperties.SetName(shield, "Build status passing");
        var peer = new ShieldAutomationPeer(shield);

        var name = peer.GetName();

        Assert.Equal("Build status passing", name);
    }

    [AvaloniaFact]
    public void Shield_AutomationPeer_Should_Return_Subject_Only_When_No_Status()
    {
        var shield = new Shield { Subject = "Version" };
        var peer = new ShieldAutomationPeer(shield);

        var name = peer.GetName();

        Assert.Equal("Version", name);
    }

    [AvaloniaFact]
    public void Shield_AutomationPeer_Should_Return_Status_Only_When_No_Subject()
    {
        var shield = new Shield { Status = "1.0.0" };
        var peer = new ShieldAutomationPeer(shield);

        var name = peer.GetName();

        Assert.Equal("1.0.0", name);
    }

    [AvaloniaFact]
    public void Shield_AutomationPeer_Should_Return_Correct_ClassName()
    {
        var shield = new Shield();
        var peer = new ShieldAutomationPeer(shield);

        var className = peer.GetClassName();

        Assert.Equal("Shield", className);
    }

    [AvaloniaFact]
    public void Shield_AutomationPeer_Should_Expose_Button_When_Interactive()
    {
        var shield = new Shield();
        var peer = new ShieldAutomationPeer(shield);

        Assert.Equal(AutomationControlType.Button, peer.GetAutomationControlType());
        Assert.NotNull(peer.GetProvider<IInvokeProvider>());
    }

    [AvaloniaFact]
    public void Shield_AutomationPeer_Invoke_Should_Click_When_Interactive()
    {
        var shield = new Shield();
        var peer = new ShieldAutomationPeer(shield);
        var clicked = false;
        shield.Click += (_, _) => clicked = true;

        peer.GetProvider<IInvokeProvider>()?.Invoke();

        Assert.True(clicked);
    }

    [AvaloniaFact]
    public void Shield_Should_Be_Enabled_By_Default()
    {
        var shield = new Shield();

        Assert.True(shield.IsEnabled);
    }

    [AvaloniaFact]
    public void Shield_Should_Support_Disabled_State()
    {
        var shield = new Shield { IsEnabled = false };

        Assert.False(shield.IsEnabled);
    }

    [AvaloniaFact]
    public void Shield_Should_Not_Be_ReadOnly_By_Default()
    {
        var shield = new Shield();

        Assert.False(shield.IsReadOnly);
    }

    [AvaloniaFact]
    public void Shield_Should_Set_ReadOnly_Property()
    {
        var shield = new Shield { IsReadOnly = true };

        Assert.True(shield.IsReadOnly);
    }

    [AvaloniaFact]
    public void Shield_ReadOnly_Should_Set_PseudoClass()
    {
        var shield = new Shield();

        shield.IsReadOnly = true;
        Assert.Contains(":readonly", shield.Classes);

        shield.IsReadOnly = false;
        Assert.DoesNotContain(":readonly", shield.Classes);
    }

    [AvaloniaFact]
    public void Shield_ReadOnly_Should_Still_Be_Enabled()
    {
        var shield = new Shield { IsReadOnly = true };

        Assert.True(shield.IsEnabled);
    }

    [AvaloniaFact]
    public void Shield_ReadOnly_AutomationPeer_Should_Not_Expose_InvokeProvider()
    {
        var shield = new Shield { IsReadOnly = true };
        var peer = new ShieldAutomationPeer(shield);

        Assert.Equal(AutomationControlType.Text, peer.GetAutomationControlType());
        Assert.Null(peer.GetProvider<IInvokeProvider>());
    }

    [AvaloniaFact]
    public void Shield_ReadOnly_AutomationPeer_Invoke_Should_Not_Click()
    {
        var shield = new Shield { IsReadOnly = true };
        var peer = new ShieldAutomationPeer(shield);
        var clicked = false;
        shield.Click += (_, _) => clicked = true;

        peer.Invoke();

        Assert.False(clicked);
    }

    [AvaloniaFact]
    public void Shield_Template_Should_Render_Focus_Border()
    {
        var shield = new Shield { Subject = "build", Status = "passing" };
        var window = new Window { Content = shield };

        try
        {
            window.Show();
            shield.ApplyTemplate();

            var focusBorder = shield
                .GetVisualDescendants()
                .OfType<Border>()
                .SingleOrDefault(border => border.Name == "PART_FocusBorder");

            Assert.NotNull(focusBorder);

            shield.BorderBrush = Brushes.Red;
            shield.BorderThickness = new Thickness(2);

            Assert.Equal(Brushes.Red, focusBorder.BorderBrush);
            Assert.Equal(new Thickness(2), focusBorder.BorderThickness);
        }
        finally
        {
            window.Close();
        }
    }
}
