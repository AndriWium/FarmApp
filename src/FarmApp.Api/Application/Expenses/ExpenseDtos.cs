namespace FarmApp.Api.Application.Expenses;

public record ExpenseDto(
    int ExpenseId, DateTime Date, int ExpenseCategoryId, decimal Amount, decimal? VatAmount,
    int? SupplierId, int? SeasonId, string? Notes, string? AttachmentPath);

/// <summary>Date is client-supplied (unlike Sale.DateTime/CustomerPayment.Date, which are always
/// server-assigned) - an expense slip is very often captured after the fact ("found this
/// receipt from last week"), so the real transaction date matters and can't be assumed to be
/// "now". VatAmount/SupplierId/SeasonId/Notes/AttachmentPath are all optional (doc 02).</summary>
public record CreateExpenseRequest(
    DateTime Date, int ExpenseCategoryId, decimal Amount, decimal? VatAmount,
    int? SupplierId, int? SeasonId, string? Notes, string? AttachmentPath);
