using DevExpress.Blazor;
using Microsoft.AspNetCore.Components;

namespace AppPortal.App.Components.BumitamaKit.Dialogs.BumiToast;

public partial class BumiToast
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public BumiToastTheme Theme { get; set; } = BumiToastTheme.Light;

    [Parameter]
    public string? CustomColor { get; set; }

    [Parameter]
    public string? CssClass { get; set; }

    [Parameter]
    public bool Disabled { get; set; }
}
