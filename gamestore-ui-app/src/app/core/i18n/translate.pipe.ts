import { Pipe, PipeTransform, inject } from '@angular/core';
import { TranslationKey, TranslationParams } from './dictionaries';
import { LanguageService } from './language.service';

/**
 * Not pure, so the text changes as soon as the language is switched.
 */
@Pipe({ name: 't', pure: false })
export class TranslatePipe implements PipeTransform {
  private readonly language = inject(LanguageService);

  transform(key: TranslationKey, params?: TranslationParams): string {
    return this.language.translate(key, params);
  }
}

/**
 * Translates a value from the API: `option | tv: 'sorting'`.
 */
@Pipe({ name: 'tv', pure: false })
export class TranslateValuePipe implements PipeTransform {
  private readonly language = inject(LanguageService);

  transform(value: string, group: string): string {
    return this.language.translateValue(group, value);
  }
}
