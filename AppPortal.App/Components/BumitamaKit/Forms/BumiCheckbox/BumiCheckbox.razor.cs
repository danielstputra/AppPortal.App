using DevExpress.Blazor;
using Microsoft.AspNetCore.Components;
using System.Linq.Expressions;

namespace AppPortal.App.Components.BumitamaKit.Forms.BumiCheckbox;

public partial class BumiCheckbox
{
    [Parameter]
    public bool Checked { get; set; }

    [Parameter]
    public EventCallback<bool> CheckedChanged { get; set; }

    [Parameter]
    public Expression<Func<bool>>? CheckedExpression { get; set; }

    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public string? HelperText { get; set; }

    [Parameter]
    public string? ErrorMessage { get; set; }

    [Parameter]
    public bool HasError { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool ReadOnly { get; set; }

    [Parameter]
    public CheckboxTheme Theme { get; set; } = CheckboxTheme.Light;

    [Parameter]
    public string? CustomColor { get; set; }

    [Parameter]
    public string? CssClass { get; set; }
}
