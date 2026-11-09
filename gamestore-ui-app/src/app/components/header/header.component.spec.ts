import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { HeaderComponent } from './header.component';

@Component({ template: '' })
class EmptyPageComponent {}

describe('HeaderComponent', () => {
  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        provideRouter([{ path: 'games', component: EmptyPageComponent }]),
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });
  });

  it('shows the big banner on the main page and a compact header on other pages', async () => {
    const fixture = TestBed.createComponent(HeaderComponent);
    await fixture.whenStable();
    const element = fixture.nativeElement as HTMLElement;

    expect(element.querySelector('.hero--compact')).toBeNull();
    expect(element.querySelector('.hero__title')).not.toBeNull();

    await TestBed.inject(Router).navigateByUrl('/games');
    await fixture.whenStable();

    expect(element.querySelector('.hero--compact')).not.toBeNull();
    expect(element.querySelector('.hero__title')).toBeNull();
  });
});
