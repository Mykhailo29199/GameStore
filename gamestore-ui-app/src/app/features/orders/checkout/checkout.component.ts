import { CurrencyPipe } from '@angular/common';
import { HttpErrorResponse, HttpResponse } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { catchError, of } from 'rxjs';
import { APP_SETTINGS } from '../../../core/config/app-settings';
import { TranslationKey } from '../../../core/i18n/dictionaries';
import { LanguageService } from '../../../core/i18n/language.service';
import { orderTotals } from '../../../core/pricing/pricing';
import { PaymentMethod, VisaCard } from '../../../models/order.model';
import { CartService } from '../../../services/cart.service';
import { OrderService } from '../../../services/order.service';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';

interface CardFields {
  readonly holder: string;
  readonly cardNumber: string;
  readonly month: string;
  readonly year: string;
  readonly cvv: string;
}

const VISA = 'Visa';
const PDF_TYPE = 'application/pdf';
const INVOICE_FILE = 'invoice.pdf';
const PAYMENT_FAILED: TranslationKey = 'checkout.failed';

function toCard(fields: CardFields): VisaCard | null {
  const card: VisaCard = {
    holder: fields.holder.trim(),
    cardNumber: fields.cardNumber.replaceAll(/\s+/g, ''),
    monthExpire: Number(fields.month),
    yearExpire: Number(fields.year),
    cvv2: Number(fields.cvv),
  };
  const numbers = [card.monthExpire, card.yearExpire, card.cvv2];
  const complete = card.holder && card.cardNumber && numbers.every((value) => value > 0);

  return complete ? card : null;
}

function saveFile(file: Blob, name: string): void {
  const url = URL.createObjectURL(file);
  const link = document.createElement('a');

  link.href = url;
  link.download = name;
  link.click();
  URL.revokeObjectURL(url);
}

@Component({
  selector: 'app-checkout',
  imports: [CurrencyPipe, RouterLink, TranslatePipe],
  templateUrl: './checkout.component.html',
  styleUrl: './checkout.component.css',
})
export class CheckoutComponent {
  private readonly orders = inject(OrderService);
  private readonly cart = inject(CartService);
  private readonly i18n = inject(LanguageService);
  private readonly taxPercent = inject(APP_SETTINGS).taxPercent;

  private readonly lines = toSignal(this.cart.getLines(), { initialValue: [] });
  protected readonly methods = toSignal(
    this.orders.getPaymentMethods().pipe(catchError(() => of<PaymentMethod[]>([]))),
    { initialValue: [] },
  );

  protected readonly totals = computed(() => orderTotals(this.lines(), this.taxPercent));
  protected readonly selected = signal('');
  protected readonly visaSelected = computed(() => this.selected() === VISA);
  protected readonly paying = signal(false);
  protected readonly error = signal('');
  protected readonly result = signal('');

  protected pay(fields: CardFields): void {
    const card = this.visaSelected() ? toCard(fields) : null;

    if (this.visaSelected() && !card) {
      this.error.set(this.i18n.translate('checkout.fillCard'));
      return;
    }

    this.error.set('');
    this.paying.set(true);
    this.orders.pay(this.selected(), card).subscribe({
      next: (response) => this.complete(response),
      error: (error: unknown) => this.fail(error),
    });
  }

  private complete(response: HttpResponse<Blob>): void {
    const file = response.body;

    if (file?.type === PDF_TYPE) {
      saveFile(file, INVOICE_FILE);
      this.result.set(this.i18n.translate('checkout.invoice'));
    } else {
      this.result.set(this.i18n.translate('checkout.paid', { method: this.selected() }));
    }

    this.paying.set(false);
    this.cart.clearCount();
  }

  private fail(error: unknown): void {
    this.paying.set(false);
    this.error.set(this.i18n.translate(PAYMENT_FAILED));

    if (error instanceof HttpErrorResponse && error.error instanceof Blob) {
      error.error
        .text()
        .then((text) => this.error.set(this.readDetail(text)))
        .catch(() => this.error.set(this.i18n.translate(PAYMENT_FAILED)));
    }
  }

  private readDetail(text: string): string {
    try {
      const problem = JSON.parse(text) as { Detail?: string; detail?: string };

      return problem.Detail ?? problem.detail ?? this.i18n.translate(PAYMENT_FAILED);
    } catch {
      return this.i18n.translate(PAYMENT_FAILED);
    }
  }
}
