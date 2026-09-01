using System;
using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;

namespace Nova.Avalonia.UI.Controls;

   /// <summary>
    /// Displays a group of avatars in a stacked or row layout with configurable overlap and overflow handling.
    /// </summary>
    public class AvatarGroup : TemplatedControl
    {
        private AvatarStackPanel? _container;

        /// <summary>
        /// Defines the <see cref="Avatars"/> property.
        /// </summary>
        public static readonly StyledProperty<AvaloniaList<Avatar>> AvatarsProperty =
            AvaloniaProperty.Register<AvatarGroup, AvaloniaList<Avatar>>(nameof(Avatars), new AvaloniaList<Avatar>());

        /// <summary>
        /// Defines the <see cref="MaxDisplayed"/> property.
        /// </summary>
        public static readonly StyledProperty<int> MaxDisplayedProperty =
            AvaloniaProperty.Register<AvatarGroup, int>(nameof(MaxDisplayed), 5);

        /// <summary>
        /// Defines the <see cref="Overlap"/> property.
        /// </summary>
        public static readonly StyledProperty<double> OverlapProperty =
            AvaloniaProperty.Register<AvatarGroup, double>(nameof(Overlap), 0.25);

        /// <summary>
        /// Defines the <see cref="ShowCount"/> property.
        /// </summary>
        public static readonly StyledProperty<bool> ShowCountProperty =
            AvaloniaProperty.Register<AvatarGroup, bool>(nameof(ShowCount), true);

        /// <summary>
        /// Defines the <see cref="Size"/> property.
        /// </summary>
        public static readonly StyledProperty<AvatarSize> SizeProperty =
            AvaloniaProperty.Register<AvatarGroup, AvatarSize>(nameof(Size), AvatarSize.Medium);

        /// <summary>
        /// Defines the <see cref="Orientation"/> property.
        /// </summary>
        public static readonly StyledProperty<Orientation> OrientationProperty =
            AvaloniaProperty.Register<AvatarGroup, Orientation>(nameof(Orientation), Orientation.Horizontal);

        /// <summary>
        /// Gets or sets the collection of avatars to display.
        /// </summary>
        public AvaloniaList<Avatar> Avatars { get => GetValue(AvatarsProperty); set => SetValue(AvatarsProperty, value); }

        /// <summary>
        /// Gets or sets the maximum number of avatars to display before showing the "+N" overflow badge.
        /// </summary>
        public int MaxDisplayed { get => GetValue(MaxDisplayedProperty); set => SetValue(MaxDisplayedProperty, value); }

        /// <summary>
        /// Gets or sets the overlap percentage (0.0 to 1.0) between adjacent avatars.
        /// </summary>
        public double Overlap { get => GetValue(OverlapProperty); set => SetValue(OverlapProperty, value); }

        /// <summary>
        /// Gets or sets whether to show the count badge for hidden avatars when the list exceeds <see cref="MaxDisplayed"/>.
        /// </summary>
        public bool ShowCount { get => GetValue(ShowCountProperty); set => SetValue(ShowCountProperty, value); }

        /// <summary>
        /// Gets or sets the size of all avatars in the group.
        /// </summary>
        public AvatarSize Size { get => GetValue(SizeProperty); set => SetValue(SizeProperty, value); }

        /// <summary>
        /// Gets or sets the layout orientation (Horizontal or Vertical).
        /// </summary>
        public Orientation Orientation { get => GetValue(OrientationProperty); set => SetValue(OrientationProperty, value); }
        
        static AvatarGroup()
        {
            BorderBrushProperty.OverrideDefaultValue<AvatarGroup>(Brushes.White);
            BorderThicknessProperty.OverrideDefaultValue<AvatarGroup>(new Thickness(2));
            AvatarsProperty.Changed.AddClassHandler<AvatarGroup>((x, e) => x.UpdateAvatars());
            MaxDisplayedProperty.Changed.AddClassHandler<AvatarGroup>((x, e) => x.UpdateAvatars());
            OverlapProperty.Changed.AddClassHandler<AvatarGroup>((x, e) => x.UpdatePanelProperties());
            OrientationProperty.Changed.AddClassHandler<AvatarGroup>((x, e) => x.UpdatePanelProperties());
            ShowCountProperty.Changed.AddClassHandler<AvatarGroup>((x, e) => x.UpdateAvatars());
            SizeProperty.Changed.AddClassHandler<AvatarGroup>((x, e) => x.UpdateAvatars());
            
            // We still trigger UpdateAvatars on Border changes to rebuild, 
            // but the Binding logic ensures dynamic updates work too.
            BorderBrushProperty.Changed.AddClassHandler<AvatarGroup>((x, e) => x.UpdateAvatars());
            BorderThicknessProperty.Changed.AddClassHandler<AvatarGroup>((x, e) => x.UpdateAvatars());
        }

        public AvatarGroup()
        {
            Avatars = new AvaloniaList<Avatar>();
            Avatars.CollectionChanged += (s, e) => UpdateAvatars();
        }

        /// <inheritdoc />
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            _container = e.NameScope.Find<AvatarStackPanel>("PART_Container");
            UpdatePanelProperties();
            UpdateAvatars();
        }
        
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new AvatarGroupAutomationPeer(this);
        }

        private void UpdatePanelProperties()
        {
            if (_container != null)
            {
                _container.Overlap = Overlap;
                _container.Orientation = Orientation;
            }
        }

        private void UpdateAvatars()
        {
            if (_container == null || Avatars == null)
                return;

            _container.Children.Clear();

            var displayCount = Math.Min(MaxDisplayed, Avatars.Count);
            var hiddenCount = Math.Max(0, Avatars.Count - MaxDisplayed);
            var avatarSize = GetAvatarSize();

            for (int i = 0; i < displayCount; i++)
            {
                var sourceAvatar = Avatars[i];
                
                var avatar = new Avatar
                {
                    DisplayName = sourceAvatar.DisplayName,
                    ImageSource = sourceAvatar.ImageSource,
                    Icon = sourceAvatar.Icon,
                    Content = sourceAvatar.Content,
                    DisplayMode = sourceAvatar.DisplayMode,
                    Shape = sourceAvatar.Shape,
                    Size = Size,
                    BackgroundColor = sourceAvatar.BackgroundColor,
                    ForegroundColor = sourceAvatar.ForegroundColor,
                    AutoGenerateBackground = sourceAvatar.AutoGenerateBackground,
                    Status = sourceAvatar.Status,
                    StatusColor = sourceAvatar.StatusColor,
                    Initials = sourceAvatar.Initials,
                    MaxInitials = sourceAvatar.MaxInitials,
                    ShowTooltip = sourceAvatar.ShowTooltip,
                    Width = avatarSize,
                    Height = avatarSize
                };
                
                var border = new Border
                {
                    Child = avatar,
                    
                    CornerRadius = avatar.Shape == AvatarShape.Circle 
                        ? new CornerRadius(avatarSize / 2) 
                        : new CornerRadius(avatarSize * 0.15),
                    
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Width = avatarSize,
                    Height = avatarSize
                };
                
                border.Bind(Border.BorderBrushProperty, this.GetObservable(BorderBrushProperty));
                border.Bind(Border.BorderThicknessProperty, this.GetObservable(BorderThicknessProperty));

                _container.Children.Add(border);
            }

            if (ShowCount && hiddenCount > 0)
            {
                var countAvatar = new Avatar
                {
                    Size = Size,
                    Content = $"+{hiddenCount}",
                    DisplayMode = AvatarDisplayMode.Content,
                    BackgroundColor = new SolidColorBrush(Color.Parse("#6B7280")),
                    ForegroundColor = Brushes.White,
                    Width = avatarSize,
                    Height = avatarSize
                };

                var countBorder = new Border
                {
                    Child = countAvatar,
                    CornerRadius = new CornerRadius(avatarSize / 2),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Width = avatarSize,
                    Height = avatarSize
                };
                
                countBorder.Bind(Border.BorderBrushProperty, this.GetObservable(BorderBrushProperty));
                countBorder.Bind(Border.BorderThicknessProperty, this.GetObservable(BorderThicknessProperty));

                _container.Children.Add(countBorder);
            }
        }

        private double GetAvatarSize()
        {
            return Size switch
            {
                AvatarSize.ExtraSmall => 24,
                AvatarSize.Small => 32,
                AvatarSize.Medium => 48,
                AvatarSize.Large => 64,
                AvatarSize.ExtraLarge => 96,
                _ => 48
            };
        }
    }
