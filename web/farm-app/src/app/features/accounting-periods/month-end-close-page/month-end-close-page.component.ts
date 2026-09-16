import { DatePipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { extractErrorMessage } from '../../../shared/http-error.util';
import { AccountingPeriodApiService } from '../accounting-period-api.service';
import { AccountingPeriodDto, CloseChecklistDto, CloseMonthResultDto } from '../accounting-period.model';

/** yyyy-MM for the current month, local time - matches the <input type="month"> value format. */
function currentYearMonth(): string {
  const now = new Date();
  const month = now.getMonth() + 1;
  return `${now.getFullYear()}-${month < 10 ? '0' + month : month}`;
}

/**
 * Month-end close (AI Guide/10-go-live-controls.md §1, backend built in Phase 4c) - this phase's
 * only job is the screen: pick a year/month, walk the 5-item checklist, close (blocked if item 1
 * fails), reopen a closed period with a required reason. The whole route sits behind ownerGuard
 * (matches AccountingPeriodsController's own CanManageMasterData gate on every action, including
 * reads) so there's no separate in-template role check here.
 */
@Component({
  selector: 'app-month-end-close-page',
  imports: [ReactiveFormsModule, DatePipe, DecimalPipe],
  templateUrl: './month-end-close-page.component.html',
  styleUrl: './month-end-close-page.component.scss',
})
export class MonthEndClosePageComponent implements OnInit {
  private api = inject(AccountingPeriodApiService);
  private fb = inject(FormBuilder);

  selectedMonth = signal(currentYearMonth());

  year = computed(() => Number(this.selectedMonth().split('-')[0]));
  month = computed(() => Number(this.selectedMonth().split('-')[1]));

  loading = signal(true);
  loadError = signal('');

  period = signal<AccountingPeriodDto | null>(null);
  checklist = signal<CloseChecklistDto | null>(null);

  // Set only right after a successful close this session, so the confirmation stays distinct from
  // "the period happens to already be Closed" (period()/checklist() cover that case on their own).
  closeResult = signal<CloseMonthResultDto | null>(null);

  closing = signal(false);
  closeError = signal('');

  reopenForm = this.fb.nonNullable.group({
    reason: ['', [Validators.required, Validators.maxLength(500)]],
  });
  reopening = signal(false);
  reopenError = signal('');
  showReopenForm = signal(false);

  blockedReason = computed(() => {
    const c = this.checklist();
    if (!c || c.tillSessionsAllClosed) return '';
    const ids = c.openTillSessions.map((s) => `#${s.tillSessionId}`).join(', ');
    return `${c.openTillSessions.length} till session(s) opened this month are still open: ${ids}. Close them first.`;
  });

  ngOnInit(): void {
    this.load();
  }

  onMonthChange(value: string): void {
    this.selectedMonth.set(value);
    this.closeResult.set(null);
    this.showReopenForm.set(false);
    this.reopenForm.reset({ reason: '' });
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.loadError.set('');
    const year = this.year();
    const month = this.month();

    forkJoin({
      period: this.api.getPeriod(year, month),
      checklist: this.api.getChecklist(year, month),
    }).subscribe({
      next: ({ period, checklist }) => {
        this.period.set(period);
        this.checklist.set(checklist);
        this.loading.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.loadError.set(extractErrorMessage(err));
        this.loading.set(false);
      },
    });
  }

  closeMonth(): void {
    this.closing.set(true);
    this.closeError.set('');
    this.api.close(this.year(), this.month()).subscribe({
      next: (result) => {
        this.closeResult.set(result);
        this.period.set(result.period);
        this.checklist.set(result.checklist);
        this.closing.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.closeError.set(extractErrorMessage(err));
        this.closing.set(false);
      },
    });
  }

  reopen(): void {
    if (this.reopenForm.invalid) return;
    this.reopening.set(true);
    this.reopenError.set('');
    const reason = this.reopenForm.getRawValue().reason;
    this.api.reopen(this.year(), this.month(), { reason }).subscribe({
      next: (period) => {
        this.period.set(period);
        this.closeResult.set(null);
        this.showReopenForm.set(false);
        this.reopening.set(false);
        this.reopenForm.reset({ reason: '' });
        this.load(); // refresh the checklist too - it's still meaningful once reopened
      },
      error: (err: HttpErrorResponse) => {
        this.reopenError.set(extractErrorMessage(err));
        this.reopening.set(false);
      },
    });
  }
}
