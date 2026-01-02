import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Tier } from '../../../core/models/entities/tier.model';
import { ListMode } from '../../../core/models/enums/list-mode.enum';
import { AnimeCardComponent } from '../anime-card/anime-card.component';

@Component({
  selector: 'app-tier-card',
  standalone: true,
  imports: [CommonModule, AnimeCardComponent],
  templateUrl: './tier-card.component.html',
  styleUrl: './tier-card.component.scss'
})
export class TierCardComponent {
  @Input() tier!: Tier;
  @Input() mode!: ListMode;
  @Input() editable: boolean = false;
  @Output() onItemClick = new EventEmitter<any>();
  @Output() onItemDelete = new EventEmitter<number>();
  @Output() onTierUpdate = new EventEmitter<Tier>();

  handleItemClick(item: any) {
    this.onItemClick.emit(item);
  }

  handleItemDelete(itemId: number) {
    this.onItemDelete.emit(itemId);
  }
}

