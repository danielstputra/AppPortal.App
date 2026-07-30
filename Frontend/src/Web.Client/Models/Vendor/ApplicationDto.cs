namespace Web.Models.Vendor;

public enum AppDisplayMode
{
    Page,       // Dashboard Example — tampilkan halaman Blazor
    Embedded    // Tampilkan dalam iframe dari URL eksternal
}

public class ApplicationDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;
    
    // ─── Display Mode ───
    public AppDisplayMode DisplayMode { get; set; } = AppDisplayMode.Page;
    
    // ─── Embedded Mode Properties ───
    public string? ExternalUrl { get; set; }
    public bool RequiresAuth { get; set; }
    
    // ─── Page Mode Properties ───
    public string? BaseRoute { get; set; }
    public bool HasCustomPage { get; set; }
    
    public bool IsActive { get; set; }
    public bool IsStaticModule { get; set; }
    public string? EmbeddedUrl { get; set; }
}
