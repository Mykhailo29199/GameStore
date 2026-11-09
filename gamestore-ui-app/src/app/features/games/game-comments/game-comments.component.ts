import { Component, computed, effect, inject, input, output, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { PaginationComponent } from '../../../components/pagination/pagination.component';
import { StarRatingComponent } from '../../../components/star-rating/star-rating.component';
import { AuthService } from '../../../core/auth/auth.service';
import { loginUrl } from '../../../core/auth/permission.guard';
import { Permissions } from '../../../core/auth/permissions';
import { countComments } from '../../../core/comments/comment-text';
import { apiErrorMessage } from '../../../core/http/api-error';
import { LanguageService } from '../../../core/i18n/language.service';
import { GameComment, NewComment } from '../../../models/comment.model';
import { CommentService } from '../../../services/comment.service';
import { CommentItemComponent, CommentResponse } from '../comment-item/comment-item.component';
import { TranslatePipe, TranslateValuePipe } from '../../../core/i18n/translate.pipe';

const COMMENTS_PER_PAGE = 5;

@Component({
  selector: 'app-game-comments',
  imports: [
    CommentItemComponent,
    PaginationComponent,
    StarRatingComponent,
    TranslatePipe,
    TranslateValuePipe,
  ],
  templateUrl: './game-comments.component.html',
  styleUrl: './game-comments.component.css',
})
export class GameCommentsComponent {
  private readonly commentService = inject(CommentService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly i18n = inject(LanguageService);

  readonly gameKey = input.required<string>();
  readonly rating = input(0);
  readonly countChange = output<number>();

  protected readonly comments = signal<GameComment[]>([]);
  protected readonly page = signal(1);
  protected readonly formOpen = signal(false);
  protected readonly target = signal<CommentResponse | null>(null);
  protected readonly sending = signal(false);
  protected readonly error = signal('');
  protected readonly banTarget = signal<GameComment | null>(null);
  protected readonly banDurations = signal<string[]>([]);
  protected readonly notice = signal('');

  protected readonly count = computed(() => countComments(this.comments()));
  protected readonly totalPages = computed(() =>
    Math.ceil(this.comments().length / COMMENTS_PER_PAGE),
  );
  protected readonly visible = computed(() => {
    const start = (this.page() - 1) * COMMENTS_PER_PAGE;

    return this.comments().slice(start, start + COMMENTS_PER_PAGE);
  });

  constructor() {
    effect(() => this.load(this.gameKey()));
    effect(() => this.countChange.emit(this.count()));
  }

  protected write(target: CommentResponse | null): void {
    if (!this.auth.hasPermission(Permissions.AddComment)) {
      this.router.navigateByUrl(loginUrl(this.router, this.router.url)).catch(() => false);
      return;
    }

    this.target.set(target);
    this.error.set('');
    this.formOpen.set(true);
  }

  protected cancel(): void {
    this.formOpen.set(false);
    this.target.set(null);
  }

  protected submit(text: string): void {
    const body = text.trim();
    if (!body || this.sending()) {
      return;
    }

    const target = this.target();
    const comment: NewComment = { body, parentId: target?.comment.id, action: target?.action };

    this.sending.set(true);
    this.commentService.addComment(this.gameKey(), comment).subscribe({
      next: () => {
        this.sending.set(false);
        this.cancel();
        this.load(this.gameKey());
      },
      error: (error: unknown) => {
        this.sending.set(false);
        this.error.set(apiErrorMessage(error, this.i18n.translate('comments.sendFailed')));
      },
    });
  }

  protected remove(comment: GameComment): void {
    this.moderate(this.commentService.deleteComment(this.gameKey(), comment.id), '');
  }

  protected askBan(comment: GameComment): void {
    this.notice.set('');
    this.error.set('');
    this.banTarget.set(comment);
    if (this.banDurations().length === 0) {
      this.commentService
        .getBanDurations()
        .subscribe((durations) => this.banDurations.set(durations));
    }
  }

  protected banUser(duration: string): void {
    const user = this.banTarget()?.name;

    if (user && duration) {
      this.moderate(
        this.commentService.banUser(user, duration),
        this.i18n.translate('comments.banned', {
          name: user,
          duration: this.i18n.translateValue('banDuration', duration),
        }),
      );
    }
  }

  private moderate(request: Observable<void>, success: string): void {
    this.error.set('');
    request.subscribe({
      next: () => {
        this.banTarget.set(null);
        this.notice.set(success);
        this.load(this.gameKey());
      },
      error: (error: unknown) =>
        this.error.set(apiErrorMessage(error, this.i18n.translate('comments.actionFailed'))),
    });
  }

  private load(gameKey: string): void {
    this.commentService.getComments(gameKey).subscribe({
      next: (comments) => this.comments.set(comments),
      error: () => this.comments.set([]),
    });
  }
}
