using FluentValidation;

namespace FarmApp.Api.Application.Common;

public static class ValidationExtensions
{
    /// <summary>Not empty + max length — the common shape for a required display name.</summary>
    public static IRuleBuilderOptions<T, string> RequiredName<T>(
        this IRuleBuilder<T, string> ruleBuilder, int maxLength = 50)
        => ruleBuilder.NotEmpty().MaximumLength(maxLength);
}
