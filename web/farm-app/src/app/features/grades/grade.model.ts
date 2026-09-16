// Mirrors FarmApp.Api.Application.Grades.GradeDtos.
export interface GradeDto {
  gradeId: number;
  name: string;
  isActive: boolean;
}

export interface CreateGradeRequest {
  name: string;
}

export interface UpdateGradeRequest {
  name: string;
  isActive: boolean;
}
