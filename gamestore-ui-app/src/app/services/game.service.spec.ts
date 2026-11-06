import { createDefaultFilter } from '../models/games/game-filter.model';
import { toGameQueryParams } from './game.service';

describe('toGameQueryParams', () => {
  it('sends only the page and the page size for the default filter', () => {
    expect(toGameQueryParams(createDefaultFilter()).toString()).toBe('page=1&pageCount=20');
  });

  it('repeats list parameters and keeps the option texts as they are', () => {
    const params = toGameQueryParams({
      ...createDefaultFilter(),
      genres: ['g1', 'g2'],
      platforms: ['p1'],
      sort: 'Price ASC',
      datePublishing: 'Last week',
      name: 'combat',
    });

    expect(params.getAll('genres')).toEqual(['g1', 'g2']);
    expect(params.getAll('platforms')).toEqual(['p1']);
    expect(params.get('sort')).toBe('Price ASC');
    expect(params.get('datePublishing')).toBe('Last week');
    expect(params.get('name')).toBe('combat');
    expect(params.has('publishers')).toBe(false);
  });

  it('sends a zero price but skips an empty one', () => {
    const params = toGameQueryParams({ ...createDefaultFilter(), minPrice: 0, maxPrice: null });

    expect(params.get('minPrice')).toBe('0');
    expect(params.has('maxPrice')).toBe(false);
  });
});
