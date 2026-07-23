using Microsoft.AspNetCore.Components;

namespace AppPortal.App.Components.BumitamaKit.Input.BumiCheckbox;

public partial class BumiCheckbox
{
    [Parameter]
    public bool Checked { get; set; }

    [Parameter]
    public EventCallback<bool> CheckedChanged { get; set; }

    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public RenderFragment? LabelContent { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool Indeterminate { get; set; }

    [Parameter]
    public CheckboxSize Size { get; set; } = CheckboxSize.Medium;

    [Parameter]
    public CheckboxColor Color { get; set; } = CheckboxColor.Primary;

    [Parameter]
    public CheckboxTheme Theme { get; set; } = CheckboxTheme.Light;

    [Parameter]
    public string? CssClass { get; set; }

    [Parameter]
    public string? CustomStyle { get; set; }

    [Parameter]
    public string? IconCssClass { get; set; }

    private async Task HandleChange(ChangeEventArgs e)
    {
        if (Disabled) return;

        Checked = (bool)(e.Value ?? false);
        await CheckedChanged.InvokeAsync(Checked);
    }

    private string GetCheckboxClasses()
    {
        var classes = new List<string>
        {
            "bumi-checkbox",
            $"bumi-checkbox-{Size.ToString().ToLower()}",
            $"bumi-checkbox-{Color.ToString().ToLower()}",
            $"bumi-theme-{Theme.ToString().ToLower()}"
        };

        if (Disabled)
            classes.Add("bumi-checkbox-disabled");

        if (Indeterminate)
            classes.Add("bumi-checkbox-indeterminate");

        if (!string.IsNullOrEmpty(CssClass))
            classes.Add(CssClass);

        return string.Join(" ", classes);
    }
}
