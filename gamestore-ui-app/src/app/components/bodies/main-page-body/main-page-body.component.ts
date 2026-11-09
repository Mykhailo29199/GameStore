import { Component } from '@angular/core';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';

@Component({
  selector: 'app-main-page-body',
  imports: [TranslatePipe],
  templateUrl: './main-page-body.component.html',
  styleUrl: './main-page-body.component.css',
})
export class MainPageBodyComponent {
  protected readonly highlights: readonly string[] = [
    'Proin commodo sodales arcu, a volutpat augue facilisis et. Nam pellentesque finibus felis eget feugiat. Nulla convallis a nisl non vehicula. Aliquam erat volutpat.',
    'Nam pellentesque finibus felis eget feugiat. Nulla convallis a nisl non vehicula. Aliquam erat volutpat.',
    'Proin commodo sodales arcu, a volutpat. Nulla convallis a nisl non vehicula. Aliquam erat volutpat. Praesent nec tincidunt purus. Donec vel erat porta.',
  ];
}
