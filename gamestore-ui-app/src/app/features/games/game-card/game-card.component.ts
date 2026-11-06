import { Component, computed, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PriceComponent } from '../../../components/price/price.component';
import { gameCoverUrl } from '../../../core/games/game-cover';
import { GameWithIdDTO } from '../../../models/games/game-filter.model';

@Component({
  selector: 'app-game-card',
  imports: [RouterLink, PriceComponent],
  templateUrl: './game-card.component.html',
  styleUrl: './game-card.component.css',
})
export class GameCardComponent {
  readonly game = input.required<GameWithIdDTO>();

  protected readonly cover = computed(() => gameCoverUrl(this.game().key));
}
