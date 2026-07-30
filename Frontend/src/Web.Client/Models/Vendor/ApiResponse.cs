namespace Web.Models.Vendor;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public string[]? Errors { get; set; }
}

public class ApiResponseSimple
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string[]? Errors { get; set; }
}
