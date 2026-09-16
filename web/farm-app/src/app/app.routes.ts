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
        path: 'plantings',
        loadComponent: () =>
          import('./features/plantings/planting-page/planting-page.component').then(
            (m) => m.PlantingPageComponent,
          ),
      },
      {
        path: 'seasons',
        loadComponent: () =>
          import('./features/seasons/season-page/season-page.component').then(
            (m) => m.SeasonPageComponent,
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
      {
        path: 'price-lists',
        loadComponent: () =>
          import('./features/price-lists/price-list-page/price-list-page.component').then(
            (m) => m.PriceListPageComponent,
          ),
      },
      {
        path: 'products',
        loadComponent: () =>
          import('./features/products/product-page/product-page.component').then(
            (m) => m.ProductPageComponent,
          ),
      },
      {
        path: 'products/:id/recipe',
        loadComponent: () =>
          import('./features/products/product-recipe-page/product-recipe-page.component').then(
            (m) => m.ProductRecipePageComponent,
          ),
      },
      {
        path: 'pack-sizes',
        loadComponent: () =>
          import('./features/pack-sizes/pack-size-page/pack-size-page.component').then(
            (m) => m.PackSizePageComponent,
          ),
      },
      {
        path: 'customers',
        loadComponent: () =>
          import('./features/customers/customer-page/customer-page.component').then(
            (m) => m.CustomerPageComponent,
          ),
      },
      {
        path: 'expense-categories',
        loadComponent: () =>
          import(
            './features/expense-categories/expense-category-page/expense-category-page.component'
          ).then((m) => m.ExpenseCategoryPageComponent),
      },
      {
        path: 'activity-types',
        loadComponent: () =>
          import('./features/activity-types/activity-type-page/activity-type-page.component').then(
            (m) => m.ActivityTypePageComponent,
          ),
      },
      {
        path: 'locations',
        loadComponent: () =>
          import('./features/locations/location-page/location-page.component').then(
            (m) => m.LocationPageComponent,
          ),
      },
      {
        path: 'rainfall-logs',
        loadComponent: () =>
          import('./features/rainfall-logs/rainfall-log-page/rainfall-log-page.component').then(
            (m) => m.RainfallLogPageComponent,
          ),
      },
      {
        path: 'stock',
        loadComponent: () =>
          import('./features/stock-batches/stock-on-hand-page/stock-on-hand-page.component').then(
            (m) => m.StockOnHandPageComponent,
          ),
      },
      {
        path: 'produce-purchases',
        loadComponent: () =>
          import(
            './features/produce-purchases/produce-purchase-list-page/produce-purchase-list-page.component'
          ).then((m) => m.ProducePurchaseListPageComponent),
      },
      {
        path: 'produce-purchases/new',
        loadComponent: () =>
          import(
            './features/produce-purchases/produce-purchase-form-page/produce-purchase-form-page.component'
          ).then((m) => m.ProducePurchaseFormPageComponent),
      },
      {
        path: 'produce-purchases/:id',
        loadComponent: () =>
          import(
            './features/produce-purchases/produce-purchase-detail-page/produce-purchase-detail-page.component'
          ).then((m) => m.ProducePurchaseDetailPageComponent),
      },
      {
        path: 'input-purchases',
        loadComponent: () =>
          import(
            './features/input-purchases/input-purchase-list-page/input-purchase-list-page.component'
          ).then((m) => m.InputPurchaseListPageComponent),
      },
      {
        path: 'input-purchases/new',
        loadComponent: () =>
          import(
            './features/input-purchases/input-purchase-form-page/input-purchase-form-page.component'
          ).then((m) => m.InputPurchaseFormPageComponent),
      },
      {
        path: 'input-purchases/:id',
        loadComponent: () =>
          import(
            './features/input-purchases/input-purchase-detail-page/input-purchase-detail-page.component'
          ).then((m) => m.InputPurchaseDetailPageComponent),
      },
      {
        path: 'stock-takes',
        loadComponent: () =>
          import(
            './features/stock-takes/stock-take-list-page/stock-take-list-page.component'
          ).then((m) => m.StockTakeListPageComponent),
      },
      {
        path: 'stock-takes/new',
        loadComponent: () =>
          import(
            './features/stock-takes/stock-take-start-page/stock-take-start-page.component'
          ).then((m) => m.StockTakeStartPageComponent),
      },
      {
        path: 'stock-takes/:id',
        loadComponent: () =>
          import(
            './features/stock-takes/stock-take-detail-page/stock-take-detail-page.component'
          ).then((m) => m.StockTakeDetailPageComponent),
      },
      {
        path: 'pos',
        loadComponent: () =>
          import('./features/sales/pos-page/pos-page.component').then((m) => m.PosPageComponent),
      },
      {
        path: 'till-sessions/:id/close',
        loadComponent: () =>
          import('./features/till-sessions/till-close-page/till-close-page.component').then(
            (m) => m.TillClosePageComponent,
          ),
      },
      {
        path: 'sales',
        loadComponent: () =>
          import('./features/sales/sale-list-page/sale-list-page.component').then(
            (m) => m.SaleListPageComponent,
          ),
      },
      {
        path: 'sales/:id',
        loadComponent: () =>
          import('./features/sales/sale-detail-page/sale-detail-page.component').then(
            (m) => m.SaleDetailPageComponent,
          ),
      },
      {
        path: 'stock-movements',
        loadComponent: () =>
          import('./features/stock-movements/movement-hub-page/movement-hub-page.component').then(
            (m) => m.MovementHubPageComponent,
          ),
      },
      {
        path: 'stock-movements/:kind',
        loadComponent: () =>
          import(
            './features/stock-movements/movement-form-page/movement-form-page.component'
          ).then((m) => m.MovementFormPageComponent),
      },
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
    ],
  },
  { path: '**', redirectTo: '' },
];
