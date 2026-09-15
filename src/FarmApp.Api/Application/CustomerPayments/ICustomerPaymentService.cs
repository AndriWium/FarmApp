using FarmApp.Api.Application.Common;

namespace FarmApp.Api.Application.CustomerPayments;

public interface ICustomerPaymentService
{
    Task<CustomerPaymentDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<List<CustomerPaymentDto>> GetByCustomerIdAsync(int customerId, CancellationToken ct);

    /// <summary>Records a payment against a customer's account - plain create, no update/delete
    /// (task brief: payments received are historical fact, same append-only spirit as
    /// StockMovement/Price; a wrong payment gets corrected with an adjusting entry, not by
    /// editing history). Validates CustomerId references a real customer.</summary>
    Task<ServiceResult<CustomerPaymentDto>> CreateAsync(CreateCustomerPaymentRequest request, CancellationToken ct);

    /// <summary>Derived balance (never stored): SUM(SalePayment.Amount WHERE Method == Account,
    /// across this customer's Complete sales - Refunded sales excluded) minus
    /// SUM(CustomerPayment.Amount for this customer). Returns null if the customer doesn't
    /// exist.</summary>
    Task<CustomerBalanceDto?> GetBalanceAsync(int customerId, CancellationToken ct);
}
