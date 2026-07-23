using DevExpress.Blazor;
using Microsoft.AspNetCore.Components;

namespace AppPortal.App.Components.BumitamaKit.Navigation.BumiTabs;

public partial class BumiTabs
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public int ActiveTabIndex { get; set; }

    [Parameter]
    public EventCallback<int> ActiveTabIndexChanged { get; set; }

    [Parameter]
    public TabsTheme Theme { get; set; } = TabsTheme.Light;

    [Parameter]
    public TabsRenderMode TabsRenderMode { get; set; } = TabsRenderMode.AllTabs;

    [Parameter]
    public string? CssClass { get; set; }
}
