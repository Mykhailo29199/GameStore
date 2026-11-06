import { Component, input, output } from '@angular/core';

const STAR_COUNT = 5;

@Component({
  selector: 'app-star-rating',
  templateUrl: './star-rating.component.html',
  styleUrl: './star-rating.component.css',
})
export class StarRatingComponent {
  readonly value = input(0);
  readonly label = input('Rating');
  readonly readonly = input(false);
  readonly valueChange = output<number>();

  protected readonly stars = Array.from({ length: STAR_COUNT }, (_, index) => index + 1);

  protected select(star: number): void {
    this.valueChange.emit(star === this.value() ? 0 : star);
  }
}
