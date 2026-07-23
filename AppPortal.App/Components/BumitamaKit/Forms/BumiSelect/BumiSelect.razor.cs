using DevExpress.Blazor;
using Microsoft.AspNetCore.Components;

namespace AppPortal.App.Components.BumitamaKit.Forms.BumiSelect;

public partial class BumiSelect<TValue, TData>
{
    [Parameter]
    public TValue? Value { get; set; }

    [Parameter]
    public EventCallback<TValue> ValueChanged { get; set; }

    [Parameter]
    public IEnumerable<TData>? Data { get; set; }

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
    public bool AllowUserInput { get; set; } = true;

    [Parameter]
    public DataEditorClearButtonDisplayMode ClearButtonDisplayMode { get; set; } = DataEditorClearButtonDisplayMode.Auto;

    [Parameter]
    public SelectSize Size { get; set; } = SelectSize.Medium;

    [Parameter]
    public SelectTheme Theme { get; set; } = SelectTheme.Light;

    [Parameter]
    public string? Width { get; set; }

    [Parameter]
    public string? CssClass { get; set; }

    private SizeMode DxSizeMode => Size switch
    {
        SelectSize.Small => SizeMode.Small,
        SelectSize.Medium => SizeMode.Medium,
        SelectSize.Large => SizeMode.Large,
        _ => SizeMode.Medium
    };
}
