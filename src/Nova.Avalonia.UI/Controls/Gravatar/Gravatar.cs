using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// A control that displays an avatar generated from an identifier or a custom image.
/// </summary>
[TemplatePart("PART_ContentPresenter", typeof(ContentPresenter))]
[TemplatePart("PART_ImagePresenter", typeof(Image))]
public class Gravatar : TemplatedControl
{
    /// <summary>
    /// Defines the identifier (email, username) to generate the avatar from.
    /// </summary>
    public static readonly StyledProperty<string?> IdProperty =
        AvaloniaProperty.Register<Gravatar, string?>(nameof(Id));

    /// <summary>
    /// Defines a custom image source that overrides the generated avatar.
    /// </summary>
    public static readonly StyledProperty<IImage?> SourceProperty =
        AvaloniaProperty.Register<Gravatar, IImage?>(nameof(Source));

    /// <summary>
    /// Defines the generator used to create the avatar visual.
    /// </summary>
    public static readonly StyledProperty<IGravatarGenerator> GeneratorProperty =
        AvaloniaProperty.Register<Gravatar, IGravatarGenerator>(nameof(Generator), defaultValue: new GithubGravatarGenerator());

    /// <summary>
    /// Defines the size of the avatar.
    /// </summary>
    public static readonly StyledProperty<double> SizeProperty =
        AvaloniaProperty.Register<Gravatar, double>(nameof(Size), defaultValue: 48.0);

    private ContentPresenter? _contentPresenter;
    private Image? _imagePresenter;

    /// <inheritdoc cref="IdProperty"/>
    public string? Id
    {
        get => GetValue(IdProperty);
        set => SetValue(IdProperty, value);
    }

    /// <inheritdoc cref="SourceProperty"/>
    public IImage? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    /// <inheritdoc cref="GeneratorProperty"/>
    public IGravatarGenerator Generator
    {
        get => GetValue(GeneratorProperty);
        set => SetValue(GeneratorProperty, value);
    }

    /// <inheritdoc cref="SizeProperty"/>
    public double Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _contentPresenter = e.NameScope.Find<ContentPresenter>("PART_ContentPresenter");
        _imagePresenter = e.NameScope.Find<Image>("PART_ImagePresenter");

        UpdateContent();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IdProperty ||
            change.Property == SourceProperty ||
            change.Property == GeneratorProperty)
        {
            UpdateContent();
        }
    }

    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new GravatarAutomationPeer(this);
    }

    private void UpdateContent()
    {
        if (_contentPresenter == null || _imagePresenter == null)
            return;

        if (Source != null)
        {
            _imagePresenter.Source = Source;
            _imagePresenter.IsVisible = true;
            _contentPresenter.IsVisible = false;
        }
        else if (!string.IsNullOrEmpty(Id))
        {
            var avatar = Generator?.GenerateAvatar(Id);
            _contentPresenter.Content = avatar;
            _contentPresenter.IsVisible = true;
            _imagePresenter.IsVisible = false;
        }
        else
        {
            _contentPresenter.Content = null;
            _contentPresenter.IsVisible = false;
            _imagePresenter.IsVisible = false;
        }
    }
}
