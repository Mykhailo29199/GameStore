import { TestBed } from '@angular/core/testing';
import { DICTIONARIES } from './dictionaries';
import { LanguageService } from './language.service';

describe('LanguageService', () => {
  let service: LanguageService;

  beforeEach(() => {
    localStorage.clear();
    service = TestBed.inject(LanguageService);
  });

  it('starts in English', () => {
    expect(service.current()).toBe('en');
    expect(service.translate('nav.games')).toBe('Games');
  });

  it('switches the language, remembers it and marks the document', () => {
    service.use('uk');
    TestBed.tick();

    expect(service.translate('nav.games')).toBe('Ігри');
    expect(service.currentName()).toBe('Українська');
    expect(localStorage.getItem('gamestore.language')).toBe('uk');
    expect(document.documentElement.lang).toBe('uk');
  });

  it('reads the remembered language', () => {
    localStorage.setItem('gamestore.language', 'pl');
    TestBed.resetTestingModule();

    expect(TestBed.inject(LanguageService).current()).toBe('pl');
  });

  it('ignores an unknown remembered language', () => {
    localStorage.setItem('gamestore.language', 'de');
    TestBed.resetTestingModule();

    expect(TestBed.inject(LanguageService).current()).toBe('en');
  });

  it('puts the parameters into the text', () => {
    service.use('pl');

    expect(service.translate('orders.order', { id: 42 })).toBe('Zamówienie 42');
  });

  it('translates known API values and keeps unknown ones', () => {
    service.use('uk');

    expect(service.translateValue('status', 'Paid')).toBe('Оплачене');
    expect(service.translateValue('status', 'Lost')).toBe('Lost');
  });

  it('has every text in every language', () => {
    for (const dictionary of Object.values(DICTIONARIES)) {
      expect(Object.values(dictionary).every((text) => text.trim().length > 0)).toBe(true);
    }
  });
});
