using DevExpress.Blazor;
using Microsoft.AspNetCore.Components;
using AppPortal.App.Components.BumitamaKit.Actions.BumiButton;

namespace AppPortal.App.Components.BumitamaKit.Actions.BumiDropDownButton;

public partial class BumiDropDownButton
{
    [Parameter]
    public string? Text { get; set; }

    [Parameter]
    public string? IconCssClass { get; set; }

    [Parameter]
    public ButtonColor Color { get; set; } = ButtonColor.Primary;

    [Parameter]
    public string? CustomColor { get; set; }

    [Parameter]
    public ButtonVariant Variant { get; set; } = ButtonVariant.Filled;

    [Parameter]
    public ButtonSize Size { get; set; } = ButtonSize.Medium;

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public ButtonTheme Theme { get; set; } = ButtonTheme.Light;

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private ButtonRenderStyle DxRenderStyle => Color switch
    {
        ButtonColor.Primary => ButtonRenderStyle.Primary,
        ButtonColor.Secondary => ButtonRenderStyle.Secondary,
        ButtonColor.Success => ButtonRenderStyle.Success,
        ButtonColor.Warning => ButtonRenderStyle.Warning,
        ButtonColor.Danger => ButtonRenderStyle.Danger,
        ButtonColor.Info => ButtonRenderStyle.Info,
        ButtonColor.Brand => ButtonRenderStyle.Primary,
        ButtonColor.Custom => ButtonRenderStyle.Primary,
        _ => ButtonRenderStyle.Primary
    };

    private ButtonRenderStyleMode DxRenderStyleMode => Variant switch
    {
        ButtonVariant.Filled => ButtonRenderStyleMode.Contained,
        ButtonVariant.Outline => ButtonRenderStyleMode.Outline,
        ButtonVariant.Text => ButtonRenderStyleMode.Text,
        ButtonVariant.Ghost => ButtonRenderStyleMode.Text,
        _ => ButtonRenderStyleMode.Contained
    };

    private SizeMode DxSizeMode => Size switch
    {
        ButtonSize.Small => SizeMode.Small,
        ButtonSize.Medium => SizeMode.Medium,
        ButtonSize.Large => SizeMode.Large,
        _ => SizeMode.Medium
    };
}
