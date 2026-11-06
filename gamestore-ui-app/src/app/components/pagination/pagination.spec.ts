import { pageWindow } from './pagination';

describe('pageWindow', () => {
  it('starts at the first page', () => {
    expect(pageWindow(1, 12)).toEqual([1, 2, 3, 4, 5]);
  });

  it('keeps the current page in the middle', () => {
    expect(pageWindow(6, 12)).toEqual([4, 5, 6, 7, 8]);
  });

  it('stops at the last page', () => {
    expect(pageWindow(12, 12)).toEqual([8, 9, 10, 11, 12]);
  });

  it('shows every page when there are fewer than the window size', () => {
    expect(pageWindow(2, 3)).toEqual([1, 2, 3]);
  });

  it('is empty without pages', () => {
    expect(pageWindow(1, 0)).toEqual([]);
  });
});
