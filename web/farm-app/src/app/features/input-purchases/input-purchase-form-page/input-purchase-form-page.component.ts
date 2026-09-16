import { DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { AuthService } from '../../../core/auth/auth.service';
import { extractErrorMessage } from '../../../shared/http-error.util';
import { InputItemDto } from '../../input-items/input-item.model';
import { InputItemsApiService } from '../../input-items/input-items-api.service';
import { SupplierDto } from '../../suppliers/supplier.model';
import { SuppliersApiService } from '../../suppliers/suppliers-api.service';
import { CreateInputPurchaseLineRequest, InputPurchaseDto } from '../input-purchase.model';
import { InputPurchasesApiService } from '../input-purchases-api.service';

function todayIso(): string {
  return new Date().toISOString().slice(0, 10);
}

@Component({
  selector: 'app-input-purchase-form-page',
  imports: [ReactiveFormsModule, RouterLink, DecimalPipe],
  templateUrl: './input-purchase-form-page.component.html',
  styleUrl: './input-purchase-form-page.component.scss',
})
export class InputPurchaseFormPageComponent implements OnInit {
  private fb = inject(FormBuilder);
  private api = inject(InputPurchasesApiService);
  private suppliersApi = inject(SuppliersApiService);
  private inputItemsApi = inject(InputItemsApiService);
  auth = inject(AuthService);

  // POST is behind CanManageMasterData (InputPurchasesController, same footing as
  // ProducePurchasesController - see DECISIONS.md Phase 3a) - unlike Planting/Season/RainfallLog,
  // which are ungated day-to-day farm capture.
  canManage = computed(() => this.auth.hasRole('Owner'));

  suppliers = signal<SupplierDto[]>([]);
  inputItems = signal<InputItemDto[]>([]);

  header = this.fb.nonNullable.group({
    supplierId: [0, [Validators.required, Validators.min(1)]],
    date: [todayIso(), [Validators.required]],
    invoiceRef: this.fb.control<string | null>(null, [Validators.maxLength(50)]),
  });

  lines = this.fb.array<ReturnType<typeof this.newLineGroup>>([]);

  submitting = signal(false);
  submitError = signal('');
  created = signal<InputPurchaseDto | null>(null);
  // InputItemId -> current on-hand, fetched right after a successful create so the success panel
  // doubles as proof the purchase actually moved stock (InputPurchaseLineDto itself carries no
  // on-hand info - InputItemsController.GetOnHand is the only place to read it).
  onHandByItemId = signal<Map<number, number>>(new Map());

  ngOnInit(): void {
    this.suppliersApi.getAll().subscribe((rows) => this.suppliers.set(rows));
    this.inputItemsApi.getAll().subscribe((rows) => this.inputItems.set(rows));
    this.addLine();
  }

  private newLineGroup() {
    return this.fb.nonNullable.group({
      inputItemId: [0, [Validators.required, Validators.min(1)]],
      qty: [0, [Validators.required, Validators.min(0.001)]],
      unitCost: [0, [Validators.required, Validators.min(0)]],
      vatAmount: this.fb.control<number | null>(null, [Validators.min(0)]),
    });
  }

  addLine(): void {
    this.lines.push(this.newLineGroup());
  }

  removeLine(index: number): void {
    this.lines.removeAt(index);
  }

  totalCost(): number {
    return this.lines.controls.reduce((sum, line) => {
      const raw = line.getRawValue();
      return sum + raw.qty * raw.unitCost;
    }, 0);
  }

  submit(): void {
    if (this.header.invalid || this.lines.invalid || this.lines.length === 0) {
      this.header.markAllAsTouched();
      this.lines.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.submitError.set('');
    this.created.set(null);
    this.onHandByItemId.set(new Map());

    const headerRaw = this.header.getRawValue();
    const lineRequests: CreateInputPurchaseLineRequest[] = this.lines.controls.map((c) => c.getRawValue());

    this.api
      .create({
        supplierId: headerRaw.supplierId,
        date: headerRaw.date,
        invoiceRef: headerRaw.invoiceRef,
        lines: lineRequests,
      })
      .subscribe({
        next: (purchase) => {
          this.submitting.set(false);
          this.created.set(purchase);
          this.loadOnHand(purchase);
          this.resetForm();
        },
        error: (err: HttpErrorResponse) => {
          this.submitting.set(false);
          this.submitError.set(extractErrorMessage(err));
        },
      });
  }

  private loadOnHand(purchase: InputPurchaseDto): void {
    const itemIds = [...new Set(purchase.lines.map((l) => l.inputItemId))];
    if (itemIds.length === 0) return;
    forkJoin(itemIds.map((id) => this.inputItemsApi.getOnHand(id))).subscribe((onHands) => {
      this.onHandByItemId.set(new Map(itemIds.map((id, i) => [id, onHands[i]])));
    });
  }

  private resetForm(): void {
    this.header.reset({ supplierId: 0, date: todayIso(), invoiceRef: null });
    this.lines.clear();
    this.addLine();
  }

  inputItemName(id: number): string {
    const item = this.inputItems().find((i) => i.inputItemId === id);
    return item ? item.name : `#${id}`;
  }

  inputItemUnit(id: number): string {
    const item = this.inputItems().find((i) => i.inputItemId === id);
    return item ? item.unit : '';
  }
}
