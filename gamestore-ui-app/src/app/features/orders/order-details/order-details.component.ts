import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, computed, effect, inject, input, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { Observable, catchError, of } from 'rxjs';
import { ConfirmButtonComponent } from '../../../components/confirm-button/confirm-button.component';
import { PriceComponent } from '../../../components/price/price.component';
import { AuthService } from '../../../core/auth/auth.service';
import { Permissions } from '../../../core/auth/permissions';
import { APP_SETTINGS } from '../../../core/config/app-settings';
import { apiErrorMessage } from '../../../core/http/api-error';
import { LanguageService } from '../../../core/i18n/language.service';
import { lineTotal, orderTotals } from '../../../core/pricing/pricing';
import { GameWithIdDTO } from '../../../models/games/game-filter.model';
import { Order, OrderLine, canEditOrder, canShipOrder } from '../../../models/order.model';
import { GameService } from '../../../services/game.service';
import { OrderService } from '../../../services/order.service';
import { TranslatePipe, TranslateValuePipe } from '../../../core/i18n/translate.pipe';

@Component({
  selector: 'app-order-details',
  imports: [
    CurrencyPipe,
    DatePipe,
    RouterLink,
    PriceComponent,
    ConfirmButtonComponent,
    TranslatePipe,
    TranslateValuePipe,
  ],
  templateUrl: './order-details.component.html',
})
export class OrderDetailsComponent {
  private readonly orderService = inject(OrderService);
  private readonly auth = inject(AuthService);
  private readonly i18n = inject(LanguageService);
  private readonly taxPercent = inject(APP_SETTINGS).taxPercent;

  readonly id = input.required<string>();

  private readonly canEdit = this.auth.hasPermission(Permissions.UpdateOrder);
  private readonly canShip = this.auth.hasPermission(Permissions.ShipOrder);
  protected readonly games = toSignal(
    this.canEdit
      ? inject(GameService)
          .getAllGames()
          .pipe(catchError(() => of<GameWithIdDTO[]>([])))
      : of<GameWithIdDTO[]>([]),
    { initialValue: [] },
  );

  protected readonly order = signal<Order | null>(null);
  protected readonly lines = signal<OrderLine[] | null>(null);
  protected readonly message = signal('');
  protected readonly error = signal('');
  protected readonly editable = computed(() => this.canEdit && canEditOrder(this.order()));
  protected readonly shippable = computed(() => this.canShip && canShipOrder(this.order()));
  protected readonly totals = computed(() => orderTotals(this.lines() ?? [], this.taxPercent));

  constructor() {
    effect(() => this.load(this.id()));
  }

  protected total(line: OrderLine): number {
    return lineTotal(line);
  }

  protected changeQuantity(line: OrderLine, change: number): void {
    this.run(this.orderService.updateQuantity(line.id, line.quantity + change), '');
  }

  protected remove(line: OrderLine): void {
    this.run(this.orderService.deleteLine(line.id), '');
  }

  protected addGame(gameKey: string): void {
    if (gameKey) {
      this.run(this.orderService.addGame(this.id(), gameKey), '');
    }
  }

  protected ship(): void {
    this.run(this.orderService.ship(this.id()), this.i18n.translate('orders.shipped'));
  }

  private run(request: Observable<void>, success: string): void {
    this.error.set('');
    this.message.set('');
    request.subscribe({
      next: () => {
        this.message.set(success);
        this.load(this.id());
      },
      error: (error: unknown) =>
        this.error.set(apiErrorMessage(error, this.i18n.translate('orders.changeFailed'))),
    });
  }

  private load(id: string): void {
    this.orderService.getOrder(id).subscribe({
      next: (order) => this.order.set(order),
      error: () => this.error.set(this.i18n.translate('orders.notFound')),
    });
    this.orderService.getOrderLines(id).subscribe({
      next: (lines) => this.lines.set(lines),
      error: () => this.lines.set([]),
    });
  }
}
