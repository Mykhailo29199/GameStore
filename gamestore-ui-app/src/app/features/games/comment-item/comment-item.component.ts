import { Component, computed, inject, input, output } from '@angular/core';
import { ConfirmButtonComponent } from '../../../components/confirm-button/confirm-button.component';
import { StarRatingComponent } from '../../../components/star-rating/star-rating.component';
import { AuthService } from '../../../core/auth/auth.service';
import { Permissions } from '../../../core/auth/permissions';
import { initialsOf, parseCommentBody } from '../../../core/comments/comment-text';
import { ReactionsStore } from '../../../core/reactions/reactions.store';
import { CommentAction, GameComment } from '../../../models/comment.model';

export interface CommentResponse {
  readonly comment: GameComment;
  readonly action: CommentAction;
}

@Component({
  selector: 'app-comment-item',
  imports: [StarRatingComponent, ConfirmButtonComponent],
  templateUrl: './comment-item.component.html',
  styleUrl: './comment-item.component.css',
})
export class CommentItemComponent {
  private readonly auth = inject(AuthService);
  protected readonly reactions = inject(ReactionsStore);
  protected readonly canDelete = this.auth.hasPermission(Permissions.DeleteComment);
  protected readonly canBan = this.auth.hasPermission(Permissions.BanUser);

  readonly comment = input.required<GameComment>();
  readonly nested = input(false);
  readonly respond = output<CommentResponse>();
  readonly remove = output<GameComment>();
  readonly ban = output<GameComment>();

  protected readonly content = computed(() => parseCommentBody(this.comment().body));
  protected readonly initials = computed(() => initialsOf(this.comment().name));
}
