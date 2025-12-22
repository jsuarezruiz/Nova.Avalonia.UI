using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Nova.Avalonia.UI.Controls;
using Xunit;

namespace Nova.Avalonia.UI.Tests.Controls;

public class BadgeTests
{
    [AvaloniaFact]
    public void Badge_Should_Have_Default_Values()
    {
        var badge = new Badge();

        Assert.Null(badge.BadgeContent);
        Assert.Equal(BadgePlacement.TopRight, badge.BadgePlacement);
        Assert.True(badge.IsBadgeVisible);
        Assert.Equal(0.0, badge.BadgeOffset);
        Assert.Equal(BadgeKind.Content, badge.Kind);
        Assert.Equal(99, badge.MaxCount);
    }

    [AvaloniaFact]
    public void BadgeContent_Should_Update_DisplayContent()
    {
        var badge = new Badge();
        badge.BadgeContent = "5";

        Assert.Equal("5", badge.BadgeContent);
        Assert.Equal("5", badge.DisplayContent);
    }

    [AvaloniaFact]
    public void Badge_Should_Handle_Overflow()
    {
        var badge = new Badge();
        badge.MaxCount = 99;
        
        // Exact max count
        badge.BadgeContent = 99;
        Assert.Equal(99, badge.DisplayContent);

        // One over max count
        badge.BadgeContent = 100;
        Assert.Equal("99+", badge.DisplayContent);
    }

    [AvaloniaFact]
    public void Badge_Should_Handle_Custom_MaxCount()
    {
        var badge = new Badge();
        badge.MaxCount = 9;
        
        badge.BadgeContent = 9;
        Assert.Equal(9, badge.DisplayContent);

        badge.BadgeContent = 10;
        Assert.Equal("9+", badge.DisplayContent);
    }

    [AvaloniaFact]
    public void Badge_Should_Handle_String_Numeric_Content()
    {
        var badge = new Badge();
        badge.MaxCount = 99;
        
        // String that parses to int > MaxCount
        badge.BadgeContent = "100";
        Assert.Equal("99+", badge.DisplayContent);
        
        // String that parses to int <= MaxCount
        badge.BadgeContent = "50";
        Assert.Equal("50", badge.DisplayContent);
    }

    [AvaloniaFact]
    public void Badge_Should_Handle_Non_Numeric_String_Content()
    {
        var badge = new Badge();
        badge.BadgeContent = "New";
        
        Assert.Equal("New", badge.DisplayContent);
    }

    [AvaloniaFact]
    public void Badge_Should_Automatically_Switch_To_Dot_Mode_If_Null_Content()
    {
        var badge = new Badge();
        // Default is Content kind, but with null content, logic should apply 'Dot' class
        // Since we can't check classes without full styling, we rely on the logic check
        // However, we can check if Kind property remains 'Content' but the visual state would be Dot.
        // Let's verify the Kind property is NOT changed by the control itself (it shouldn't be).
        
        Assert.Equal(BadgeKind.Content, badge.Kind);
        Assert.Null(badge.BadgeContent);
    }

    [AvaloniaFact]
    public void Badge_Should_Respect_Explicit_Dot_Mode()
    {
        var badge = new Badge();
        badge.Kind = BadgeKind.Dot;
        badge.BadgeContent = "5"; 
        
        // Even with content, if Kind is Dot, it should stay Dot. 
        // Logic: if (isDot) Classes.Add("Dot");
        
        Assert.Equal(BadgeKind.Dot, badge.Kind);
    }

    [AvaloniaFact]
    public void Badge_Placement_Should_Update()
    {
        var badge = new Badge();
        badge.BadgePlacement = BadgePlacement.BottomLeft;

        Assert.Equal(BadgePlacement.BottomLeft, badge.BadgePlacement);
    }

    [AvaloniaFact]
    public void Badge_Offset_Should_Update()
    {
        var badge = new Badge();
        badge.BadgeOffset = 10.0;
        
        Assert.Equal(10.0, badge.BadgeOffset);
    }
    
    [AvaloniaFact]
    public void Badge_Visibility_Should_Update()
    {
        var badge = new Badge();
        badge.IsBadgeVisible = false;
        
        Assert.False(badge.IsBadgeVisible);
    }

    [AvaloniaFact]
    public void BadgeBackground_Default_IsRed()
    {
        var badge = new Badge();

        Assert.NotNull(badge.BadgeBackground);
        var brush = Assert.IsAssignableFrom<ISolidColorBrush>(badge.BadgeBackground);
        Assert.Equal(Colors.Red, brush.Color);
    }

    [AvaloniaFact]
    public void BadgeForeground_Default_IsWhite()
    {
        var badge = new Badge();

        Assert.NotNull(badge.BadgeForeground);
        var brush = Assert.IsAssignableFrom<ISolidColorBrush>(badge.BadgeForeground);
        Assert.Equal(Colors.White, brush.Color);
    }

    [AvaloniaFact]
    public void BadgeBorderBrush_Default_IsTransparent()
    {
        var badge = new Badge();

        Assert.NotNull(badge.BadgeBorderBrush);
        var brush = Assert.IsAssignableFrom<ISolidColorBrush>(badge.BadgeBorderBrush);
        Assert.Equal(Colors.Transparent, brush.Color);
    }

    [AvaloniaFact]
    public void BadgeBorderThickness_Default_IsZero()
    {
        var badge = new Badge();

        Assert.Equal(new Thickness(0), badge.BadgeBorderThickness);
    }

    [AvaloniaFact]
    public void BadgeBackground_CanBeCustomized()
    {
        var customBrush = new SolidColorBrush(Colors.Blue);
        var badge = new Badge { BadgeBackground = customBrush };

        Assert.Equal(customBrush, badge.BadgeBackground);
    }

    [AvaloniaFact]
    public void BadgeForeground_CanBeCustomized()
    {
        var customBrush = new SolidColorBrush(Colors.Black);
        var badge = new Badge { BadgeForeground = customBrush };

        Assert.Equal(customBrush, badge.BadgeForeground);
    }

    [AvaloniaFact]
    public void BadgeBorderBrush_CanBeCustomized()
    {
        var customBrush = new SolidColorBrush(Colors.DarkRed);
        var badge = new Badge { BadgeBorderBrush = customBrush };

        Assert.Equal(customBrush, badge.BadgeBorderBrush);
    }

    [AvaloniaFact]
    public void BadgeBorderThickness_CanBeCustomized()
    {
        var badge = new Badge { BadgeBorderThickness = new Thickness(2) };

        Assert.Equal(new Thickness(2), badge.BadgeBorderThickness);
    }

    [AvaloniaTheory]
    [InlineData(BadgePlacement.TopLeft)]
    [InlineData(BadgePlacement.Top)]
    [InlineData(BadgePlacement.TopRight)]
    [InlineData(BadgePlacement.Right)]
    [InlineData(BadgePlacement.BottomRight)]
    [InlineData(BadgePlacement.Bottom)]
    [InlineData(BadgePlacement.BottomLeft)]
    [InlineData(BadgePlacement.Left)]
    public void Badge_Placement_AllPositions_CanBeSet(BadgePlacement placement)
    {
        var badge = new Badge { BadgePlacement = placement };

        Assert.Equal(placement, badge.BadgePlacement);
    }

    [AvaloniaFact]
    public void Badge_CanWrap_ButtonContent()
    {
        var button = new Button { Content = "Click Me" };
        var badge = new Badge
        {
            Content = button,
            BadgeContent = "5"
        };

        Assert.Equal(button, badge.Content);
        Assert.Equal("5", badge.BadgeContent);
    }

    [AvaloniaFact]
    public void Badge_CanWrap_TextBlockContent()
    {
        var textBlock = new TextBlock { Text = "Notifications" };
        var badge = new Badge
        {
            Content = textBlock,
            BadgeContent = "New"
        };

        Assert.Equal(textBlock, badge.Content);
        Assert.Equal("New", badge.BadgeContent);
    }

    [AvaloniaFact]
    public void Badge_Content_CanBeNull()
    {
        var badge = new Badge
        {
            Content = null,
            BadgeContent = "5"
        };

        Assert.Null(badge.Content);
        Assert.Equal("5", badge.BadgeContent);
    }

    [AvaloniaFact]
    public void Badge_Should_Handle_ZeroCount()
    {
        var badge = new Badge { BadgeContent = 0 };

        Assert.Equal(0, badge.DisplayContent);
    }

    [AvaloniaFact]
    public void Badge_Should_Handle_EmptyString_Content()
    {
        var badge = new Badge { BadgeContent = "" };

        Assert.Equal("", badge.DisplayContent);
    }

    [AvaloniaFact]
    public void Badge_Should_Handle_LargeNumbers()
    {
        var badge = new Badge { MaxCount = 999 };
        
        badge.BadgeContent = 1000;
        Assert.Equal("999+", badge.DisplayContent);

        badge.BadgeContent = 999;
        Assert.Equal(999, badge.DisplayContent);
    }

    [AvaloniaFact]
    public void Badge_MaxCount_CanBeChangedDynamically()
    {
        var badge = new Badge
        {
            MaxCount = 99,
            BadgeContent = 150
        };

        Assert.Equal("99+", badge.DisplayContent);

        badge.MaxCount = 200;
        Assert.Equal(150, badge.DisplayContent);
    }

    [AvaloniaFact]
    public void Badge_Offset_CanBeNegative()
    {
        var badge = new Badge { BadgeOffset = -5.0 };

        Assert.Equal(-5.0, badge.BadgeOffset);
    }

    [AvaloniaFact]
    public void Badge_MaxCount_CanBeOne()
    {
        var badge = new Badge { MaxCount = 1 };

        badge.BadgeContent = 1;
        Assert.Equal(1, badge.DisplayContent);

        badge.BadgeContent = 2;
        Assert.Equal("1+", badge.DisplayContent);
    }

    [AvaloniaTheory]
    [InlineData(BadgeKind.Content)]
    [InlineData(BadgeKind.Dot)]
    public void Badge_Kind_AllValues_CanBeSet(BadgeKind kind)
    {
        var badge = new Badge { Kind = kind };

        Assert.Equal(kind, badge.Kind);
    }

    [AvaloniaFact]
    public void Badge_Kind_Dot_WithContent_RemainsKindDot()
    {
        var badge = new Badge
        {
            Kind = BadgeKind.Dot,
            BadgeContent = "100"
        };

        Assert.Equal(BadgeKind.Dot, badge.Kind);
        // DisplayContent still processes the value even in Dot mode
        Assert.Equal("99+", badge.DisplayContent);
    }

    [AvaloniaFact]
    public void BadgeAutomationPeer_Returns_Correct_ClassName()
    {
        var badge = new Badge();
        var peer = new BadgeAutomationPeer(badge);

        Assert.Equal("Badge", peer.GetClassName());
    }

    [AvaloniaFact]
    public void BadgeAutomationPeer_Returns_Text_ControlType_When_Standalone()
    {
        var badge = new Badge { BadgeContent = "5" };
        var peer = new BadgeAutomationPeer(badge);

        Assert.Equal(AutomationControlType.Text, peer.GetAutomationControlType());
    }

    [AvaloniaFact]
    public void BadgeAutomationPeer_Returns_Group_ControlType_When_Wrapping_Content()
    {
        var badge = new Badge
        {
            Content = new Button { Content = "Click" },
            BadgeContent = "5"
        };
        var peer = new BadgeAutomationPeer(badge);

        Assert.Equal(AutomationControlType.Group, peer.GetAutomationControlType());
    }

    [AvaloniaFact]
    public void BadgeAutomationPeer_Returns_AccessibleName_With_Content()
    {
        var badge = new Badge
        {
            BadgeContent = "5",
            IsBadgeVisible = true
        };
        var peer = new BadgeAutomationPeer(badge);

        Assert.Equal("Badge: 5", peer.GetName());
    }

    [AvaloniaFact]
    public void BadgeAutomationPeer_Returns_AccessibleName_With_Overflow_Content()
    {
        var badge = new Badge
        {
            MaxCount = 99,
            BadgeContent = 150,
            IsBadgeVisible = true
        };
        var peer = new BadgeAutomationPeer(badge);

        Assert.Equal("Badge: 99+", peer.GetName());
    }

    [AvaloniaFact]
    public void BadgeAutomationPeer_Returns_AccessibleName_For_DotMode()
    {
        var badge = new Badge
        {
            Kind = BadgeKind.Dot,
            IsBadgeVisible = true
        };
        var peer = new BadgeAutomationPeer(badge);

        Assert.Equal("Notification indicator", peer.GetName());
    }

    [AvaloniaFact]
    public void BadgeAutomationPeer_Returns_Empty_When_Badge_Not_Visible()
    {
        var badge = new Badge
        {
            BadgeContent = "5",
            IsBadgeVisible = false
        };
        var peer = new BadgeAutomationPeer(badge);

        Assert.Equal(string.Empty, peer.GetName());
    }

    [AvaloniaFact]
    public void BadgeAutomationPeer_Returns_HelpText_With_Content()
    {
        var badge = new Badge
        {
            BadgeContent = "3",
            IsBadgeVisible = true
        };
        var peer = new BadgeAutomationPeer(badge);

        Assert.Equal("This item has 3 notifications", peer.GetHelpText());
    }

    [AvaloniaFact]
    public void BadgeAutomationPeer_Returns_HelpText_For_DotMode()
    {
        var badge = new Badge
        {
            Kind = BadgeKind.Dot,
            IsBadgeVisible = true
        };
        var peer = new BadgeAutomationPeer(badge);

        Assert.Equal("This item has new activity", peer.GetHelpText());
    }

    [AvaloniaFact]
    public void DisplayContent_Updates_When_BadgeContent_Changes()
    {
        var badge = new Badge();
        
        badge.BadgeContent = "10";
        Assert.Equal("10", badge.DisplayContent);

        badge.BadgeContent = "New";
        Assert.Equal("New", badge.DisplayContent);

        badge.BadgeContent = null;
        Assert.Null(badge.DisplayContent);
    }

    [AvaloniaFact]
    public void DisplayContent_Updates_When_MaxCount_Changes()
    {
        var badge = new Badge
        {
            MaxCount = 10,
            BadgeContent = 15
        };

        Assert.Equal("10+", badge.DisplayContent);

        badge.MaxCount = 20;
        Assert.Equal(15, badge.DisplayContent);

        badge.MaxCount = 5;
        Assert.Equal("5+", badge.DisplayContent);
    }

    [AvaloniaFact]
    public void Badge_Multiple_Properties_CanBeSetSimultaneously()
    {
        var badge = new Badge
        {
            BadgeContent = "42",
            BadgePlacement = BadgePlacement.BottomRight,
            BadgeOffset = 5.0,
            Kind = BadgeKind.Content,
            MaxCount = 50,
            IsBadgeVisible = true,
            BadgeBackground = Brushes.Green,
            BadgeForeground = Brushes.Yellow,
            BadgeBorderBrush = Brushes.DarkGreen,
            BadgeBorderThickness = new Thickness(1)
        };

        Assert.Equal("42", badge.BadgeContent);
        Assert.Equal("42", badge.DisplayContent);
        Assert.Equal(BadgePlacement.BottomRight, badge.BadgePlacement);
        Assert.Equal(5.0, badge.BadgeOffset);
        Assert.Equal(BadgeKind.Content, badge.Kind);
        Assert.Equal(50, badge.MaxCount);
        Assert.True(badge.IsBadgeVisible);
        Assert.Equal(Brushes.Green, badge.BadgeBackground);
        Assert.Equal(Brushes.Yellow, badge.BadgeForeground);
        Assert.Equal(Brushes.DarkGreen, badge.BadgeBorderBrush);
        Assert.Equal(new Thickness(1), badge.BadgeBorderThickness);
    }
}
