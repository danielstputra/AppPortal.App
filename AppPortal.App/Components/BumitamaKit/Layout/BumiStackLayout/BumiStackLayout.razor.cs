using DevExpress.Blazor;
using Microsoft.AspNetCore.Components;

namespace AppPortal.App.Components.BumitamaKit.Layout.BumiStackLayout;

public partial class BumiStackLayout
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public BumiStackLayoutTheme Theme { get; set; } = BumiStackLayoutTheme.Light;

    [Parameter]
    public string? CustomColor { get; set; }

    [Parameter]
    public string? CssClass { get; set; }

    [Parameter]
    public bool Disabled { get; set; }
}
