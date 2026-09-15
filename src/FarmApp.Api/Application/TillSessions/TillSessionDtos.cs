namespace FarmApp.Api.Application.TillSessions;

public record TillSessionDto(
    int TillSessionId, int LocationId, DateTime OpenedAt, int OpenedBy, DateTime? ClosedAt);

public record OpenTillSessionRequest(int LocationId);
