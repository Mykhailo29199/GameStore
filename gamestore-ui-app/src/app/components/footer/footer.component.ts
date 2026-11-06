import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

interface FooterLink {
  readonly label: string;
  readonly path: string;
}

@Component({
  selector: 'app-footer',
  imports: [RouterLink],
  templateUrl: './footer.component.html',
  styleUrl: './footer.component.css',
})
export class FooterComponent {
  protected readonly siteLinks: readonly FooterLink[] = [
    { label: 'Games', path: '/games' },
    { label: 'Genres', path: '/genres' },
    { label: 'Publishers', path: '/publishers' },
    { label: 'Orders', path: '/orders' },
    { label: 'My Bucket', path: '/basket' },
  ];

  protected readonly companyItems: readonly string[] = [
    'About',
    'Awards',
    'Reviews',
    'Testimonials',
    'Contact',
  ];
}
