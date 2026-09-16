using FluentValidation;

namespace FarmApp.Api.Application.Expenses;

public class CreateExpenseRequestValidator : AbstractValidator<CreateExpenseRequest>
{
    public CreateExpenseRequestValidator()
    {
        RuleFor(x => x.ExpenseCategoryId).GreaterThan(0);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.VatAmount).GreaterThanOrEqualTo(0).When(x => x.VatAmount.HasValue);
        RuleFor(x => x.SupplierId).GreaterThan(0).When(x => x.SupplierId.HasValue);
        RuleFor(x => x.SeasonId).GreaterThan(0).When(x => x.SeasonId.HasValue);
        RuleFor(x => x.Notes).MaximumLength(1000);
        RuleFor(x => x.AttachmentPath).MaximumLength(260);
    }
}
