import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../../core/auth/auth.service';
import { extractErrorMessage } from '../../../shared/http-error.util';
import { CreateGradeRequest, GradeDto, UpdateGradeRequest } from '../grade.model';
import { GradesApiService } from '../grades-api.service';

@Component({
  selector: 'app-grade-page',
  imports: [ReactiveFormsModule],
  templateUrl: './grade-page.component.html',
  styleUrl: './grade-page.component.scss',
})
export class GradePageComponent implements OnInit {
  private api = inject(GradesApiService);
  private fb = inject(FormBuilder);
  auth = inject(AuthService);

  canManage = computed(() => this.auth.hasRole('Owner'));

  grades = signal<GradeDto[]>([]);
  showInactive = signal(false);

  createForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(50)]],
  });
  createError = signal('');

  editingId = signal<number | null>(null);
  editForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(50)]],
    isActive: [true],
  });
  editError = signal('');

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.api.getAll(this.showInactive()).subscribe((rows) => this.grades.set(rows));
  }

  toggleShowInactive(): void {
    this.showInactive.update((v) => !v);
    this.load();
  }

  add(): void {
    if (this.createForm.invalid) return;
    this.createError.set('');
    const req: CreateGradeRequest = this.createForm.getRawValue();
    this.api.create(req).subscribe({
      next: () => {
        this.createForm.reset({ name: '' });
        this.load();
      },
      error: (err: HttpErrorResponse) => this.createError.set(extractErrorMessage(err)),
    });
  }

  startEdit(grade: GradeDto): void {
    this.editingId.set(grade.gradeId);
    this.editError.set('');
    this.editForm.setValue({ name: grade.name, isActive: grade.isActive });
  }

  cancelEdit(): void {
    this.editingId.set(null);
  }

  saveEdit(id: number): void {
    if (this.editForm.invalid) return;
    this.editError.set('');
    const req: UpdateGradeRequest = this.editForm.getRawValue();
    this.api.update(id, req).subscribe({
      next: () => {
        this.editingId.set(null);
        this.load();
      },
      error: (err: HttpErrorResponse) => this.editError.set(extractErrorMessage(err)),
    });
  }

  deactivate(grade: GradeDto): void {
    if (!confirm(`Deactivate grade "${grade.name}"?`)) return;
    this.api.deactivate(grade.gradeId).subscribe(() => this.load());
  }
}
