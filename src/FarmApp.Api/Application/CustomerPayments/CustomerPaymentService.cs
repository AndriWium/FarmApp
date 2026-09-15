using FarmApp.Api.Application.Common;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;

namespace FarmApp.Api.Application.CustomerPayments;

public class CustomerPaymentService(
    ICustomerPaymentRepository repo,
    ICustomerRepository customerRepo,
    ISalePaymentRepository salePaymentRepo,
    IUnitOfWork uow) : ICustomerPaymentService
{
    public Task<CustomerPaymentDto?> GetByIdAsync(int id, CancellationToken ct)
        => repo.GetByIdAsync(id, ToDtoExpr, ct);

    public Task<List<CustomerPaymentDto>> GetByCustomerIdAsync(int customerId, CancellationToken ct)
        => repo.GetByCustomerIdAsync(customerId, ToDtoExpr, ct);

    public async Task<ServiceResult<CustomerPaymentDto>> CreateAsync(CreateCustomerPaymentRequest request, CancellationToken ct)
    {
        if (await customerRepo.GetByIdAsync(request.CustomerId, ct) is null)
            return ServiceResult<CustomerPaymentDto>.Fail(ServiceError.NotFound);

        var payment = new CustomerPayment
        {
            CustomerId = request.CustomerId,
            Date = DateTime.UtcNow,
            Amount = request.Amount,
            Method = request.Method,
            Ref = request.Ref,
        };
        await repo.AddAsync(payment, ct);
        await uow.SaveChangesAsync(ct); // single-entity write, one atomic save

        return ServiceResult<CustomerPaymentDto>.Ok(ToDto(payment));
    }

    public async Task<CustomerBalanceDto?> GetBalanceAsync(int customerId, CancellationToken ct)
    {
        if (await customerRepo.GetByIdAsync(customerId, ct) is null)
            return null;

        // Owed: Account-method SalePayment amounts on this customer's Complete sales (a Refunded
        // sale's Account portion is excluded entirely - GetAccountTotalForCustomerAsync's own
        // filter, doc's own "why refunds matter for balance correctness" callout). Paid: every
        // CustomerPayment recorded against them.
        var owed = await salePaymentRepo.GetAccountTotalForCustomerAsync(customerId, ct);
        var paid = await repo.GetTotalByCustomerIdAsync(customerId, ct);

        return new CustomerBalanceDto(customerId, owed - paid);
    }

    private static readonly System.Linq.Expressions.Expression<Func<CustomerPayment, CustomerPaymentDto>> ToDtoExpr =
        x => new CustomerPaymentDto(x.CustomerPaymentId, x.CustomerId, x.Date, x.Amount, x.Method, x.Ref);

    private static CustomerPaymentDto ToDto(CustomerPayment x) =>
        new(x.CustomerPaymentId, x.CustomerId, x.Date, x.Amount, x.Method, x.Ref);
}
