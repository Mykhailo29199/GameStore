import { en } from './en';

export type TranslationKey = keyof typeof en;

/**
 * Every language has every key, so a missing text is found when the project is built.
 */
export type Dictionary = Readonly<Record<TranslationKey, string>>;

export type TranslationParams = Readonly<Record<string, string | number>>;
