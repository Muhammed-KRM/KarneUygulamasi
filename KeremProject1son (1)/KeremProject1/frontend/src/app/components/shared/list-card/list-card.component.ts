import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AnimeList } from '../../../core/models/entities/anime-list.model';

@Component({
  selector: 'app-list-card',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './list-card.component.html',
  styleUrl: './list-card.component.scss'
})
export class ListCardComponent {
  @Input() list!: AnimeList;
  @Input() showActions: boolean = true;
  @Output() onClick = new EventEmitter<AnimeList>();
  @Output() onDelete = new EventEmitter<number>();
  @Output() onShare = new EventEmitter<number>();

  handleClick() {
    this.onClick.emit(this.list);
  }

  handleDelete(event: Event) {
    event.stopPropagation();
    this.onDelete.emit(this.list.id);
  }

  handleShare(event: Event) {
    event.stopPropagation();
    this.onShare.emit(this.list.id);
  }
}

