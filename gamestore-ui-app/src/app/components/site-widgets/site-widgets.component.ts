import { Component } from '@angular/core';

interface Tweet {
  readonly title: string;
  readonly link: string;
  readonly posted: string;
}

@Component({
  selector: 'app-site-widgets',
  templateUrl: './site-widgets.component.html',
  styleUrl: './site-widgets.component.css',
})
export class SiteWidgetsComponent {
  protected readonly tweets: readonly Tweet[] = [
    { title: 'Devil May Cry 5', link: 'http://goo.gl/eJVm', posted: 'about 1 hour ago reply' },
    {
      title: 'Dead or Alive 6 Special Edition',
      link: 'http://goo.gl/eJVm',
      posted: 'about 1 hour ago reply',
    },
    {
      title: 'Resident Evil 2 Deluxe Edition',
      link: 'http://goo.gl/eJVm',
      posted: 'about 1 hour ago reply',
    },
  ];

  protected readonly posts: readonly string[] = [
    'Proin commodo sodales arcu, a volutpat. Nulla convallis.',
    'Proin commodo sodales arcu, volutpat. Nulla convallis a non.',
    'Curabitur a aliquet turpis, sagittis eleifend sem.',
    'Nam pellentesque finibus felis eget feugiat. Nulla a nisl non veh.',
  ];
}
