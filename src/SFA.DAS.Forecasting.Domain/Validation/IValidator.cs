namespace SFA.DAS.Forecasting.Domain.Validation;

public interface IValidator<in T>
{
    Task<ValidationResult> ValidateAsync(T item);
}