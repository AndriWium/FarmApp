import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { ShellComponent } from './core/layout/shell/shell.component';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./core/auth/login-page/login-page.component').then((m) => m.LoginPageComponent),
  },
  {
    path: '',
    component: ShellComponent,
    canActivate: [authGuard],
    children: [
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./features/dashboard/dashboard-page/dashboard-page.component').then(
            (m) => m.DashboardPageComponent,
          ),
      },
      {
        path: 'grades',
        loadComponent: () =>
          import('./features/grades/grade-page/grade-page.component').then(
            (m) => m.GradePageComponent,
          ),
      },
      {
        path: 'blocks',
        loadComponent: () =>
          import('./features/blocks/block-page/block-page.component').then(
            (m) => m.BlockPageComponent,
          ),
      },
      {
        path: 'crops',
        loadComponent: () =>
          import('./features/crops/crop-page/crop-page.component').then(
            (m) => m.CropPageComponent,
          ),
      },
      {
        path: 'cultivars',
        loadComponent: () =>
          import('./features/cultivars/cultivar-page/cultivar-page.component').then(
            (m) => m.CultivarPageComponent,
          ),
      },
      {
        path: 'input-items',
        loadComponent: () =>
          import('./features/input-items/input-item-page/input-item-page.component').then(
            (m) => m.InputItemPageComponent,
          ),
      },
      {
        path: 'suppliers',
        loadComponent: () =>
          import('./features/suppliers/supplier-page/supplier-page.component').then(
            (m) => m.SupplierPageComponent,
          ),
      },
      // Phase 5b-2 adds: price lists, products, pack sizes, customers, expense categories,
      // activity types (same pattern as above).
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
    ],
  },
  { path: '**', redirectTo: '' },
];
