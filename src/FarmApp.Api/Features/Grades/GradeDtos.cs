namespace FarmApp.Api.Features.Grades;

public record GradeDto(
    int GradeId,
    string Name
    );

public record CreateGradeRequest(string Name);
