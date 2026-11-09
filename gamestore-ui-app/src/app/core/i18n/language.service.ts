import { DOCUMENT } from '@angular/common';
import { Injectable, computed, effect, inject, signal } from '@angular/core';
import { DICTIONARIES, TranslationKey, TranslationParams, isTranslationKey } from './dictionaries';
import { DEFAULT_LANGUAGE, LANGUAGES, LanguageCode, isLanguageCode } from './languages';

const STORAGE_KEY = 'gamestore.language';

function readStoredLanguage(): LanguageCode {
  try {
    const stored = localStorage.getItem(STORAGE_KEY);
    return isLanguageCode(stored) ? stored : DEFAULT_LANGUAGE;
  } catch {
    return DEFAULT_LANGUAGE;
  }
}

/**
 * Keeps the selected language of the interface. The texts come from the dictionaries;
 * a text that is missing in the selected language is shown in English.
 */
@Injectable({ providedIn: 'root' })
export class LanguageService {
  private readonly document = inject(DOCUMENT);

  readonly current = signal<LanguageCode>(readStoredLanguage());

  readonly currentName = computed(
    () => LANGUAGES.find((language) => language.code === this.current())?.name ?? '',
  );

  // New records are created only in the default language, so their English data is always real.
  readonly isDefault = computed(() => this.current() === DEFAULT_LANGUAGE);

  constructor() {
    effect(() => {
      this.document.documentElement.lang = this.current();
    });
  }

  use(language: LanguageCode): void {
    this.current.set(language);

    try {
      localStorage.setItem(STORAGE_KEY, language);
    } catch {
      // The choice still works for the current page when the storage is not available.
    }
  }

  translate(key: TranslationKey, params?: TranslationParams): string {
    const text = DICTIONARIES[this.current()][key] || DICTIONARIES[DEFAULT_LANGUAGE][key];

    return params
      ? text.replaceAll(/\{(\w+)\}/g, (placeholder, name: string) =>
          name in params ? String(params[name]) : placeholder,
        )
      : text;
  }

  /**
   * Translates a value that comes from the API, for example a sorting option or an order status.
   * A value without a translation is shown as it is.
   */
  translateValue(group: string, value: string): string {
    const key = `${group}.${value}`;

    return isTranslationKey(key) ? this.translate(key) : value;
  }
}
