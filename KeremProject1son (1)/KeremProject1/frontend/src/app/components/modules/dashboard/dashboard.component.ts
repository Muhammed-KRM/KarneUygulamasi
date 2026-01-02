import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { AnimeListService } from '../../../core/services/api/anime-list.service';
import { RecommendationService } from '../../../core/services/api/recommendation.service';
import { StatisticsService } from '../../../core/services/api/statistics.service';
import { ListGeneratorService } from '../../../core/services/api/list-generator.service';
import { AnimeListDto } from '../../../core/models/responses/anime-list-responses.model';
import { RecommendationDto } from '../../../core/models/responses/recommendation-responses.model';
import { UserStatisticsDto } from '../../../core/models/responses/statistics-responses.model';
import { isResponseSuccessful } from '../../../core/utils/api-response.util';
import { normalizeListMode } from '../../../core/utils/list-mode.util';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  lists: AnimeListDto[] = [];
  recommendations: RecommendationDto[] = [];
  trendingLists: any[] = [];
  statistics: UserStatisticsDto | null = null;
  loading = false;
  generating = false;

  constructor(
    private animeListService: AnimeListService,
    private recommendationService: RecommendationService,
    private statisticsService: StatisticsService,
    private listGeneratorService: ListGeneratorService,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.loading = true;
    
    // Listeleri getir
    this.animeListService.getAllLists().subscribe({
      next: (response) => {
        this.loading = false;
        if (isResponseSuccessful(response)) {
          if (response.response && Array.isArray(response.response)) {
            this.lists = response.response.map((list) => normalizeListMode(list));
          } else {
            this.lists = [];
          }
        } else {
          const errorMsg = (response as any).errorMessage || (response as any).message || 'Bilinmeyen hata';
          console.error('Listeler yüklenemedi:', errorMsg);
          this.lists = [];
        }
      },
      error: (error) => {
        this.loading = false;
        console.error('Listeler yüklenirken hata:', error);
        this.lists = [];
      }
    });

    // Önerileri getir
    this.recommendationService.getRecommendations(10).subscribe({
      next: (response) => {
        if (isResponseSuccessful(response) && response.response) {
          this.recommendations = Array.isArray(response.response) ? response.response : [];
        } else {
          this.recommendations = [];
        }
      },
      error: (error) => {
        console.error('Öneriler yüklenirken hata:', error);
        this.recommendations = [];
      }
    });

    // Trending listeleri getir
    this.recommendationService.getTrendingLists(1, 5).subscribe({
      next: (response) => {
        if (isResponseSuccessful(response) && response.response) {
          const lists = response.response.lists || [];
          this.trendingLists = lists.map((list: any) =>
            'mode' in list ? normalizeListMode(list as any) : list
          );
        } else {
          this.trendingLists = [];
        }
      },
      error: (error) => {
        console.error('Trending listeler yüklenirken hata:', error);
        this.trendingLists = [];
      }
    });

    // İstatistikleri getir
    this.statisticsService.getMyStatistics().subscribe({
      next: (response) => {
        if (isResponseSuccessful(response) && response.response) {
          this.statistics = response.response;
        } else {
          this.statistics = null;
        }
      },
      error: (error) => {
        console.error('İstatistikler yüklenirken hata:', error);
        this.statistics = null;
      }
    });
  }

  generateByScore() {
    this.generating = true;
    this.listGeneratorService.generateByScore().subscribe({
      next: (response) => {
        this.generating = false;
        console.log('GenerateByScore response:', response);
        
        if (isResponseSuccessful(response) && response.response && response.response.listId) {
          const listId = response.response.listId;
          console.log('Liste oluşturuldu, ID:', listId);
          
          Swal.fire({
            icon: 'success',
            title: 'Başarılı!',
            text: 'Liste oluşturuldu!',
            timer: 2000
          }).then(() => {
            this.router.navigate(['/list/view', listId]);
          });
        } else {
          const errorMsg = (response as any).errorMessage || (response as any).message || 'Liste oluşturulamadı';
          console.error('Liste oluşturma başarısız:', errorMsg, response);
          
          Swal.fire({
            icon: 'error',
            title: 'Hata',
            text: errorMsg
          });
        }
      },
      error: (error) => {
        this.generating = false;
        console.error('GenerateByScore error:', error);
        
        // Detaylı hata mesajı
        let errorMessage = 'Liste oluşturulurken bir hata oluştu';
        if (error.error?.errorMessage) {
          errorMessage = error.error.errorMessage;
        } else if (error.message) {
          errorMessage = error.message;
        }
        
        Swal.fire({
          icon: 'error',
          title: 'Hata',
          text: errorMessage
        });
      }
    });
  }

  generateByYear() {
    this.generating = true;
    this.listGeneratorService.generateByYear().subscribe({
      next: (response) => {
        this.generating = false;
        console.log('GenerateByYear response:', response);
        
        if (isResponseSuccessful(response) && response.response && response.response.listId) {
          const listId = response.response.listId;
          console.log('Liste oluşturuldu, ID:', listId);
          
          Swal.fire({
            icon: 'success',
            title: 'Başarılı!',
            text: 'Liste oluşturuldu!',
            timer: 2000
          }).then(() => {
            this.router.navigate(['/list/view', listId]);
          });
        } else {
          const errorMsg = (response as any).errorMessage || (response as any).message || 'Liste oluşturulamadı';
          console.error('Liste oluşturma başarısız:', errorMsg, response);
          
          Swal.fire({
            icon: 'error',
            title: 'Hata',
            text: errorMsg
          });
        }
      },
      error: (error) => {
        this.generating = false;
        console.error('GenerateByYear error:', error);
        
        let errorMessage = 'Liste oluşturulurken bir hata oluştu';
        if (error.error?.errorMessage) {
          errorMessage = error.error.errorMessage;
        } else if (error.message) {
          errorMessage = error.message;
        }
        
        Swal.fire({
          icon: 'error',
          title: 'Hata',
          text: errorMessage
        });
      }
    });
  }

  generateByGenre() {
    this.router.navigate(['/list/generate']);
  }

  viewList(listId: number) {
    this.router.navigate(['/list/view', listId]);
  }

  editList(listId: number) {
    this.router.navigate(['/list/edit', listId]);
  }

  getListImageUrl(listImageLink: string | null | undefined): string {
    if (!listImageLink) {
      return 'https://via.placeholder.com/400x200/2a2a2a/808080?text=Görsel+Yok';
    }
    
    // Backend'den gelen imageLink direkt kullanılabilir (upload sonrası dönen)
    if (listImageLink.startsWith('http://') || listImageLink.startsWith('https://')) {
      return listImageLink;
    }
    
    // Dosya adı ise, backend'den tam URL almak için endpoint kullanılabilir
    // Şimdilik placeholder döndürüyoruz
    return 'https://via.placeholder.com/400x200/2a2a2a/808080?text=Görsel+Yükleniyor';
  }

  onImageError(event: Event) {
    const img = event.target as HTMLImageElement;
    if (img) {
      img.src = 'https://via.placeholder.com/400x200/2a2a2a/808080?text=Görsel+Yüklenemedi';
    }
  }
}

