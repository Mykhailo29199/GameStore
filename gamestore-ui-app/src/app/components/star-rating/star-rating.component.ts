import { Component, input, output } from '@angular/core';
import { TranslationKey } from '../../core/i18n/dictionaries';
import { TranslatePipe } from '../../core/i18n/translate.pipe';

const STAR_COUNT = 5;

@Component({
  selector: 'app-star-rating',
  imports: [TranslatePipe],
  templateUrl: './star-rating.component.html',
  styleUrl: './star-rating.component.css',
})
export class StarRatingComponent {
  readonly value = input(0);
  readonly label = input<TranslationKey>('rating.default');
  readonly readonly = input(false);
  readonly valueChange = output<number>();

  protected readonly stars = Array.from({ length: STAR_COUNT }, (_, index) => index + 1);

  protected select(star: number): void {
    this.valueChange.emit(star === this.value() ? 0 : star);
  }
}
