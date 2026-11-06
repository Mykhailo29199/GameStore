import {
  discountedPrice,
  hasDiscount,
  lineTotal,
  orderTotals,
  priceWithTax,
  roundMoney,
  taxAmount,
  unitPriceBeforeDiscount,
} from './pricing';

describe('roundMoney', () => {
  it.each([
    [1.005, 1.01],
    [2.675, 2.68],
    [10, 10],
    [0.004, 0],
  ])('rounds %d to %d', (amount, expected) => {
    expect(roundMoney(amount)).toBe(expected);
  });
});

describe('hasDiscount', () => {
  it.each([
    [20, true],
    [0, false],
    [-5, false],
    [null, false],
    [undefined, false],
  ])('is %s for %s percent', (discount, expected) => {
    expect(hasDiscount(discount)).toBe(expected);
  });
});

describe('discountedPrice', () => {
  it('takes a whole number of percent off the price', () => {
    expect(discountedPrice(59.99, 20)).toBe(47.99);
  });

  it('keeps the price without a discount', () => {
    expect(discountedPrice(59.99, 0)).toBe(59.99);
    expect(discountedPrice(59.99, null)).toBe(59.99);
  });

  it('never goes below zero or above the original price', () => {
    expect(discountedPrice(10, 150)).toBe(0);
    expect(discountedPrice(10, -30)).toBe(10);
  });
});

describe('taxAmount', () => {
  it('is the configured percent of the price', () => {
    expect(taxAmount(119.97, 2.5)).toBe(3);
    expect(taxAmount(59.99, 20)).toBe(12);
  });

  it('is zero for a zero or negative tax percent', () => {
    expect(taxAmount(59.99, 0)).toBe(0);
    expect(taxAmount(59.99, -10)).toBe(0);
  });
});

describe('priceWithTax', () => {
  it('adds the tax to the price', () => {
    expect(priceWithTax(119.97, 2.5)).toBe(122.97);
  });
});

describe('lineTotal', () => {
  it('multiplies the discounted unit price by the quantity', () => {
    expect(lineTotal({ unitPrice: 59.99, quantity: 3, discount: 20 })).toBe(143.97);
  });

  it('uses the full price without a discount', () => {
    expect(lineTotal({ unitPrice: 19.99, quantity: 2, discount: null })).toBe(39.98);
  });
});

describe('orderTotals', () => {
  it('matches the cart from the mock-up', () => {
    const lines = [
      { unitPrice: 59.99, quantity: 1, discount: 0 },
      { unitPrice: 19.99, quantity: 1, discount: 0 },
      { unitPrice: 39.99, quantity: 1, discount: 0 },
    ];

    expect(orderTotals(lines, 2.5)).toEqual({
      itemCount: 3,
      subtotal: 119.97,
      taxes: 3,
      total: 122.97,
    });
  });

  it('counts quantities and discounts', () => {
    const lines = [
      { unitPrice: 100, quantity: 2, discount: 50 },
      { unitPrice: 10, quantity: 3, discount: 0 },
    ];

    expect(orderTotals(lines, 10)).toEqual({ itemCount: 5, subtotal: 130, taxes: 13, total: 143 });
  });

  it('is zero for an empty cart', () => {
    expect(orderTotals([], 2.5)).toEqual({ itemCount: 0, subtotal: 0, taxes: 0, total: 0 });
  });
});

describe('unitPriceBeforeDiscount', () => {
  it('restores the original unit price from a discounted line total', () => {
    expect(unitPriceBeforeDiscount(38.9935, 1, 35)).toBe(59.99);
    expect(unitPriceBeforeDiscount(95.984, 2, 20)).toBe(59.99);
  });

  it('returns the unit price when there is no discount', () => {
    expect(unitPriceBeforeDiscount(39.98, 2, null)).toBe(19.99);
  });

  it('is zero when the price cannot be restored', () => {
    expect(unitPriceBeforeDiscount(0, 1, 100)).toBe(0);
    expect(unitPriceBeforeDiscount(10, 0, 0)).toBe(0);
  });
});
