using DevExpress.Blazor;
using Microsoft.AspNetCore.Components;
using AppPortal.App.Components.BumitamaKit.Actions.BumiButton;

namespace AppPortal.App.Components.BumitamaKit.Actions.BumiSplitButton;

public partial class BumiSplitButton
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
    public string? CssClass { get; set; }

    [Parameter]
    public ButtonTheme Theme { get; set; } = ButtonTheme.Light;

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private ButtonRenderStyle DxRenderStyle
    {
        get => Color switch
        {
            ButtonColor.Primary => ButtonRenderStyle.Primary,
            ButtonColor.Secondary => ButtonRenderStyle.Secondary,
            ButtonColor.Success => ButtonRenderStyle.Success,
            ButtonColor.Warning => ButtonRenderStyle.Warning,
            ButtonColor.Danger => ButtonRenderStyle.Danger,
            ButtonColor.Info => ButtonRenderStyle.Info,
            ButtonColor.Brand => ButtonRenderStyle.Primary, // Brand uses Primary style as base
            ButtonColor.Custom => ButtonRenderStyle.Primary, // Use Primary as base for custom
            _ => ButtonRenderStyle.Primary
        };
    }

    protected string ButtonCssClass =>
        $"bumi-btn bumi-btn-{Theme.ToString().ToLower()} {(Color == ButtonColor.Custom ? "bumi-btn-custom" : "")} {CssClass}".Trim();

    private ButtonRenderStyleMode DxRenderStyleMode
    {
        get => Variant switch
        {
            ButtonVariant.Filled => ButtonRenderStyleMode.Contained,
            ButtonVariant.Outline => ButtonRenderStyleMode.Outline,
            ButtonVariant.Text => ButtonRenderStyleMode.Text,
            ButtonVariant.Ghost => ButtonRenderStyleMode.Text,
            _ => ButtonRenderStyleMode.Contained
        };
    }

    private SizeMode DxSizeMode
    {
        get => Size switch
        {
            ButtonSize.Small => SizeMode.Small,
            ButtonSize.Medium => SizeMode.Medium,
            ButtonSize.Large => SizeMode.Large,
            _ => SizeMode.Medium
        };
    }
}
