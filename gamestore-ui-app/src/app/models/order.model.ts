import { unitPriceBeforeDiscount } from '../core/pricing/pricing';
import { GameWithIdDTO } from './games/game-filter.model';

export interface OrderLineDTO {
  id: string;
  productId: string;
  price: number;
  quantity: number;
  discount: number | null;
}

export interface OrderLine {
  readonly id: string;
  readonly productId: string;
  readonly gameKey: string;
  readonly gameName: string;
  readonly quantity: number;
  readonly unitPrice: number;
  readonly discount: number;
}

export type OrderStatus = 'Open' | 'Checkout' | 'Paid' | 'Cancelled' | 'Shipped';

export interface Order {
  id: string;
  customerId: string;
  date: string;
  status: OrderStatus;
  sum: number | null;
}

const CLOSED_STATUSES: readonly OrderStatus[] = ['Shipped', 'Cancelled'];

export function canEditOrder(order: Order | null): boolean {
  return order !== null && !CLOSED_STATUSES.includes(order.status);
}

export function canShipOrder(order: Order | null): boolean {
  return order?.status === 'Paid';
}

export interface PaymentMethod {
  imageUrl: string;
  title: string;
  description: string;
}

export interface VisaCard {
  holder: string;
  cardNumber: string;
  monthExpire: number;
  yearExpire: number;
  cvv2: number;
}

const UNKNOWN_GAME = 'Unavailable game';

export function toOrderLine(dto: OrderLineDTO, game: GameWithIdDTO | null): OrderLine {
  const restored = unitPriceBeforeDiscount(dto.price, dto.quantity, dto.discount);

  return {
    id: dto.id,
    productId: dto.productId,
    gameKey: game?.key ?? '',
    gameName: game?.name ?? UNKNOWN_GAME,
    quantity: dto.quantity,
    unitPrice: restored || (game?.price ?? 0),
    discount: dto.discount ?? 0,
  };
}
