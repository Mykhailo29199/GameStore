import { TestBed } from '@angular/core/testing';
import { ReactionsStore } from './reactions.store';

describe('ReactionsStore', () => {
  let store: ReactionsStore;

  beforeEach(() => {
    store = TestBed.inject(ReactionsStore);
  });

  it('starts without ratings and likes', () => {
    expect(store.gameRating('vane')).toBe(0);
    expect(store.commentRating('c1')).toBe(0);
    expect(store.isLiked('c1')).toBe(false);
  });

  it('keeps a rating per game', () => {
    store.rateGame('vane', 4);
    store.rateGame('megalith', 2);

    expect(store.gameRating('vane')).toBe(4);
    expect(store.gameRating('megalith')).toBe(2);
  });

  it('keeps a rating per comment', () => {
    store.rateComment('c1', 5);
    store.rateComment('c1', 3);

    expect(store.commentRating('c1')).toBe(3);
    expect(store.commentRating('c2')).toBe(0);
  });

  it('toggles a like on and off', () => {
    store.toggleLike('c1');
    expect(store.isLiked('c1')).toBe(true);

    store.toggleLike('c1');
    expect(store.isLiked('c1')).toBe(false);
  });
});
