namespace Nova.Avalonia.UI.BarcodeGenerator;

/// <summary>
/// Error correction levels for QR codes. Higher levels allow more damage recovery but reduce data capacity.
/// </summary>
public enum QRErrorCorrectionLevel
{
    /// <summary>Low - 7% error recovery capacity</summary>
    L,
    /// <summary>Medium - 15% error recovery capacity (default)</summary>
    M,
    /// <summary>Quartile - 25% error recovery capacity</summary>
    Q,
    /// <summary>High - 30% error recovery capacity (recommended for logo overlay)</summary>
    H
}