import { Permissions } from '../../core/auth/permissions';

export type CatalogKind = 'genres' | 'platforms' | 'publishers';
export type CatalogValues = Record<string, string>;

export interface CatalogField {
  readonly key: string;
  readonly label: string;
  readonly control: 'text' | 'textarea' | 'parent';
  readonly required: boolean;
}

export interface CatalogItem {
  readonly id: string;
  readonly values: CatalogValues;
}

export interface CatalogConfig {
  readonly title: string;
  readonly singular: string;
  readonly listPath: string;
  readonly itemPath: string;
  readonly wrapper: string;
  readonly fields: readonly CatalogField[];
  readonly addPermission: string;
  readonly updatePermission: string;
  readonly deletePermission: string;
}

export const CATALOG_CONFIGS: Readonly<Record<CatalogKind, CatalogConfig>> = {
  genres: {
    title: 'Genres',
    singular: 'Genre',
    listPath: 'genres',
    itemPath: 'genre',
    wrapper: 'genre',
    fields: [
      { key: 'name', label: 'Name', control: 'text', required: true },
      { key: 'parent', label: 'Parent genre', control: 'parent', required: false },
    ],
    addPermission: Permissions.AddGenre,
    updatePermission: Permissions.UpdateGenre,
    deletePermission: Permissions.DeleteGenre,
  },
  platforms: {
    title: 'Platforms',
    singular: 'Platform',
    listPath: 'platforms',
    itemPath: 'platform',
    wrapper: 'platform',
    fields: [{ key: 'type', label: 'Type', control: 'text', required: true }],
    addPermission: Permissions.AddPlatform,
    updatePermission: Permissions.UpdatePlatform,
    deletePermission: Permissions.DeletePlatform,
  },
  publishers: {
    title: 'Publishers',
    singular: 'Publisher',
    listPath: 'publishers',
    itemPath: 'publisher',
    wrapper: 'publisher',
    fields: [
      { key: 'companyName', label: 'Company name', control: 'text', required: true },
      { key: 'homePage', label: 'Home page', control: 'text', required: false },
      { key: 'description', label: 'Description', control: 'textarea', required: false },
    ],
    addPermission: Permissions.AddPublisher,
    updatePermission: Permissions.UpdatePublisher,
    deletePermission: Permissions.DeletePublisher,
  },
};

type ApiRecord = Record<string, string | null | undefined>;

export function toCatalogItem(record: ApiRecord, fields: readonly CatalogField[]): CatalogItem {
  const values: CatalogValues = {};

  for (const field of fields) {
    values[field.key] = record[field.key] ?? '';
  }

  return { id: record['id'] ?? '', values };
}

export function toCatalogPayload(
  config: CatalogConfig,
  values: CatalogValues,
  id: string | null,
): Record<string, ApiRecord> {
  const record: ApiRecord = id ? { id } : {};

  for (const field of config.fields) {
    const value = (values[field.key] ?? '').trim();
    record[field.key] = value || (field.required ? '' : null);
  }

  return { [config.wrapper]: record };
}

export function missingField(config: CatalogConfig, values: CatalogValues): CatalogField | null {
  return config.fields.find((field) => field.required && !(values[field.key] ?? '').trim()) ?? null;
}
