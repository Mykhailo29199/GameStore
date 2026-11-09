import { Component, input, output, signal } from '@angular/core';
import { TranslationKey } from '../../core/i18n/dictionaries';
import { TranslatePipe } from '../../core/i18n/translate.pipe';

@Component({
  selector: 'app-confirm-button',
  imports: [TranslatePipe],
  template: `
    @if (asking()) {
      <span class="confirm">
        <button class="link confirm__yes" type="button" (click)="confirm()">
          {{ 'common.confirm' | t }} {{ (label() | t).toLowerCase() }}
        </button>
        <button class="link" type="button" (click)="asking.set(false)">
          {{ 'common.cancel' | t }}
        </button>
      </span>
    } @else {
      <button class="link" type="button" [disabled]="disabled()" (click)="asking.set(true)">
        {{ label() | t }}
      </button>
    }
  `,
  styles: `
    .confirm {
      display: inline-flex;
      gap: 14px;
    }

    .confirm__yes {
      color: var(--color-danger);
      font-weight: 600;
    }
  `,
})
export class ConfirmButtonComponent {
  readonly label = input<TranslationKey>('common.delete');
  readonly disabled = input(false);
  readonly confirmed = output<void>();

  protected readonly asking = signal(false);

  protected confirm(): void {
    this.asking.set(false);
    this.confirmed.emit();
  }
}
