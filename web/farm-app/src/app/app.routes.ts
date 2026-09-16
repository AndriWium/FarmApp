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
      // Phase 5b adds: grades, blocks, products, stock, pos, reports (all lazy, all children
      // of ShellComponent, same pattern as dashboard above).
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
    ],
  },
  { path: '**', redirectTo: '' },
];
