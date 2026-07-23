using Microsoft.AspNetCore.Components;

namespace AppPortal.App.Components.BumitamaKit.Indicator.BumiSteps;

public partial class BumiSteps
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public int CurrentStep { get; set; } = 0;

    [Parameter]
    public StepsOrientation Orientation { get; set; } = StepsOrientation.Horizontal;

    [Parameter]
    public StepsTheme Theme { get; set; } = StepsTheme.Light;

    [Parameter]
    public string? Width { get; set; }

    [Parameter]
    public string? CssClass { get; set; }
}
