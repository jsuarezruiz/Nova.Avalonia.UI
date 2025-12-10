using Avalonia.Controls;
using Avalonia.Interactivity;
using Nova.Avalonia.UI.Controls;

namespace Nova.Avalonia.UI.Gallery.Views;

public partial class RatingControlView : UserControl
{
    public RatingControlView()
    {
        InitializeComponent();
    }

    private void OnRatingValueChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is RatingControl ratingControl)
        {
            RatingValueText.Text = $"Value: {ratingControl.Value:F1}";
        }
    }
}