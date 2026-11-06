import { InjectionToken } from '@angular/core';

export interface AppSettings {
  readonly apiUrl: string;
  readonly taxPercent: number;
}

export const appSettings: AppSettings = {
  apiUrl: '/api',
  taxPercent: 2.5,
};

export const APP_SETTINGS = new InjectionToken<AppSettings>('APP_SETTINGS', {
  providedIn: 'root',
  factory: () => appSettings,
});
