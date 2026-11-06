import { ComponentFixture, TestBed } from '@angular/core/testing';
import { StarRatingComponent } from './star-rating.component';

describe('StarRatingComponent', () => {
  let fixture: ComponentFixture<StarRatingComponent>;
  let element: HTMLElement;
  let emitted: number[];

  beforeEach(async () => {
    fixture = TestBed.createComponent(StarRatingComponent);
    element = fixture.nativeElement as HTMLElement;
    emitted = [];
    fixture.componentInstance.valueChange.subscribe((value) => emitted.push(value));
    fixture.componentRef.setInput('value', 3);
    await fixture.whenStable();
  });

  it('highlights as many stars as the value', () => {
    expect(element.querySelectorAll('.star')).toHaveLength(5);
    expect(element.querySelectorAll('.star--on')).toHaveLength(3);
  });

  it('emits the clicked star', () => {
    element.querySelectorAll<HTMLButtonElement>('button')[4].click();

    expect(emitted).toEqual([5]);
  });

  it('clears the rating when the current star is clicked again', () => {
    element.querySelectorAll<HTMLButtonElement>('button')[2].click();

    expect(emitted).toEqual([0]);
  });

  it('has no buttons when it is read-only', async () => {
    fixture.componentRef.setInput('readonly', true);
    await fixture.whenStable();

    expect(element.querySelectorAll('button')).toHaveLength(0);
    expect(element.querySelectorAll('.star--on')).toHaveLength(3);
  });
});
