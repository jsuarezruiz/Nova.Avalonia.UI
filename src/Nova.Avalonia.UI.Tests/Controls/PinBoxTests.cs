using System;
using Avalonia.Headless.XUnit;
using Xunit;
using Nova.Avalonia.UI.Controls;

namespace Nova.Avalonia.UI.Tests.Controls;

public class PinBoxTests
{
    [AvaloniaFact]
    public void DefaultValues_AreCorrect()
    {
        var pinBox = new PinBox();

        Assert.Equal(string.Empty, pinBox.Text);
        Assert.Equal(6, pinBox.Length);
        Assert.False(pinBox.IsPassword);
        Assert.Equal('●', pinBox.PasswordChar);
        Assert.Equal(8.0, pinBox.Spacing);
        Assert.True(pinBox.ShowCursor);
        Assert.False(pinBox.HasError);
    }

    [AvaloniaFact]
    public void Length_IsClamped()
    {
        var pinBox = new PinBox();

        pinBox.Length = 0;
        Assert.Equal(1, pinBox.Length);

        pinBox.Length = 20;
        Assert.Equal(12, pinBox.Length);

        pinBox.Length = 4;
        Assert.Equal(4, pinBox.Length);
    }

    [AvaloniaFact]
    public void Text_IsEnforcedByLength()
    {
        var pinBox = new PinBox { Length = 4 };

        pinBox.Text = "123456";
        Assert.Equal("1234", pinBox.Text);
    }

    [AvaloniaFact]
    public void TextChanged_RaisesEvent()
    {
        var pinBox = new PinBox();
        var raised = false;
        string? oldText = null;
        string? newText = null;

        pinBox.TextChanged += (s, e) =>
        {
            raised = true;
            oldText = e.OldText;
            newText = e.NewText;
        };

        pinBox.Text = "1234";

        Assert.True(raised);
        Assert.Equal(string.Empty, oldText);
        Assert.Equal("1234", newText);
    }

    [AvaloniaFact]
    public void Completed_RaisesEvent()
    {
        var pinBox = new PinBox { Length = 4 };
        var completed = false;
        string? completedPin = null;

        pinBox.Completed += (s, e) =>
        {
            completed = true;
            completedPin = e.Pin;
        };

        pinBox.Text = "1234";

        Assert.True(completed);
        Assert.Equal("1234", completedPin);
    }

    [AvaloniaFact]
    public void Validator_SetsHasErrorAndErrorText()
    {
        var pinBox = new PinBox
        {
            Length = 4,
            Validator = text => text == "1234" ? null : "Invalid PIN"
        };

        pinBox.Text = "9999";

        Assert.True(pinBox.HasError);
        Assert.Equal("Invalid PIN", pinBox.ErrorText);
    }

    [AvaloniaFact]
    public void Validator_ClearsErrorOnValidInput()
    {
        var pinBox = new PinBox
        {
            Length = 4,
            Validator = text => text == "1234" ? null : "Invalid PIN"
        };

        pinBox.Text = "1234";

        Assert.False(pinBox.HasError);
        Assert.Null(pinBox.ErrorText);
    }



    [AvaloniaFact]
    public void IsPassword_DefaultsToFalse()
    {
        var pinBox = new PinBox();

        Assert.False(pinBox.IsPassword);
    }

    [AvaloniaFact]
    public void Clear_ResetsTextToEmpty()
    {
        var pinBox = new PinBox { Length = 4 };
        pinBox.Text = "1234";

        pinBox.Clear();

        Assert.Equal(string.Empty, pinBox.Text);
    }

    [AvaloniaFact]
    public void DigitsOnly_DefaultsToTrue()
    {
        var pinBox = new PinBox();

        Assert.True(pinBox.DigitsOnly);
    }

    [AvaloniaFact]
    public void DigitsOnly_CanBeSetToFalse()
    {
        var pinBox = new PinBox { DigitsOnly = false };

        Assert.False(pinBox.DigitsOnly);
    }

    [AvaloniaFact]
    public void Text_EmptyString_IsValid()
    {
        var pinBox = new PinBox { Length = 4 };
        pinBox.Text = "";

        Assert.Equal(string.Empty, pinBox.Text);
        Assert.False(pinBox.HasError);
    }

    [AvaloniaFact]
    public void Text_PartialEntry_DoesNotTriggerCompleted()
    {
        var pinBox = new PinBox { Length = 4 };
        var completed = false;

        pinBox.Completed += (s, e) => completed = true;

        pinBox.Text = "12";

        Assert.False(completed);
    }

    [AvaloniaFact]
    public void Validator_CalledOnEveryChange()
    {
        var callCount = 0;
        var pinBox = new PinBox
        {
            Length = 4,
            Validator = text =>
            {
                callCount++;
                return null;
            }
        };

        pinBox.Text = "1";
        pinBox.Text = "12";
        pinBox.Text = "123";

        Assert.Equal(3, callCount);
    }

    [AvaloniaFact]
    public void Validator_NotCalledWhenNull()
    {
        var pinBox = new PinBox { Length = 4 };
        pinBox.Text = "1234";

        Assert.False(pinBox.HasError);
        Assert.Null(pinBox.ErrorText);
    }


    [AvaloniaFact]
    public void TextChanged_NotRaisedWhenSameValue()
    {
        var pinBox = new PinBox { Length = 4 };
        pinBox.Text = "1234";

        var raised = false;
        pinBox.TextChanged += (s, e) => raised = true;

        pinBox.Text = "1234"; // Same value

        Assert.False(raised);
    }

    [AvaloniaFact]
    public void AnimationDuration_HasDefaultValue()
    {
        var pinBox = new PinBox();

        Assert.Equal(TimeSpan.FromMilliseconds(150), pinBox.AnimationDuration);
    }

    [AvaloniaFact]
    public void AnimationDuration_CanBeChanged()
    {
        var pinBox = new PinBox { AnimationDuration = TimeSpan.FromMilliseconds(300) };

        Assert.Equal(TimeSpan.FromMilliseconds(300), pinBox.AnimationDuration);
    }

    [AvaloniaFact]
    public void ErrorText_ClearsWhenValidatorReturnsNull()
    {
        var pinBox = new PinBox
        {
            Length = 4,
            Validator = text => text.Length < 4 ? "Too short" : null
        };

        pinBox.Text = "12"; // Error
        Assert.True(pinBox.HasError);

        pinBox.Text = "1234"; // Valid
        Assert.False(pinBox.HasError);
        Assert.Null(pinBox.ErrorText);
    }
}
