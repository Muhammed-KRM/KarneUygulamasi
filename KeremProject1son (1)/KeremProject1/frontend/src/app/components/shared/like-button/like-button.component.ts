import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SocialService } from '../../../core/services/api/social.service';

@Component({
  selector: 'app-like-button',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './like-button.component.html',
  styleUrl: './like-button.component.scss'
})
export class LikeButtonComponent {
  @Input() listId!: number;
  @Input() isLiked: boolean = false;
  @Input() likeCount: number = 0;
  @Output() liked = new EventEmitter<boolean>();

  constructor(private socialService: SocialService) {}

  toggleLike() {
    this.socialService.likeList(this.listId).subscribe({
      next: () => {
        this.isLiked = !this.isLiked;
        this.likeCount += this.isLiked ? 1 : -1;
        this.liked.emit(this.isLiked);
      }
    });
  }
}

