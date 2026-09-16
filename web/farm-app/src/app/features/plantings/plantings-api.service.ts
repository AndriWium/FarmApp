import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { CreatePlantingRequest, PlantingDto, UpdatePlantingRequest } from './planting.model';

@Injectable({ providedIn: 'root' })
export class PlantingsApiService {
  private http = inject(HttpClient);
  private url = `${environment.apiUrl}/plantings`;

  // blockId narrows to one block's plantings (PlantingsController.GetAll) - unused by this
  // phase's screens (which list every planting) but wired through for a future block-detail view.
  getAll(blockId?: number) {
    return this.http.get<PlantingDto[]>(this.url, blockId ? { params: { blockId } } : {});
  }

  getById(id: number) {
    return this.http.get<PlantingDto>(`${this.url}/${id}`);
  }

  create(req: CreatePlantingRequest) {
    return this.http.post<PlantingDto>(this.url, req);
  }

  update(id: number, req: UpdatePlantingRequest) {
    return this.http.put<void>(`${this.url}/${id}`, req);
  }
}
