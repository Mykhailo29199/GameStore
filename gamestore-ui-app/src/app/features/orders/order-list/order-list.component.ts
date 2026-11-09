import { DatePipe } from '@angular/common';
import { Component, effect, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { apiErrorMessage } from '../../../core/http/api-error';
import { LanguageService } from '../../../core/i18n/language.service';
import { Order } from '../../../models/order.model';
import { OrderService } from '../../../services/order.service';
import { TranslatePipe, TranslateValuePipe } from '../../../core/i18n/translate.pipe';

@Component({
  selector: 'app-order-list',
  imports: [DatePipe, RouterLink, TranslatePipe, TranslateValuePipe],
  templateUrl: './order-list.component.html',
})
export class OrderListComponent {
  private readonly orderService = inject(OrderService);
  private readonly i18n = inject(LanguageService);

  readonly history = input(false);

  protected readonly orders = signal<Order[] | null>(null);
  protected readonly error = signal('');

  constructor() {
    effect(() => this.load(this.history(), '', ''));
  }

  protected load(history: boolean, start: string, end: string): void {
    const request = history
      ? this.orderService.getHistory(start, end)
      : this.orderService.getOrders();

    this.error.set('');
    request.subscribe({
      next: (orders) => this.orders.set(orders),
      error: (error: unknown) => {
        this.orders.set([]);
        this.error.set(apiErrorMessage(error, this.i18n.translate('orders.loadFailed')));
      },
    });
  }
}
