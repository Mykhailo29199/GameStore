import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { AppComponent } from './app.component';

describe('AppComponent', () => {
  let element: HTMLElement;

  beforeEach(async () => {
    localStorage.clear();
    await TestBed.configureTestingModule({
      imports: [AppComponent],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    const fixture = TestBed.createComponent(AppComponent);
    await fixture.whenStable();
    element = fixture.nativeElement as HTMLElement;
  });

  it('renders the header, the shared widgets and the footer', () => {
    expect(element.querySelector('app-header h1')?.textContent).toContain('What Can Make Your');
    expect(element.querySelectorAll('app-site-widgets .widget')).toHaveLength(3);
    expect(element.querySelector('app-footer')).not.toBeNull();
  });

  it('shows the main navigation from the mock-up', () => {
    const labels = Array.from(element.querySelectorAll('app-header .nav__link')).map((link) =>
      link.textContent?.trim(),
    );

    expect(labels).toEqual(['Games', 'Genres', 'Publishers', 'Orders', 'Cart', 'Login']);
  });

  it('keeps only the site navigation links active in the footer', () => {
    const links = Array.from(element.querySelectorAll<HTMLAnchorElement>('app-footer a'));

    expect(links.map((link) => link.getAttribute('href'))).toEqual([
      '/games',
      '/genres',
      '/publishers',
      '/orders',
      '/basket',
    ]);
  });
});
