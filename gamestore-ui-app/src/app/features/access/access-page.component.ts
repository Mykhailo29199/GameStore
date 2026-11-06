import { HttpClient } from '@angular/common/http';
import { Component, computed, effect, inject, input, signal } from '@angular/core';
import { Observable, map } from 'rxjs';
import { ConfirmButtonComponent } from '../../components/confirm-button/confirm-button.component';
import { APP_SETTINGS } from '../../core/config/app-settings';
import { apiErrorMessage } from '../../core/http/api-error';
import { TranslationKey } from '../../core/i18n/dictionaries';
import { LanguageService } from '../../core/i18n/language.service';
import { TranslatePipe } from '../../core/i18n/translate.pipe';

export type AccessKind = 'users' | 'roles';

interface NamedItem {
  id: string;
  name: string;
}

interface Option {
  readonly value: string;
  readonly label: string;
}

interface Draft {
  readonly id: string | null;
  readonly name: string;
  readonly selected: string[];
}

interface AccessConfig {
  readonly title: TranslationKey;
  readonly addTitle: TranslationKey;
  readonly newTitle: TranslationKey;
  readonly editTitle: TranslationKey;
  readonly optionsTitle: TranslationKey;
  readonly optionsPath: string;
  readonly selectedPath: string;
  readonly hasPassword: boolean;
}

const MIN_PASSWORD_LENGTH = 4;

const CONFIGS: Readonly<Record<AccessKind, AccessConfig>> = {
  users: {
    title: 'access.users.title',
    addTitle: 'access.users.add',
    newTitle: 'access.users.new',
    editTitle: 'access.users.edit',
    optionsTitle: 'access.users.options',
    optionsPath: 'roles',
    selectedPath: 'roles',
    hasPassword: true,
  },
  roles: {
    title: 'access.roles.title',
    addTitle: 'access.roles.add',
    newTitle: 'access.roles.new',
    editTitle: 'access.roles.edit',
    optionsTitle: 'access.roles.options',
    optionsPath: 'roles/permissions',
    selectedPath: 'permissions',
    hasPassword: false,
  },
};

function toOption(item: NamedItem | string): Option {
  return typeof item === 'string'
    ? { value: item, label: item }
    : { value: item.id, label: item.name };
}

@Component({
  selector: 'app-access-page',
  imports: [ConfirmButtonComponent, TranslatePipe],
  templateUrl: './access-page.component.html',
})
export class AccessPageComponent {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = inject(APP_SETTINGS).apiUrl;
  private readonly i18n = inject(LanguageService);

  readonly kind = input.required<AccessKind>();

  protected readonly config = computed(() => CONFIGS[this.kind()]);
  protected readonly items = signal<NamedItem[] | null>(null);
  protected readonly options = signal<Option[]>([]);
  protected readonly draft = signal<Draft | null>(null);
  protected readonly error = signal('');

  constructor() {
    effect(() => {
      const config = this.config();

      this.draft.set(null);
      this.error.set('');
      this.load();
      this.http
        .get<(NamedItem | string)[]>(`${this.apiUrl}/${config.optionsPath}`)
        .pipe(map((options) => options.map(toOption)))
        .subscribe({
          next: (options) => this.options.set(options),
          error: () => this.options.set([]),
        });
    });
  }

  protected create(): void {
    this.error.set('');
    this.draft.set({ id: null, name: '', selected: [] });
  }

  protected edit(item: NamedItem): void {
    const url = `${this.itemsUrl()}/${item.id}/${this.config().selectedPath}`;

    this.error.set('');
    this.http
      .get<(NamedItem | string)[]>(url)
      .pipe(map((selected) => selected.map((option) => toOption(option).value)))
      .subscribe({
        next: (selected) => this.draft.set({ id: item.id, name: item.name, selected }),
        error: (error: unknown) =>
          this.error.set(apiErrorMessage(error, this.i18n.translate('common.loadFailed'))),
      });
  }

  protected toggle(value: string, checked: boolean): void {
    this.draft.update((draft) => {
      if (!draft) {
        return draft;
      }

      const others = draft.selected.filter((item) => item !== value);

      return { ...draft, selected: checked ? [...others, value] : others };
    });
  }

  protected save(name: string, password: string): void {
    const draft = this.draft();
    const problem = draft ? this.validate(draft, name.trim(), password) : '';

    this.error.set(problem);
    if (!draft || problem) {
      return;
    }

    const body = this.buildBody(draft, name.trim(), password);

    this.run(
      draft.id
        ? this.http.put<void>(this.itemsUrl(), body)
        : this.http.post<void>(this.itemsUrl(), body),
    );
  }

  protected remove(item: NamedItem): void {
    this.run(this.http.delete<void>(`${this.itemsUrl()}/${item.id}`));
  }

  private validate(draft: Draft, name: string, password: string): string {
    if (!name) {
      return this.i18n.translate('common.nameRequired');
    }

    const passwordNeeded = this.config().hasPassword && (!draft.id || password.length > 0);

    return passwordNeeded && password.length < MIN_PASSWORD_LENGTH
      ? this.i18n.translate('common.passwordLength', { count: MIN_PASSWORD_LENGTH })
      : '';
  }

  private buildBody(draft: Draft, name: string, password: string): object {
    const record = draft.id ? { id: draft.id, name } : { name };

    return this.kind() === 'users'
      ? { user: record, roles: draft.selected, password: password || null }
      : { role: record, permissions: draft.selected };
  }

  private itemsUrl(): string {
    return `${this.apiUrl}/${this.kind()}`;
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
    this.http.get<NamedItem[]>(this.itemsUrl()).subscribe({
      next: (items) => this.items.set(items),
      error: () => this.items.set([]),
    });
  }
}
