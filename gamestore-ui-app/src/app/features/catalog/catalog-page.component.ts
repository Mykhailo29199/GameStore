import { HttpClient } from '@angular/common/http';
import { Component, computed, effect, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Observable } from 'rxjs';
import { ConfirmButtonComponent } from '../../components/confirm-button/confirm-button.component';
import { AuthService } from '../../core/auth/auth.service';
import { APP_SETTINGS } from '../../core/config/app-settings';
import { apiErrorMessage } from '../../core/http/api-error';
import { LanguageService } from '../../core/i18n/language.service';
import {
  CATALOG_CONFIGS,
  CatalogItem,
  CatalogKind,
  CatalogValues,
  missingField,
  toCatalogItem,
  toCatalogPayload,
} from './catalog-config';
import { TranslatePipe } from '../../core/i18n/translate.pipe';
import { CreateInEnglishComponent } from '../../components/create-in-english/create-in-english.component';

interface Draft {
  readonly id: string | null;
  readonly values: CatalogValues;
}

@Component({
  selector: 'app-catalog-page',
  imports: [RouterLink, ConfirmButtonComponent, TranslatePipe, CreateInEnglishComponent],
  templateUrl: './catalog-page.component.html',
})
export class CatalogPageComponent {
  private readonly http = inject(HttpClient);
  private readonly auth = inject(AuthService);
  private readonly i18n = inject(LanguageService);
  private readonly apiUrl = inject(APP_SETTINGS).apiUrl;

  readonly kind = input.required<CatalogKind>();

  protected readonly config = computed(() => CATALOG_CONFIGS[this.kind()]);
  protected readonly languageName = this.i18n.currentName;
  protected readonly languageIsDefault = this.i18n.isDefault;
  protected readonly items = signal<CatalogItem[] | null>(null);
  protected readonly draft = signal<Draft | null>(null);
  protected readonly error = signal('');
  protected readonly canAdd = computed(() => this.auth.hasPermission(this.config().addPermission));
  protected readonly canUpdate = computed(() =>
    this.auth.hasPermission(this.config().updatePermission),
  );
  protected readonly canDelete = computed(() =>
    this.auth.hasPermission(this.config().deletePermission),
  );

  constructor() {
    effect(() => {
      this.draft.set(null);
      this.error.set('');
      this.load();
    });
  }

  protected title(item: CatalogItem): string {
    return item.values[this.config().fields[0].key];
  }

  protected notes(item: CatalogItem): string[] {
    return this.config()
      .fields.slice(1)
      .map((field) => this.display(field.control, item.values[field.key]))
      .filter(Boolean);
  }

  protected gamesQuery(item: CatalogItem): Record<string, string> {
    return { [this.kind()]: item.id };
  }

  protected parentOptions(): CatalogItem[] {
    return (this.items() ?? []).filter((item) => item.id !== this.draft()?.id);
  }

  protected edit(item: CatalogItem | null): void {
    this.error.set('');
    this.draft.set({ id: item?.id ?? null, values: { ...item?.values } });
  }

  protected setValue(key: string, value: string): void {
    this.draft.update((draft) => draft && { ...draft, values: { ...draft.values, [key]: value } });
  }

  protected save(): void {
    const draft = this.draft();
    if (!draft) {
      return;
    }

    const missing = missingField(this.config(), draft.values);
    if (missing) {
      this.error.set(
        this.i18n.translate('common.fieldRequired', { field: this.i18n.translate(missing.label) }),
      );
      return;
    }

    const url = `${this.apiUrl}/${this.config().itemPath}`;
    const payload = toCatalogPayload(this.config(), draft.values, draft.id);

    this.run(draft.id ? this.http.put<void>(url, payload) : this.http.post<void>(url, payload));
  }

  protected remove(item: CatalogItem): void {
    this.run(this.http.delete<void>(`${this.apiUrl}/${this.config().itemPath}/${item.id}`));
  }

  private display(control: string, value: string): string {
    if (control !== 'parent') {
      return value;
    }

    const parent = (this.items() ?? []).find((item) => item.id === value);

    return parent ? this.i18n.translate('catalog.parent', { name: this.title(parent) }) : '';
  }

  private run(request: Observable<void>): void {
    this.error.set('');
    request.subscribe({
      next: () => {
        this.draft.set(null);
        this.load();
      },
      error: (error: unknown) =>
        this.error.set(apiErrorMessage(error, this.i18n.translate('common.saveFailed'))),
    });
  }

  private load(): void {
    const config = this.config();

    this.http.get<Record<string, string | null>[]>(`${this.apiUrl}/${config.listPath}`).subscribe({
      next: (records) =>
        this.items.set(records.map((record) => toCatalogItem(record, config.fields))),
      error: () => this.items.set([]),
    });
  }
}
