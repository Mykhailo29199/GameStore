import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { languageInterceptor } from './language.interceptor';
import { LanguageService } from './language.service';

describe('languageInterceptor', () => {
  let http: HttpClient;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([languageInterceptor])),
        provideHttpClientTesting(),
      ],
    });
    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('sends the selected language with API requests', () => {
    TestBed.inject(LanguageService).use('pl');

    http.get('/api/games').subscribe();

    expect(httpMock.expectOne('/api/games').request.headers.get('Accept-Language')).toBe('pl');
  });

  it('does not change requests to other addresses', () => {
    http.get('/assets/configuration.json').subscribe();

    expect(
      httpMock.expectOne('/assets/configuration.json').request.headers.has('Accept-Language'),
    ).toBe(false);
  });
});
