import { Component, computed, input, output } from '@angular/core';
import { pageWindow } from './pagination';

@Component({
  selector: 'app-pagination',
  templateUrl: './pagination.component.html',
  styleUrl: './pagination.component.css',
})
export class PaginationComponent {
  readonly current = input.required<number>();
  readonly total = input.required<number>();
  readonly pageChange = output<number>();

  protected readonly pages = computed(() => pageWindow(this.current(), this.total()));

  protected select(page: number): void {
    if (page >= 1 && page <= this.total() && page !== this.current()) {
      this.pageChange.emit(page);
    }
  }
}
