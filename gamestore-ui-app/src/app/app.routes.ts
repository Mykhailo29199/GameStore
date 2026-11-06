import { Routes } from '@angular/router';
import { MainPageBodyComponent } from './components/bodies/main-page-body/main-page-body.component';
import { permissionGuard } from './core/auth/permission.guard';
import { Permissions } from './core/auth/permissions';
import { AccessPageComponent } from './features/access/access-page.component';
import { LoginComponent } from './features/auth/login/login.component';
import { BasketComponent } from './features/cart/basket/basket.component';
import { CatalogPageComponent } from './features/catalog/catalog-page.component';
import { GameDetailsComponent } from './features/games/game-details/game-details.component';
import { GameFormComponent } from './features/games/game-form/game-form.component';
import { GameListComponent } from './features/games/game-list/game-list.component';
import { CheckoutComponent } from './features/orders/checkout/checkout.component';
import { OrderDetailsComponent } from './features/orders/order-details/order-details.component';
import { OrderListComponent } from './features/orders/order-list/order-list.component';

export const routes: Routes = [
  { path: '', component: MainPageBodyComponent, pathMatch: 'full' },
  { path: 'games', component: GameListComponent },
  {
    path: 'games/new',
    component: GameFormComponent,
    canActivate: [permissionGuard(Permissions.AddGame)],
  },
  {
    path: 'games/:key/edit',
    component: GameFormComponent,
    canActivate: [permissionGuard(Permissions.UpdateGame)],
  },
  { path: 'games/:key', component: GameDetailsComponent },
  { path: 'genres', component: CatalogPageComponent, data: { kind: 'genres' } },
  { path: 'platforms', component: CatalogPageComponent, data: { kind: 'platforms' } },
  { path: 'publishers', component: CatalogPageComponent, data: { kind: 'publishers' } },
  {
    path: 'orders',
    component: OrderListComponent,
    canActivate: [permissionGuard(Permissions.ViewOrders)],
  },
  {
    path: 'orders/:id',
    component: OrderDetailsComponent,
    canActivate: [permissionGuard(Permissions.ViewOrders)],
  },
  {
    path: 'history',
    component: OrderListComponent,
    data: { history: true },
    canActivate: [permissionGuard(Permissions.ViewOrderHistory)],
  },
  {
    path: 'basket',
    component: BasketComponent,
    canActivate: [permissionGuard(Permissions.BuyGame)],
  },
  {
    path: 'order',
    component: CheckoutComponent,
    canActivate: [permissionGuard(Permissions.BuyGame)],
  },
  {
    path: 'users',
    component: AccessPageComponent,
    data: { kind: 'users' },
    canActivate: [permissionGuard(Permissions.ManageUsers)],
  },
  {
    path: 'roles',
    component: AccessPageComponent,
    data: { kind: 'roles' },
    canActivate: [permissionGuard(Permissions.ManageRoles)],
  },
  { path: 'login', component: LoginComponent, data: { mode: 'login' } },
  { path: 'register', component: LoginComponent, data: { mode: 'register' } },
  { path: '**', redirectTo: '' },
];
