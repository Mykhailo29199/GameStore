import { Component, computed, effect, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { Permissions } from '../../core/auth/permissions';
import { CartService } from '../../services/cart.service';

interface NavLink {
  readonly label: string;
  readonly path: string;
}

interface ManageLink extends NavLink {
  readonly permission: string;
}

const MANAGE_LINKS: readonly ManageLink[] = [
  { label: 'Add Game', path: '/games/new', permission: Permissions.AddGame },
  { label: 'Platforms', path: '/platforms', permission: Permissions.AddPlatform },
  { label: 'History', path: '/history', permission: Permissions.ViewOrderHistory },
  { label: 'Users', path: '/users', permission: Permissions.ManageUsers },
  { label: 'Roles', path: '/roles', permission: Permissions.ManageRoles },
];

@Component({
  selector: 'app-header',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './header.component.html',
  styleUrl: './header.component.css',
})
export class HeaderComponent {
  private readonly router = inject(Router);
  protected readonly auth = inject(AuthService);
  protected readonly cart = inject(CartService);

  protected readonly links: readonly NavLink[] = [
    { label: 'Games', path: '/games' },
    { label: 'Genres', path: '/genres' },
    { label: 'Publishers', path: '/publishers' },
    { label: 'Orders', path: '/orders' },
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
