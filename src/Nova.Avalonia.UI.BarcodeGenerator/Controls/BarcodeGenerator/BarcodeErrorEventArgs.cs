namespace Nova.Avalonia.UI.BarcodeGenerator;

/// <summary>
/// Event arguments for failed barcode generation.
/// </summary>
public class BarcodeErrorEventArgs : EventArgs
{
    public Exception Exception { get; }
    public BarcodeErrorEventArgs(Exception exception) => Exception = exception;
}