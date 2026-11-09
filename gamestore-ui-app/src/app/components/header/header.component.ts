import { Component, computed, effect, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { Permissions } from '../../core/auth/permissions';
import { TranslationKey } from '../../core/i18n/dictionaries';
import { LanguageService } from '../../core/i18n/language.service';
import { LANGUAGES } from '../../core/i18n/languages';
import { TranslatePipe } from '../../core/i18n/translate.pipe';
import { CartService } from '../../services/cart.service';

interface NavLink {
  readonly label: TranslationKey;
  readonly path: string;
}

interface ManageLink extends NavLink {
  readonly permission: string;
}

const MANAGE_LINKS: readonly ManageLink[] = [
  { label: 'nav.addGame', path: '/games/new', permission: Permissions.AddGame },
  { label: 'nav.platforms', path: '/platforms', permission: Permissions.AddPlatform },
  { label: 'nav.history', path: '/history', permission: Permissions.ViewOrderHistory },
  { label: 'nav.users', path: '/users', permission: Permissions.ManageUsers },
  { label: 'nav.roles', path: '/roles', permission: Permissions.ManageRoles },
];

@Component({
  selector: 'app-header',
  imports: [RouterLink, RouterLinkActive, TranslatePipe],
  templateUrl: './header.component.html',
  styleUrl: './header.component.css',
})
export class HeaderComponent {
  private readonly router = inject(Router);
  protected readonly auth = inject(AuthService);
  protected readonly cart = inject(CartService);
  protected readonly language = inject(LanguageService);
  protected readonly languages = LANGUAGES;

  protected readonly links: readonly NavLink[] = [
    { label: 'nav.games', path: '/games' },
    { label: 'nav.genres', path: '/genres' },
    { label: 'nav.publishers', path: '/publishers' },
    { label: 'nav.orders', path: '/orders' },
  ];

  protected readonly manageLinks = computed(() => {
    const session = this.auth.session();

    return MANAGE_LINKS.filter((link) => session?.permissions.includes(link.permission));
  });

  constructor() {
    effect(() => {
      if (this.auth.session()?.permissions.includes(Permissions.BuyGame)) {
        this.cart.refreshCount();
      } else {
        this.cart.clearCount();
      }
    });
  }

  protected logout(): void {
    this.auth.logout();
    this.router.navigate(['/']).catch(() => false);
  }
}
