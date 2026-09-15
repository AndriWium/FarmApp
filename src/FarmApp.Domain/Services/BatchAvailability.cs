namespace FarmApp.Domain.Services;

/// <summary>One stock batch's remaining quantity, as of the moment the caller fetched it. Not an
/// EF entity - a plain projection the repository builds and the FIFO algorithm consumes.
/// Callers must supply these already sorted oldest-first by Date.</summary>
public record BatchAvailability(int BatchId, DateTime Date, decimal QtyAvailable);
