using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Nova.Avalonia.UI.Controls
{
    /// <summary>
    /// Represents an individual item within the RatingControl (e.g., a single star).
    /// </summary>
    public class RatingItem : Control
    {
        private double _fillRatio;
        private IBrush? _ratedFill;
        private IBrush? _unratedFill;
        private IBrush? _ratedStroke;
        private IBrush? _unratedStroke;
        private double _strokeThickness;
        private Pen? _ratedPen;
        private Pen? _unratedPen;

        /// <summary>
        /// Gets or sets the geometry to render for this item.
        /// </summary>
        public Geometry? Geometry { get; set; }

        /// <summary>
        /// Gets or sets the thickness of the stroke.
        /// </summary>
        public double StrokeThickness
        {
            get => _strokeThickness;
            set
            {
                if (_strokeThickness != value)
                {
                    _strokeThickness = value;
                    InvalidatePens();
                    InvalidateVisual();
                }
            }
        }

        /// <summary>
        /// Gets or sets the fill brush for the rated portion.
        /// </summary>
        public IBrush? RatedFill 
        { 
            get => _ratedFill; 
            set { _ratedFill = value; InvalidateVisual(); } 
        }

        /// <summary>
        /// Gets or sets the fill brush for the unrated portion.
        /// </summary>
        public IBrush? UnratedFill 
        { 
            get => _unratedFill; 
            set { _unratedFill = value; InvalidateVisual(); } 
        }

        /// <summary>
        /// Gets or sets the stroke brush for the rated portion.
        /// </summary>
        public IBrush? RatedStroke
        {
            get => _ratedStroke;
            set { _ratedStroke = value; _ratedPen = null; InvalidateVisual(); }
        }

        /// <summary>
        /// Gets or sets the stroke brush for the unrated portion.
        /// </summary>
        public IBrush? UnratedStroke
        {
            get => _unratedStroke;
            set { _unratedStroke = value; _unratedPen = null; InvalidateVisual(); }
        }

        /// <summary>
        /// Gets or sets the ratio of the item that is filled (0.0 to 1.0).
        /// </summary>
        public double FillRatio
        {
            get => _fillRatio;
            set { _fillRatio = value; InvalidateVisual(); }
        }

        public override void Render(DrawingContext context)
        {
            if (Geometry == null) return;

            var bounds = Geometry.Bounds;
            if (bounds.Width <= 0 || bounds.Height <= 0) return;

            var safePadding = Math.Max(2.0, StrokeThickness * 1.5);
            var availableWidth = Bounds.Width - (safePadding * 2);
            var availableHeight = Bounds.Height - (safePadding * 2);

            if (availableWidth <= 0 || availableHeight <= 0) return;

            var scale = Math.Min(availableWidth / bounds.Width, availableHeight / bounds.Height);
            var tx = safePadding + (availableWidth - (bounds.Width * scale)) / 2.0 - (bounds.X * scale);
            var ty = safePadding + (availableHeight - (bounds.Height * scale)) / 2.0 - (bounds.Y * scale);

            using (context.PushTransform(Matrix.CreateTranslation(tx, ty) * Matrix.CreateScale(scale, scale)))
            {
                var unratedPen = GetUnratedPen(scale);
                var ratedPen = GetRatedPen(scale);

                if (UnratedFill != null || unratedPen != null)
                {
                    context.DrawGeometry(UnratedFill, unratedPen, Geometry);
                }

                if (FillRatio > 0)
                {
                    if (FillRatio >= 0.999)
                    {
                        context.DrawGeometry(RatedFill, ratedPen, Geometry);
                    }
                    else
                    {
                        var largeBuffer = 10000.0;
                        var visibleWidth = bounds.X + (bounds.Width * FillRatio);

                        var clipRect = new Rect(
                            -largeBuffer,
                            -largeBuffer,
                            largeBuffer + visibleWidth,
                            largeBuffer * 2);

                        using (context.PushClip(clipRect))
                        {
                            context.DrawGeometry(RatedFill, ratedPen, Geometry);
                        }
                    }
                }
            }
        }

        private Pen? GetRatedPen(double scale)
        {
            if (RatedStroke == null || StrokeThickness <= 0)
                return null;

            _ratedPen ??= new Pen(RatedStroke, StrokeThickness, lineJoin: PenLineJoin.Round);
            _ratedPen.Thickness = StrokeThickness / scale;
            return _ratedPen;
        }

        private Pen? GetUnratedPen(double scale)
        {
            if (UnratedStroke == null || StrokeThickness <= 0)
                return null;

            _unratedPen ??= new Pen(UnratedStroke, StrokeThickness, lineJoin: PenLineJoin.Round);
            _unratedPen.Thickness = StrokeThickness / scale;
            return _unratedPen;
        }

        private void InvalidatePens()
        {
            _ratedPen = null;
            _unratedPen = null;
        }
    }
}