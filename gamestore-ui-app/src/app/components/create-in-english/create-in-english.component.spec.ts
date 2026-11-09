import { TestBed } from '@angular/core/testing';
import { LanguageService } from '../../core/i18n/language.service';
import { CreateInEnglishComponent } from './create-in-english.component';

describe('CreateInEnglishComponent', () => {
  beforeEach(() => localStorage.clear());

  it('explains the rule in the selected language and switches to English', async () => {
    const language = TestBed.inject(LanguageService);
    language.use('uk');

    const fixture = TestBed.createComponent(CreateInEnglishComponent);
    await fixture.whenStable();
    const element = fixture.nativeElement as HTMLElement;

    expect(element.textContent).toContain('Нові записи створюються англійською');
    expect(language.isDefault()).toBe(false);

    element.querySelector('button')?.click();

    expect(language.current()).toBe('en');
    expect(language.isDefault()).toBe(true);
  });
});
