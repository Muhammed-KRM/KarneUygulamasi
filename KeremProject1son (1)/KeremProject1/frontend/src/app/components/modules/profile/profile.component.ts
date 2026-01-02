import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ActivatedRoute, Router } from '@angular/router';
import { UserService } from '../../../core/services/api/user.service';
import { SocialService } from '../../../core/services/api/social.service';
import { StatisticsService } from '../../../core/services/api/statistics.service';
import { ActivityService } from '../../../core/services/api/activity.service';
import { AnimeListService } from '../../../core/services/api/anime-list.service';
import { AuthService } from '../../../core/services/public/auth.service';
import { UserProfileDto } from '../../../core/models/responses/social-responses.model';
import { UserStatisticsDto } from '../../../core/models/responses/statistics-responses.model';
import { isResponseSuccessful } from '../../../core/utils/api-response.util';
import { normalizeListMode } from '../../../core/utils/list-mode.util';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss'
})
export class ProfileComponent implements OnInit {
  userId!: number;
  profile: UserProfileDto | null = null;
  statistics: UserStatisticsDto | null = null;
  lists: any[] = [];
  loading = false;
  isOwnProfile = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private userService: UserService,
    private socialService: SocialService,
    private statisticsService: StatisticsService,
    private activityService: ActivityService,
    private animeListService: AnimeListService,
    public authService: AuthService
  ) {}

  ngOnInit() {
    this.route.params.subscribe(params => {
      const id = params['id'];
      if (id === 'me') {
        this.userId = this.authService.getUserId() || 0;
        this.isOwnProfile = true;
      } else {
        this.userId = +id;
        this.isOwnProfile = false;
      }
      this.loadProfile();
    });
  }

  loadProfile() {
    this.loading = true;
    this.socialService.getUserProfile(this.userId).subscribe({
      next: (response) => {
        if (isResponseSuccessful(response) && response.response) {
          this.profile = response.response;
        } else {
          const errorMsg = (response as any).errorMessage || (response as any).message || 'Bilinmeyen hata';
          console.error('Profil yüklenemedi:', errorMsg);
          this.profile = null;
        }
        this.loadStatistics();
        this.loadLists();
      },
      error: (error) => {
        this.loading = false;
        console.error('Profil yüklenirken hata:', error);
        this.profile = null;
      }
    });
  }

  loadStatistics() {
    this.statisticsService.getUserStatistics(this.userId).subscribe({
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

  loadLists() {
    if (this.isOwnProfile) {
      // Kendi profilimiz - kendi listelerimizi getir
      this.animeListService.getAllLists().subscribe({
        next: (response) => {
          this.loading = false;
          if (isResponseSuccessful(response)) {
            if (response.response && Array.isArray(response.response)) {
              this.lists = response.response.map((list: any) => normalizeListMode(list));
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
    } else {
      // Başka birinin profili - public listelerini getir
      this.animeListService.getUserPublicLists(this.userId).subscribe({
        next: (response) => {
          this.loading = false;
          if (isResponseSuccessful(response) && response.response) {
            // Response formatı: { lists: [...], userId: number, username: string, totalCount: number }
            this.lists = (response.response.lists || []).map((list: any) => normalizeListMode(list));
          } else {
            const errorMsg = (response as any).errorMessage || (response as any).message || 'Bilinmeyen hata';
            console.error('Public listeler yüklenemedi:', errorMsg);
            this.lists = [];
          }
        },
        error: (error) => {
          this.loading = false;
          console.error('Public listeler yüklenirken hata:', error);
          this.lists = [];
        }
      });
    }
  }

  followUser() {
    if (!this.profile) return;
    this.socialService.followUser({ userId: this.userId }).subscribe({
      next: (response) => {
        if (isResponseSuccessful(response)) {
          if (this.profile) {
            this.profile.isFollowing = !this.profile.isFollowing;
          }
        } else {
          const errorMsg = (response as any).errorMessage || (response as any).message || 'Bilinmeyen hata';
          console.error('Takip işlemi başarısız:', errorMsg);
        }
      },
      error: (error) => {
        console.error('Takip işlemi sırasında hata:', error);
      }
    });
  }

  getProfileAvatar(): string {
    if (this.profile?.userImageLink) {
      return this.profile.userImageLink;
    }

    const name = this.profile?.username ?? 'User';
    const encoded = encodeURIComponent(name);
    return `https://ui-avatars.com/api/?background=1f2937&color=ffffff&name=${encoded}`;
  }

  editList(listId: number, event: Event) {
    event.stopPropagation();
    event.preventDefault();
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

