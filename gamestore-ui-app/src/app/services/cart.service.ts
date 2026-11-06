import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { Observable, catchError, forkJoin, map, of, switchMap, tap } from 'rxjs';
import { APP_SETTINGS } from '../core/config/app-settings';
import { OrderLine, OrderLineDTO, toOrderLine } from '../models/order.model';
import { GameService } from './game.service';

@Injectable({ providedIn: 'root' })
export class CartService {
  private readonly http = inject(HttpClient);
  private readonly games = inject(GameService);
  private readonly apiUrl = inject(APP_SETTINGS).apiUrl;
  private readonly count = signal(0);

  readonly itemCount = this.count.asReadonly();

  getLines(): Observable<OrderLine[]> {
    return this.http.get<OrderLineDTO[]>(`${this.apiUrl}/orders/cart`).pipe(
      catchError(() => of<OrderLineDTO[]>([])),
      switchMap((lines) => this.withGames(lines)),
      tap((lines) => this.count.set(lines.reduce((sum, line) => sum + line.quantity, 0))),
    );
  }

  refreshCount(): void {
    this.getLines().subscribe();
  }

  clearCount(): void {
    this.count.set(0);
  }

  addGame(gameKey: string): Observable<void> {
    return this.http
      .post<void>(`${this.gameUrl(gameKey)}/buy`, null)
      .pipe(tap(() => this.count.update((count) => count + 1)));
  }

  removeOne(gameKey: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/orders/cart/${encodeURIComponent(gameKey)}`);
  }

  removePosition(gameKey: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/orders/cart/${encodeURIComponent(gameKey)}/all`);
  }

  withGames(lines: OrderLineDTO[]): Observable<OrderLine[]> {
    if (lines.length === 0) {
      return of([]);
    }

    return forkJoin(
      lines.map((line) =>
        this.games.getGameById(line.productId).pipe(
          catchError(() => of(null)),
          map((game) => toOrderLine(line, game)),
        ),
      ),
    );
  }

  private gameUrl(gameKey: string): string {
    return `${this.apiUrl}/games/${encodeURIComponent(gameKey)}`;
  }
}
