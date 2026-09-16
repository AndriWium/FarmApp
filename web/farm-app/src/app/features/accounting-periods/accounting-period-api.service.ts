import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import {
  AccountingPeriodDto,
  CloseChecklistDto,
  CloseMonthResultDto,
  ReopenMonthRequest,
} from './accounting-period.model';

@Injectable({ providedIn: 'root' })
export class AccountingPeriodApiService {
  private http = inject(HttpClient);
  private url = `${environment.apiUrl}/accounting-periods`;

  getPeriod(year: number, month: number) {
    return this.http.get<AccountingPeriodDto>(`${this.url}/${year}/${month}`);
  }

  getChecklist(year: number, month: number) {
    return this.http.get<CloseChecklistDto>(`${this.url}/${year}/${month}/checklist`);
  }

  close(year: number, month: number) {
    return this.http.post<CloseMonthResultDto>(`${this.url}/${year}/${month}/close`, {});
  }

  reopen(year: number, month: number, req: ReopenMonthRequest) {
    return this.http.post<AccountingPeriodDto>(`${this.url}/${year}/${month}/reopen`, req);
  }
}
