using ZXing.Common;

namespace Nova.Avalonia.UI.BarcodeGenerator;

/// <summary>
/// Event arguments for successful barcode generation.
/// </summary>
public class BarcodeGeneratedEventArgs : EventArgs
{
    public BitMatrix Matrix { get; }
    public BarcodeGeneratedEventArgs(BitMatrix matrix) => Matrix = matrix;
}