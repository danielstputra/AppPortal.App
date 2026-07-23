using DevExpress.Blazor;
using Microsoft.AspNetCore.Components;

namespace AppPortal.App.Components.BumitamaKit.Layout.BumiDrawer;

public partial class BumiDrawer
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public BumiDrawerTheme Theme { get; set; } = BumiDrawerTheme.Light;

    [Parameter]
    public string? CustomColor { get; set; }

    [Parameter]
    public string? CssClass { get; set; }

    [Parameter]
    public bool Disabled { get; set; }
}
