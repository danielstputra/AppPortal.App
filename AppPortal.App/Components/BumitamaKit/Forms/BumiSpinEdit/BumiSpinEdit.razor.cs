using DevExpress.Blazor;
using Microsoft.AspNetCore.Components;

namespace AppPortal.App.Components.BumitamaKit.Forms.BumiSpinEdit;

public partial class BumiSpinEdit
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public BumiSpinEditTheme Theme { get; set; } = BumiSpinEditTheme.Light;

    [Parameter]
    public string? CustomColor { get; set; }

    [Parameter]
    public string? CssClass { get; set; }

    [Parameter]
    public bool Disabled { get; set; }
}
