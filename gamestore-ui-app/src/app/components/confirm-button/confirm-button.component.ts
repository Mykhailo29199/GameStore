import { Component, input, output, signal } from '@angular/core';

@Component({
  selector: 'app-confirm-button',
  template: `
    @if (asking()) {
      <span class="confirm">
        <button class="link confirm__yes" type="button" (click)="confirm()">
          Confirm {{ label().toLowerCase() }}
        </button>
        <button class="link" type="button" (click)="asking.set(false)">Cancel</button>
      </span>
    } @else {
      <button class="link" type="button" [disabled]="disabled()" (click)="asking.set(true)">
        {{ label() }}
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
  readonly label = input('Delete');
  readonly disabled = input(false);
  readonly confirmed = output<void>();

  protected readonly asking = signal(false);

  protected confirm(): void {
    this.asking.set(false);
    this.confirmed.emit();
  }
}
