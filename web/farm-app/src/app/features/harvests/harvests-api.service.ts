import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { CreateHarvestRequest, HarvestDto } from './harvest.model';

@Injectable({ providedIn: 'root' })
export class HarvestsApiService {
  private http = inject(HttpClient);
  private url = `${environment.apiUrl}/harvests`;

  getAll(seasonId?: number) {
    return this.http.get<HarvestDto[]>(this.url, seasonId ? { params: { seasonId } } : {});
  }

  getById(id: number) {
    return this.http.get<HarvestDto>(`${this.url}/${id}`);
  }

  // Creates the header + every line + the resulting StockBatch/HarvestIn movement per line
  // atomically in one backend call (HarvestService.CreateHarvestAsync) - and, where the block is
  // withholding-locked for this date, enforces (not just warns) that WithholdingOverrideReason was
  // supplied, returning ServiceError.WithholdingLocked (409) if not. The frontend's proactive
  // check (BlocksApiService.getWithholdingStatus) is a UX nicety on top of this real server-side
  // gate, not a replacement for it.
  create(req: CreateHarvestRequest) {
    return this.http.post<HarvestDto>(this.url, req);
  }
}
