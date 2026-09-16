import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { extractErrorMessage } from '../../../shared/http-error.util';
import { CreateRainfallLogRequest, RainfallLogDto, UpdateRainfallLogRequest } from '../rainfall-log.model';
import { RainfallLogsApiService } from '../rainfall-logs-api.service';

function todayIso(): string {
  return new Date().toISOString().slice(0, 10);
}

@Component({
  selector: 'app-rainfall-log-page',
  imports: [ReactiveFormsModule],
  templateUrl: './rainfall-log-page.component.html',
  styleUrl: './rainfall-log-page.component.scss',
})
export class RainfallLogPageComponent implements OnInit {
  private api = inject(RainfallLogsApiService);
  private fb = inject(FormBuilder);

  // No canManage gate - RainfallLogsController carries no CanManageMasterData attribute on
  // either verb (verified directly against the controller: "any authenticated user... same
  // footing as Planting/Season/Activity", RainfallLogsController's own doc comment).
  logs = signal<RainfallLogDto[]>([]);
  loading = signal(true);

  createForm = this.fb.nonNullable.group({
    date: [todayIso(), [Validators.required]],
    mm: [0, [Validators.required, Validators.min(0)]],
    notes: this.fb.control<string | null>(null, [Validators.maxLength(500)]),
  });
  createError = signal('');

  editingId = signal<number | null>(null);
  editForm = this.fb.nonNullable.group({
    mm: [0, [Validators.required, Validators.min(0)]],
    notes: this.fb.control<string | null>(null, [Validators.maxLength(500)]),
  });
  editError = signal('');

  filterForm = this.fb.nonNullable.group({
    from: this.fb.control<string | null>(null),
    to: this.fb.control<string | null>(null),
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    const { from, to } = this.filterForm.getRawValue();
    this.api.getAll(from ?? undefined, to ?? undefined).subscribe((rows) => {
      // Newest reading first - most relevant when checking "did it rain lately".
      this.logs.set([...rows].sort((a, b) => b.date.localeCompare(a.date) || b.rainfallLogId - a.rainfallLogId));
      this.loading.set(false);
    });
  }

  applyFilter(): void {
    this.load();
  }

  clearFilter(): void {
    this.filterForm.reset({ from: null, to: null });
    this.load();
  }

  add(): void {
    if (this.createForm.invalid) return;
    this.createError.set('');
    const req: CreateRainfallLogRequest = this.createForm.getRawValue();
    this.api.create(req).subscribe({
      next: () => {
        this.createForm.reset({ date: todayIso(), mm: 0, notes: null });
        this.load();
      },
      // Duplicate-date creation surfaces here as a 409 "Duplicate name" ProblemDetails
      // (RainfallLogService.CreateAsync -> ServiceError.DuplicateName - the closest existing
      // error case; see DECISIONS.md Phase 3a) - extractErrorMessage renders its detail text
      // verbatim, e.g. "rainfall log already exists" style wording from ApiControllerBase.
      error: (err: HttpErrorResponse) => this.createError.set(extractErrorMessage(err)),
    });
  }

  startEdit(log: RainfallLogDto): void {
    this.editingId.set(log.rainfallLogId);
    this.editError.set('');
    this.editForm.setValue({ mm: log.mm, notes: log.notes });
  }

  cancelEdit(): void {
    this.editingId.set(null);
  }

  saveEdit(id: number): void {
    if (this.editForm.invalid) return;
    this.editError.set('');
    const req: UpdateRainfallLogRequest = this.editForm.getRawValue();
    this.api.update(id, req).subscribe({
      next: () => {
        this.editingId.set(null);
        this.load();
      },
      error: (err: HttpErrorResponse) => this.editError.set(extractErrorMessage(err)),
    });
  }
}
