namespace Newsletter.Services;

public class ValidationResult
{
    public bool IsSuccess { get; set; }
    public List<string> Errors { get; set; } = new List<string>();

    public ValidationResult(bool isSuccess)
    {
        IsSuccess = isSuccess;
    }

    public ValidationResult(bool isSuccess, List<string> errors)
    {
        IsSuccess = isSuccess;
        Errors = errors;
    }

    public static ValidationResult Success() => new ValidationResult(true);

    public static ValidationResult Failure(params string[] errors) 
        => new ValidationResult(false, errors.ToList());
}