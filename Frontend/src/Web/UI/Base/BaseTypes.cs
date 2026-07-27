namespace Web.UI.Base;

/// <summary>Variant tampilan untuk AppButton.</summary>
public enum AppButtonVariant { Primary, Secondary, Danger, Outline }

/// <summary>Ukuran untuk AppButton.</summary>
public enum AppButtonSize { Small, Medium, Large }

/// <summary>Tipe skeleton untuk AppSkeleton.</summary>
public enum SkeletonType { Text, Card, Table, StatCard, Avatar, Circle }

/// <summary>Ukuran heading untuk AppTitle.</summary>
public enum TitleSize { H1, H2, H3, H4, H5, H6 }

/// <summary>Varian warna untuk AppAlertBox.</summary>
public enum AppAlertVariant { Success, Danger, Warning, Info }

/// <summary>Jenis editor untuk form field (AppFormLayout).</summary>
public enum FormFieldType { Text, Email, Password, Number, TextArea, ComboBox, CheckBox, RadioList, DatePicker }

/// <summary>Definisi satu field dalam form layout (AppFormLayout).</summary>
public class FormFieldMeta
{
    public string FieldName { get; set; } = "";
    public string Caption { get; set; } = "";
    public FormFieldType Type { get; set; } = FormFieldType.Text;
    public string? Placeholder { get; set; }
    public bool Required { get; set; }
    public string? ValidationMessage { get; set; }
    public int ColSpan { get; set; } = 12;
    public int ColSpanXl { get; set; }
    public int ColSpanMd { get; set; }
    public IEnumerable<string>? Options { get; set; }
    public string? CssClass { get; set; }
    public bool IsDisabled { get; set; }
    public bool IsReadOnly { get; set; }
    public bool BeginRow { get; set; }
}

/// <summary>Column definition for grid column chooser.</summary>
public class GridColumnMeta
{
    public string Field { get; set; } = "";
    public string Caption { get; set; } = "";
    public bool Visible { get; set; } = true;
}
