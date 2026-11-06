import { Component, computed, inject, signal } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute } from '@angular/router';
import { Observable, catchError, debounceTime, map, of, switchMap } from 'rxjs';
import { PaginationComponent } from '../../../components/pagination/pagination.component';
import { Genre, Platform, Publisher } from '../../../models/catalog.model';
import {
  FilteredGamesResponseDTO,
  GameFilterDTO,
  GameFilterOptions,
  createDefaultFilter,
} from '../../../models/games/game-filter.model';
import { CatalogService } from '../../../services/catalog.service';
import { GameService } from '../../../services/game.service';
import { GameCardComponent } from '../game-card/game-card.component';
import { GameFiltersComponent } from '../game-filters/game-filters.component';

interface GamesState {
  readonly response: FilteredGamesResponseDTO | null;
  readonly failed: boolean;
}

const FILTER_DEBOUNCE_MS = 300;
const EMPTY_OPTIONS: GameFilterOptions = { sorting: [], publishDates: [], pageSizes: [] };

function orEmpty<T>(source: Observable<T[]>): Observable<T[]> {
  return source.pipe(catchError(() => of<T[]>([])));
}

@Component({
  selector: 'app-game-list',
  imports: [GameCardComponent, GameFiltersComponent, PaginationComponent],
  templateUrl: './game-list.component.html',
  styleUrl: './game-list.component.css',
})
export class GameListComponent {
  private readonly games = inject(GameService);
  private readonly catalog = inject(CatalogService);

  private readonly query = inject(ActivatedRoute).snapshot.queryParamMap;

  protected readonly filter = signal<GameFilterDTO>({
    ...createDefaultFilter(),
    genres: this.query.getAll('genres'),
    platforms: this.query.getAll('platforms'),
    publishers: this.query.getAll('publishers'),
  });

  protected readonly options = toSignal(
    this.games.getFilterOptions().pipe(catchError(() => of(EMPTY_OPTIONS))),
    { initialValue: EMPTY_OPTIONS },
  );

  protected readonly genres = toSignal(
    orEmpty(this.catalog.getGenres()).pipe(
      map((genres: Genre[]) => [...genres].sort((a, b) => a.name.localeCompare(b.name))),
    ),
    { initialValue: [] },
  );

  protected readonly platforms = toSignal(orEmpty<Platform>(this.catalog.getPlatforms()), {
    initialValue: [],
  });

  protected readonly publishers = toSignal(orEmpty<Publisher>(this.catalog.getPublishers()), {
    initialValue: [],
  });

  private readonly state = toSignal(
    toObservable(this.filter).pipe(
      debounceTime(FILTER_DEBOUNCE_MS),
      switchMap((filter) => this.load(filter)),
    ),
    { initialValue: { response: null, failed: false } },
  );

  protected readonly loading = computed(() => !this.state().failed && !this.state().response);
  protected readonly failed = computed(() => this.state().failed);
  protected readonly items = computed(() => this.state().response?.games ?? []);
  protected readonly totalPages = computed(() => this.state().response?.totalPages ?? 0);

  protected applyFilter(change: Partial<GameFilterDTO>): void {
    this.filter.update((filter) => ({ ...filter, page: 1, ...change }));
  }

  private load(filter: GameFilterDTO): Observable<GamesState> {
    return this.games.getGames(filter).pipe(
      map((response) => ({ response, failed: false })),
      catchError(() => of({ response: null, failed: true })),
    );
  }
}
