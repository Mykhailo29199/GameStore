import { CATALOG_CONFIGS, missingField, toCatalogItem, toCatalogPayload } from './catalog-config';

describe('catalog config', () => {
  const genres = CATALOG_CONFIGS.genres;
  const publishers = CATALOG_CONFIGS.publishers;

  it('reads only the configured fields of an API record', () => {
    const item = toCatalogItem(
      { id: 'g1', name: 'Action', parent: null, extra: 'x' },
      genres.fields,
    );

    expect(item).toEqual({ id: 'g1', values: { name: 'Action', parent: '' } });
  });

  it('wraps a new record the way the API expects', () => {
    expect(toCatalogPayload(genres, { name: ' Racing ', parent: '' }, null)).toEqual({
      genre: { name: 'Racing', parent: null },
    });
  });

  it('adds the id for an update', () => {
    const values = { companyName: 'Valve', homePage: 'https://valve.com', description: '' };

    expect(toCatalogPayload(publishers, values, 'p1')).toEqual({
      publisher: {
        id: 'p1',
        companyName: 'Valve',
        homePage: 'https://valve.com',
        description: null,
      },
    });
  });

  it('finds the first empty required field', () => {
    expect(missingField(publishers, { companyName: '  ' })?.label).toBe(
      'catalog.field.companyName',
    );
    expect(missingField(publishers, { companyName: 'Valve' })).toBeNull();
  });
});
