import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ActivatedRoute, Router } from '@angular/router';
import { ShareService } from '../../../core/services/api/share.service';
import { CopyService } from '../../../core/services/api/copy.service';
import { SocialService } from '../../../core/services/api/social.service';
import { CommentService } from '../../../core/services/api/comment.service';
import { AuthService } from '../../../core/services/public/auth.service';
import { AnimeListDto } from '../../../core/models/responses/anime-list-responses.model';
import { CommentDto } from '../../../core/models/responses/comment-responses.model';
import Swal from 'sweetalert2';
import { isResponseSuccessful } from '../../../core/utils/api-response.util';
import { normalizeListMode } from '../../../core/utils/list-mode.util';

@Component({
  selector: 'app-share-view',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './share-view.component.html',
  styleUrl: './share-view.component.scss'
})
export class ShareViewComponent implements OnInit {
  shareToken: string = '';
  list: AnimeListDto | null = null;
  comments: CommentDto[] = [];
  loading = false;
  newComment = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private shareService: ShareService,
    private copyService: CopyService,
    private socialService: SocialService,
    private commentService: CommentService,
    public authService: AuthService
  ) {}

  ngOnInit() {
    this.route.params.subscribe(params => {
      this.shareToken = params['token'];
      this.loadPublicList();
    });
  }

  loadPublicList() {
    this.loading = true;
    this.shareService.getPublicList(this.shareToken).subscribe({
      next: (response) => {
        this.loading = false;
        if (isResponseSuccessful(response) && response.response) {
          this.list = normalizeListMode(response.response.list);
          this.loadComments();
        }
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  loadComments() {
    if (this.list) {
      this.commentService.getComments(this.list.id).subscribe({
        next: (response) => {
          if (isResponseSuccessful(response) && response.response) {
            this.comments = response.response;
          }
        }
      });
    }
  }

  copyList() {
    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/login']);
      return;
    }

    if (!this.list) return;

    const newTitle = prompt('Yeni liste adı:');
    if (!newTitle) return;

    this.copyService.copyList(this.list.id, newTitle).subscribe({
      next: (response) => {
        if (isResponseSuccessful(response) && response.response) {
          Swal.fire({
            icon: 'success',
            title: 'Başarılı',
            text: 'Liste kopyalandı!'
          }).then(() => {
            if (response.response) {
              this.router.navigate(['/list/edit', response.response.listId]);
            }
          });
        }
      }
    });
  }

  addComment() {
    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/login']);
      return;
    }

    if (!this.list || !this.newComment.trim()) return;

    this.commentService.addComment(this.list.id, this.newComment).subscribe({
      next: () => {
        this.newComment = '';
        this.loadComments();
      }
    });
  }
}

