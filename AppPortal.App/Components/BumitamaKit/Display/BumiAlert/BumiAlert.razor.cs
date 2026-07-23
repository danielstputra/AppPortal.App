using Microsoft.AspNetCore.Components;

namespace AppPortal.App.Components.BumitamaKit.Display.BumiAlert;

public partial class BumiAlert
{
    [Parameter]
    public string? Title { get; set; }

    [Parameter]
    public string? Message { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public string? Icon { get; set; }

    [Parameter]
    public AlertType Type { get; set; } = AlertType.Info;

    [Parameter]
    public AlertVariant Variant { get; set; } = AlertVariant.Filled;

    [Parameter]
    public AlertTheme Theme { get; set; } = AlertTheme.Light;

    [Parameter]
    public bool Dismissible { get; set; }

    [Parameter]
    public EventCallback OnDismiss { get; set; }

    [Parameter]
    public string? Width { get; set; }

    [Parameter]
    public string? CssClass { get; set; }

    [Parameter]
    public string? CustomStyle { get; set; }
}
