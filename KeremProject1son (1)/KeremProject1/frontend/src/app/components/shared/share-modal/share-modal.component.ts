import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ShareService } from '../../../core/services/api/share.service';
import { ExportService } from '../../../core/services/api/export.service';
import Swal from 'sweetalert2';
import { isResponseSuccessful } from '../../../core/utils/api-response.util';

@Component({
  selector: 'app-share-modal',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './share-modal.component.html',
  styleUrl: './share-modal.component.scss'
})
export class ShareModalComponent {
  @Input() listId!: number;
  shareUrl: string = '';
  embedCode: string = '';
  showModal = false;

  constructor(
    private shareService: ShareService,
    private exportService: ExportService
  ) {}

  openModal() {
    this.showModal = true;
    this.generateShareLink();
  }

  closeModal() {
    this.showModal = false;
  }

  generateShareLink() {
    this.shareService.generateShareLink({ listId: this.listId }).subscribe({
      next: (response) => {
        if (isResponseSuccessful(response) && response.response) {
          this.shareUrl = response.response.shareUrl;
        }
      }
    });

    this.exportService.getEmbedCode(this.listId).subscribe({
      next: (response) => {
        if (isResponseSuccessful(response) && response.response) {
          this.embedCode = response.response.embedCode;
        }
      }
    });
  }

  copyLink() {
    navigator.clipboard.writeText(this.shareUrl).then(() => {
      Swal.fire({
        icon: 'success',
        title: 'Kopyalandı!',
        text: 'Link panoya kopyalandı'
      });
    });
  }

  copyEmbedCode() {
    navigator.clipboard.writeText(this.embedCode).then(() => {
      Swal.fire({
        icon: 'success',
        title: 'Kopyalandı!',
        text: 'Embed kodu panoya kopyalandı'
      });
    });
  }
}

