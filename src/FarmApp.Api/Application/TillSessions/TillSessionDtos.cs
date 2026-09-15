namespace FarmApp.Api.Application.TillSessions;

public record TillSessionDto(
    int TillSessionId, int LocationId, DateTime OpenedAt, int OpenedBy, DateTime? ClosedAt,
    decimal? SystemCardTotal, decimal? CardMachineBatchTotal, decimal? Difference, string? DifferenceNote);

public record OpenTillSessionRequest(int LocationId);

/// <summary>Day close (doc 01 Module 4 / doc 02): the cashier reads the card machine's own
/// settlement batch total off the physical terminal and enters it here. CardMachineBatchTotal
/// and DifferenceNote are the only two values a client ever supplies - SystemCardTotal and
/// Difference are always computed server-side from SalePayment rows, never trusted from the
/// client.</summary>
public record CloseTillSessionRequest(decimal CardMachineBatchTotal, string? DifferenceNote);
