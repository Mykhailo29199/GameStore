import { Permissions } from '../../core/auth/permissions';
import { TranslationKey } from '../../core/i18n/dictionaries';

export type CatalogKind = 'genres' | 'platforms' | 'publishers';
export type CatalogValues = Record<string, string>;

export interface CatalogField {
  readonly key: string;
  readonly label: TranslationKey;
  readonly control: 'text' | 'textarea' | 'parent';
  readonly required: boolean;
}

export interface CatalogItem {
  readonly id: string;
  readonly values: CatalogValues;
}

export interface CatalogConfig {
  readonly title: TranslationKey;
  readonly addTitle: TranslationKey;
  readonly newTitle: TranslationKey;
  readonly editTitle: TranslationKey;
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
    title: 'catalog.genres.title',
    addTitle: 'catalog.genres.add',
    newTitle: 'catalog.genres.new',
    editTitle: 'catalog.genres.edit',
    listPath: 'genres',
    itemPath: 'genre',
    wrapper: 'genre',
    fields: [
      { key: 'name', label: 'catalog.field.name', control: 'text', required: true },
      { key: 'parent', label: 'catalog.field.parent', control: 'parent', required: false },
    ],
    addPermission: Permissions.AddGenre,
    updatePermission: Permissions.UpdateGenre,
    deletePermission: Permissions.DeleteGenre,
  },
  platforms: {
    title: 'catalog.platforms.title',
    addTitle: 'catalog.platforms.add',
    newTitle: 'catalog.platforms.new',
    editTitle: 'catalog.platforms.edit',
    listPath: 'platforms',
    itemPath: 'platform',
    wrapper: 'platform',
    fields: [{ key: 'type', label: 'catalog.field.type', control: 'text', required: true }],
    addPermission: Permissions.AddPlatform,
    updatePermission: Permissions.UpdatePlatform,
    deletePermission: Permissions.DeletePlatform,
  },
  publishers: {
    title: 'catalog.publishers.title',
    addTitle: 'catalog.publishers.add',
    newTitle: 'catalog.publishers.new',
    editTitle: 'catalog.publishers.edit',
    listPath: 'publishers',
    itemPath: 'publisher',
    wrapper: 'publisher',
    fields: [
      { key: 'companyName', label: 'catalog.field.companyName', control: 'text', required: true },
      { key: 'homePage', label: 'catalog.field.homePage', control: 'text', required: false },
      {
        key: 'description',
        label: 'catalog.field.description',
        control: 'textarea',
        required: false,
      },
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
