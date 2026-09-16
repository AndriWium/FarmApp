import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../../core/auth/auth.service';
import { extractErrorMessage } from '../../../shared/http-error.util';
import { CreateLocationRequest, LocationDto, UpdateLocationRequest } from '../location.model';
import { LocationsApiService } from '../locations-api.service';

@Component({
  selector: 'app-location-page',
  imports: [ReactiveFormsModule],
  templateUrl: './location-page.component.html',
  styleUrl: './location-page.component.scss',
})
export class LocationPageComponent implements OnInit {
  private api = inject(LocationsApiService);
  private fb = inject(FormBuilder);
  auth = inject(AuthService);

  canManage = computed(() => this.auth.hasRole('Owner'));

  locations = signal<LocationDto[]>([]);
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
    this.api.getAll(this.showInactive()).subscribe((rows) => this.locations.set(rows));
  }

  toggleShowInactive(): void {
    this.showInactive.update((v) => !v);
    this.load();
  }

  add(): void {
    if (this.createForm.invalid) return;
    this.createError.set('');
    const req: CreateLocationRequest = this.createForm.getRawValue();
    this.api.create(req).subscribe({
      next: () => {
        this.createForm.reset({ name: '' });
        this.load();
      },
      error: (err: HttpErrorResponse) => this.createError.set(extractErrorMessage(err)),
    });
  }

  startEdit(location: LocationDto): void {
    this.editingId.set(location.locationId);
    this.editError.set('');
    this.editForm.setValue({ name: location.name, isActive: location.isActive });
  }

  cancelEdit(): void {
    this.editingId.set(null);
  }

  saveEdit(id: number): void {
    if (this.editForm.invalid) return;
    this.editError.set('');
    const req: UpdateLocationRequest = this.editForm.getRawValue();
    this.api.update(id, req).subscribe({
      next: () => {
        this.editingId.set(null);
        this.load();
      },
      error: (err: HttpErrorResponse) => this.editError.set(extractErrorMessage(err)),
    });
  }

  deactivate(location: LocationDto): void {
    if (!confirm(`Deactivate location "${location.name}"?`)) return;
    this.api.deactivate(location.locationId).subscribe(() => this.load());
  }
}
