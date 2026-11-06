import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, forkJoin } from 'rxjs';
import { APP_SETTINGS } from '../core/config/app-settings';
import { Genre, Platform, Publisher } from '../models/catalog.model';
import {
  FilteredGamesResponseDTO,
  GameFilterDTO,
  GameFilterOptions,
  GameWithIdDTO,
  SaveGameRequest,
} from '../models/games/game-filter.model';

type FilterValue = string | number | string[] | null;

export function toGameQueryParams(filter: GameFilterDTO): HttpParams {
  let params = new HttpParams();

  for (const [key, value] of Object.entries(filter) as [string, FilterValue][]) {
    const values = Array.isArray(value) ? value : [value];

    for (const item of values) {
      if (item !== null && item !== '') {
        params = params.append(key, item);
      }
    }
  }

  return params;
}

@Injectable({ providedIn: 'root' })
export class GameService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = inject(APP_SETTINGS).apiUrl;
  private readonly gamesUrl = `${this.apiUrl}/games`;

  getGames(filter: GameFilterDTO): Observable<FilteredGamesResponseDTO> {
    return this.http.get<FilteredGamesResponseDTO>(this.gamesUrl, {
      params: toGameQueryParams(filter),
    });
  }

  getFilterOptions(): Observable<GameFilterOptions> {
    return forkJoin({
      sorting: this.http.get<string[]>(`${this.gamesUrl}/sorting-options`),
      publishDates: this.http.get<string[]>(`${this.gamesUrl}/publish-date-options`),
      pageSizes: this.http.get<string[]>(`${this.gamesUrl}/pagination-options`),
    });
  }

  getAllGames(): Observable<GameWithIdDTO[]> {
    return this.http.get<GameWithIdDTO[]>(`${this.gamesUrl}/all`);
  }

  saveGame(request: SaveGameRequest): Observable<void> {
    const url = `${this.apiUrl}/game`;

    return request.game.id ? this.http.put<void>(url, request) : this.http.post<void>(url, request);
  }

  deleteGame(key: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/game/${encodeURIComponent(key)}`);
  }

  getGame(key: string): Observable<GameWithIdDTO> {
    return this.http.get<GameWithIdDTO>(`${this.apiUrl}/game/${encodeURIComponent(key)}`);
  }

  getGameById(id: string): Observable<GameWithIdDTO> {
    return this.http.get<GameWithIdDTO>(`${this.apiUrl}/game/${id}`);
  }

  getGameGenres(key: string): Observable<Genre[]> {
    return this.http.get<Genre[]>(`${this.apiUrl}/genres/game/${encodeURIComponent(key)}`);
  }

  getGamePlatforms(key: string): Observable<Platform[]> {
    return this.http.get<Platform[]>(`${this.apiUrl}/platforms/game/${encodeURIComponent(key)}`);
  }

  getGamePublisher(key: string): Observable<Publisher | null> {
    return this.http.get<Publisher | null>(
      `${this.apiUrl}/publisher/game/${encodeURIComponent(key)}`,
    );
  }
}
