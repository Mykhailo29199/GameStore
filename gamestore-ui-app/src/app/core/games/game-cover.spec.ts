import { gameCoverUrl, gameGalleryUrls } from './game-cover';

describe('gameCoverUrl', () => {
  it('returns one of the bundled covers', () => {
    expect(gameCoverUrl('ace-combat-7')).toMatch(/^games\/cover-(0[1-9]|1\d|2[0-4])\.jpg$/);
  });

  it('always returns the same cover for the same game', () => {
    expect(gameCoverUrl('eden-tomorrow')).toBe(gameCoverUrl('eden-tomorrow'));
  });

  it('falls back to the first cover without a key', () => {
    expect(gameCoverUrl(undefined)).toBe('games/cover-01.jpg');
    expect(gameCoverUrl('')).toBe('games/cover-01.jpg');
  });
});

describe('gameGalleryUrls', () => {
  it('starts with the game cover and adds two more pictures', () => {
    const gallery = gameGalleryUrls('vane');

    expect(gallery).toHaveLength(3);
    expect(gallery[0]).toBe(gameCoverUrl('vane'));
    expect(new Set(gallery).size).toBe(3);
  });

  it('wraps around after the last cover', () => {
    expect(gameGalleryUrls('')).toEqual([
      'games/cover-01.jpg',
      'games/cover-02.jpg',
      'games/cover-03.jpg',
    ]);
  });
});
