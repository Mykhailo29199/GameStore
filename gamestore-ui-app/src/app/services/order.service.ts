import { HttpClient, HttpParams, HttpResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map, switchMap } from 'rxjs';
import { APP_SETTINGS } from '../core/config/app-settings';
import { Order, OrderLine, OrderLineDTO, PaymentMethod, VisaCard } from '../models/order.model';
import { CartService } from './cart.service';

interface PaymentMethodsResponse {
  paymentMethods: PaymentMethod[];
}

@Injectable({ providedIn: 'root' })
export class OrderService {
  private readonly http = inject(HttpClient);
  private readonly cart = inject(CartService);
  private readonly ordersUrl = `${inject(APP_SETTINGS).apiUrl}/orders`;

  getPaymentMethods(): Observable<PaymentMethod[]> {
    return this.http
      .get<PaymentMethodsResponse>(`${this.ordersUrl}/payment-methods`)
      .pipe(map((response) => response.paymentMethods));
  }

  pay(method: string, card: VisaCard | null): Observable<HttpResponse<Blob>> {
    return this.http.post(
      `${this.ordersUrl}/payment`,
      { method, model: card },
      { observe: 'response', responseType: 'blob' },
    );
  }

  getOrders(): Observable<Order[]> {
    return this.http.get<Order[]>(this.ordersUrl);
  }

  getHistory(start: string, end: string): Observable<Order[]> {
    let params = new HttpParams();
    params = start ? params.set('start', start) : params;
    params = end ? params.set('end', end) : params;

    return this.http.get<Order[]>(`${this.ordersUrl}/history`, { params });
  }

  getOrder(id: string): Observable<Order> {
    return this.http.get<Order>(`${this.ordersUrl}/${id}`);
  }

  getOrderLines(id: string): Observable<OrderLine[]> {
    return this.http
      .get<OrderLineDTO[]>(`${this.ordersUrl}/${id}/details`)
      .pipe(switchMap((lines) => this.cart.withGames(lines)));
  }

  updateQuantity(lineId: string, count: number): Observable<void> {
    return this.http.patch<void>(`${this.ordersUrl}/details/${lineId}/quantity`, { count });
  }

  deleteLine(lineId: string): Observable<void> {
    return this.http.delete<void>(`${this.ordersUrl}/details/${lineId}`);
  }

  addGame(orderId: string, gameKey: string): Observable<void> {
    return this.http.post<void>(
      `${this.ordersUrl}/${orderId}/details/${encodeURIComponent(gameKey)}`,
      null,
    );
  }

  ship(orderId: string): Observable<void> {
    return this.http.post<void>(`${this.ordersUrl}/${orderId}/ship`, null);
  }
}
