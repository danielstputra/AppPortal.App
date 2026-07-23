using Microsoft.AspNetCore.Components;

namespace AppPortal.App.Components.BumitamaKit.Display.BumiCard;

public partial class BumiCard
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public string? Header { get; set; }

    [Parameter]
    public RenderFragment? HeaderContent { get; set; }

    [Parameter]
    public string? Footer { get; set; }

    [Parameter]
    public RenderFragment? FooterContent { get; set; }

    [Parameter]
    public CardVariant Variant { get; set; } = CardVariant.Default;

    [Parameter]
    public CardTheme Theme { get; set; } = CardTheme.Light;

    [Parameter]
    public string? Width { get; set; }

    [Parameter]
    public string? Height { get; set; }

    [Parameter]
    public string? CssClass { get; set; }

    [Parameter]
    public string? CustomStyle { get; set; }
}
