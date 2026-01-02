import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CommentService } from '../../../core/services/api/comment.service';
import { AuthService } from '../../../core/services/public/auth.service';
import { CommentDto } from '../../../core/models/responses/comment-responses.model';
import { DateFormatPipe } from '../../../core/pipes/date-format.pipe';
import { isResponseSuccessful } from '../../../core/utils/api-response.util';

@Component({
  selector: 'app-comment-section',
  standalone: true,
  imports: [CommonModule, FormsModule, DateFormatPipe],
  templateUrl: './comment-section.component.html',
  styleUrl: './comment-section.component.scss'
})
export class CommentSectionComponent implements OnInit {
  @Input() listId!: number;
  comments: CommentDto[] = [];
  newComment = '';
  loading = false;

  constructor(
    private commentService: CommentService,
    public authService: AuthService
  ) {}

  ngOnInit() {
    this.loadComments();
  }

  loadComments() {
    this.commentService.getComments(this.listId).subscribe({
      next: (response) => {
        if (isResponseSuccessful(response) && response.response) {
          this.comments = response.response;
        }
      }
    });
  }

  addComment() {
    if (!this.newComment.trim()) return;

    this.loading = true;
    this.commentService.addComment(this.listId, this.newComment).subscribe({
      next: () => {
        this.newComment = '';
        this.loading = false;
        this.loadComments();
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  deleteComment(commentId: number) {
    if (confirm('Yorumu silmek istediğinize emin misiniz?')) {
      this.commentService.deleteComment(commentId).subscribe({
        next: () => {
          this.loadComments();
        }
      });
    }
  }
}

