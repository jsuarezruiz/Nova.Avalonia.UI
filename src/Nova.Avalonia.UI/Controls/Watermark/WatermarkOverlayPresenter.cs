using Avalonia.Controls;
using Avalonia.Media;

namespace Nova.Avalonia.UI.Controls;

internal sealed class WatermarkOverlayPresenter : Control
{
    private Watermark? _owner;

    internal Watermark? Owner
    {
        get => _owner;
        set
        {
            if (ReferenceEquals(_owner, value))
            {
                return;
            }

            _owner = value;
            InvalidateVisual();
        }
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        _owner?.RenderWatermark(context, Bounds.Size);
    }
}
