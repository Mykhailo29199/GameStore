export interface GameComment {
  id: string;
  name: string;
  body: string;
  childComments: GameComment[];
}

export type CommentAction = 'Reply' | 'Quote';

export interface NewComment {
  body: string;
  parentId?: string;
  action?: CommentAction;
}
