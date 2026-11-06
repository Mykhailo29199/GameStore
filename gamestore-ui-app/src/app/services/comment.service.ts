import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { APP_SETTINGS } from '../core/config/app-settings';
import { GameComment, NewComment } from '../models/comment.model';

@Injectable({ providedIn: 'root' })
export class CommentService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = inject(APP_SETTINGS).apiUrl;
  private readonly gamesUrl = `${this.apiUrl}/games`;

  getComments(gameKey: string): Observable<GameComment[]> {
    return this.http.get<GameComment[]>(this.commentsUrl(gameKey));
  }

  addComment(gameKey: string, comment: NewComment): Observable<void> {
    const body = {
      comment: { body: comment.body },
      parentId: comment.parentId ?? null,
      action: comment.action ?? null,
    };

    return this.http.post<void>(this.commentsUrl(gameKey), body);
  }

  deleteComment(gameKey: string, commentId: string): Observable<void> {
    return this.http.delete<void>(`${this.commentsUrl(gameKey)}/${commentId}`);
  }

  getBanDurations(): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}/comments/ban/durations`);
  }

  banUser(user: string, duration: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/comments/ban`, { user, duration });
  }

  private commentsUrl(gameKey: string): string {
    return `${this.gamesUrl}/${encodeURIComponent(gameKey)}/comments`;
  }
}
