namespace FarmApp.Api.Application.Grades;

public record GradeDto(int GradeId, string Name, bool IsActive);

public record CreateGradeRequest(string Name);

public record UpdateGradeRequest(string Name, bool IsActive);
