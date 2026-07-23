using Microsoft.AspNetCore.Components;

namespace AppPortal.App.Components.BumitamaKit.Indicator.BumiSteps;

public partial class BumiStepsItem
{
    [Parameter]
    public int StepNumber { get; set; }

    [Parameter]
    public string? Title { get; set; }

    [Parameter]
    public string? Description { get; set; }

    [Parameter]
    public string? Icon { get; set; }

    [Parameter]
    public bool IsActive { get; set; }

    [Parameter]
    public bool IsCompleted { get; set; }
}
