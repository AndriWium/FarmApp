import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../../core/auth/auth.service';
import { extractErrorMessage } from '../../../shared/http-error.util';
import {
  CreateRoadmapItemRequest,
  RoadmapItemDto,
  RoadmapItemStatus,
  UpdateRoadmapItemRequest,
} from '../roadmap.model';
import { RoadmapApiService } from '../roadmap-api.service';

const STATUSES: RoadmapItemStatus[] = ['Planned', 'InProgress', 'Done'];
const STATUS_LABELS: Record<RoadmapItemStatus, string> = {
  Planned: 'Planned',
  InProgress: 'In Progress',
  Done: 'Done',
};

// doc 14: "Done items stay visible ... for a month - seeing progress builds trust in the system."
// Read as: prominent for 30 days after CompletedOn, then folded into a collapsed "Earlier" group
// rather than removed - still there, just not competing with recent progress for attention
// (judgment call - see DECISIONS.md).
const RECENT_DONE_DAYS = 30;

@Component({
  selector: 'app-roadmap-page',
  imports: [ReactiveFormsModule, DatePipe],
  templateUrl: './roadmap-page.component.html',
  styleUrl: './roadmap-page.component.scss',
})
export class RoadmapPageComponent implements OnInit {
  private api = inject(RoadmapApiService);
  private fb = inject(FormBuilder);
  auth = inject(AuthService);

  statuses = STATUSES;
  statusLabels = STATUS_LABELS;

  canManage = computed(() => this.auth.hasRole('Owner'));

  items = signal<RoadmapItemDto[]>([]);

  columns = computed(() => {
    const all = this.items();
    const byStatus = (status: RoadmapItemStatus) =>
      all.filter((i) => i.status === status).sort((a, b) => a.sortOrder - b.sortOrder);

    const done = byStatus('Done');
    const cutoff = Date.now() - RECENT_DONE_DAYS * 24 * 60 * 60 * 1000;
    const recentDone = done.filter((i) => !i.completedOn || new Date(i.completedOn).getTime() >= cutoff);
    const olderDone = done.filter((i) => i.completedOn && new Date(i.completedOn).getTime() < cutoff);

    return {
      Planned: byStatus('Planned'),
      InProgress: byStatus('InProgress'),
      Done: recentDone,
      olderDone,
    };
  });

  showAddForm = signal(false);
  createForm = this.fb.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    description: ['', [Validators.required, Validators.maxLength(2000)]],
    status: ['Planned' as RoadmapItemStatus, [Validators.required]],
    targetPhase: [''],
  });
  createError = signal('');

  editingId = signal<number | null>(null);
  editForm = this.fb.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    description: ['', [Validators.required, Validators.maxLength(2000)]],
    status: ['Planned' as RoadmapItemStatus, [Validators.required]],
    targetPhase: [''],
    sortOrder: [0],
  });
  editError = signal('');

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.api.getAll().subscribe((rows) => this.items.set(rows));
  }

  toggleAddForm(): void {
    this.showAddForm.update((v) => !v);
  }

  add(): void {
    if (this.createForm.invalid) return;
    this.createError.set('');
    const raw = this.createForm.getRawValue();
    const status = raw.status;
    const nextSortOrder = this.nextSortOrderFor(status);
    const req: CreateRoadmapItemRequest = {
      title: raw.title,
      description: raw.description,
      status,
      targetPhase: raw.targetPhase || null,
      sortOrder: nextSortOrder,
    };
    this.api.create(req).subscribe({
      next: () => {
        this.createForm.reset({ title: '', description: '', status: 'Planned', targetPhase: '' });
        this.showAddForm.set(false);
        this.load();
      },
      error: (err: HttpErrorResponse) => this.createError.set(extractErrorMessage(err)),
    });
  }

  private nextSortOrderFor(status: RoadmapItemStatus): number {
    const inColumn = this.items().filter((i) => i.status === status);
    return inColumn.length === 0 ? 10 : Math.max(...inColumn.map((i) => i.sortOrder)) + 10;
  }

  startEdit(item: RoadmapItemDto): void {
    this.editingId.set(item.roadmapItemId);
    this.editError.set('');
    this.editForm.setValue({
      title: item.title,
      description: item.description,
      status: item.status,
      targetPhase: item.targetPhase ?? '',
      sortOrder: item.sortOrder,
    });
  }

  cancelEdit(): void {
    this.editingId.set(null);
  }

  saveEdit(id: number): void {
    if (this.editForm.invalid) return;
    this.editError.set('');
    const raw = this.editForm.getRawValue();
    const req: UpdateRoadmapItemRequest = {
      title: raw.title,
      description: raw.description,
      status: raw.status,
      targetPhase: raw.targetPhase || null,
      sortOrder: raw.sortOrder,
    };
    this.api.update(id, req).subscribe({
      next: () => {
        this.editingId.set(null);
        this.load();
      },
      error: (err: HttpErrorResponse) => this.editError.set(extractErrorMessage(err)),
    });
  }

  // Mark-done shortcut: sets Status straight to Done without opening the full edit form.
  // CompletedOn is stamped server-side (RoadmapItemService.UpdateAsync), not sent from here.
  markDone(item: RoadmapItemDto): void {
    const req: UpdateRoadmapItemRequest = {
      title: item.title,
      description: item.description,
      status: 'Done',
      targetPhase: item.targetPhase,
      sortOrder: item.sortOrder,
    };
    this.api.update(item.roadmapItemId, req).subscribe(() => this.load());
  }

  // Reorder within the same status column - swaps SortOrder with the given neighbour and saves
  // both. No drag-and-drop library in this project yet (task brief allows either a Kanban-style
  // or a plain sorted list; up/down keeps reordering simple and dependency-free - see
  // DECISIONS.md).
  move(item: RoadmapItemDto, direction: -1 | 1): void {
    const column = this.items()
      .filter((i) => i.status === item.status)
      .sort((a, b) => a.sortOrder - b.sortOrder);
    const index = column.findIndex((i) => i.roadmapItemId === item.roadmapItemId);
    const neighbourIndex = index + direction;
    if (neighbourIndex < 0 || neighbourIndex >= column.length) return;

    const neighbour = column[neighbourIndex];
    const reqA: UpdateRoadmapItemRequest = { ...toUpdateRequest(item), sortOrder: neighbour.sortOrder };
    const reqB: UpdateRoadmapItemRequest = { ...toUpdateRequest(neighbour), sortOrder: item.sortOrder };

    this.api.update(item.roadmapItemId, reqA).subscribe(() => {
      this.api.update(neighbour.roadmapItemId, reqB).subscribe(() => this.load());
    });
  }
}

function toUpdateRequest(item: RoadmapItemDto): UpdateRoadmapItemRequest {
  return {
    title: item.title,
    description: item.description,
    status: item.status,
    targetPhase: item.targetPhase,
    sortOrder: item.sortOrder,
  };
}
