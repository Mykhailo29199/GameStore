import { GameComment } from '../../models/comment.model';
import { countComments, initialsOf, parseCommentBody } from './comment-text';

function comment(id: string, childComments: GameComment[] = []): GameComment {
  return { id, name: 'user', body: 'text', childComments };
}

describe('parseCommentBody', () => {
  it('returns plain text without a quote', () => {
    expect(parseCommentBody('Great game')).toEqual({ quote: '', text: 'Great game' });
  });

  it('separates the quoted text from the answer', () => {
    expect(parseCommentBody('> a piece of an art\n\nI agree')).toEqual({
      quote: 'a piece of an art',
      text: 'I agree',
    });
  });

  it('keeps a body that only looks like a quote as text', () => {
    expect(parseCommentBody('> not a quote')).toEqual({ quote: '', text: '> not a quote' });
  });
});

describe('countComments', () => {
  it('counts replies on every level', () => {
    const tree = [comment('1', [comment('2', [comment('3')])]), comment('4')];

    expect(countComments(tree)).toBe(4);
  });

  it('is zero without comments', () => {
    expect(countComments([])).toBe(0);
  });
});

describe('initialsOf', () => {
  it.each([
    ['John Doe', 'JD'],
    ['admin', 'A'],
    ['  alex   mill  junior ', 'AM'],
    ['', ''],
  ])('turns "%s" into "%s"', (name, expected) => {
    expect(initialsOf(name)).toBe(expected);
  });
});
