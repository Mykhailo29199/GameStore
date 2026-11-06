import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { APP_SETTINGS } from '../../../core/config/app-settings';
import { BasketComponent } from './basket.component';

const GAMES = [
  { id: 'g1', key: 'ace-combat', name: 'Ace Combat 7', price: 60, unitInStock: 5, discount: 25 },
  { id: 'g2', key: 'vane', name: 'Vane', price: 20, unitInStock: 5, discount: 0 },
];

describe('BasketComponent', () => {
  let fixture: ComponentFixture<BasketComponent>;
  let element: HTMLElement;
  let httpMock: HttpTestingController;

  function text(selector: string): string {
    return element.querySelector(selector)?.textContent?.replaceAll(/\s+/g, ' ').trim() ?? '';
  }

  async function flushCart(lines: object[]): Promise<void> {
    httpMock.expectOne('/api/orders/cart').flush(lines);
    for (const game of GAMES) {
      for (const request of httpMock.match(`/api/game/${game.id}`)) {
        request.flush(game);
      }
    }
    await fixture.whenStable();
  }

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [BasketComponent],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: APP_SETTINGS, useValue: { apiUrl: '/api', taxPercent: 10 } },
      ],
    });
    fixture = TestBed.createComponent(BasketComponent);
    element = fixture.nativeElement as HTMLElement;
    httpMock = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
  });

  afterEach(() => httpMock.verify());

  it('shows discounted lines and the order summary with tax', async () => {
    await flushCart([
      { id: 'l1', productId: 'g1', price: 90, quantity: 2, discount: 25 },
      { id: 'l2', productId: 'g2', price: 20, quantity: 1, discount: 0 },
    ]);

    expect(text('.cart__count')).toBe('3 items');
    expect(text('.cart__line s')).toBe('$60.00');
    expect(text('.cart__line strong')).toBe('$45.00');
    expect(text('.summary__rows')).toBe('Subtotal$110.00Taxes$11.00Total$121.00');
  });

  it('adds one more copy with the plus button and reloads the cart', async () => {
    await flushCart([{ id: 'l2', productId: 'g2', price: 20, quantity: 1, discount: 0 }]);

    element.querySelector<HTMLButtonElement>('.quantity__button--plus')?.click();
    httpMock.expectOne({ method: 'POST', url: '/api/games/vane/buy' }).flush(null);
    await flushCart([{ id: 'l2', productId: 'g2', price: 40, quantity: 2, discount: 0 }]);

    expect(text('.quantity__value')).toBe('2');
    expect(text('.summary__row--total')).toBe('Total$44.00');
  });

  it('removes one copy and the whole position through different endpoints', async () => {
    await flushCart([{ id: 'l2', productId: 'g2', price: 40, quantity: 2, discount: 0 }]);

    element.querySelector<HTMLButtonElement>('.quantity__button')?.click();
    httpMock.expectOne({ method: 'DELETE', url: '/api/orders/cart/vane' }).flush(null);
    await flushCart([{ id: 'l2', productId: 'g2', price: 20, quantity: 1, discount: 0 }]);

    element.querySelector<HTMLButtonElement>('.cart__remove')?.click();
    httpMock.expectOne({ method: 'DELETE', url: '/api/orders/cart/vane/all' }).flush(null);
    await flushCart([]);

    expect(text('.cart__message')).toContain('Your cart is empty.');
    expect(element.querySelector('.summary__checkout')).toBeNull();
  });
});
