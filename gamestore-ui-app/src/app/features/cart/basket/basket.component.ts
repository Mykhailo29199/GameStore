import { CurrencyPipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Observable } from 'rxjs';
import { PriceComponent } from '../../../components/price/price.component';
import { APP_SETTINGS } from '../../../core/config/app-settings';
import { gameCoverUrl } from '../../../core/games/game-cover';
import { apiErrorMessage } from '../../../core/http/api-error';
import { orderTotals } from '../../../core/pricing/pricing';
import { OrderLine } from '../../../models/order.model';
import { CartService } from '../../../services/cart.service';

@Component({
  selector: 'app-basket',
  imports: [CurrencyPipe, RouterLink, PriceComponent],
  templateUrl: './basket.component.html',
  styleUrl: './basket.component.css',
})
export class BasketComponent {
  private readonly cart = inject(CartService);
  private readonly taxPercent = inject(APP_SETTINGS).taxPercent;

  protected readonly lines = signal<OrderLine[] | null>(null);
  protected readonly busy = signal(false);
  protected readonly error = signal('');
  protected readonly totals = computed(() => orderTotals(this.lines() ?? [], this.taxPercent));

  constructor() {
    this.reload();
  }

  protected cover(line: OrderLine): string {
    return gameCoverUrl(line.gameKey);
  }

  protected increase(line: OrderLine): void {
    this.change(this.cart.addGame(line.gameKey));
  }

  protected decrease(line: OrderLine): void {
    this.change(this.cart.removeOne(line.gameKey));
  }

  protected remove(line: OrderLine): void {
    this.change(this.cart.removePosition(line.gameKey));
  }

  private change(request: Observable<void>): void {
    if (this.busy()) {
      return;
    }

    this.busy.set(true);
    this.error.set('');
    request.subscribe({
      next: () => this.reload(),
      error: (error: unknown) => {
        this.error.set(apiErrorMessage(error, 'The cart could not be updated.'));
        this.reload();
      },
    });
  }

  private reload(): void {
    this.cart.getLines().subscribe((lines) => {
      this.lines.set(lines);
      this.busy.set(false);
    });
  }
}
