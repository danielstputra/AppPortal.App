using Microsoft.AspNetCore.Components;

namespace AppPortal.App.Components.BumitamaKit.Display.BumiBadge;

public partial class BumiBadge
{
    [Parameter]
    public string? Text { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public string? IconCssClass { get; set; }

    [Parameter]
    public BadgeColor Color { get; set; } = BadgeColor.Primary;

    [Parameter]
    public string? CustomColor { get; set; }

    [Parameter]
    public BadgeVariant Variant { get; set; } = BadgeVariant.Filled;

    [Parameter]
    public BadgeSize Size { get; set; } = BadgeSize.Medium;

    [Parameter]
    public BadgeTheme Theme { get; set; } = BadgeTheme.Light;

    [Parameter]
    public string? CssClass { get; set; }

    [Parameter]
    public string? CustomStyle { get; set; }
}
