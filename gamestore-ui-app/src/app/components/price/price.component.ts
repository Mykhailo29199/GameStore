import { CurrencyPipe } from '@angular/common';
import { Component, computed, input } from '@angular/core';
import { discountedPrice, hasDiscount } from '../../core/pricing/pricing';

@Component({
  selector: 'app-price',
  imports: [CurrencyPipe],
  templateUrl: './price.component.html',
  styleUrl: './price.component.css',
})
export class PriceComponent {
  readonly price = input.required<number>();
  readonly discount = input<number | null | undefined>(0);

  protected readonly discounted = computed(() => hasDiscount(this.discount()));
  protected readonly finalPrice = computed(() => discountedPrice(this.price(), this.discount()));
}
