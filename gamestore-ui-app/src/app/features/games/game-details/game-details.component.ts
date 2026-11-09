import { CurrencyPipe } from '@angular/common';
import { Component, computed, inject, input, signal } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { Router, RouterLink } from '@angular/router';
import { Observable, catchError, forkJoin, map, of, startWith, switchMap } from 'rxjs';
import { ConfirmButtonComponent } from '../../../components/confirm-button/confirm-button.component';
import { PriceComponent } from '../../../components/price/price.component';
import { StarRatingComponent } from '../../../components/star-rating/star-rating.component';
import { AuthService } from '../../../core/auth/auth.service';
import { loginUrl } from '../../../core/auth/permission.guard';
import { Permissions } from '../../../core/auth/permissions';
import { APP_SETTINGS } from '../../../core/config/app-settings';
import { gameGalleryUrls } from '../../../core/games/game-cover';
import { apiErrorMessage } from '../../../core/http/api-error';
import { LanguageService } from '../../../core/i18n/language.service';
import { discountedPrice, priceWithTax, taxAmount } from '../../../core/pricing/pricing';
import { ReactionsStore } from '../../../core/reactions/reactions.store';
import { Genre, Platform, Publisher } from '../../../models/catalog.model';
import { GameWithIdDTO } from '../../../models/games/game-filter.model';
import { CartService } from '../../../services/cart.service';
import { GameService } from '../../../services/game.service';
import { GameCommentsComponent } from '../game-comments/game-comments.component';
import { GameGalleryComponent } from '../game-gallery/game-gallery.component';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';

interface GameView {
  readonly game: GameWithIdDTO;
  readonly genres: Genre[];
  readonly platforms: Platform[];
  readonly publisher: Publisher | null;
}

interface CartMessage {
  readonly text: string;
  readonly failed: boolean;
}

type ViewState = GameView | 'loading' | 'missing';

@Component({
  selector: 'app-game-details',
  imports: [
    CurrencyPipe,
    RouterLink,
    PriceComponent,
    StarRatingComponent,
    GameGalleryComponent,
    GameCommentsComponent,
    ConfirmButtonComponent,
    TranslatePipe,
  ],
  templateUrl: './game-details.component.html',
  styleUrl: './game-details.component.css',
})
export class GameDetailsComponent {
  private readonly games = inject(GameService);
  private readonly cart = inject(CartService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly i18n = inject(LanguageService);
  protected readonly reactions = inject(ReactionsStore);
  protected readonly taxPercent = inject(APP_SETTINGS).taxPercent;

  protected readonly canEdit = this.auth.hasPermission(Permissions.UpdateGame);
  protected readonly canDelete = this.auth.hasPermission(Permissions.DeleteGame);

  readonly key = input.required<string>();

  private readonly state = toSignal(
    toObservable(this.key).pipe(switchMap((key) => this.load(key))),
    { initialValue: 'loading' as ViewState },
  );

  protected readonly loading = computed(() => this.state() === 'loading');
  protected readonly view = computed(() => {
    const state = this.state();

    return typeof state === 'string' ? null : state;
  });
  protected readonly gallery = computed(() => gameGalleryUrls(this.key()));
  protected readonly finalPrice = computed(() => {
    const game = this.view()?.game;

    return game ? discountedPrice(game.price, game.discount) : 0;
  });
  protected readonly tax = computed(() => taxAmount(this.finalPrice(), this.taxPercent));
  protected readonly totalWithTax = computed(() =>
    priceWithTax(this.finalPrice(), this.taxPercent),
  );
  protected readonly commentCount = signal(0);
  protected readonly adding = signal(false);
  protected readonly cartMessage = signal<CartMessage | null>(null);

  protected addToCart(): void {
    if (!this.auth.hasPermission(Permissions.BuyGame)) {
      this.router.navigateByUrl(loginUrl(this.router, this.router.url)).catch(() => false);
      return;
    }

    this.adding.set(true);
    this.cart.addGame(this.key()).subscribe({
      next: () => this.finishAdding({ text: this.i18n.translate('game.added'), failed: false }),
      error: (error: unknown) =>
        this.finishAdding({
          text: apiErrorMessage(error, this.i18n.translate('game.addFailed')),
          failed: true,
        }),
    });
  }

  protected deleteGame(): void {
    this.games.deleteGame(this.key()).subscribe({
      next: () => {
        this.router.navigate(['/games']).catch(() => false);
      },
      error: (error: unknown) =>
        this.cartMessage.set({
          text: apiErrorMessage(error, this.i18n.translate('game.deleteFailed')),
          failed: true,
        }),
    });
  }

  private finishAdding(message: CartMessage): void {
    this.adding.set(false);
    this.cartMessage.set(message);
  }

  private load(key: string): Observable<ViewState> {
    return forkJoin({
      game: this.games.getGame(key),
      genres: this.games.getGameGenres(key).pipe(catchError(() => of<Genre[]>([]))),
      platforms: this.games.getGamePlatforms(key).pipe(catchError(() => of<Platform[]>([]))),
      publisher: this.games.getGamePublisher(key).pipe(catchError(() => of(null))),
    }).pipe(
      map((view): ViewState => view),
      catchError(() => of<ViewState>('missing')),
      startWith<ViewState>('loading'),
    );
  }
}
