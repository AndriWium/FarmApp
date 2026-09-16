using FarmApp.Api.Application.Common;
using FluentValidation;

namespace FarmApp.Api.Application.ExpenseCategories;

public class CreateExpenseCategoryRequestValidator : AbstractValidator<CreateExpenseCategoryRequest>
{
    public CreateExpenseCategoryRequestValidator()
    {
        RuleFor(x => x.Name).RequiredName();
    }
}

public class UpdateExpenseCategoryRequestValidator : AbstractValidator<UpdateExpenseCategoryRequest>
{
    public UpdateExpenseCategoryRequestValidator()
    {
        RuleFor(x => x.Name).RequiredName();
    }
}
