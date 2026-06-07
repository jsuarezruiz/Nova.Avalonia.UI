using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Headless;
using Avalonia.Controls;
using Avalonia.Automation;
using Avalonia.Automation.Provider;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;
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
        Assert.False(pinBox.IsReadOnly);
        Assert.Null(pinBox.TextNormalizer);
        Assert.Equal(0, pinBox.GroupLength);
        Assert.Null(pinBox.GroupLengths);
        Assert.Null(pinBox.Separator);
    }

    [AvaloniaFact]
    public void ControlMetadata_DeclaresTemplatePartsAndPseudoClasses()
    {
        var templateParts = typeof(PinBox).GetCustomAttributesData()
            .Where(attribute => attribute.AttributeType.Name == "TemplatePartAttribute")
            .Select(attribute => attribute.ConstructorArguments[0].Value?.ToString())
            .ToArray();

        Assert.Contains("PART_ItemsPanel", templateParts);
        Assert.Contains("PART_InputTextBox", templateParts);
        Assert.Contains(":readonly", GetPseudoClassMetadata(typeof(PinBox)));
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
    public void Text_FiltersInvalidCharacters()
    {
        var pinBox = new PinBox { Length = 4 };

        pinBox.Text = "1a2٣3";

        Assert.Equal("123", pinBox.Text);
    }

    [AvaloniaFact]
    public void TextChanged_RaisesForNormalizedText()
    {
        var pinBox = new PinBox { Length = 4 };
        string? oldText = null;
        string? newText = null;

        pinBox.TextChanged += (_, e) =>
        {
            oldText = e.OldText;
            newText = e.NewText;
        };

        pinBox.Text = "12a345";

        Assert.Equal(string.Empty, oldText);
        Assert.Equal("1234", newText);
    }

    [AvaloniaFact]
    public void Text_AllowsAsciiLettersWhenDigitsOnlyIsFalse()
    {
        var pinBox = new PinBox
        {
            DigitsOnly = false,
            Length = 6
        };

        pinBox.Text = "A-1_ß2z";

        Assert.Equal("A12z", pinBox.Text);
    }

    [AvaloniaFact]
    public void TextNormalizer_RunsBeforeFilteringAndLengthClamp()
    {
        var pinBox = new PinBox
        {
            DigitsOnly = false,
            Length = 6,
            TextNormalizer = text => text.Replace('o', '0').ToUpperInvariant()
        };

        pinBox.Text = "ab-o1-xyz";

        Assert.Equal("AB01XY", pinBox.Text);
    }

    [AvaloniaFact]
    public void TextNormalizer_ChangingValueReNormalizesCurrentText()
    {
        var pinBox = new PinBox
        {
            DigitsOnly = false,
            Length = 4,
            Text = "ab12"
        };

        pinBox.TextNormalizer = text => text.ToUpperInvariant();

        Assert.Equal("AB12", pinBox.Text);
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
    public void CompletedCommand_ExecutesWithPinWhenParameterIsUnset()
    {
        var command = new TestCommand();
        var pinBox = new PinBox
        {
            Length = 4,
            CompletedCommand = command
        };

        pinBox.Text = "1234";

        Assert.Equal(1, command.ExecuteCount);
        Assert.Equal("1234", command.LastParameter);
    }

    [AvaloniaFact]
    public void CompletedCommand_UsesExplicitParameter()
    {
        var parameter = new object();
        var command = new TestCommand();
        var pinBox = new PinBox
        {
            Length = 4,
            CompletedCommand = command,
            CompletedCommandParameter = parameter
        };

        pinBox.Text = "1234";

        Assert.Equal(1, command.ExecuteCount);
        Assert.Same(parameter, command.LastParameter);
    }

    [AvaloniaFact]
    public void CompletedCommand_RespectsCanExecute()
    {
        var command = new TestCommand(canExecute: false);
        var pinBox = new PinBox
        {
            Length = 4,
            CompletedCommand = command
        };

        pinBox.Text = "1234";

        Assert.Equal(0, command.ExecuteCount);
    }

    [AvaloniaFact]
    public void GroupLength_CoercesInvalidValues()
    {
        var pinBox = new PinBox();

        pinBox.GroupLength = -1;
        Assert.Equal(0, pinBox.GroupLength);

        pinBox.GroupLength = 20;
        Assert.Equal(12, pinBox.GroupLength);
    }

    [AvaloniaFact]
    public void GroupLength_CreatesVisualSeparators()
    {
        var pinBox = new PinBox
        {
            Length = 6,
            GroupLength = 3,
            Separator = "-"
        };

        var panel = AttachItemsPanel(pinBox);

        Assert.Equal(7, panel.Children.Count);
        var separator = Assert.IsType<TextBlock>(panel.Children[3]);
        Assert.Equal("-", separator.Text);
        Assert.Single(panel.Children.OfType<TextBlock>());
    }

    [AvaloniaFact]
    public void GroupLengths_TakePrecedenceOverGroupLength()
    {
        var pinBox = new PinBox
        {
            Length = 10,
            GroupLength = 3,
            GroupLengths = new[] { 4, 2, 4 },
            Separator = " "
        };

        var panel = AttachItemsPanel(pinBox);
        var sequence = string.Concat(panel.Children.Select(child => child is TextBlock ? "|" : "x"));

        Assert.Equal("xxxx|xx|xxxx", sequence);
    }

    [AvaloniaFact]
    public void Completed_RaisesOnce_ForTextInputThatFillsLength()
    {
        var pinBox = new TestablePinBox { Length = 4, Text = "123" };
        var completedCount = 0;
        string? completedPin = null;

        pinBox.Completed += (_, e) =>
        {
            completedCount++;
            completedPin = e.Pin;
        };

        pinBox.SimulateTextInput("4");

        Assert.Equal(1, completedCount);
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
    public void Length_Shrink_TrimsText()
    {
        var pinBox = new PinBox { Text = "123456" };

        pinBox.Length = 4;

        Assert.Equal("1234", pinBox.Text);
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
    public void DigitsOnly_ChangingToTrue_ReNormalizesText()
    {
        var pinBox = new PinBox
        {
            DigitsOnly = false,
            Text = "AB12"
        };

        pinBox.DigitsOnly = true;

        Assert.Equal("12", pinBox.Text);
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

        callCount = 0;
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

        pinBox.Text = "1234";

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
    public void AnimationDuration_CoercesNegativeValues()
    {
        var pinBox = new PinBox { AnimationDuration = TimeSpan.FromMilliseconds(-1) };

        Assert.Equal(TimeSpan.Zero, pinBox.AnimationDuration);
    }

    [AvaloniaFact]
    public void IsReadOnly_PreventsUserInputButAllowsProgrammaticText()
    {
        var pinBox = new TestablePinBox
        {
            Length = 4,
            Text = "12",
            IsReadOnly = true
        };

        pinBox.SimulateTextInput("3");
        pinBox.SimulateKeyDown(Key.Back);

        Assert.Equal("12", pinBox.Text);
        Assert.Contains(":readonly", pinBox.Classes);

        pinBox.Text = "1234";
        pinBox.IsReadOnly = false;

        Assert.Equal("1234", pinBox.Text);
        Assert.DoesNotContain(":readonly", pinBox.Classes);
    }

    [AvaloniaFact]
    public void ErrorText_ClearsWhenValidatorReturnsNull()
    {
        var pinBox = new PinBox
        {
            Length = 4,
            Validator = text => text.Length < 4 ? "Too short" : null
        };

        pinBox.Text = "12";
        Assert.True(pinBox.HasError);

        pinBox.Text = "1234";
        Assert.False(pinBox.HasError);
        Assert.Null(pinBox.ErrorText);
    }

    [AvaloniaFact]
    public void ValidationState_ClearsErrorVisualsWhenInputBecomesValid()
    {
        var filledTheme = new PinBoxTheme();
        var errorTheme = new PinBoxTheme();
        var pinBox = new PinBox
        {
            Length = 4,
            FilledTheme = filledTheme,
            ErrorTheme = errorTheme,
            Validator = text => text == "1234" ? null : "Invalid PIN"
        };
        AttachItemsPanel(pinBox);

        pinBox.Text = "9999";
        Assert.All(GetItems(pinBox), item => Assert.Equal(PinBoxItemState.Error, item.State));
        Assert.All(GetItems(pinBox), item => Assert.Same(errorTheme, item.BoxTheme));

        pinBox.Text = "1234";

        Assert.All(GetItems(pinBox), item => Assert.Equal(PinBoxItemState.Filled, item.State));
        Assert.All(GetItems(pinBox), item => Assert.Same(filledTheme, item.BoxTheme));
    }

    [AvaloniaFact]
    public void RuntimeVisualProperties_UpdateItems()
    {
        var cursorBrush = Brushes.Red;
        var pinBox = new PinBox { Length = 4 };
        AttachItemsPanel(pinBox);

        pinBox.Spacing = 16;
        pinBox.IsPassword = true;
        pinBox.PasswordChar = '*';
        pinBox.ShowCursor = false;
        pinBox.CursorBrush = cursorBrush;

        var items = GetItems(pinBox);
        Assert.Equal(16, items[1].Margin.Left);
        Assert.All(items, item => Assert.True(item.IsPassword));
        Assert.All(items, item => Assert.Equal('*', item.PasswordChar));
        Assert.All(items, item => Assert.False(item.ShowCursor));
        Assert.All(items, item => Assert.Same(cursorBrush, item.CursorBrush));
    }

    [AvaloniaFact]
    public void NestedThemePropertyChanges_UpdateExistingItems()
    {
        var theme = new PinBoxTheme
        {
            Width = 40,
            Height = 44,
            Background = Brushes.White,
            BorderBrush = Brushes.Gray
        };
        var pinBox = new PinBox
        {
            Length = 4,
            DefaultTheme = theme,
            FocusedTheme = theme,
            FilledTheme = theme,
            ErrorTheme = theme,
            IsResponsive = false
        };
        var window = ShowInWindow(pinBox, width: 400, height: 120);

        try
        {
            var item = GetVisualItems(pinBox)[0];

            Assert.Equal(40, item.Bounds.Width);
            Assert.Equal(44, item.Bounds.Height);

            theme.Width = 72;
            theme.Height = 64;
            window.UpdateLayout();

            Assert.Equal(72, item.Bounds.Width);
            Assert.Equal(64, item.Bounds.Height);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void AutomationPeer_ExposesValueProvider()
    {
        var pinBox = new PinBox
        {
            Length = 4,
            Text = "1234"
        };
        var peer = new PinBoxAutomationPeer(pinBox);
        var provider = Assert.IsAssignableFrom<IValueProvider>(peer.GetProvider<IValueProvider>());

        Assert.False(provider.IsReadOnly);
        Assert.Equal("PIN entry, 4 of 4 characters", peer.GetName());
        Assert.Equal("1234", provider.Value);

        provider.SetValue("9a8");

        Assert.Equal("98", pinBox.Text);
        Assert.Equal("98", provider.Value);

        pinBox.IsPassword = true;

        Assert.Equal("PIN entry, 2 of 4 characters entered", peer.GetName());
        Assert.Equal("●●", provider.Value);
    }

    [AvaloniaFact]
    public void AutomationPeer_SetValueThrowsWhenDisabled()
    {
        var pinBox = new PinBox
        {
            IsEnabled = false,
            Text = "12"
        };
        var provider = Assert.IsAssignableFrom<IValueProvider>(
            new PinBoxAutomationPeer(pinBox).GetProvider<IValueProvider>());

        Assert.True(provider.IsReadOnly);
        Assert.Throws<ElementNotEnabledException>(() => provider.SetValue("1234"));
    }

    [AvaloniaFact]
    public void AutomationPeer_SetValueDoesNotChangeReadOnlyText()
    {
        var pinBox = new PinBox
        {
            IsReadOnly = true,
            Text = "12"
        };
        var provider = Assert.IsAssignableFrom<IValueProvider>(
            new PinBoxAutomationPeer(pinBox).GetProvider<IValueProvider>());

        Assert.True(provider.IsReadOnly);

        provider.SetValue("1234");

        Assert.Equal("12", pinBox.Text);
    }

    [AvaloniaFact]
    public void AutomationPeer_PrioritizesErrorNameInPasswordMode()
    {
        var pinBox = new PinBox
        {
            IsPassword = true,
            Length = 4,
            Validator = text => text == "1234" ? null : "Invalid PIN"
        };
        pinBox.Text = "9999";
        var peer = new PinBoxAutomationPeer(pinBox);

        Assert.Equal("PIN entry, error: Invalid PIN", peer.GetName());
    }

    [AvaloniaFact]
    public void Template_InputTextBoxSyncsWithText()
    {
        var pinBox = new PinBox { Length = 4 };
        var window = ShowInWindow(pinBox);

        try
        {
            var inputTextBox = GetTemplatePart<TextBox>(pinBox, "PART_InputTextBox");

            Assert.False(inputTextBox.IsHitTestVisible);
            Assert.False(inputTextBox.IsTabStop);
            Assert.Equal(AccessibilityView.Raw, AutomationProperties.GetAccessibilityView(inputTextBox));

            pinBox.Text = "12";
            Assert.Equal("12", inputTextBox.Text);

            pinBox.Focus();
            window.KeyTextInput("3");
            window.KeyTextInput("a");
            window.KeyTextInput("4");

            Assert.Equal("1234", pinBox.Text);
            Assert.Equal("1234", inputTextBox.Text);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Template_PasswordPropertiesSyncInputTextBoxAtRuntime()
    {
        var pinBox = new PinBox
        {
            Length = 4,
            Text = "12"
        };
        var window = ShowInWindow(pinBox);

        try
        {
            var inputTextBox = GetTemplatePart<TextBox>(pinBox, "PART_InputTextBox");

            Assert.Equal('\0', inputTextBox.PasswordChar);

            pinBox.IsPassword = true;
            Assert.Equal('●', inputTextBox.PasswordChar);

            pinBox.PasswordChar = '*';
            Assert.Equal('*', inputTextBox.PasswordChar);

            pinBox.IsPassword = false;
            Assert.Equal('\0', inputTextBox.PasswordChar);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Template_ReadOnlySyncsInputTextBoxAndPreventsInput()
    {
        var pinBox = new PinBox
        {
            Length = 4,
            Text = "12",
            IsReadOnly = true
        };
        var window = ShowInWindow(pinBox);

        try
        {
            var inputTextBox = GetTemplatePart<TextBox>(pinBox, "PART_InputTextBox");

            Assert.True(inputTextBox.IsReadOnly);

            pinBox.Focus();
            window.KeyTextInput("3");

            Assert.Equal("12", pinBox.Text);

            pinBox.IsReadOnly = false;
            Assert.False(inputTextBox.IsReadOnly);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Template_TextInputUsesTextNormalizer()
    {
        var pinBox = new PinBox
        {
            DigitsOnly = false,
            Length = 4,
            TextNormalizer = text => text.ToUpperInvariant()
        };
        var window = ShowInWindow(pinBox);

        try
        {
            pinBox.Focus();
            window.KeyTextInput("ab-c");

            Assert.Equal("ABC", pinBox.Text);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Template_PasteStyleTextInputFiltersBeforeLengthLimit()
    {
        var pinBox = new PinBox { Length = 4 };
        var window = ShowInWindow(pinBox);

        try
        {
            pinBox.Focus();
            window.KeyTextInput("12a34");

            Assert.Equal("1234", pinBox.Text);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Template_DeleteRemovesLastCharacter()
    {
        var pinBox = new PinBox
        {
            Length = 4,
            Text = "1234"
        };
        var window = ShowInWindow(pinBox);

        try
        {
            pinBox.Focus();
            window.KeyPress(Key.Delete, RawInputModifiers.None, PhysicalKey.Delete, "Delete");

            Assert.Equal("123", pinBox.Text);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Template_ArrowKeysDoNotMoveDeletionAwayFromEnd()
    {
        var pinBox = new PinBox
        {
            Length = 4,
            Text = "1234"
        };
        var window = ShowInWindow(pinBox);

        try
        {
            pinBox.Focus();
            window.KeyPress(Key.Left, RawInputModifiers.None, PhysicalKey.ArrowLeft, "ArrowLeft");
            window.KeyPress(Key.Back, RawInputModifiers.None, PhysicalKey.Backspace, "Backspace");

            Assert.Equal("123", pinBox.Text);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void ResponsiveLayout_ShrinksItemsWhenWidthIsConstrained()
    {
        var pinBox = new PinBox
        {
            Length = 4,
            Spacing = 8,
            MinItemWidth = 20
        };
        var window = ShowInWindow(pinBox, width: 140, height: 90);

        try
        {
            var items = GetVisualItems(pinBox);

            Assert.Equal(4, items.Length);
            Assert.All(items, item => Assert.InRange(item.Bounds.Width, 20, 55));
            Assert.All(items, item => Assert.True(item.Bounds.Width < PinBoxTheme.Default.Width));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void PinBoxItem_StateUpdatesPseudoClasses()
    {
        var item = new PinBoxItem();

        Assert.Contains(":empty", item.Classes);

        item.State = PinBoxItemState.Filled;
        Assert.Contains(":filled", item.Classes);
        Assert.DoesNotContain(":empty", item.Classes);

        item.State = PinBoxItemState.Error;
        Assert.Contains(":error", item.Classes);

        item.State = PinBoxItemState.Disabled;
        Assert.Contains(":disabled", item.Classes);
    }

    [AvaloniaFact]
    public void PinBox_RendersRoundedAndErrorStates()
    {
        var panel = new StackPanel
        {
            Width = 280,
            Height = 170,
            Spacing = 12,
            Children =
            {
                new PinBox
                {
                    Length = 4,
                    Text = "12",
                    Classes = { "rounded" }
                },
                new PinBox
                {
                    Length = 4,
                    Text = "9999",
                    Validator = text => text == "1234" ? null : "Invalid PIN"
                }
            }
        };
        var window = ShowInWindow(panel, width: 320, height: 220);

        try
        {
            var bitmap = new RenderTargetBitmap(new PixelSize(320, 220), new Vector(96, 96));

            bitmap.Render(window);

            Assert.Equal(new PixelSize(320, 220), bitmap.PixelSize);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task ShakeAsync_ReservesHorizontalSpaceForAnimation()
    {
        var pinBox = new PinBox
        {
            Length = 4,
            AnimationDuration = TimeSpan.Zero
        };
        var window = ShowInWindow(pinBox);

        try
        {
            var itemsPanel = GetTemplatePart<StackPanel>(pinBox, "PART_ItemsPanel");

            Assert.False(itemsPanel.ClipToBounds);
            Assert.Equal(8, itemsPanel.Margin.Left);
            Assert.Equal(8, itemsPanel.Margin.Right);

            await pinBox.ShakeAsync();

            var transform = Assert.IsType<TranslateTransform>(itemsPanel.RenderTransform);
            Assert.Equal(0, transform.X);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void FilledItems_UseFilledState()
    {
        var pinBox = new PinBox { Length = 4 };
        AttachItemsPanel(pinBox);

        pinBox.Text = "12";

        var items = GetItems(pinBox);
        Assert.Equal(PinBoxItemState.Filled, items[0].State);
        Assert.Equal(PinBoxItemState.Filled, items[1].State);
        Assert.Equal(PinBoxItemState.Default, items[2].State);
        Assert.Equal(PinBoxItemState.Default, items[3].State);
    }

    [AvaloniaFact]
    public void Spacing_CoercesInvalidValues()
    {
        var pinBox = new PinBox();

        pinBox.Spacing = -1;
        Assert.Equal(0, pinBox.Spacing);

        pinBox.Spacing = double.NaN;
        Assert.Equal(0, pinBox.Spacing);
    }

    private static StackPanel AttachItemsPanel(PinBox pinBox)
    {
        var panel = new StackPanel();
        typeof(PinBox)
            .GetField("_itemsPanel", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(pinBox, panel);
        typeof(PinBox)
            .GetMethod("CreateItems", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(pinBox, null);

        return panel;
    }

    private static IReadOnlyList<PinBoxItem> GetItems(PinBox pinBox)
    {
        return (IReadOnlyList<PinBoxItem>)typeof(PinBox)
            .GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(pinBox)!;
    }

    private static PinBoxItem[] GetVisualItems(PinBox pinBox)
    {
        return pinBox.GetVisualDescendants().OfType<PinBoxItem>().ToArray();
    }

    private static T GetTemplatePart<T>(Control control, string name) where T : Control
    {
        return control.GetVisualDescendants().OfType<T>().Single(part => part.Name == name);
    }

    private static string[] GetPseudoClassMetadata(Type type)
    {
        return type.GetCustomAttributesData()
            .Where(attribute => attribute.AttributeType.Name == "PseudoClassesAttribute")
            .SelectMany(attribute => attribute.ConstructorArguments)
            .SelectMany(argument =>
            {
                if (argument.Value is IEnumerable<CustomAttributeTypedArgument> values)
                {
                    return values.Select(value => value.Value?.ToString());
                }

                return new[] { argument.Value?.ToString() };
            })
            .Where(value => value != null)
            .Cast<string>()
            .ToArray();
    }

    private static Window ShowInWindow(Control control, double width = 500, double height = 200)
    {
        var window = new Window
        {
            Width = width,
            Height = height,
            Content = control
        };

        window.Show();
        control.ApplyTemplate();
        control.UpdateLayout();

        return window;
    }

    private sealed class TestablePinBox : PinBox
    {
        public void SimulateTextInput(string text)
        {
            OnTextInput(new TextInputEventArgs
            {
                Text = text
            });
        }

        public void SimulateKeyDown(Key key)
        {
            OnKeyDown(new KeyEventArgs
            {
                Key = key
            });
        }
    }

    private sealed class TestCommand : ICommand
    {
        private readonly bool _canExecute;

        public TestCommand(bool canExecute = true)
        {
            _canExecute = canExecute;
        }

        public int ExecuteCount { get; private set; }

        public object? LastParameter { get; private set; }

        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object? parameter) => _canExecute;

        public void Execute(object? parameter)
        {
            ExecuteCount++;
            LastParameter = parameter;
        }
    }
}
