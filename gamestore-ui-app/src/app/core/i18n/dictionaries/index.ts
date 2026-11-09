import { LanguageCode } from '../languages';
import { Dictionary, TranslationKey } from './dictionary';
import { en } from './en';
import { pl } from './pl';
import { uk } from './uk';

export type { Dictionary, TranslationKey, TranslationParams } from './dictionary';

export const DICTIONARIES: Readonly<Record<LanguageCode, Dictionary>> = { en, uk, pl };

export function isTranslationKey(key: string): key is TranslationKey {
  return key in en;
}
