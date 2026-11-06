const COVER_COUNT = 24;
const GALLERY_SIZE = 3;
const HASH_MULTIPLIER = 31;

function hashText(text: string): number {
  let hash = 0;

  for (const char of text) {
    hash = (hash * HASH_MULTIPLIER + (char.codePointAt(0) ?? 0)) % Number.MAX_SAFE_INTEGER;
  }

  return hash;
}

function coverUrl(index: number): string {
  return `games/cover-${String((index % COVER_COUNT) + 1).padStart(2, '0')}.jpg`;
}

export function gameCoverUrl(gameKey: string | null | undefined): string {
  return coverUrl(hashText(gameKey ?? ''));
}

export function gameGalleryUrls(gameKey: string | null | undefined): string[] {
  const first = hashText(gameKey ?? '');

  return Array.from({ length: GALLERY_SIZE }, (_, offset) => coverUrl(first + offset));
}
