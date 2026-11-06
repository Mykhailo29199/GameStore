import { Component, computed, effect, inject, input, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { Router, RouterLink } from '@angular/router';
import { catchError, forkJoin, of } from 'rxjs';
import { apiErrorMessage } from '../../../core/http/api-error';
import { TranslationKey } from '../../../core/i18n/dictionaries';
import { LanguageService } from '../../../core/i18n/language.service';
import { Genre, Platform, Publisher } from '../../../models/catalog.model';
import { GameWithIdDTO, SaveGameRequest } from '../../../models/games/game-filter.model';
import { CatalogService } from '../../../services/catalog.service';
import { GameService } from '../../../services/game.service';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';
import { CreateInEnglishComponent } from '../../../components/create-in-english/create-in-english.component';

export interface GameFormFields {
  readonly name: string;
  readonly key: string;
  readonly description: string;
  readonly price: string;
  readonly unitInStock: string;
  readonly discount: string;
  readonly publisher: string;
}

type SelectionKey = 'genres' | 'platforms';

const MAX_DISCOUNT = 100;

/**
 * Returns the key of the problem text, or an empty string when the game is valid.
 */
export function validateGame(fields: GameFormFields): TranslationKey | '' {
  const price = Number(fields.price);
  const stock = Number(fields.unitInStock);
  const discount = Number(fields.discount);

  if (!fields.name.trim()) {
    return 'common.nameRequired';
  }

  if (!fields.publisher) {
    return 'gameForm.publisherRequired';
  }

  if (Number.isNaN(price) || price < 0 || !Number.isInteger(stock) || stock < 0) {
    return 'gameForm.negative';
  }

  return Number.isInteger(discount) && discount >= 0 && discount <= MAX_DISCOUNT
    ? ''
    : 'gameForm.discountRange';
}

@Component({
  selector: 'app-game-form',
  imports: [RouterLink, TranslatePipe, CreateInEnglishComponent],
  templateUrl: './game-form.component.html',
})
export class GameFormComponent {
  private readonly games = inject(GameService);
  private readonly catalog = inject(CatalogService);
  private readonly router = inject(Router);
  private readonly i18n = inject(LanguageService);

  readonly key = input<string>();

  protected readonly languageName = this.i18n.currentName;
  protected readonly creationBlocked = computed(() => !this.key() && !this.i18n.isDefault());

  protected readonly genres = toSignal(
    this.catalog.getGenres().pipe(catchError(() => of<Genre[]>([]))),
    { initialValue: [] },
  );
  protected readonly platforms = toSignal(
    this.catalog.getPlatforms().pipe(catchError(() => of<Platform[]>([]))),
    { initialValue: [] },
  );
  protected readonly publishers = toSignal(
    this.catalog.getPublishers().pipe(catchError(() => of<Publisher[]>([]))),
    { initialValue: [] },
  );

  protected readonly game = signal<GameWithIdDTO | null>(null);
  protected readonly publisherId = signal('');
  protected readonly selected = signal<Record<SelectionKey, string[]>>({
    genres: [],
    platforms: [],
  });
  protected readonly error = signal('');
  protected readonly saving = signal(false);

  constructor() {
    effect(() => {
      const key = this.key();

      if (key) {
        this.load(key);
      }
    });
  }

  protected toggle(list: SelectionKey, id: string, checked: boolean): void {
    this.selected.update((selected) => {
      const others = selected[list].filter((item) => item !== id);

      return { ...selected, [list]: checked ? [...others, id] : others };
    });
  }

  protected save(fields: GameFormFields): void {
    const problem = validateGame(fields);

    this.error.set(problem ? this.i18n.translate(problem) : '');
    if (problem || this.saving()) {
      return;
    }

    const request: SaveGameRequest = {
      game: {
        id: this.game()?.id,
        name: fields.name.trim(),
        key: fields.key.trim() || undefined,
        description: fields.description.trim(),
        price: Number(fields.price),
        unitInStock: Number(fields.unitInStock),
        discount: Number(fields.discount),
      },
      genres: this.selected().genres,
      platforms: this.selected().platforms,
      publisher: fields.publisher,
    };

    this.saving.set(true);
    this.games.saveGame(request).subscribe({
      next: () => {
        this.router.navigate(['/games']).catch(() => false);
      },
      error: (error: unknown) => {
        this.saving.set(false);
        this.error.set(apiErrorMessage(error, this.i18n.translate('gameForm.saveFailed')));
      },
    });
  }

  private load(key: string): void {
    forkJoin({
      game: this.games.getGame(key),
      genres: this.games.getGameGenres(key).pipe(catchError(() => of<Genre[]>([]))),
      platforms: this.games.getGamePlatforms(key).pipe(catchError(() => of<Platform[]>([]))),
      publisher: this.games.getGamePublisher(key).pipe(catchError(() => of(null))),
    }).subscribe({
      next: ({ game, genres, platforms, publisher }) => {
        this.game.set(game);
        this.publisherId.set(publisher?.id ?? '');
        this.selected.set({
          genres: genres.map((genre) => genre.id),
          platforms: platforms.map((platform) => platform.id),
        });
      },
      error: () => this.error.set(this.i18n.translate('game.notFound')),
    });
  }
}
