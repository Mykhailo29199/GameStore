import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PriceComponent } from './price.component';

describe('PriceComponent', () => {
  let fixture: ComponentFixture<PriceComponent>;
  let element: HTMLElement;

  beforeEach(() => {
    fixture = TestBed.createComponent(PriceComponent);
    element = fixture.nativeElement as HTMLElement;
  });

  it('shows the crossed old price and the highlighted new price for a discount', async () => {
    fixture.componentRef.setInput('price', 59.99);
    fixture.componentRef.setInput('discount', 20);
    await fixture.whenStable();

    expect(element.querySelector('s')?.textContent).toBe('$59.99');
    expect(element.querySelector('strong')?.textContent).toBe('$47.99');
  });

  it('shows a single price without a discount', async () => {
    fixture.componentRef.setInput('price', 59.99);
    await fixture.whenStable();

    expect(element.querySelector('s')).toBeNull();
    expect(element.textContent?.trim()).toBe('$59.99');
  });
});
