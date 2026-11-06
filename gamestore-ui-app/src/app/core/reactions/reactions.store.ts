import { Injectable, signal } from '@angular/core';

type Ratings = Readonly<Record<string, number>>;

@Injectable({ providedIn: 'root' })
export class ReactionsStore {
  private readonly gameRatings = signal<Ratings>({});
  private readonly commentRatings = signal<Ratings>({});
  private readonly likedComments = signal<ReadonlySet<string>>(new Set());

  gameRating(gameKey: string): number {
    return this.gameRatings()[gameKey] ?? 0;
  }

  rateGame(gameKey: string, rating: number): void {
    this.gameRatings.update((ratings) => ({ ...ratings, [gameKey]: rating }));
  }

  commentRating(commentId: string): number {
    return this.commentRatings()[commentId] ?? 0;
  }

  rateComment(commentId: string, rating: number): void {
    this.commentRatings.update((ratings) => ({ ...ratings, [commentId]: rating }));
  }

  isLiked(commentId: string): boolean {
    return this.likedComments().has(commentId);
  }

  toggleLike(commentId: string): void {
    this.likedComments.update((liked) => {
      const next = new Set(liked);

      if (!next.delete(commentId)) {
        next.add(commentId);
      }

      return next;
    });
  }
}
