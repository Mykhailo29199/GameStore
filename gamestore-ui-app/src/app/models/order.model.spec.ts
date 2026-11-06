import { GameWithIdDTO } from './games/game-filter.model';
import { Order, OrderStatus, canEditOrder, canShipOrder, toOrderLine } from './order.model';

const GAME: GameWithIdDTO = {
  id: 'g1',
  key: 'vane',
  name: 'Vane',
  price: 24.99,
  unitInStock: 3,
  discount: 35,
};

describe('toOrderLine', () => {
  it('restores the unit price from the discounted line total of the API', () => {
    const dto = { id: 'l1', productId: 'g1', price: 32.487, quantity: 2, discount: 35 };

    expect(toOrderLine(dto, GAME)).toEqual({
      id: 'l1',
      productId: 'g1',
      gameKey: 'vane',
      gameName: 'Vane',
      quantity: 2,
      unitPrice: 24.99,
      discount: 35,
    });
  });

  it('falls back to the current game price for a fully discounted line', () => {
    const dto = { id: 'l1', productId: 'g1', price: 0, quantity: 1, discount: 100 };

    expect(toOrderLine(dto, GAME).unitPrice).toBe(24.99);
  });

  it('keeps a line whose game can no longer be loaded', () => {
    const dto = { id: 'l1', productId: 'gone', price: 10, quantity: 1, discount: null };

    expect(toOrderLine(dto, null)).toMatchObject({
      gameKey: '',
      gameName: 'Unavailable game',
      unitPrice: 10,
      discount: 0,
    });
  });
});

describe('order actions by status', () => {
  function order(status: OrderStatus): Order {
    return { id: 'o1', customerId: 'c1', date: '2026-10-08', status, sum: 10 };
  }

  it.each<[OrderStatus, boolean, boolean]>([
    ['Open', true, false],
    ['Checkout', true, false],
    ['Paid', true, true],
    ['Shipped', false, false],
    ['Cancelled', false, false],
  ])('%s: editable %s, can be shipped %s', (status, editable, shippable) => {
    expect(canEditOrder(order(status))).toBe(editable);
    expect(canShipOrder(order(status))).toBe(shippable);
  });

  it('allows nothing until the order is loaded', () => {
    expect(canEditOrder(null)).toBe(false);
    expect(canShipOrder(null)).toBe(false);
  });
});
