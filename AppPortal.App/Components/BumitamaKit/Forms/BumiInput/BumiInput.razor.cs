using DevExpress.Blazor;
using Microsoft.AspNetCore.Components;
using System.Linq.Expressions;

namespace AppPortal.App.Components.BumitamaKit.Forms.BumiInput;

public partial class BumiInput
{
    [Parameter]
    public string? Value { get; set; }

    [Parameter]
    public EventCallback<string> ValueChanged { get; set; }

    [Parameter]
    public Expression<Func<string>>? ValueExpression { get; set; }

    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public string? Placeholder { get; set; }

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
    public InputSize Size { get; set; } = InputSize.Medium;

    [Parameter]
    public InputTheme Theme { get; set; } = InputTheme.Light;

    [Parameter]
    public string? Width { get; set; }

    [Parameter]
    public string? CustomColor { get; set; }

    [Parameter]
    public string? CssClass { get; set; }

    [Parameter]
    public string Type { get; set; } = "text";

    private SizeMode DxSizeMode => Size switch
    {
        InputSize.Small => SizeMode.Small,
        InputSize.Medium => SizeMode.Medium,
        InputSize.Large => SizeMode.Large,
        _ => SizeMode.Medium
    };
}
