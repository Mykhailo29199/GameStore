import { Component } from '@angular/core';
import { TranslatePipe } from '../../core/i18n/translate.pipe';

interface Tweet {
  readonly title: string;
  readonly link: string;
}

@Component({
  selector: 'app-site-widgets',
  imports: [TranslatePipe],
  templateUrl: './site-widgets.component.html',
  styleUrl: './site-widgets.component.css',
})
export class SiteWidgetsComponent {
  protected readonly tweets: readonly Tweet[] = [
    { title: 'Devil May Cry 5', link: 'http://goo.gl/eJVm' },
    {
      title: 'Dead or Alive 6 Special Edition',
      link: 'http://goo.gl/eJVm',
    },
    {
      title: 'Resident Evil 2 Deluxe Edition',
      link: 'http://goo.gl/eJVm',
    },
  ];

  protected readonly posts: readonly string[] = [
    'Proin commodo sodales arcu, a volutpat. Nulla convallis.',
    'Proin commodo sodales arcu, volutpat. Nulla convallis a non.',
    'Curabitur a aliquet turpis, sagittis eleifend sem.',
    'Nam pellentesque finibus felis eget feugiat. Nulla a nisl non veh.',
  ];
}
