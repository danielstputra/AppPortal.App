using DevExpress.Blazor;
using Microsoft.AspNetCore.Components;

namespace AppPortal.App.Components.BumitamaKit.Forms.BumiTextarea;

public partial class BumiTextarea
{
    [Parameter]
    public string? Value { get; set; }

    [Parameter]
    public EventCallback<string> ValueChanged { get; set; }

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
    public int Rows { get; set; } = 3;

    [Parameter]
    public int? MaxLength { get; set; }

    [Parameter]
    public bool ShowCharacterCount { get; set; }

    [Parameter]
    public TextareaSize Size { get; set; } = TextareaSize.Medium;

    [Parameter]
    public TextareaTheme Theme { get; set; } = TextareaTheme.Light;

    [Parameter]
    public string? Width { get; set; }

    [Parameter]
    public string? Height { get; set; }

    [Parameter]
    public string? CssClass { get; set; }

    private SizeMode DxSizeMode => Size switch
    {
        TextareaSize.Small => SizeMode.Small,
        TextareaSize.Medium => SizeMode.Medium,
        TextareaSize.Large => SizeMode.Large,
        _ => SizeMode.Medium
    };
}
