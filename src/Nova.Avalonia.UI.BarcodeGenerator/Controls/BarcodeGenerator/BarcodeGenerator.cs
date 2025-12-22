using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Threading;
using ZXing;
using ZXing.Common;

namespace Nova.Avalonia.UI.BarcodeGenerator;

    /// <summary>
    /// A templated control that generates and renders various barcode symbologies (QR Code, EAN-13, Code 128, etc.) 
    /// using the ZXing library. Optimized for performance via BitMatrix/Bitmap caching and supports 
    /// full accessibility as a semantic Image.
    /// </summary>
    public class BarcodeGenerator : TemplatedControl
    {
        private Control? _surface;
        private TextBlock? _caption;

        // Internal caching fields to optimize rendering performance
        // We cache the BitMatrix to avoid re-encoding data (slow)
        // We cache the RenderTargetBitmap to avoid re-drawing thousands of rectangles every frame (slow)
        private BitMatrix? _cachedMatrix;
        private RenderTargetBitmap? _cachedBitmap;
        private string _lastValue = "";
        private BarcodeSymbology _lastSymbology;
        private int _lastQuietZone = -1;
        private Size _lastRenderSize;

        /// <summary>
        /// Defines the <see cref="Value"/> property.
        /// </summary>
        public static readonly StyledProperty<string> ValueProperty =
            AvaloniaProperty.Register<BarcodeGenerator, string>(nameof(Value), "");

        /// <summary>
        /// Defines the <see cref="Symbology"/> property.
        /// </summary>
        public static readonly StyledProperty<BarcodeSymbology> SymbologyProperty =
            AvaloniaProperty.Register<BarcodeGenerator, BarcodeSymbology>(nameof(Symbology), BarcodeSymbology.QRCode);

        /// <summary>
        /// Defines the <see cref="BarBrush"/> property.
        /// </summary>
        public static readonly StyledProperty<IBrush> BarBrushProperty =
            AvaloniaProperty.Register<BarcodeGenerator, IBrush>(nameof(BarBrush), Brushes.Black);

        /// <summary>
        /// Defines the <see cref="BackgroundBrush"/> property.
        /// </summary>
        public static readonly StyledProperty<IBrush> BackgroundBrushProperty =
            AvaloniaProperty.Register<BarcodeGenerator, IBrush>(nameof(BackgroundBrush), Brushes.White);

        /// <summary>
        /// Defines the <see cref="QuietZone"/> property.
        /// </summary>
        public static readonly StyledProperty<int> QuietZoneProperty =
            AvaloniaProperty.Register<BarcodeGenerator, int>(nameof(QuietZone), 2);

        /// <summary>
        /// Defines the <see cref="ShowText"/> property.
        /// </summary>
        public static readonly StyledProperty<bool> ShowTextProperty =
            AvaloniaProperty.Register<BarcodeGenerator, bool>(nameof(ShowText), false);

        /// <summary>
        /// Defines the <see cref="TextFontSize"/> property.
        /// </summary>
        public static readonly StyledProperty<double> TextFontSizeProperty =
            AvaloniaProperty.Register<BarcodeGenerator, double>(nameof(TextFontSize), 14d);

        /// <summary>
        /// Defines the <see cref="TextForeground"/> property.
        /// </summary>
        public static readonly StyledProperty<IBrush> TextForegroundProperty =
            AvaloniaProperty.Register<BarcodeGenerator, IBrush>(nameof(TextForeground), Brushes.Black);

        /// <summary>
        /// Defines the <see cref="TextMargin"/> property.
        /// </summary>
        public static readonly StyledProperty<Thickness> TextMarginProperty =
            AvaloniaProperty.Register<BarcodeGenerator, Thickness>(nameof(TextMargin), new Thickness(0, 8, 0, 0));

        /// <summary>
        /// Defines the <see cref="TextAlignment"/> property.
        /// </summary>
        public static readonly StyledProperty<TextAlignment> TextAlignmentProperty =
            AvaloniaProperty.Register<BarcodeGenerator, TextAlignment>(nameof(TextAlignment), TextAlignment.Center);

        /// <summary>
        /// Defines the <see cref="ErrorCorrectionLevel"/> property. Applies to QR and Aztec codes only.
        /// </summary>
        public static readonly StyledProperty<QRErrorCorrectionLevel> ErrorCorrectionLevelProperty =
            AvaloniaProperty.Register<BarcodeGenerator, QRErrorCorrectionLevel>(nameof(ErrorCorrectionLevel), QRErrorCorrectionLevel.M);

        /// <summary>
        /// Defines the <see cref="Logo"/> property. Applies to QR and Aztec codes only.
        /// </summary>
        public static readonly StyledProperty<IImage?> LogoProperty =
            AvaloniaProperty.Register<BarcodeGenerator, IImage?>(nameof(Logo));

        /// <summary>
        /// Defines the <see cref="LogoSizePercent"/> property. Applies to QR and Aztec codes only.
        /// </summary>
        public static readonly StyledProperty<double> LogoSizePercentProperty =
            AvaloniaProperty.Register<BarcodeGenerator, double>(nameof(LogoSizePercent), 0.25);

        /// <summary>
        /// Defines the <see cref="ErrorMessage"/> property.
        /// </summary>
        public static readonly DirectProperty<BarcodeGenerator, string?> ErrorMessageProperty =
            AvaloniaProperty.RegisterDirect<BarcodeGenerator, string?>(nameof(ErrorMessage), o => o.ErrorMessage);

        private string? _errorMessage;
        private QRErrorCorrectionLevel _lastErrorCorrectionLevel;

        /// <summary>
        /// Gets or sets the string data to encode into the barcode.
        /// </summary>
        public string Value { get => GetValue(ValueProperty); set => SetValue(ValueProperty, value); }

        /// <summary>
        /// Gets or sets the type of barcode to generate (e.g., QRCode, Code128).
        /// </summary>
        public BarcodeSymbology Symbology { get => GetValue(SymbologyProperty); set => SetValue(SymbologyProperty, value); }

        /// <summary>
        /// Gets or sets the brush used to draw the barcode bars/pixels.
        /// </summary>
        public IBrush BarBrush { get => GetValue(BarBrushProperty); set => SetValue(BarBrushProperty, value); }

        /// <summary>
        /// Gets or sets the brush used for the background of the barcode area.
        /// </summary>
        public IBrush BackgroundBrush { get => GetValue(BackgroundBrushProperty); set => SetValue(BackgroundBrushProperty, value); }

        /// <summary>
        /// Gets or sets the margin (quiet zone) around the barcode in pixels/modules.
        /// </summary>
        public int QuietZone { get => GetValue(QuietZoneProperty); set => SetValue(QuietZoneProperty, value); }

        /// <summary>
        /// Gets or sets a value indicating whether to display the raw text value below the barcode.
        /// </summary>
        public bool ShowText { get => GetValue(ShowTextProperty); set => SetValue(ShowTextProperty, value); }

        /// <summary>
        /// Gets or sets the font size for the caption text.
        /// </summary>
        public double TextFontSize { get => GetValue(TextFontSizeProperty); set => SetValue(TextFontSizeProperty, value); }

        /// <summary>
        /// Gets or sets the brush used for the caption text.
        /// </summary>
        public IBrush TextForeground { get => GetValue(TextForegroundProperty); set => SetValue(TextForegroundProperty, value); }

        /// <summary>
        /// Gets or sets the margin around the caption text.
        /// </summary>
        public Thickness TextMargin { get => GetValue(TextMarginProperty); set => SetValue(TextMarginProperty, value); }

        /// <summary>
        /// Gets or sets the horizontal alignment of the caption text.
        /// </summary>
        public TextAlignment TextAlignment { get => GetValue(TextAlignmentProperty); set => SetValue(TextAlignmentProperty, value); }

        /// <summary>
        /// Gets or sets the error correction level for QR and Aztec codes (L=7%, M=15%, Q=25%, H=30%).
        /// <para><b>Applies to:</b> QR Code, Aztec only. Ignored for other symbologies.</para>
        /// </summary>
        public QRErrorCorrectionLevel ErrorCorrectionLevel { get => GetValue(ErrorCorrectionLevelProperty); set => SetValue(ErrorCorrectionLevelProperty, value); }

        /// <summary>
        /// Gets or sets an optional logo/watermark image to overlay in the center of QR/Aztec codes.
        /// Requires ErrorCorrectionLevel of Q or H for best results.
        /// <para><b>Applies to:</b> QR Code, Aztec only. Ignored for other symbologies.</para>
        /// </summary>
        public IImage? Logo { get => GetValue(LogoProperty); set => SetValue(LogoProperty, value); }

        /// <summary>
        /// Gets or sets the size of the logo as a percentage of barcode size (0.1 to 0.4). Default is 0.25 (25%).
        /// <para><b>Applies to:</b> QR Code, Aztec only. Ignored for other symbologies.</para>
        /// </summary>
        public double LogoSizePercent { get => GetValue(LogoSizePercentProperty); set => SetValue(LogoSizePercentProperty, value); }

        /// <summary>
        /// Gets the last error message if barcode generation failed, or null if successful.
        /// </summary>
        public string? ErrorMessage
        {
            get => _errorMessage;
            private set => SetAndRaise(ErrorMessageProperty, ref _errorMessage, value);
        }

        /// <summary>
        /// Occurs when a barcode has been successfully generated and encoded.
        /// </summary>
        public event EventHandler<BarcodeGeneratedEventArgs>? BarcodeGenerated;

        /// <summary>
        /// Occurs when barcode generation fails due to invalid data or constraints.
        /// </summary>
        public event EventHandler<BarcodeErrorEventArgs>? BarcodeError;

        static BarcodeGenerator()
        {
            // When data properties change, we must invalidate our cached matrix and bitmap
            ValueProperty.Changed.AddClassHandler<BarcodeGenerator>((x, e) => x.OnDataChanged(e));
            SymbologyProperty.Changed.AddClassHandler<BarcodeGenerator>((x, e) => x.OnDataChanged(e));
            QuietZoneProperty.Changed.AddClassHandler<BarcodeGenerator>((x, e) => x.OnDataChanged(e));
            ErrorCorrectionLevelProperty.Changed.AddClassHandler<BarcodeGenerator>((x, e) => x.OnDataChanged(e));

            // Visual properties only require a re-render, not regeneration of the matrix
            AffectsRender<BarcodeGenerator>(BarBrushProperty, BackgroundBrushProperty, LogoProperty, LogoSizePercentProperty);

            // Caption properties affect layout/measure
            AffectsMeasure<BarcodeGenerator>(ShowTextProperty, TextFontSizeProperty, TextMarginProperty);

            // Update the text block when relevant properties change
            ValueProperty.Changed.AddClassHandler<BarcodeGenerator>((x, _) => x.UpdateCaption());
            ShowTextProperty.Changed.AddClassHandler<BarcodeGenerator>((x, _) => x.UpdateCaption());
            TextFontSizeProperty.Changed.AddClassHandler<BarcodeGenerator>((x, _) => x.UpdateCaption());
            TextForegroundProperty.Changed.AddClassHandler<BarcodeGenerator>((x, _) => x.UpdateCaption());
            TextMarginProperty.Changed.AddClassHandler<BarcodeGenerator>((x, _) => x.UpdateCaption());
            TextAlignmentProperty.Changed.AddClassHandler<BarcodeGenerator>((x, _) => x.UpdateCaption());
        }

        /// <inheritdoc/>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            // Accessibility: Expose this control as an Image to screen readers
            return new BarcodeGeneratorAutomationPeer(this);
        }

        /// <inheritdoc/>
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            _surface = e.NameScope.Find<Control>("PART_Surface");
            _caption = e.NameScope.Find<TextBlock>("PART_Caption");
            UpdateCaption();
            InvalidateVisual();
        }

        private void OnDataChanged(AvaloniaPropertyChangedEventArgs e)
        {
            // Critical for performance: Invalidate heavy caches when data changes
            _cachedMatrix = null;
            ClearBitmapCache();
            InvalidateVisual();
        }
        
        /// <inheritdoc/>
        protected override Size ArrangeOverride(Size finalSize)
        {
            var result = base.ArrangeOverride(finalSize);
            
            // If the control size changes significantly, the cached bitmap is invalid
            // We use a small threshold (1px) to avoid regenerating on sub-pixel layout shifts
            if (_cachedBitmap != null && 
                (Math.Abs(_lastRenderSize.Width - result.Width) > 1 || 
                 Math.Abs(_lastRenderSize.Height - result.Height) > 1))
            {
                ClearBitmapCache();
            }
            _lastRenderSize = result;
            return result;
        }

        private void UpdateCaption()
        {
            if (_caption is null) return;
            _caption.IsVisible = ShowText;
            _caption.Text = Value;
            _caption.FontSize = TextFontSize;
            _caption.Foreground = TextForeground;
            _caption.Margin = TextMargin;
            _caption.TextAlignment = TextAlignment;
        }

        /// <summary>
        /// Generates or retrieves the cached ZXing BitMatrix. 
        /// Handles exceptions internally and updates the ErrorMessage property.
        /// </summary>
        private BitMatrix? GetOrCreateMatrix(Size targetSize)
        {
            // Return existing cache if input parameters haven't changed
            if (_cachedMatrix != null && 
                _lastValue == Value && 
                _lastSymbology == Symbology && 
                _lastQuietZone == QuietZone &&
                _lastErrorCorrectionLevel == ErrorCorrectionLevel)
            {
                return _cachedMatrix;
            }

            if (string.IsNullOrWhiteSpace(Value) || targetSize.Width < 1 || targetSize.Height < 1)
            {
                return null;
            }

            try
            {
                var format = ToFormat(Symbology);
                var hints = new Dictionary<EncodeHintType, object> { { EncodeHintType.MARGIN, QuietZone } };
                
                // Add error correction level for QR codes
                if (Symbology == BarcodeSymbology.QRCode)
                {
                    hints[EncodeHintType.ERROR_CORRECTION] = ToZXingErrorCorrectionLevel(ErrorCorrectionLevel);
                }
                // Aztec uses integer error correction percentage (1-99)
                else if (Symbology == BarcodeSymbology.Aztec)
                {
                    hints[EncodeHintType.ERROR_CORRECTION] = ToAztecErrorCorrectionLevel(ErrorCorrectionLevel);
                }
                
                var w = (int)Math.Max(1, targetSize.Width);
                var h = (int)Math.Max(1, targetSize.Height);

                // 1D barcodes (like Code128) require a minimum width to generate valid bars.
                if (Is1DBarcode(Symbology))
                {
                    w = Math.Max(w, 240);
                    h = Math.Max(h, 100);
                }

                // Heavy operation: Encoding the string into a matrix
                var matrix = new MultiFormatWriter().encode(Value, format, w, h, hints);

                // Cache the result
                _cachedMatrix = matrix;
                _lastValue = Value;
                _lastSymbology = Symbology;
                _lastQuietZone = QuietZone;
                _lastErrorCorrectionLevel = ErrorCorrectionLevel;
                
                ErrorMessage = null;
                Dispatcher.UIThread.Post(() => BarcodeGenerated?.Invoke(this, new BarcodeGeneratedEventArgs(matrix)));
                
                return matrix;
            }
            catch (Exception ex)
            {
                _cachedMatrix = null;
                ErrorMessage = ex.Message;
                Dispatcher.UIThread.Post(() => BarcodeError?.Invoke(this, new BarcodeErrorEventArgs(ex)));
                return null;
            }
        }

        /// <inheritdoc/>
        public override void Render(DrawingContext context)
        {
            base.Render(context);

            var renderBounds = new Rect(Bounds.Size);
            if (renderBounds.Width < 1 || renderBounds.Height < 1) return;

            // Determine where to draw based on the template part 'PART_Surface'
            var target = _surface is null
                ? renderBounds
                : new Rect(_surface.Bounds.Size).WithX(_surface.Bounds.X).WithY(_surface.Bounds.Y);

            // Background is cheap to draw, do it every frame
            context.FillRectangle(BackgroundBrush, renderBounds);

            // PERFORMANCE OPTIMIZATION:
            // Drawing thousands of tiny rectangles for a barcode is expensive in immediate mode.
            // We render the barcode once into a RenderTargetBitmap and reuse that bitmap
            // on subsequent frames until the data or size changes.
            if (_cachedBitmap != null)
            {
                context.DrawImage(_cachedBitmap, target);
                
                // Draw logo overlay for QR codes on every render (not cached)
                DrawLogoIfNeeded(context, target);
                return;
            }

            var matrix = GetOrCreateMatrix(target.Size);
            if (matrix == null) return;
            
            // Create a new off-screen bitmap
            var pixelSize = new PixelSize((int)Math.Max(1, target.Width), (int)Math.Max(1, target.Height));
            var rtb = new RenderTargetBitmap(pixelSize);

            using (var rtbContext = rtb.CreateDrawingContext())
            {
                // Render the detailed barcode geometry onto the off-screen bitmap
                var operation = new BarcodeDrawOperation(new Rect(rtb.Size), matrix, BarBrush);
                rtbContext.Custom(operation);
            }
            
            _cachedBitmap = rtb;
            context.DrawImage(_cachedBitmap, target);
            
            // Draw logo overlay for QR codes if specified
            DrawLogoIfNeeded(context, target);
        }
        
        private void DrawLogoIfNeeded(DrawingContext context, Rect target)
        {
            if (Logo != null && (Symbology == BarcodeSymbology.QRCode || Symbology == BarcodeSymbology.Aztec))
            {
                var logoSize = Math.Min(target.Width, target.Height) * Math.Clamp(LogoSizePercent, 0.1, 0.4);
                var logoRect = new Rect(
                    target.X + (target.Width - logoSize) / 2,
                    target.Y + (target.Height - logoSize) / 2,
                    logoSize,
                    logoSize);
                
                // Draw white background behind logo for readability
                context.FillRectangle(BackgroundBrush, logoRect);
                context.DrawImage(Logo, logoRect);
            }
        }
        
        private void ClearBitmapCache()
        {
            _cachedBitmap?.Dispose();
            _cachedBitmap = null;
        }

        // Helper to map enum to ZXing format
        private static BarcodeFormat ToFormat(BarcodeSymbology s) => s switch
        {
            BarcodeSymbology.QRCode => BarcodeFormat.QR_CODE,
            BarcodeSymbology.DataMatrix => BarcodeFormat.DATA_MATRIX,
            BarcodeSymbology.Code128 => BarcodeFormat.CODE_128,
            BarcodeSymbology.Code39 => BarcodeFormat.CODE_39,
            BarcodeSymbology.Code93 => BarcodeFormat.CODE_93,
            BarcodeSymbology.EAN8 => BarcodeFormat.EAN_8,
            BarcodeSymbology.EAN13 => BarcodeFormat.EAN_13,
            BarcodeSymbology.UPCA => BarcodeFormat.UPC_A,
            BarcodeSymbology.UPCE => BarcodeFormat.UPC_E,
            BarcodeSymbology.Codabar => BarcodeFormat.CODABAR,
            BarcodeSymbology.PDF417 => BarcodeFormat.PDF_417,
            BarcodeSymbology.Aztec => BarcodeFormat.AZTEC,
            BarcodeSymbology.ITF => BarcodeFormat.ITF,
            _ => BarcodeFormat.QR_CODE
        };
        
        // Helper to detect 1D barcodes for sizing constraints
        private static bool Is1DBarcode(BarcodeSymbology s) => s switch
        {
            BarcodeSymbology.Code128 or BarcodeSymbology.Code39 or BarcodeSymbology.Code93 or
            BarcodeSymbology.EAN8 or BarcodeSymbology.EAN13 or BarcodeSymbology.UPCA or
            BarcodeSymbology.UPCE or BarcodeSymbology.Codabar or BarcodeSymbology.ITF => true,
            _ => false
        };
        
        // Helper to convert enum to ZXing error correction level for QR codes
        private static object ToZXingErrorCorrectionLevel(QRErrorCorrectionLevel level) => level switch
        {
            QRErrorCorrectionLevel.L => ZXing.QrCode.Internal.ErrorCorrectionLevel.L,
            QRErrorCorrectionLevel.M => ZXing.QrCode.Internal.ErrorCorrectionLevel.M,
            QRErrorCorrectionLevel.Q => ZXing.QrCode.Internal.ErrorCorrectionLevel.Q,
            QRErrorCorrectionLevel.H => ZXing.QrCode.Internal.ErrorCorrectionLevel.H,
            _ => ZXing.QrCode.Internal.ErrorCorrectionLevel.M
        };
        
        // Helper to convert enum to Aztec error correction level (integer percentage 1-99)
        private static int ToAztecErrorCorrectionLevel(QRErrorCorrectionLevel level) => level switch
        {
            QRErrorCorrectionLevel.L => 15,  // Low - ~15% error recovery
            QRErrorCorrectionLevel.M => 25,  // Medium - ~25% error recovery (default)
            QRErrorCorrectionLevel.Q => 35,  // Quartile - ~35% error recovery
            QRErrorCorrectionLevel.H => 45,  // High - ~45% error recovery
            _ => 25
        };

        /// <summary>
        /// Custom draw operation to render the ZXing BitMatrix directly to the drawing context.
        /// </summary>
        private sealed class BarcodeDrawOperation : ICustomDrawOperation
        {
            private readonly Rect _bounds;
            private readonly BitMatrix _matrix;
            private readonly IBrush _barBrush;

            public BarcodeDrawOperation(Rect bounds, BitMatrix matrix, IBrush barBrush)
            {
                _bounds = bounds;
                _matrix = matrix;
                _barBrush = barBrush;
            }

            public Rect Bounds => _bounds;

            public void Dispose() { }

            public bool HitTest(Point p) => _bounds.Contains(p);
            
            public void Render(ImmediateDrawingContext context)
            {
                // Calculate scaling to fit the matrix into the target bounds
                var sx = _bounds.Width / _matrix.Width;
                var sy = _bounds.Height / _matrix.Height;
                
                // Ensure bars align to pixels to avoid anti-aliasing blurring
                var scale = Math.Max(1.0, Math.Floor(Math.Min(sx, sy)));
                
                var drawW = _matrix.Width * scale;
                var drawH = _matrix.Height * scale;
                
                // Center the barcode within the bounds
                var left = _bounds.X + (_bounds.Width - drawW) / 2.0;
                var top = _bounds.Y + (_bounds.Height - drawH) / 2.0;

                // Convert to immutable brush for thread safety in the render pass
                var immutableBrush = (_barBrush ?? Brushes.Black).ToImmutable();

                for (var y = 0; y < _matrix.Height; y++)
                {
                    double ry = top + y * scale;
                    int run = -1;
                    
                    // Run-length encoding optimization:
                    // Instead of drawing 1x1 rectangles for every pixel, we group consecutive
                    // black pixels into a single wider rectangle to reduce draw calls.
                    for (var x = 0; x < _matrix.Width; x++)
                    {
                        var on = _matrix[x, y];
                        if (on && run < 0) run = x;
                        
                        if ((!on || x == _matrix.Width - 1) && run >= 0)
                        {
                            var end = on && x == _matrix.Width - 1 ? x + 1 : x;
                            var rx = left + run * scale;
                            var rw = (end - run) * scale;
                            context.FillRectangle(immutableBrush, new Rect(rx, ry, rw, scale));
                            run = -1;
                        }
                    }
                }
            }
            
            /// <summary>
            /// checks equality to determine if the scene graph can cache this operation.
            /// Returns true only if the matrix and bounds are identical.
            /// </summary>
            public bool Equals(ICustomDrawOperation? other)
            {
                return other is BarcodeDrawOperation op && 
                       op._bounds == _bounds &&
                       ReferenceEquals(op._matrix, _matrix) &&
                       Equals(op._barBrush, _barBrush);
            }
        }
    }