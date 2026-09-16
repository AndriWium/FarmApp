import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { catchError, of } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PriceDto } from './price.model';

@Injectable({ providedIn: 'root' })
export class PricesApiService {
  private http = inject(HttpClient);
  private url = `${environment.apiUrl}/prices`;

  // GetCurrent 404s (plain NotFound(), no ProblemDetails body) when no price has ever been set
  // for this exact PriceList/Product/Grade/PackSize combination - a real, expected state for a
  // sparsely-priced catalog, not an error worth surfacing as one. Callers get `null` and fall back
  // to a manually-typed unit price (see pos-page's judgment call in DECISIONS.md).
  getCurrent(priceListId: number, productId: number, gradeId: number | null, packSizeId: number | null) {
    const params: Record<string, string | number> = { priceListId, productId };
    if (gradeId !== null) params['gradeId'] = gradeId;
    if (packSizeId !== null) params['packSizeId'] = packSizeId;
    return this.http
      .get<PriceDto>(this.url + '/current', { params })
      .pipe(catchError(() => of(null)));
  }
}
