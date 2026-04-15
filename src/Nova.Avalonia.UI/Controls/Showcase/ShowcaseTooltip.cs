using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Templates;

namespace Nova.Avalonia.UI.Controls;

/// <summary>
/// A tooltip control for displaying showcase step information.
/// </summary>
[PseudoClasses(":custom-template")]
public class ShowcaseTooltip : ContentControl
{
    /// <summary>
    /// Defines the <see cref="Title"/> property.
    /// </summary>
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<ShowcaseTooltip, string?>(nameof(Title));
    
    /// <summary>
    /// Defines the <see cref="Description"/> property.
    /// </summary>
    public static readonly StyledProperty<string?> DescriptionProperty =
        AvaloniaProperty.Register<ShowcaseTooltip, string?>(nameof(Description));

    /// <summary>
    /// Defines the <see cref="FooterContent"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> FooterContentProperty =
        AvaloniaProperty.Register<ShowcaseTooltip, object?>(nameof(FooterContent));

    /// <summary>
    /// Defines the <see cref="FooterContentTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> FooterContentTemplateProperty =
        AvaloniaProperty.Register<ShowcaseTooltip, IDataTemplate?>(nameof(FooterContentTemplate));

    /// <summary>
    /// Defines the <see cref="ShowDefaultBody"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> ShowDefaultBodyProperty =
        AvaloniaProperty.Register<ShowcaseTooltip, bool>(nameof(ShowDefaultBody), true);

    /// <summary>
    /// Defines the <see cref="ShowCustomBody"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> ShowCustomBodyProperty =
        AvaloniaProperty.Register<ShowcaseTooltip, bool>(nameof(ShowCustomBody));

    static ShowcaseTooltip()
    {
        ContentTemplateProperty.Changed.AddClassHandler<ShowcaseTooltip>((x, _) => x.UpdateTemplateState());
    }
    
    /// <summary>
    /// Gets or sets the title of the tooltip.
    /// </summary>
    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the description of the tooltip.
    /// </summary>
    public string? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>
    /// Gets or sets the footer content shown below the main tooltip body.
    /// </summary>
    public object? FooterContent
    {
        get => GetValue(FooterContentProperty);
        set => SetValue(FooterContentProperty, value);
    }

    /// <summary>
    /// Gets or sets the template used to render <see cref="FooterContent"/>.
    /// </summary>
    public IDataTemplate? FooterContentTemplate
    {
        get => GetValue(FooterContentTemplateProperty);
        set => SetValue(FooterContentTemplateProperty, value);
    }

    /// <summary>
    /// Gets whether the default title/description body should be shown.
    /// </summary>
    public bool ShowDefaultBody
    {
        get => GetValue(ShowDefaultBodyProperty);
        private set => SetCurrentValue(ShowDefaultBodyProperty, value);
    }

    /// <summary>
    /// Gets whether the custom tooltip body should be shown.
    /// </summary>
    public bool ShowCustomBody
    {
        get => GetValue(ShowCustomBodyProperty);
        private set => SetCurrentValue(ShowCustomBodyProperty, value);
    }

    private void UpdateTemplateState()
    {
        var hasCustomTemplate = ContentTemplate != null;
        PseudoClasses.Set(":custom-template", hasCustomTemplate);
        ShowDefaultBody = !hasCustomTemplate;
        ShowCustomBody = hasCustomTemplate;
    }
}
