namespace Web.UI.Base;

/// <summary>
/// Provides context for custom validation in <see cref="AppFormLayout{TModel}"/>.
/// Use <c>AddError(fieldName, message)</c> to register validation errors.
/// </summary>
public class FormValidationContext
{
    private readonly Dictionary<string, string?> _errors;

    internal FormValidationContext(Dictionary<string, string?> errors)
    {
        _errors = errors;
    }

    /// <summary>True if no validation errors have been added.</summary>
    public bool IsValid => _errors.Count == 0;

    /// <summary>The current error dictionary (field name → error message).</summary>
    public Dictionary<string, string?> Errors => _errors;

    /// <summary>Add a validation error for a specific field.</summary>
    public void AddError(string fieldName, string errorMessage)
    {
        _errors[fieldName] = errorMessage;
    }
}
