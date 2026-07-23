using Microsoft.AspNetCore.Components;

namespace AppPortal.App.Components.BumitamaKit.Navigation.BumiBreadcrumb;

public partial class BumiBreadcrumb
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public BreadcrumbTheme Theme { get; set; } = BreadcrumbTheme.Light;

    [Parameter]
    public string? CssClass { get; set; }
}
