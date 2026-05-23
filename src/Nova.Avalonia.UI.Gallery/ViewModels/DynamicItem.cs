using Avalonia.Media;

namespace Nova.Avalonia.UI.Gallery.ViewModels;

/// <summary>
/// Simple reusable item for dynamic panel demos.
/// </summary>
public record DynamicItem(string Title, string ColorHex)
{
    public IBrush Brush => new SolidColorBrush(Color.Parse(ColorHex));
}
