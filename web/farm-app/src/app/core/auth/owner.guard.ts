import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

// Route-level gate for the Reports section (Phase 5f-1). Mirrors the backend's CanViewReports
// policy exactly (Program.cs: `.AddPolicy("CanViewReports", p => p.RequireRole("Owner"))`) - a
// non-Owner is bounced straight back to the dashboard rather than being let into a screen whose
// every underlying API call would 403 anyway. This is still just a UI convenience (same disclaimer
// as AuthService.hasRole itself): the API's [Authorize(Policy = "CanViewReports")] on
// ReportsController is the real enforcement.
export const ownerGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  return auth.hasRole('Owner') ? true : router.parseUrl('/dashboard');
};
