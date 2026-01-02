import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { Router } from '@angular/router';
import { AnimeListService } from '../../../core/services/api/anime-list.service';
import { ListMode } from '../../../core/models/enums/list-mode.enum';
import { CreateListRequest } from '../../../core/models/requests/anime-list-requests.model';
import Swal from 'sweetalert2';
import { isResponseSuccessful } from '../../../core/utils/api-response.util';

@Component({
  selector: 'app-list-create',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './list-create.component.html',
  styleUrl: './list-create.component.scss'
})
export class ListCreateComponent {
  title: string = '';
  mode: ListMode = ListMode.Ranked;
  useDefaultTiers: boolean = true;
  customTiers: Array<{ title: string; color: string; order: number }> = [];
  loading = false;

  constructor(
    private animeListService: AnimeListService,
    private router: Router
  ) {}

  addCustomTier() {
    this.customTiers.push({
      title: '',
      color: '#FFFFFF',
      order: this.customTiers.length
    });
  }

  removeCustomTier(index: number) {
    this.customTiers.splice(index, 1);
    this.customTiers.forEach((tier, i) => tier.order = i);
  }

  createList() {
    if (!this.title.trim()) {
      Swal.fire({
        icon: 'error',
        title: 'Hata',
        text: 'Liste başlığı gereklidir'
      });
      return;
    }

    this.loading = true;
    const request: CreateListRequest = {
      title: this.title,
      mode: this.mode,
      tiers: !this.useDefaultTiers && this.customTiers.length > 0 ? this.customTiers : undefined
    };

    this.animeListService.createList(request).subscribe({
      next: (response) => {
        this.loading = false;
        if (isResponseSuccessful(response) && response.response) {
          Swal.fire({
            icon: 'success',
            title: 'Başarılı',
            text: 'Liste oluşturuldu!'
          }).then(() => {
            if (response.response) {
              this.router.navigate(['/list/edit', response.response.listId]);
            }
          });
        }
      },
      error: () => {
        this.loading = false;
      }
    });
  }
}

