import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { GameListComponent } from './game-list.component';

const GAMES_URL = '/api/games';

describe('GameListComponent', () => {
  let fixture: ComponentFixture<GameListComponent>;
  let element: HTMLElement;
  let httpMock: HttpTestingController;

  function flushReferenceData(): void {
    httpMock.expectOne(`${GAMES_URL}/sorting-options`).flush(['Most popular', 'Price ASC']);
    httpMock.expectOne(`${GAMES_URL}/publish-date-options`).flush(['Last week', 'All time']);
    httpMock.expectOne(`${GAMES_URL}/pagination-options`).flush(['10', '20', 'All']);
    httpMock.expectOne('/api/genres').flush([
      { id: 'g2', name: 'Strategy' },
      { id: 'g1', name: 'Action' },
    ]);
    httpMock.expectOne('/api/platforms').flush([{ id: 'p1', type: 'Desktop' }]);
    httpMock.expectOne('/api/publishers').flush([{ id: 'pub1', companyName: 'Valve' }]);
  }

  async function settle(): Promise<void> {
    await vi.advanceTimersByTimeAsync(400);
    await fixture.whenStable();
  }

  beforeEach(async () => {
    vi.useFakeTimers();
    TestBed.configureTestingModule({
      imports: [GameListComponent],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    });
    fixture = TestBed.createComponent(GameListComponent);
    element = fixture.nativeElement as HTMLElement;
    httpMock = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    flushReferenceData();
    await settle();
  });

  afterEach(() => {
    httpMock.verify();
    vi.useRealTimers();
  });

  it('shows games with a discounted price and sorted genre filters', async () => {
    httpMock.expectOne((request) => request.url === GAMES_URL).flush({
      games: [
        { id: '1', key: 'ace-combat', name: 'Ace Combat 7', price: 60, discount: 25 },
        { id: '2', key: 'vane', name: 'Vane', price: 24.99, discount: 0 },
      ],
      totalPages: 3,
      currentPage: 1,
    });
    await settle();

    const cards = element.querySelectorAll('app-game-card');
    const genres = Array.from(element.querySelectorAll('fieldset'))
      .find((group) => group.querySelector('legend')?.textContent === 'Genre')
      ?.textContent?.replaceAll(/\s+/g, ' ');

    expect(cards).toHaveLength(2);
    expect(cards[0].querySelector('s')?.textContent).toBe('$60.00');
    expect(cards[0].querySelector('strong')?.textContent).toBe('$45.00');
    expect(cards[1].querySelector('s')).toBeNull();
    expect(genres).toContain('Action Strategy');
    expect(element.querySelectorAll('.pagination__page')).toHaveLength(3);
  });

  it('reloads from the first page when a filter changes', async () => {
    httpMock
      .expectOne((request) => request.url === GAMES_URL)
      .flush({ games: [], totalPages: 0, currentPage: 1 });
    await settle();

    const search = element.querySelector<HTMLInputElement>('.search__input');
    if (search) {
      search.value = 'combat';
      search.dispatchEvent(new Event('input'));
    }
    await settle();

    const request = httpMock.expectOne((candidate) => candidate.url === GAMES_URL);
    request.flush({ games: [], totalPages: 0, currentPage: 1 });
    await settle();

    expect(request.request.params.get('name')).toBe('combat');
    expect(request.request.params.get('page')).toBe('1');
    expect(element.querySelector('.catalog__message')?.textContent).toContain('No games match');
  });

  it('reports a failed request', async () => {
    httpMock
      .expectOne((request) => request.url === GAMES_URL)
      .flush(null, { status: 500, statusText: 'Server Error' });
    await settle();

    expect(element.querySelector('[role="alert"]')?.textContent).toContain('could not be loaded');
  });
});
