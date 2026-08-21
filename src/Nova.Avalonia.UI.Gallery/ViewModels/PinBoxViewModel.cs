using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Nova.Avalonia.UI.Gallery.ViewModels;

public partial class PinBoxViewModel : PageViewModel
{
    [ObservableProperty]
    private string _pin = string.Empty;

    [ObservableProperty]
    private string _otpCode = string.Empty;

    [ObservableProperty]
    private string _passwordPin = string.Empty;

    [ObservableProperty]
    private string _groupedCode = string.Empty;

    [ObservableProperty]
    private string _backupCode = string.Empty;

    [ObservableProperty]
    private string _statusText = "Enter your PIN";

    [ObservableProperty]
    private string _interactivePin = string.Empty;

    [ObservableProperty]
    private string _validationPin = string.Empty;

    [ObservableProperty]
    private bool _isVerifying;

    public PinBoxViewModel() : base("PinBox")
    {
    }

    [RelayCommand]
    private void PinCompleted(string pin)
    {
        StatusText = $"PIN entered: {pin}";
    }

    [RelayCommand]
    private void Clear()
    {
        Pin = string.Empty;
        OtpCode = string.Empty;
        PasswordPin = string.Empty;
        GroupedCode = string.Empty;
        BackupCode = string.Empty;
        InteractivePin = string.Empty;
        ValidationPin = string.Empty;
        StatusText = "Cleared all PINs";
    }

    [RelayCommand]
    private void ClearValidation()
    {
        ValidationPin = string.Empty;
    }

    public string? ValidatePin(string text)
    {
        if (string.IsNullOrEmpty(text))
            return null;

        if (text.Length == 4 && text != "1234")
            return "Invalid PIN. Try 1234";

        return null;
    }

    public string NormalizeBackupCode(string text)
    {
        return text.ToUpperInvariant();
    }
}
