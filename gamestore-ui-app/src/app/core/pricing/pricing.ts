const PERCENT = 100;
const CENTS = 100;
const MAX_DISCOUNT = 100;

export function roundMoney(amount: number): number {
  return Math.round((amount + Number.EPSILON) * CENTS) / CENTS;
}

export function hasDiscount(discountPercent: number | null | undefined): boolean {
  return (discountPercent ?? 0) > 0;
}

export function discountedPrice(price: number, discountPercent: number | null | undefined): number {
  const discount = Math.min(Math.max(discountPercent ?? 0, 0), MAX_DISCOUNT);

  return roundMoney(price * (1 - discount / PERCENT));
}

export function taxAmount(price: number, taxPercent: number): number {
  return roundMoney((price * Math.max(taxPercent, 0)) / PERCENT);
}

export function priceWithTax(price: number, taxPercent: number): number {
  return roundMoney(price + taxAmount(price, taxPercent));
}

export interface PricedLine {
  readonly unitPrice: number;
  readonly quantity: number;
  readonly discount: number | null | undefined;
}

export interface OrderTotals {
  readonly itemCount: number;
  readonly subtotal: number;
  readonly taxes: number;
  readonly total: number;
}

export function lineTotal(line: PricedLine): number {
  return roundMoney(discountedPrice(line.unitPrice, line.discount) * line.quantity);
}

export function orderTotals(lines: readonly PricedLine[], taxPercent: number): OrderTotals {
  const subtotal = roundMoney(lines.reduce((sum, line) => sum + lineTotal(line), 0));
  const taxes = taxAmount(subtotal, taxPercent);

  return {
    itemCount: lines.reduce((count, line) => count + line.quantity, 0),
    subtotal,
    taxes,
    total: roundMoney(subtotal + taxes),
  };
}

export function unitPriceBeforeDiscount(
  discountedLineTotal: number,
  quantity: number,
  discountPercent: number | null | undefined,
): number {
  const paidShare = 1 - Math.min(Math.max(discountPercent ?? 0, 0), MAX_DISCOUNT) / PERCENT;

  return quantity > 0 && paidShare > 0 ? roundMoney(discountedLineTotal / quantity / paidShare) : 0;
}
