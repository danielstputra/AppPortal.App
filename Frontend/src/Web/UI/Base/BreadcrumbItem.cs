namespace Web.UI.Base;

/// <summary>
/// Represents a single breadcrumb item.
/// </summary>
public class BreadcrumbItem
{
    /// <summary>Display text for the breadcrumb.</summary>
    public string Text { get; set; } = "";

    /// <summary>Optional URL link. null/empty = current page (not clickable).</summary>
    public string? Url { get; set; }
}
