namespace FarmApp.Domain.Enums;

/// <summary>How a customer settled money against their account balance - deliberately a
/// separate enum from SalePaymentMethod (doc 02), not a reuse of it. SalePaymentMethod.Account
/// describes how a *sale* was paid for (put on the tab); it is never a way a customer could
/// settle a debt, so reusing that enum here would let a CustomerPayment be recorded with
/// Method: Account, which is nonsensical - "paying the account by putting it on the account".
/// The POS's own card-only decision (doc 01 §4: no cash handling at the till, to remove
/// float/change complexity) is specific to in-person checkout; it doesn't apply here, since
/// settling a debt happens away from the till (EFT from home, cash handed to the owner) with
/// none of the float/change/security concerns that motivated card-only at the stall. See
/// DECISIONS.md.</summary>
public enum CustomerPaymentMethod
{
    Cash,
    Card,
    EFT,
}
