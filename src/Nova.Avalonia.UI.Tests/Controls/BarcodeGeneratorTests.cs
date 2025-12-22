using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Nova.Avalonia.UI.BarcodeGenerator;
using Xunit;
using BarcodeGeneratorControl = Nova.Avalonia.UI.BarcodeGenerator.BarcodeGenerator;

namespace Nova.Avalonia.UI.Tests.Controls;

public class BarcodeGeneratorTests
{
    [AvaloniaFact]
    public void BarcodeGenerator_Should_Have_Default_Values()
    {
        var barcode = new BarcodeGeneratorControl();

        Assert.Equal("", barcode.Value);
        Assert.Equal(BarcodeSymbology.QRCode, barcode.Symbology);
        Assert.Equal(2, barcode.QuietZone);
        Assert.False(barcode.ShowText);
        Assert.Equal(14d, barcode.TextFontSize);
        Assert.Equal(TextAlignment.Center, barcode.TextAlignment);
        Assert.Equal(QRErrorCorrectionLevel.M, barcode.ErrorCorrectionLevel);
        Assert.Equal(0.25, barcode.LogoSizePercent);
        Assert.Null(barcode.Logo);
        Assert.Null(barcode.ErrorMessage);
    }

    [AvaloniaFact]
    public void Value_Property_Should_Update()
    {
        var barcode = new BarcodeGeneratorControl { Value = "https://avaloniaui.net" };

        Assert.Equal("https://avaloniaui.net", barcode.Value);
    }

    [AvaloniaTheory]
    [InlineData(BarcodeSymbology.QRCode)]
    [InlineData(BarcodeSymbology.DataMatrix)]
    [InlineData(BarcodeSymbology.Code128)]
    [InlineData(BarcodeSymbology.Code39)]
    [InlineData(BarcodeSymbology.Code93)]
    [InlineData(BarcodeSymbology.EAN8)]
    [InlineData(BarcodeSymbology.EAN13)]
    [InlineData(BarcodeSymbology.UPCA)]
    [InlineData(BarcodeSymbology.UPCE)]
    [InlineData(BarcodeSymbology.Codabar)]
    [InlineData(BarcodeSymbology.PDF417)]
    [InlineData(BarcodeSymbology.Aztec)]
    [InlineData(BarcodeSymbology.ITF)]
    public void Symbology_AllValues_CanBeSet(BarcodeSymbology symbology)
    {
        var barcode = new BarcodeGeneratorControl { Symbology = symbology };

        Assert.Equal(symbology, barcode.Symbology);
    }

    [AvaloniaFact]
    public void BarBrush_Default_IsBlack()
    {
        var barcode = new BarcodeGeneratorControl();

        Assert.NotNull(barcode.BarBrush);
        var brush = Assert.IsAssignableFrom<ISolidColorBrush>(barcode.BarBrush);
        Assert.Equal(Colors.Black, brush.Color);
    }

    [AvaloniaFact]
    public void BackgroundBrush_Default_IsWhite()
    {
        var barcode = new BarcodeGeneratorControl();

        Assert.NotNull(barcode.BackgroundBrush);
        var brush = Assert.IsAssignableFrom<ISolidColorBrush>(barcode.BackgroundBrush);
        Assert.Equal(Colors.White, brush.Color);
    }

    [AvaloniaFact]
    public void BarBrush_CanBeCustomized()
    {
        var customBrush = new SolidColorBrush(Colors.Blue);
        var barcode = new BarcodeGeneratorControl { BarBrush = customBrush };

        Assert.Equal(customBrush, barcode.BarBrush);
    }

    [AvaloniaFact]
    public void BackgroundBrush_CanBeCustomized()
    {
        var customBrush = new SolidColorBrush(Colors.LightGray);
        var barcode = new BarcodeGeneratorControl { BackgroundBrush = customBrush };

        Assert.Equal(customBrush, barcode.BackgroundBrush);
    }

    [AvaloniaFact]
    public void QuietZone_CanBeModified()
    {
        var barcode = new BarcodeGeneratorControl { QuietZone = 5 };

        Assert.Equal(5, barcode.QuietZone);
    }

    [AvaloniaFact]
    public void ShowText_CanBeEnabled()
    {
        var barcode = new BarcodeGeneratorControl { ShowText = true };

        Assert.True(barcode.ShowText);
    }

    [AvaloniaFact]
    public void TextFontSize_CanBeModified()
    {
        var barcode = new BarcodeGeneratorControl { TextFontSize = 20 };

        Assert.Equal(20, barcode.TextFontSize);
    }

    [AvaloniaFact]
    public void TextForeground_Default_IsBlack()
    {
        var barcode = new BarcodeGeneratorControl();

        Assert.NotNull(barcode.TextForeground);
        var brush = Assert.IsAssignableFrom<ISolidColorBrush>(barcode.TextForeground);
        Assert.Equal(Colors.Black, brush.Color);
    }

    [AvaloniaFact]
    public void TextForeground_CanBeCustomized()
    {
        var customBrush = new SolidColorBrush(Colors.Red);
        var barcode = new BarcodeGeneratorControl { TextForeground = customBrush };

        Assert.Equal(customBrush, barcode.TextForeground);
    }

    [AvaloniaFact]
    public void TextMargin_Default()
    {
        var barcode = new BarcodeGeneratorControl();

        Assert.Equal(new Thickness(0, 8, 0, 0), barcode.TextMargin);
    }

    [AvaloniaFact]
    public void TextMargin_CanBeModified()
    {
        var barcode = new BarcodeGeneratorControl { TextMargin = new Thickness(5, 10, 5, 0) };

        Assert.Equal(new Thickness(5, 10, 5, 0), barcode.TextMargin);
    }

    [AvaloniaTheory]
    [InlineData(TextAlignment.Left)]
    [InlineData(TextAlignment.Center)]
    [InlineData(TextAlignment.Right)]
    public void TextAlignment_AllValues_CanBeSet(TextAlignment alignment)
    {
        var barcode = new BarcodeGeneratorControl { TextAlignment = alignment };

        Assert.Equal(alignment, barcode.TextAlignment);
    }

    [AvaloniaTheory]
    [InlineData(QRErrorCorrectionLevel.L)]
    [InlineData(QRErrorCorrectionLevel.M)]
    [InlineData(QRErrorCorrectionLevel.Q)]
    [InlineData(QRErrorCorrectionLevel.H)]
    public void ErrorCorrectionLevel_AllValues_CanBeSet(QRErrorCorrectionLevel level)
    {
        var barcode = new BarcodeGeneratorControl { ErrorCorrectionLevel = level };

        Assert.Equal(level, barcode.ErrorCorrectionLevel);
    }

    [AvaloniaFact]
    public void LogoSizePercent_CanBeModified()
    {
        var barcode = new BarcodeGeneratorControl { LogoSizePercent = 0.35 };

        Assert.Equal(0.35, barcode.LogoSizePercent);
    }

    [AvaloniaFact]
    public void Multiple_Properties_CanBeSetSimultaneously()
    {
        var barcode = new BarcodeGeneratorControl
        {
            Value = "TEST123",
            Symbology = BarcodeSymbology.Code128,
            BarBrush = Brushes.DarkBlue,
            BackgroundBrush = Brushes.AliceBlue,
            QuietZone = 4,
            ShowText = true,
            TextFontSize = 16,
            TextForeground = Brushes.DarkBlue,
            TextAlignment = TextAlignment.Left
        };

        Assert.Equal("TEST123", barcode.Value);
        Assert.Equal(BarcodeSymbology.Code128, barcode.Symbology);
        Assert.Equal(Brushes.DarkBlue, barcode.BarBrush);
        Assert.Equal(Brushes.AliceBlue, barcode.BackgroundBrush);
        Assert.Equal(4, barcode.QuietZone);
        Assert.True(barcode.ShowText);
        Assert.Equal(16, barcode.TextFontSize);
        Assert.Equal(Brushes.DarkBlue, barcode.TextForeground);
        Assert.Equal(TextAlignment.Left, barcode.TextAlignment);
    }

    [AvaloniaFact]
    public void QRCode_With_ErrorCorrection_Properties()
    {
        var barcode = new BarcodeGeneratorControl
        {
            Value = "https://example.com",
            Symbology = BarcodeSymbology.QRCode,
            ErrorCorrectionLevel = QRErrorCorrectionLevel.H,
            LogoSizePercent = 0.30
        };

        Assert.Equal(BarcodeSymbology.QRCode, barcode.Symbology);
        Assert.Equal(QRErrorCorrectionLevel.H, barcode.ErrorCorrectionLevel);
        Assert.Equal(0.30, barcode.LogoSizePercent);
    }

    [AvaloniaFact]
    public void EmptyValue_IsValid()
    {
        var barcode = new BarcodeGeneratorControl { Value = "" };

        Assert.Equal("", barcode.Value);
        Assert.Null(barcode.ErrorMessage);
    }

    [AvaloniaFact]
    public void Value_CanBeChangedDynamically()
    {
        var barcode = new BarcodeGeneratorControl { Value = "Initial" };
        Assert.Equal("Initial", barcode.Value);

        barcode.Value = "Updated";
        Assert.Equal("Updated", barcode.Value);

        barcode.Value = "Final";
        Assert.Equal("Final", barcode.Value);
    }

    [AvaloniaFact]
    public void Symbology_CanBeChangedDynamically()
    {
        var barcode = new BarcodeGeneratorControl { Symbology = BarcodeSymbology.QRCode };
        Assert.Equal(BarcodeSymbology.QRCode, barcode.Symbology);

        barcode.Symbology = BarcodeSymbology.Code128;
        Assert.Equal(BarcodeSymbology.Code128, barcode.Symbology);

        barcode.Symbology = BarcodeSymbology.DataMatrix;
        Assert.Equal(BarcodeSymbology.DataMatrix, barcode.Symbology);
    }

    [AvaloniaFact]
    public void QuietZone_CanBeZero()
    {
        var barcode = new BarcodeGeneratorControl { QuietZone = 0 };

        Assert.Equal(0, barcode.QuietZone);
    }

    [AvaloniaFact]
    public void LogoSizePercent_EdgeValues()
    {
        var barcode = new BarcodeGeneratorControl { LogoSizePercent = 0.1 };
        Assert.Equal(0.1, barcode.LogoSizePercent);

        barcode.LogoSizePercent = 0.4;
        Assert.Equal(0.4, barcode.LogoSizePercent);
    }

    [AvaloniaFact]
    public void BarBrush_CanUseGradientBrush()
    {
        var gradientBrush = new LinearGradientBrush
        {
            GradientStops = new GradientStops
            {
                new GradientStop(Colors.Black, 0),
                new GradientStop(Colors.DarkBlue, 1)
            }
        };
        var barcode = new BarcodeGeneratorControl { BarBrush = gradientBrush };

        Assert.Equal(gradientBrush, barcode.BarBrush);
    }

    [AvaloniaFact]
    public void BackgroundBrush_CanUseGradientBrush()
    {
        var gradientBrush = new LinearGradientBrush
        {
            GradientStops = new GradientStops
            {
                new GradientStop(Colors.White, 0),
                new GradientStop(Colors.LightBlue, 1)
            }
        };
        var barcode = new BarcodeGeneratorControl { BackgroundBrush = gradientBrush };

        Assert.Equal(gradientBrush, barcode.BackgroundBrush);
    }

    [AvaloniaFact]
    public void Value_WithSpecialCharacters()
    {
        var barcode = new BarcodeGeneratorControl { Value = "Test!@#$%^&*()_+-=[]{}|;':\",./<>?" };

        Assert.Equal("Test!@#$%^&*()_+-=[]{}|;':\",./<>?", barcode.Value);
    }

    [AvaloniaFact]
    public void Value_WithUnicodeCharacters()
    {
        var barcode = new BarcodeGeneratorControl { Value = "こんにちは世界" };

        Assert.Equal("こんにちは世界", barcode.Value);
    }

    [AvaloniaFact]
    public void Value_WithEmoji()
    {
        var barcode = new BarcodeGeneratorControl { Value = "Hello 🌍" };

        Assert.Equal("Hello 🌍", barcode.Value);
    }

    [AvaloniaFact]
    public void Value_LongString()
    {
        var longValue = new string('A', 1000);
        var barcode = new BarcodeGeneratorControl { Value = longValue };

        Assert.Equal(longValue, barcode.Value);
    }

    [AvaloniaFact]
    public void Value_WithWhitespace()
    {
        var barcode = new BarcodeGeneratorControl { Value = "   spaces   " };

        Assert.Equal("   spaces   ", barcode.Value);
    }

    [AvaloniaFact]
    public void Value_WithNewlines()
    {
        var barcode = new BarcodeGeneratorControl { Value = "Line1\nLine2\nLine3" };

        Assert.Equal("Line1\nLine2\nLine3", barcode.Value);
    }

    [AvaloniaFact]
    public void QuietZone_LargeValue()
    {
        var barcode = new BarcodeGeneratorControl { QuietZone = 100 };

        Assert.Equal(100, barcode.QuietZone);
    }

    [AvaloniaFact]
    public void TextFontSize_SmallValue()
    {
        var barcode = new BarcodeGeneratorControl { TextFontSize = 6 };

        Assert.Equal(6, barcode.TextFontSize);
    }

    [AvaloniaFact]
    public void TextFontSize_LargeValue()
    {
        var barcode = new BarcodeGeneratorControl { TextFontSize = 72 };

        Assert.Equal(72, barcode.TextFontSize);
    }

    [AvaloniaFact]
    public void TextMargin_Asymmetric()
    {
        var barcode = new BarcodeGeneratorControl { TextMargin = new Thickness(1, 2, 3, 4) };

        Assert.Equal(new Thickness(1, 2, 3, 4), barcode.TextMargin);
    }

    [AvaloniaFact]
    public void TextMargin_Zero()
    {
        var barcode = new BarcodeGeneratorControl { TextMargin = new Thickness(0) };

        Assert.Equal(new Thickness(0), barcode.TextMargin);
    }

    [AvaloniaFact]
    public void TextMargin_Uniform()
    {
        var barcode = new BarcodeGeneratorControl { TextMargin = new Thickness(10) };

        Assert.Equal(new Thickness(10), barcode.TextMargin);
    }

    [AvaloniaFact]
    public void LogoSizePercent_BelowMin_StillSetsValue()
    {
        var barcode = new BarcodeGeneratorControl { LogoSizePercent = 0.05 };

        Assert.Equal(0.05, barcode.LogoSizePercent);
    }

    [AvaloniaFact]
    public void LogoSizePercent_AboveMax_StillSetsValue()
    {
        var barcode = new BarcodeGeneratorControl { LogoSizePercent = 0.5 };

        Assert.Equal(0.5, barcode.LogoSizePercent);
    }

    [AvaloniaFact]
    public void ErrorCorrectionLevel_CanBeChangedDynamically()
    {
        var barcode = new BarcodeGeneratorControl { ErrorCorrectionLevel = QRErrorCorrectionLevel.L };
        Assert.Equal(QRErrorCorrectionLevel.L, barcode.ErrorCorrectionLevel);

        barcode.ErrorCorrectionLevel = QRErrorCorrectionLevel.H;
        Assert.Equal(QRErrorCorrectionLevel.H, barcode.ErrorCorrectionLevel);
    }

    [AvaloniaFact]
    public void ShowText_CanBeToggledDynamically()
    {
        var barcode = new BarcodeGeneratorControl { ShowText = false };
        Assert.False(barcode.ShowText);

        barcode.ShowText = true;
        Assert.True(barcode.ShowText);

        barcode.ShowText = false;
        Assert.False(barcode.ShowText);
    }

    [AvaloniaFact]
    public void BarBrush_CanBeChangedDynamically()
    {
        var barcode = new BarcodeGeneratorControl { BarBrush = Brushes.Black };
        Assert.Equal(Brushes.Black, barcode.BarBrush);

        barcode.BarBrush = Brushes.Red;
        Assert.Equal(Brushes.Red, barcode.BarBrush);

        barcode.BarBrush = Brushes.Blue;
        Assert.Equal(Brushes.Blue, barcode.BarBrush);
    }

    [AvaloniaFact]
    public void BackgroundBrush_CanBeChangedDynamically()
    {
        var barcode = new BarcodeGeneratorControl { BackgroundBrush = Brushes.White };
        Assert.Equal(Brushes.White, barcode.BackgroundBrush);

        barcode.BackgroundBrush = Brushes.LightGray;
        Assert.Equal(Brushes.LightGray, barcode.BackgroundBrush);
    }

    [AvaloniaFact]
    public void Code128_Properties()
    {
        var barcode = new BarcodeGeneratorControl
        {
            Value = "CODE128TEST",
            Symbology = BarcodeSymbology.Code128,
            ShowText = true
        };

        Assert.Equal("CODE128TEST", barcode.Value);
        Assert.Equal(BarcodeSymbology.Code128, barcode.Symbology);
        Assert.True(barcode.ShowText);
    }

    [AvaloniaFact]
    public void EAN13_Properties()
    {
        var barcode = new BarcodeGeneratorControl
        {
            Value = "5901234123457",
            Symbology = BarcodeSymbology.EAN13,
            ShowText = true
        };

        Assert.Equal("5901234123457", barcode.Value);
        Assert.Equal(BarcodeSymbology.EAN13, barcode.Symbology);
    }

    [AvaloniaFact]
    public void DataMatrix_Properties()
    {
        var barcode = new BarcodeGeneratorControl
        {
            Value = "DataMatrix Test",
            Symbology = BarcodeSymbology.DataMatrix
        };

        Assert.Equal("DataMatrix Test", barcode.Value);
        Assert.Equal(BarcodeSymbology.DataMatrix, barcode.Symbology);
    }

    [AvaloniaFact]
    public void PDF417_Properties()
    {
        var barcode = new BarcodeGeneratorControl
        {
            Value = "PDF417 High Capacity Data",
            Symbology = BarcodeSymbology.PDF417,
            ShowText = true
        };

        Assert.Equal("PDF417 High Capacity Data", barcode.Value);
        Assert.Equal(BarcodeSymbology.PDF417, barcode.Symbology);
    }

    [AvaloniaFact]
    public void Aztec_WithErrorCorrection()
    {
        var barcode = new BarcodeGeneratorControl
        {
            Value = "Aztec Code",
            Symbology = BarcodeSymbology.Aztec,
            ErrorCorrectionLevel = QRErrorCorrectionLevel.H
        };

        Assert.Equal(BarcodeSymbology.Aztec, barcode.Symbology);
        Assert.Equal(QRErrorCorrectionLevel.H, barcode.ErrorCorrectionLevel);
    }

    [AvaloniaFact]
    public void ITF_NumericValue()
    {
        var barcode = new BarcodeGeneratorControl
        {
            Value = "12345678901234",
            Symbology = BarcodeSymbology.ITF
        };

        Assert.Equal("12345678901234", barcode.Value);
        Assert.Equal(BarcodeSymbology.ITF, barcode.Symbology);
    }

    [AvaloniaFact]
    public void Value_NumericOnly()
    {
        var barcode = new BarcodeGeneratorControl { Value = "1234567890" };

        Assert.Equal("1234567890", barcode.Value);
    }

    [AvaloniaFact]
    public void Value_URL()
    {
        var barcode = new BarcodeGeneratorControl { Value = "https://www.example.com/path?query=value#anchor" };

        Assert.Equal("https://www.example.com/path?query=value#anchor", barcode.Value);
    }

    [AvaloniaFact]
    public void Value_Email()
    {
        var barcode = new BarcodeGeneratorControl { Value = "mailto:test@example.com" };

        Assert.Equal("mailto:test@example.com", barcode.Value);
    }

    [AvaloniaFact]
    public void TextAlignment_CanBeChangedDynamically()
    {
        var barcode = new BarcodeGeneratorControl { TextAlignment = TextAlignment.Left };
        Assert.Equal(TextAlignment.Left, barcode.TextAlignment);

        barcode.TextAlignment = TextAlignment.Center;
        Assert.Equal(TextAlignment.Center, barcode.TextAlignment);

        barcode.TextAlignment = TextAlignment.Right;
        Assert.Equal(TextAlignment.Right, barcode.TextAlignment);
    }

    [AvaloniaFact]
    public void QuietZone_CanBeChangedDynamically()
    {
        var barcode = new BarcodeGeneratorControl { QuietZone = 1 };
        Assert.Equal(1, barcode.QuietZone);

        barcode.QuietZone = 5;
        Assert.Equal(5, barcode.QuietZone);

        barcode.QuietZone = 10;
        Assert.Equal(10, barcode.QuietZone);
    }

    [AvaloniaFact]
    public void TextFontSize_CanBeChangedDynamically()
    {
        var barcode = new BarcodeGeneratorControl { TextFontSize = 10 };
        Assert.Equal(10, barcode.TextFontSize);

        barcode.TextFontSize = 20;
        Assert.Equal(20, barcode.TextFontSize);
    }

    [AvaloniaFact]
    public void TextForeground_CanBeChangedDynamically()
    {
        var barcode = new BarcodeGeneratorControl { TextForeground = Brushes.Black };
        Assert.Equal(Brushes.Black, barcode.TextForeground);

        barcode.TextForeground = Brushes.DarkGray;
        Assert.Equal(Brushes.DarkGray, barcode.TextForeground);
    }

    [AvaloniaFact]
    public void TextMargin_CanBeChangedDynamically()
    {
        var barcode = new BarcodeGeneratorControl { TextMargin = new Thickness(0) };
        Assert.Equal(new Thickness(0), barcode.TextMargin);

        barcode.TextMargin = new Thickness(5, 10, 5, 0);
        Assert.Equal(new Thickness(5, 10, 5, 0), barcode.TextMargin);
    }

    [AvaloniaFact]
    public void LogoSizePercent_CanBeChangedDynamically()
    {
        var barcode = new BarcodeGeneratorControl { LogoSizePercent = 0.2 };
        Assert.Equal(0.2, barcode.LogoSizePercent);

        barcode.LogoSizePercent = 0.35;
        Assert.Equal(0.35, barcode.LogoSizePercent);
    }

    [AvaloniaFact]
    public void AllSymbologies_WithSameValue()
    {
        var testValue = "TEST123";
        var symbologies = new[]
        {
            BarcodeSymbology.QRCode, BarcodeSymbology.DataMatrix, BarcodeSymbology.Code128,
            BarcodeSymbology.Code39, BarcodeSymbology.Code93, BarcodeSymbology.PDF417,
            BarcodeSymbology.Aztec
        };

        foreach (var symbology in symbologies)
        {
            var barcode = new BarcodeGeneratorControl
            {
                Value = testValue,
                Symbology = symbology
            };

            Assert.Equal(testValue, barcode.Value);
            Assert.Equal(symbology, barcode.Symbology);
        }
    }
}

