import { GameComment } from '../../models/comment.model';

export interface CommentText {
  readonly quote: string;
  readonly text: string;
}

const QUOTE_PREFIX = '> ';
const QUOTE_SEPARATOR = '\n\n';

export function parseCommentBody(body: string): CommentText {
  const separatorIndex = body.indexOf(QUOTE_SEPARATOR);

  if (!body.startsWith(QUOTE_PREFIX) || separatorIndex < 0) {
    return { quote: '', text: body };
  }

  return {
    quote: body.slice(QUOTE_PREFIX.length, separatorIndex),
    text: body.slice(separatorIndex + QUOTE_SEPARATOR.length),
  };
}

export function countComments(comments: readonly GameComment[]): number {
  return comments.reduce((total, comment) => total + 1 + countComments(comment.childComments), 0);
}

export function initialsOf(name: string): string {
  return name
    .split(/\s+/)
    .filter(Boolean)
    .slice(0, 2)
    .map((word) => word.charAt(0).toUpperCase())
    .join('');
}
