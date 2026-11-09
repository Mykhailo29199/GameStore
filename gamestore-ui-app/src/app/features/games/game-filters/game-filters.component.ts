import { Component, input, output } from '@angular/core';
import { Genre, Platform, Publisher } from '../../../models/catalog.model';
import { GameFilterDTO, GameFilterOptions } from '../../../models/games/game-filter.model';
import { TranslatePipe, TranslateValuePipe } from '../../../core/i18n/translate.pipe';

type ListKey = 'genres' | 'platforms' | 'publishers';
type PriceKey = 'minPrice' | 'maxPrice';

@Component({
  selector: 'app-game-filters',
  imports: [TranslatePipe, TranslateValuePipe],
  templateUrl: './game-filters.component.html',
  styleUrl: './game-filters.component.css',
})
export class GameFiltersComponent {
  readonly filter = input.required<GameFilterDTO>();
  readonly options = input.required<GameFilterOptions>();
  readonly genres = input.required<Genre[]>();
  readonly platforms = input.required<Platform[]>();
  readonly publishers = input.required<Publisher[]>();
  readonly filterChange = output<Partial<GameFilterDTO>>();

  protected toggle(key: ListKey, id: string, checked: boolean): void {
    const others = this.filter()[key].filter((item) => item !== id);

    this.filterChange.emit({ [key]: checked ? [...others, id] : others });
  }

  protected setPrice(key: PriceKey, text: string): void {
    const price = Number.parseFloat(text);

    this.filterChange.emit({ [key]: Number.isNaN(price) ? null : Math.max(price, 0) });
  }

  protected isSelectedDate(option: string, index: number): boolean {
    const selected = this.filter().datePublishing;

    return selected === '' ? index === this.options().publishDates.length - 1 : selected === option;
  }
}
