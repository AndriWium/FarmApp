import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { ActivityDto, CreateActivityRequest } from './activity.model';

@Injectable({ providedIn: 'root' })
export class ActivitiesApiService {
  private http = inject(HttpClient);
  private url = `${environment.apiUrl}/activities`;

  getAll(seasonId?: number) {
    return this.http.get<ActivityDto[]>(this.url, seasonId ? { params: { seasonId } } : {});
  }

  getById(id: number) {
    return this.http.get<ActivityDto>(`${this.url}/${id}`);
  }

  // Creates the header + every input line + the resulting InputStockMovement(s) atomically in one
  // backend call (ActivityService.CreateActivityAsync) - same shape as InputPurchasesApiService.create.
  create(req: CreateActivityRequest) {
    return this.http.post<ActivityDto>(this.url, req);
  }
}
