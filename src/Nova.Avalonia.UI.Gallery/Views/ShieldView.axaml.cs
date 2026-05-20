using Avalonia.Controls;

namespace Nova.Avalonia.UI.Gallery.Views;

public partial class ShieldView : UserControl
{
    public ShieldView()
    {
        InitializeComponent();
    }

    private void OnShieldClick(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (this.FindControl<TextBlock>("EventResultTextBlock") is { } textBlock)
        {
            textBlock.Text = $"Clicked at {System.DateTime.Now.ToLongTimeString()}";
        }
    }
}
