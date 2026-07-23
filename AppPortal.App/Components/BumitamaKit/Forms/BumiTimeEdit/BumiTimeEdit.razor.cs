using DevExpress.Blazor;
using Microsoft.AspNetCore.Components;

namespace AppPortal.App.Components.BumitamaKit.Forms.BumiTimeEdit;

public partial class BumiTimeEdit
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public BumiTimeEditTheme Theme { get; set; } = BumiTimeEditTheme.Light;

    [Parameter]
    public string? CustomColor { get; set; }

    [Parameter]
    public string? CssClass { get; set; }

    [Parameter]
    public bool Disabled { get; set; }
}
