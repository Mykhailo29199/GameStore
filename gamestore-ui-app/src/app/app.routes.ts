import { Routes } from '@angular/router';
import { MainPageBodyComponent } from './components/bodies/main-page-body/main-page-body.component';
import { LoginComponent } from './features/auth/login/login.component';

export const routes: Routes = [
  { path: '', component: MainPageBodyComponent, pathMatch: 'full' },
  { path: 'login', component: LoginComponent, data: { mode: 'login' } },
  { path: 'register', component: LoginComponent, data: { mode: 'register' } },
  { path: '**', redirectTo: '' },
];
