import { Component, OnInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ActivatedRoute, Router } from '@angular/router';
import { AnimeListService } from '../../../core/services/api/anime-list.service';
import { SocialService } from '../../../core/services/api/social.service';
import { CommentService } from '../../../core/services/api/comment.service';
import { ShareService } from '../../../core/services/api/share.service';
import { ExportService } from '../../../core/services/api/export.service';
import { StatisticsService } from '../../../core/services/api/statistics.service';
import { AuthService } from '../../../core/services/public/auth.service';
import { AnimeListDto, ListStatisticsDto } from '../../../core/models/responses/anime-list-responses.model';
import { CommentDto } from '../../../core/models/responses/comment-responses.model';
import Swal from 'sweetalert2';
import { isResponseSuccessful } from '../../../core/utils/api-response.util';
import { normalizeListMode } from '../../../core/utils/list-mode.util';

@Component({
  selector: 'app-list-view',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, DatePipe],
  templateUrl: './list-view.component.html',
  styleUrl: './list-view.component.scss'
})
export class ListViewComponent implements OnInit {
  listId!: number;
  list: AnimeListDto | null = null;
  statistics: ListStatisticsDto | null = null;
  comments: CommentDto[] = [];
  isLiked = false;
  likeCount = 0;
  loading = false;
  newComment = '';
  showShareModal = false;
  shareUrl = '';
  embedCode = '';
  isOwner = false;
  showTemplateModal = false;
  templateName = '';
  templateDescription = '';
  listImageLink: string | null = null; // API'den gelen liste görseli linki
  exporting = false; // Export işlemi devam ediyor mu?

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private animeListService: AnimeListService,
    private socialService: SocialService,
    private commentService: CommentService,
    private shareService: ShareService,
    private exportService: ExportService,
    private statisticsService: StatisticsService,
    public authService: AuthService
  ) {}

  ngOnInit() {
    this.route.params.subscribe(params => {
      this.listId = +params['id'];
      this.loadList();
      this.loadComments();
      this.loadStatistics();
    });
  }

  loadList() {
    this.loading = true;
    console.log('Liste yükleniyor, ID:', this.listId);
    
    this.animeListService.getList(this.listId).subscribe({
      next: (response) => {
        this.loading = false;
        console.log('Liste yanıtı:', response);
        
        if (isResponseSuccessful(response) && response.response) {
          // Dokümantasyona göre response formatı: { list: {...}, ownerId, ownerUsername, isPublic, isOwner }
          let listData = response.response;
          const responseData = response.response as any;
          
          // Eğer response.response içinde 'list' property'si varsa (yeni format)
          if (listData && typeof listData === 'object' && 'list' in listData) {
            listData = (listData as any).list;
            // Yeni format: ownerId, ownerUsername, isOwner bilgilerini kullan
            if (typeof responseData.isOwner === 'boolean') {
              this.isOwner = responseData.isOwner;
            } else {
              // Fallback: Eski yöntemle kontrol et
              const userId = this.authService.getUserId();
              this.isOwner = userId !== null && listData.userId === userId;
            }
            
            // listImageLink'i response.response'dan al
            if (responseData.listImageLink) {
              this.listImageLink = responseData.listImageLink;
            }
          } else {
            // Eski format: direkt liste objesi
            const userId = this.authService.getUserId();
            this.isOwner = userId !== null && listData.userId === userId;
            
            // listImageLink list objesinde veya response.response'da olabilir
            if ((listData as any).listImageLink) {
              this.listImageLink = (listData as any).listImageLink;
            } else if (responseData.listImageLink) {
              this.listImageLink = responseData.listImageLink;
            }
          }
          
          this.list = normalizeListMode(listData);
          console.log('Liste yüklendi:', this.list);
          console.log('Liste sahibi mi?', this.isOwner);

          // Like bilgileri response.response içinde olabilir (yeni format)
          if (typeof responseData.likeCount === 'number') {
            this.likeCount = responseData.likeCount;
          } else if (typeof (listData as any).likeCount === 'number') {
            this.likeCount = (listData as any).likeCount;
          }

          if (typeof responseData.isLiked === 'boolean') {
            this.isLiked = responseData.isLiked;
          } else if (typeof (listData as any).isLiked === 'boolean') {
            this.isLiked = (listData as any).isLiked;
          }
        } else {
          console.error('Liste yüklenemedi:', response);
        }
      },
      error: (error) => {
        this.loading = false;
        console.error('Liste yükleme hatası:', error);
        
        Swal.fire({
          icon: 'error',
          title: 'Hata',
          text: 'Liste yüklenirken bir hata oluştu. Liste bulunamadı veya erişim izniniz yok.'
        }).then(() => {
          this.router.navigate(['/dashboard']);
        });
      }
    });
  }

  loadComments() {
    this.commentService.getComments(this.listId).subscribe({
      next: (response) => {
        if (isResponseSuccessful(response) && response.response) {
          this.comments = response.response;
        }
      }
    });
  }

  loadStatistics() {
    this.statisticsService.getListStatistics(this.listId).subscribe({
      next: (response) => {
        if (isResponseSuccessful(response) && response.response) {
          this.statistics = response.response;
        }
      }
    });
  }

  toggleLike() {
    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/login']);
      return;
    }

    this.socialService.likeList(this.listId).subscribe({
      next: (response) => {
        if (isResponseSuccessful(response)) {
          this.isLiked = !this.isLiked;
          this.likeCount += this.isLiked ? 1 : -1;
        }
      }
    });
  }

  addComment() {
    if (!this.newComment.trim()) return;

    this.commentService.addComment(this.listId, this.newComment).subscribe({
      next: (response) => {
        if (isResponseSuccessful(response)) {
          this.newComment = '';
          this.loadComments();
        }
      }
    });
  }

  openShareModal() {
    this.showShareModal = true;
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

  copyShareUrl() {
    navigator.clipboard.writeText(this.shareUrl);
    Swal.fire({
      icon: 'success',
      title: 'Kopyalandı!',
      text: 'Paylaşım linki panoya kopyalandı.',
      timer: 2000,
      showConfirmButton: false
    });
  }

  copyEmbedCode() {
    navigator.clipboard.writeText(this.embedCode);
    Swal.fire({
      icon: 'success',
      title: 'Kopyalandı!',
      text: 'Embed kodu panoya kopyalandı.',
      timer: 2000,
      showConfirmButton: false
    });
  }

  getListImageUrl(listImageLink: string | null | undefined): string {
    if (!listImageLink) {
      return 'https://via.placeholder.com/800x400/2a2a2a/808080?text=Görsel+Yok';
    }
    
    // Backend'den gelen imageLink direkt kullanılabilir (tam URL)
    if (listImageLink.startsWith('http://') || listImageLink.startsWith('https://')) {
      return listImageLink;
    }
    
    // Dosya adı ise, backend'den tam URL almak için endpoint kullanılabilir
    // Şimdilik placeholder döndürüyoruz
    return 'https://via.placeholder.com/800x400/2a2a2a/808080?text=Görsel+Yükleniyor';
  }

  onImageError(event: Event) {
    const img = event.target as HTMLImageElement;
    if (img) {
      img.src = 'https://via.placeholder.com/800x400/2a2a2a/808080?text=Görsel+Yüklenemedi';
    }
  }

  exportAsImage() {
    this.exporting = true;
    
    // Loading göstergesi göster
    Swal.fire({
      title: 'Export ediliyor...',
      text: 'Lütfen bekleyin, liste görseli oluşturuluyor.',
      allowOutsideClick: false,
      allowEscapeKey: false,
      showConfirmButton: false,
      didOpen: () => {
        Swal.showLoading();
      }
    });

    this.exportService.exportListAsImage(this.listId).subscribe({
      next: (response) => {
        if (isResponseSuccessful(response) && response.response) {
          const exportData = response.response;
          
          // imageBase64'ü decode et (base64 encoded JSON string)
          try {
            const decodedJson = atob(exportData.imageBase64);
            const listData = JSON.parse(decodedJson);
            
            // Canvas ile görsel oluştur
            this.createImageFromListData(listData, exportData.imageUrl);
            this.exporting = false;
          } catch (error) {
            console.error('Export verisi decode edilemedi:', error);
            this.exporting = false;
            Swal.fire({
              icon: 'error',
              title: 'Hata',
              text: 'Export verisi işlenirken bir hata oluştu.'
            });
          }
        } else {
          this.exporting = false;
          Swal.fire({
            icon: 'error',
            title: 'Hata',
            text: (response as any).errorMessage || 'Export işlemi başarısız oldu.'
          });
        }
      },
      error: (error) => {
        console.error('Export hatası:', error);
        this.exporting = false;
        Swal.fire({
          icon: 'error',
          title: 'Hata',
          text: error.error?.errorMessage || 'Export işlemi sırasında bir hata oluştu.'
        });
      }
    });
  }

  private createImageFromListData(listData: any, imageUrl: string) {
    // Canvas oluştur
    const canvas = document.createElement('canvas');
    const ctx = canvas.getContext('2d');
    
    if (!ctx) {
      this.exporting = false;
      Swal.close(); // Loading modal'ını kapat
      Swal.fire({
        icon: 'error',
        title: 'Hata',
        text: 'Canvas oluşturulamadı.'
      });
      return;
    }

    // Canvas boyutlarını ayarla
    const padding = 40;
    const tierHeight = 150;
    const itemWidth = 120;
    const itemHeight = 140;
    const itemsPerRow = 8;
    
    // Toplam genişlik ve yükseklik hesapla
    let maxTierWidth = 0;
    let totalHeight = padding;
    
    listData.Tiers.forEach((tier: any) => {
      const tierWidth = padding * 2 + Math.min(tier.Items.length, itemsPerRow) * (itemWidth + 10);
      maxTierWidth = Math.max(maxTierWidth, tierWidth);
      const rows = Math.ceil(tier.Items.length / itemsPerRow);
      totalHeight += 50 + (rows * (itemHeight + 10)) + 20; // Tier header + items + margin
    });
    
    canvas.width = Math.max(800, maxTierWidth);
    canvas.height = totalHeight + padding;
    
    // Arka plan
    ctx.fillStyle = '#1a1a1a';
    ctx.fillRect(0, 0, canvas.width, canvas.height);
    
    // Başlık
    ctx.fillStyle = '#ffffff';
    ctx.font = 'bold 32px Arial';
    ctx.textAlign = 'center';
    ctx.fillText(listData.Title, canvas.width / 2, 50);
    
    // Mode bilgisi
    ctx.font = '18px Arial';
    ctx.fillStyle = '#888888';
    ctx.fillText(listData.Mode, canvas.width / 2, 80);
    
    let currentY = 120;
    
    // Tier'ları çiz
    listData.Tiers.forEach((tier: any) => {
      // Tier header
      const tierHeaderHeight = 50;
      ctx.fillStyle = tier.Color || '#333333';
      ctx.fillRect(0, currentY, canvas.width, tierHeaderHeight);
      
      ctx.fillStyle = '#ffffff';
      ctx.font = 'bold 20px Arial';
      ctx.textAlign = 'left';
      ctx.fillText(tier.Title, 20, currentY + 30);
      
      currentY += tierHeaderHeight + 10;
      
      // Tier items
      let currentX = padding;
      let itemsInRow = 0;
      
      tier.Items.forEach((item: any, index: number) => {
        if (itemsInRow >= itemsPerRow) {
          itemsInRow = 0;
          currentX = padding;
          currentY += itemHeight + 10;
        }
        
        // Item container
        ctx.fillStyle = '#2a2a2a';
        ctx.fillRect(currentX, currentY, itemWidth, itemHeight);
        
        // Image placeholder (gerçek görsel yüklenemez, placeholder çiz)
        ctx.fillStyle = '#444444';
        ctx.fillRect(currentX + 5, currentY + 5, itemWidth - 10, itemWidth - 10);
        
        // Title
        ctx.fillStyle = '#ffffff';
        ctx.font = '12px Arial';
        ctx.textAlign = 'center';
        const titleY = currentY + itemWidth + 15;
        const maxTitleWidth = itemWidth - 10;
        const title = item.Title.length > 20 ? item.Title.substring(0, 20) + '...' : item.Title;
        ctx.fillText(title, currentX + itemWidth / 2, titleY);
        
        // Rank (eğer varsa)
        if (item.RankInTier) {
          ctx.fillStyle = '#888888';
          ctx.font = '10px Arial';
          ctx.fillText(`#${item.RankInTier}`, currentX + itemWidth / 2, titleY + 15);
        }
        
        currentX += itemWidth + 10;
        itemsInRow++;
      });
      
      currentY += itemHeight + 20;
    });
    
    // Canvas'ı görsele çevir ve indir
    canvas.toBlob((blob) => {
      if (blob) {
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `${listData.Title.replace(/[^a-z0-9]/gi, '_')}_tier_list.png`;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        URL.revokeObjectURL(url);
        
        this.exporting = false;
        Swal.close(); // Loading modal'ını kapat
        Swal.fire({
          icon: 'success',
          title: 'Başarılı',
          text: 'Liste görseli başarıyla indirildi!',
          timer: 2000,
          showConfirmButton: false
        });
      } else {
        this.exporting = false;
        Swal.close(); // Loading modal'ını kapat
        Swal.fire({
          icon: 'error',
          title: 'Hata',
          text: 'Görsel oluşturulamadı.'
        });
      }
    }, 'image/png');
  }

  openTemplateModal() {
    if (!this.isOwner) {
      Swal.fire({
        icon: 'warning',
        title: 'Uyarı',
        text: 'Sadece liste sahibi şablon oluşturabilir.'
      });
      return;
    }
    this.templateName = this.list?.title || '';
    this.templateDescription = '';
    this.showTemplateModal = true;
  }

  createTemplate() {
    if (!this.templateName || this.templateName.trim() === '') {
      Swal.fire({
        icon: 'warning',
        title: 'Uyarı',
        text: 'Şablon adı gereklidir.'
      });
      return;
    }

    const request = {
      listId: this.listId,
      templateName: this.templateName.trim(),
      description: this.templateDescription.trim() || ''
    };

    this.socialService.createTemplate(request).subscribe({
      next: (response: any) => {
        if (isResponseSuccessful(response)) {
          Swal.fire({
            icon: 'success',
            title: 'Başarılı',
            text: response.errorMessage || 'Şablon başarıyla oluşturuldu!',
            timer: 2000,
            showConfirmButton: false
          });
          this.showTemplateModal = false;
          this.templateName = '';
          this.templateDescription = '';
        } else {
          Swal.fire({
            icon: 'error',
            title: 'Hata',
            text: response.errorMessage || 'Şablon oluşturulurken bir hata oluştu.'
          });
        }
      },
      error: (error) => {
        console.error('Şablon oluşturma hatası:', error);
        Swal.fire({
          icon: 'error',
          title: 'Hata',
          text: 'Şablon oluşturulurken bir hata oluştu.'
        });
      }
    });
  }
}

