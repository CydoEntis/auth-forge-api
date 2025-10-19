using AuthForge.Domain.Errors;

namespace AuthForge.Domain.Common;

public sealed class ValidationResult : Result
{
    private ValidationResult(Error[] errors) 
        : base(false, errors.Length > 0 ? errors[0] : Error.None) 
    {
        Errors = errors;
    }

    public Error[] Errors { get; }

    public static ValidationResult WithErrors(params Error[] errors)
    {
        return new ValidationResult(errors);
    }
}

public sealed class ValidationResult<T> : Result<T>
{
    private ValidationResult(Error[] errors) 
        : base(default, false, errors.Length > 0 ? errors[0] : Error.None) 
    {
        Errors = errors;
    }

    public Error[] Errors { get; }

    public static ValidationResult<T> WithErrors(params Error[] errors)
    {
        return new ValidationResult<T>(errors);
    }
}