import { Component, inject } from '@angular/core';
import { LanguageService } from '../../core/i18n/language.service';
import { TranslatePipe } from '../../core/i18n/translate.pipe';

/**
 * Shown instead of a create form in another language: the English data of a new record must be real,
 * so records are created in English and translated by editing.
 */
@Component({
  selector: 'app-create-in-english',
  imports: [TranslatePipe],
  template: `
    <p class="form-note">{{ 'common.createInEnglish' | t }}</p>
    <button class="btn btn--ghost" type="button" (click)="language.use('en')">
      {{ 'common.switchToEnglish' | t }}
    </button>
  `,
  styles: `
    :host {
      display: block;
      margin-bottom: 24px;
    }
  `,
})
export class CreateInEnglishComponent {
  protected readonly language = inject(LanguageService);
}
