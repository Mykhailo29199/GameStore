import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslationKey } from '../../core/i18n/dictionaries';
import { TranslatePipe } from '../../core/i18n/translate.pipe';

interface FooterLink {
  readonly label: TranslationKey;
  readonly path: string;
}

@Component({
  selector: 'app-footer',
  imports: [RouterLink, TranslatePipe],
  templateUrl: './footer.component.html',
  styleUrl: './footer.component.css',
})
export class FooterComponent {
  protected readonly siteLinks: readonly FooterLink[] = [
    { label: 'nav.games', path: '/games' },
    { label: 'nav.genres', path: '/genres' },
    { label: 'nav.publishers', path: '/publishers' },
    { label: 'nav.orders', path: '/orders' },
    { label: 'footer.myBucket', path: '/basket' },
  ];

  protected readonly companyItems: readonly TranslationKey[] = [
    'footer.about',
    'footer.awards',
    'footer.reviews',
    'footer.testimonials',
    'footer.contact',
  ];
}
