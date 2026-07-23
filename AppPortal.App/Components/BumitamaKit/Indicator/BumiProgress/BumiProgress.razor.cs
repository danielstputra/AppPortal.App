using Microsoft.AspNetCore.Components;

namespace AppPortal.App.Components.BumitamaKit.Indicator.BumiProgress;

public partial class BumiProgress
{
    [Parameter]
    public int Value { get; set; }

    [Parameter]
    public int Minimum { get; set; } = 0;

    [Parameter]
    public int Maximum { get; set; } = 100;

    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public bool ShowLabel { get; set; } = true;

    [Parameter]
    public bool ShowPercentage { get; set; } = true;

    [Parameter]
    public string? HelperText { get; set; }

    [Parameter]
    public ProgressColor Color { get; set; } = ProgressColor.Primary;

    [Parameter]
    public string? CustomColor { get; set; }

    [Parameter]
    public ProgressTheme Theme { get; set; } = ProgressTheme.Light;

    [Parameter]
    public string? Width { get; set; }

    [Parameter]
    public string? Height { get; set; }

    [Parameter]
    public string? CssClass { get; set; }
}
