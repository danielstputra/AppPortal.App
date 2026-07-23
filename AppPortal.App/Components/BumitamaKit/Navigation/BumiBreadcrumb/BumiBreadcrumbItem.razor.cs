using Microsoft.AspNetCore.Components;

namespace AppPortal.App.Components.BumitamaKit.Navigation.BumiBreadcrumb;

public partial class BumiBreadcrumbItem
{
    [Parameter]
    public string? Text { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public string? Href { get; set; }

    [Parameter]
    public string? Icon { get; set; }

    [Parameter]
    public bool IsActive { get; set; }

    [Parameter]
    public string Separator { get; set; } = "/";
}
