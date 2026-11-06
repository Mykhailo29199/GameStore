import { Routes } from '@angular/router';
import { MainPageBodyComponent } from './components/bodies/main-page-body/main-page-body.component';
import { permissionGuard } from './core/auth/permission.guard';
import { Permissions } from './core/auth/permissions';
import { LoginComponent } from './features/auth/login/login.component';
import { CatalogPageComponent } from './features/catalog/catalog-page.component';
import { GameDetailsComponent } from './features/games/game-details/game-details.component';
import { GameFormComponent } from './features/games/game-form/game-form.component';
import { GameListComponent } from './features/games/game-list/game-list.component';

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
  { path: 'login', component: LoginComponent, data: { mode: 'login' } },
  { path: 'register', component: LoginComponent, data: { mode: 'register' } },
  { path: '**', redirectTo: '' },
];
