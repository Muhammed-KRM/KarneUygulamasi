import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AnimeListService } from '../../services/api/anime-list.service';
import { AddTierRequest, UpdateTierRequest } from '../../models/requests/anime-list-requests.model';
import Swal from 'sweetalert2';
import { isResponseSuccessful } from '../../utils/api-response.util';

@Component({
  selector: 'app-tier-add-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './tier-add-modal.component.html',
  styleUrl: './tier-add-modal.component.scss'
})
export class TierAddModalComponent implements OnInit {
  @Input() listId!: number;
  @Input() tierId?: number; // Düzenleme modu için
  @Input() existingTitle?: string;
  @Input() existingColor?: string;
  @Input() existingOrder?: number;
  @Output() onClose = new EventEmitter<void>();
  @Output() onTierAdded = new EventEmitter<void>();

  title: string = '';
  color: string = '#FFFFFF';
  order: number = 0;
  showModal = true;
  isEditMode = false;

  constructor(private animeListService: AnimeListService) {}

  ngOnInit() {
    if (this.tierId && this.existingTitle) {
      this.isEditMode = true;
      this.title = this.existingTitle;
      this.color = this.existingColor || '#FFFFFF';
      this.order = this.existingOrder || 0;
    }
  }

  save() {
    if (!this.title.trim()) {
      Swal.fire({
        icon: 'error',
        title: 'Hata',
        text: 'Tier ismi gereklidir'
      });
      return;
    }

    if (this.isEditMode && this.tierId) {
      // Güncelleme
      const request: UpdateTierRequest = {
        tierId: this.tierId,
        title: this.title,
        color: this.color,
        order: this.order
      };

      this.animeListService.updateTier(request).subscribe({
        next: (response) => {
          if (isResponseSuccessful(response)) {
            Swal.fire({
              icon: 'success',
              title: 'Başarılı',
              text: 'Tier güncellendi!'
            });
            this.onTierAdded.emit();
            this.close();
          }
        }
      });
    } else {
      // Ekleme
      const request: AddTierRequest = {
        listId: this.listId,
        title: this.title,
        color: this.color,
        order: this.order
      };

      this.animeListService.addTier(request).subscribe({
        next: (response) => {
          if (isResponseSuccessful(response)) {
            Swal.fire({
              icon: 'success',
              title: 'Başarılı',
              text: 'Tier eklendi!'
            });
            this.onTierAdded.emit();
            this.close();
          }
        }
      });
    }
  }

  close() {
    this.showModal = false;
    this.onClose.emit();
  }
}

