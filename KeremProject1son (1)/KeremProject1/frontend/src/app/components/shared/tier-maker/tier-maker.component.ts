import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Tier } from '../../../core/models/entities/tier.model';
import { ListMode } from '../../../core/models/enums/list-mode.enum';
import { TierCardComponent } from '../tier-card/tier-card.component';
import { DragdropService } from '../../../core/services/api/dragdrop.service';

@Component({
  selector: 'app-tier-maker',
  standalone: true,
  imports: [CommonModule, TierCardComponent],
  templateUrl: './tier-maker.component.html',
  styleUrl: './tier-maker.component.scss'
})
export class TierMakerComponent {
  @Input() tiers: Tier[] = [];
  @Input() mode!: ListMode;
  @Input() editable: boolean = false;
  @Output() onItemMove = new EventEmitter<{ itemId: number; targetTierId: number; newRank: number }>();
  @Output() onItemReorder = new EventEmitter<{ tierId: number; items: Array<{ itemId: number; rankInTier: number }> }>();

  constructor(private dragdropService: DragdropService) {}

  handleItemClick(item: any) {
    // Item click handler
  }

  handleItemDelete(itemId: number) {
    // Item delete handler
  }

  handleItemMove(itemId: number, targetTierId: number, newRank: number) {
    this.dragdropService.moveItem(itemId, targetTierId, newRank).subscribe({
      next: () => {
        this.onItemMove.emit({ itemId, targetTierId, newRank });
      }
    });
  }

  handleItemReorder(tierId: number, items: Array<{ itemId: number; rankInTier: number }>) {
    this.dragdropService.reorderItems(tierId, items).subscribe({
      next: () => {
        this.onItemReorder.emit({ tierId, items });
      }
    });
  }
}

