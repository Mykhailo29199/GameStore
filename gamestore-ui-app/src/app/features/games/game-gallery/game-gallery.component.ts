import { Component, computed, input, signal } from '@angular/core';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';

@Component({
  selector: 'app-game-gallery',
  imports: [TranslatePipe],
  templateUrl: './game-gallery.component.html',
  styleUrl: './game-gallery.component.css',
})
export class GameGalleryComponent {
  readonly images = input.required<string[]>();
  readonly title = input('');

  protected readonly index = signal(0);
  protected readonly current = computed(() => this.images()[this.index()] ?? '');

  protected show(index: number): void {
    const count = this.images().length;

    this.index.set((index + count) % count);
  }
}
