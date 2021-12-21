namespace Encore.Validation;
public class ValidationResult
{
    public string Message { get; protected set; } = string.Empty;
    public bool IsValid { get; protected set; }

    public static ValidationResult InvalidResult(string message) => new ValidationResult() { Message = message, IsValid = false };

    public static ValidationResult ValidResult(string message = "") => new ValidationResult() { Message = message, IsValid = true };
}

