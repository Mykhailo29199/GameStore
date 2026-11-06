import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ConfirmButtonComponent } from './confirm-button.component';

describe('ConfirmButtonComponent', () => {
  let fixture: ComponentFixture<ConfirmButtonComponent>;
  let element: HTMLElement;
  let confirmed: number;

  function buttons(): HTMLButtonElement[] {
    return Array.from(element.querySelectorAll('button'));
  }

  beforeEach(async () => {
    fixture = TestBed.createComponent(ConfirmButtonComponent);
    element = fixture.nativeElement as HTMLElement;
    confirmed = 0;
    fixture.componentInstance.confirmed.subscribe(() => confirmed++);
    await fixture.whenStable();
  });

  it('asks before it reports the action', async () => {
    buttons()[0].click();
    await fixture.whenStable();

    expect(confirmed).toBe(0);
    expect(buttons().map((button) => button.textContent?.trim())).toEqual([
      'Confirm delete',
      'Cancel',
    ]);

    buttons()[0].click();
    await fixture.whenStable();

    expect(confirmed).toBe(1);
    expect(buttons()).toHaveLength(1);
  });

  it('does nothing when the action is cancelled', async () => {
    buttons()[0].click();
    await fixture.whenStable();
    buttons()[1].click();
    await fixture.whenStable();

    expect(confirmed).toBe(0);
    expect(buttons()[0].textContent?.trim()).toBe('Delete');
  });
});
