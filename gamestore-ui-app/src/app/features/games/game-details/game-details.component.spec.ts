import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { APP_SETTINGS } from '../../../core/config/app-settings';
import { GameDetailsComponent } from './game-details.component';

const GAME = {
  id: '1',
  key: 'resident-evil-2',
  name: 'Resident Evil 2',
  description: 'Survival horror',
  price: 60,
  unitInStock: 4,
  discount: 25,
};

const COMMENTS = [
  {
    id: 'c1',
    name: 'John Doe',
    body: 'A piece of an art',
    childComments: [
      { id: 'c2', name: 'Richard', body: '> A piece of an art\n\nAgreed', childComments: [] },
    ],
  },
];

describe('GameDetailsComponent', () => {
  let fixture: ComponentFixture<GameDetailsComponent>;
  let element: HTMLElement;
  let httpMock: HttpTestingController;

  function text(selector: string): string {
    return element.querySelector(selector)?.textContent?.replaceAll(/\s+/g, ' ').trim() ?? '';
  }

  async function render(game: object | null): Promise<void> {
    fixture.componentRef.setInput('key', GAME.key);
    fixture.detectChanges();

    const gameRequest = httpMock.expectOne(`/api/game/${GAME.key}`);
    httpMock.expectOne(`/api/genres/game/${GAME.key}`).flush([{ id: 'g1', name: 'Horror' }]);
    httpMock.expectOne(`/api/platforms/game/${GAME.key}`).flush([{ id: 'p1', type: 'PS4' }]);
    httpMock.expectOne(`/api/publisher/game/${GAME.key}`).flush({ id: 'u1', companyName: 'Capcom' });
    if (game) {
      gameRequest.flush(game);
    } else {
      gameRequest.flush(null, { status: 404, statusText: 'Not Found' });
    }
    await fixture.whenStable();

    for (const request of httpMock.match(`/api/games/${GAME.key}/comments`)) {
      request.flush(COMMENTS);
    }
    await fixture.whenStable();
  }

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      imports: [GameDetailsComponent],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: APP_SETTINGS, useValue: { apiUrl: '/api', taxPercent: 10 } },
      ],
    });
    fixture = TestBed.createComponent(GameDetailsComponent);
    element = fixture.nativeElement as HTMLElement;
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('shows the discount and the tax calculated from the configured percent', async () => {
    await render(GAME);

    expect(text('.product__title')).toBe('Resident Evil 2');
    expect(text('.product__price s')).toBe('$60.00');
    expect(text('.product__price strong')).toBe('$45.00');
    expect(text('.product__tax')).toBe('Tax (10%): $4.50 Price with tax: $49.50');
    expect(text('.product__publisher')).toBe('Capcom');
    expect(text('.product__genre')).toBe('Horror');
  });

  it('counts comments of every level and renders quotes', async () => {
    await render(GAME);

    expect(text('.product__comments')).toBe('2 Comments');
    expect(text('.comments__title')).toBe('2 Comments');
    expect(text('.comment__quote')).toBe('A piece of an art');
  });

  it('rates the game on the UI side only', async () => {
    await render(GAME);

    element.querySelectorAll<HTMLButtonElement>('.product__rating .star--button')[3].click();
    await fixture.whenStable();

    expect(element.querySelectorAll('.product__rating .star--on')).toHaveLength(4);
    expect(text('.comments__score')).toBe('4.0');
  });

  it('rates and likes a comment on the UI side only', async () => {
    await render(GAME);

    element.querySelectorAll<HTMLButtonElement>('.comment__rating .star--button')[2].click();
    element.querySelector<HTMLButtonElement>('.comment__actions button')?.click();
    await fixture.whenStable();

    expect(element.querySelectorAll('.comment__rating .star--on')).toHaveLength(3);
    expect(text('.comment__actions button')).toBe('Liked (1)');
  });

  it('reports a missing game', async () => {
    await render(null);

    expect(text('[role="alert"]')).toBe('The game was not found.');
  });
});
