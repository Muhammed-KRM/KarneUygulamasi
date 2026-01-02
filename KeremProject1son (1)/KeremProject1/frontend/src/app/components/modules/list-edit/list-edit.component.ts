import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ActivatedRoute, Router } from '@angular/router';
import { DragDropModule, CdkDragDrop, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { AnimeListService } from '../../../core/services/api/anime-list.service';
import { SearchService } from '../../../core/services/api/search.service';
import { StatisticsService } from '../../../core/services/api/statistics.service';
import { AuthService } from '../../../core/services/public/auth.service';
import { AnimeListDto, ListStatisticsDto } from '../../../core/models/responses/anime-list-responses.model';
import { 
  SaveListRequest, 
  AddTierRequest, 
  UpdateTierRequest, 
  RemoveTierRequest 
} from '../../../core/models/requests/anime-list-requests.model';
import { AnimeSearchModalComponent } from '../../../core/modals/anime-search-modal/anime-search-modal.component';
import Swal from 'sweetalert2';
import { isResponseSuccessful } from '../../../core/utils/api-response.util';
import { normalizeListMode } from '../../../core/utils/list-mode.util';

@Component({
  selector: 'app-list-edit',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, DragDropModule, AnimeSearchModalComponent, DatePipe],
  templateUrl: './list-edit.component.html',
  styleUrl: './list-edit.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ListEditComponent implements OnInit {
  listId!: number;
  list: AnimeListDto | null = null;
  statistics: ListStatisticsDto | null = null;
  /** Drag-drop bağlantıları için önceden hesaplanmış tier id listesi */
  connectedTierIds: string[] = [];
  loading = false;
  error: string | null = null;
  showSearchModal = false;
  selectedTierId: number | null = null;
  showTierModal = false;
  editingTier: any = null;
  newTierTitle = '';
  newTierColor = '#FFFFFF';
  editingTitle = false;
  editedTitle = '';
  isOwner = false;
  selectedImageFile: File | null = null;
  previewImageUrl: string | null = null;
  uploadingImage = false;
  listImageLink: string | null = null; // API'den gelen liste görseli linki

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private animeListService: AnimeListService,
    private searchService: SearchService,
    private statisticsService: StatisticsService,
    private authService: AuthService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.route.params.subscribe(params => {
      this.listId = +params['id'];
      this.loadList();
      this.loadStatistics();
    });
  }

  loadList() {
    this.loading = true;
    this.error = null;
    this.cdr.markForCheck();
    this.animeListService.getList(this.listId).subscribe({
      next: (response) => {
        this.loading = false;
        console.log('Liste yükleme response (raw):', response);
        console.log('Response type:', typeof response);
        console.log('Response.response:', response.response);
        console.log('Response.response type:', typeof response.response);
        
        if (isResponseSuccessful(response) && response.response) {
          // API response'u kontrol et - belki response.response.list veya direkt response.response
          let listData = response.response;
          
          // Eğer response.response bir object ise ve içinde 'list' property'si varsa
          const responseData = response.response as any;
          if (listData && typeof listData === 'object' && 'list' in listData) {
            listData = (listData as any).list;
            console.log('List data found in response.response.list:', listData);
            
            // listImageLink'i response.response'dan al
            if (responseData.listImageLink) {
              this.listImageLink = responseData.listImageLink;
            }
          } else {
            // Eski format: listImageLink list objesinde olabilir
            if ((listData as any).listImageLink) {
              this.listImageLink = (listData as any).listImageLink;
            } else if (responseData.listImageLink) {
              this.listImageLink = responseData.listImageLink;
            }
          }
          
          // Eğer listData bir array ise (yanlış format), ilk elemanı al
          if (Array.isArray(listData)) {
            console.warn('Response.response is an array, taking first element');
            if (listData.length > 0) {
              listData = listData[0];
            } else {
              console.error('Liste array boş!');
              this.error = 'Liste bulunamadı veya boş.';
              Swal.fire({
                icon: 'error',
                title: 'Hata',
                text: 'Liste bulunamadı veya boş.'
              }).then(() => {
                this.router.navigate(['/dashboard']);
              });
              this.cdr.markForCheck();
              return;
            }
          }
          
          // listData null/undefined kontrolü
          if (!listData) {
            console.error('List data null veya undefined!');
            this.error = 'Liste verisi alınamadı.';
            Swal.fire({
              icon: 'error',
              title: 'Hata',
              text: 'Liste verisi alınamadı.'
            }).then(() => {
              this.router.navigate(['/dashboard']);
            });
            this.cdr.markForCheck();
            return;
          }
          
          this.list = normalizeListMode(listData);
          
          // normalizeListMode sonrası null kontrolü
          if (!this.list) {
            console.error('Liste normalize edilemedi!');
            this.error = 'Liste işlenirken bir hata oluştu.';
            Swal.fire({
              icon: 'error',
              title: 'Hata',
              text: 'Liste işlenirken bir hata oluştu.'
            }).then(() => {
              this.router.navigate(['/dashboard']);
            });
            this.cdr.markForCheck();
            return;
          }
          
          // Başarılı yükleme - error'u temizle
          this.error = null;
          
          this.editedTitle = this.list.title;
          // Drag-drop için bağlantılı tier id'lerini önceden hesapla
          this.connectedTierIds = this.list.tiers?.map(t => this.getTierId(t.id)) || [];
          
          console.log('Normalize edilmiş liste:', this.list);
          console.log('Tier sayısı:', this.list.tiers?.length);
          if (this.list.tiers && this.list.tiers.length > 0) {
            console.log('İlk tier items:', this.list.tiers[0].items?.length);
            console.log('İlk tier:', this.list.tiers[0]);
          } else {
            console.warn('Tier array boş veya undefined!');
          }
          
          // Liste sahibi kontrolü - userId'leri number'a çevirerek karşılaştır
          const userId = this.authService.getUserId();
          const currentUserId = userId !== null ? Number(userId) : null;
          const listUserId = this.list.userId !== undefined && this.list.userId !== null ? Number(this.list.userId) : null;
          
          console.log('Liste sahibi kontrolü:', {
            currentUserId,
            listUserId,
            listUserIdRaw: this.list.userId,
            list: this.list
          });
          
          // Eğer API'den userId geliyorsa kontrol yap, gelmiyorsa sahibi olduğunu varsay
          if (listUserId !== null) {
            // userId API'den geliyor, kontrol yap
            this.isOwner = currentUserId !== null && currentUserId === listUserId;
            
            if (!this.isOwner) {
              Swal.fire({
                icon: 'error',
                title: 'Erişim Reddedildi',
                text: 'Bu listeyi sadece sahibi düzenleyebilir.'
              }).then(() => {
                this.router.navigate(['/list/view', this.listId]);
              });
              return;
            }
          } else {
            // userId API'den gelmiyor, kullanıcı giriş yapmışsa sahibi olduğunu varsay
            // getAllLists zaten kullanıcının kendi listelerini döndürüyor, bu yüzden sahibi olduğunu varsay
            this.isOwner = true;
            console.log('userId API\'den gelmedi, getAllLists kullanıldığı için sahibi olduğu varsayılıyor');
          }
          
          console.log('isOwner son değeri:', this.isOwner);
        } else {
          // Başarısız response - hata mesajını göster
          console.error('Liste yüklenemedi veya response başarısız:', response);
          const errorMessage = (response as any).errorMessage || 'Liste yüklenemedi. Liste bulunamadı veya erişim izniniz yok.';
          this.error = errorMessage;
          Swal.fire({
            icon: 'error',
            title: 'Hata',
            text: errorMessage
          }).then(() => {
            this.router.navigate(['/dashboard']);
          });
        }
        this.cdr.markForCheck();
      },
      error: (error) => {
        this.loading = false;
        console.error('Liste yükleme hatası:', error);
        
        // HTTP error - hata mesajını göster
        const errorMessage = error.error?.errorMessage || error.error?.message || error.message || 'Liste yüklenirken bir hata oluştu. Liste bulunamadı veya erişim izniniz yok.';
        this.error = errorMessage;
        
        Swal.fire({
          icon: 'error',
          title: 'Hata',
          text: errorMessage
        }).then(() => {
          this.router.navigate(['/dashboard']);
        });
        
        this.cdr.markForCheck();
      }
    });
  }

  loadStatistics() {
    this.statisticsService.getListStatistics(this.listId).subscribe({
      next: (response) => {
        if (isResponseSuccessful(response) && response.response) {
          this.statistics = response.response;
          this.cdr.markForCheck();
        }
        // Başarısız olsa bile sessizce devam et (statistics opsiyonel)
      },
      error: (error) => {
        // Statistics hatası kritik değil, sessizce devam et
        console.warn('İstatistikler yüklenemedi:', error);
        this.statistics = null;
        this.cdr.markForCheck();
      }
    });
  }

  saveList() {
    if (!this.list) return;

    this.loading = true;
    
    // Tüm tier'lerdeki rankInTier değerlerini güncelle (güvenlik için)
    this.list.tiers.forEach(tier => {
      tier.items.forEach((item, index) => {
        item.rankInTier = index + 1;
      });
    });

    const request: SaveListRequest = {
      listId: this.listId,
      tiers: this.list.tiers.map(tier => ({
        title: tier.title,
        color: tier.color,
        order: tier.order,
        items: tier.items.map(item => ({
          animeMalId: item.animeMalId,
          rankInTier: item.rankInTier
        }))
      }))
    };

    this.animeListService.saveList(request).subscribe({
      next: (response) => {
        this.loading = false;
        if (isResponseSuccessful(response)) {
          Swal.fire({
            toast: true,
            position: 'top-end',
            icon: 'success',
            title: 'Başarılı',
            text: 'Liste başarıyla kaydedildi!',
            showConfirmButton: false,
            timer: 2000,
            timerProgressBar: true
          });
          // Liste zaten güncel, sadece istatistikleri yenile
          this.loadStatistics();
          this.cdr.markForCheck();
        } else {
          Swal.fire({
            toast: true,
            position: 'top-end',
            icon: 'error',
            title: 'Hata',
            text: 'Liste kaydedilirken bir hata oluştu.',
            showConfirmButton: false,
            timer: 3000
          });
          this.cdr.markForCheck();
        }
      },
      error: (error) => {
        this.loading = false;
        console.error('Liste kaydetme hatası:', error);
        Swal.fire({
          toast: true,
          position: 'top-end',
          icon: 'error',
          title: 'Hata',
          text: 'Liste kaydedilirken bir hata oluştu.',
          showConfirmButton: false,
          timer: 3000
        });
        this.cdr.markForCheck();
      }
    });
  }

  updateTitle() {
    if (!this.editedTitle.trim()) {
      Swal.fire({
        icon: 'error',
        title: 'Hata',
        text: 'Başlık boş olamaz'
      });
      return;
    }

    this.animeListService.updateListTitle(this.listId, this.editedTitle).subscribe({
      next: (response) => {
        if (isResponseSuccessful(response)) {
          if (this.list) {
            this.list.title = this.editedTitle;
          }
          this.editingTitle = false;
        }
      }
    });
  }

  openAddAnimeModal(tierId: number) {
    this.selectedTierId = tierId;
    this.showSearchModal = true;
  }

  getTierItems(tierId: number): any[] {
    if (!this.list) return [];
    const tier = this.list.tiers.find(t => t.id === tierId);
    return tier ? tier.items : [];
  }

  onItemAdded(addedItem?: any) {
    if (!this.list) {
      // Fallback: Eğer liste yoksa sayfayı yenile
      this.loadList();
      this.loadStatistics();
      return;
    }

    // Optimistic update: Item'ı direkt listeye ekle (sayfa yenilemeden)
    const tier = this.list.tiers.find(t => t.id === this.selectedTierId);
    if (tier && addedItem) {
      // Yeni item oluştur - imageUrl'i garanti et
      const newItem = {
        id: addedItem.id || addedItem.itemId || Date.now(), // Geçici ID
        animeMalId: addedItem.animeMalId,
        title: addedItem.title || '',
        imageUrl: addedItem.imageUrl || addedItem.image || '',
        rankInTier: addedItem.rankInTier || (tier.items.length + 1)
      };
      
      // Item'ı tier'e ekle
      tier.items.push(newItem);
      
      // rankInTier değerlerini güncelle
      tier.items.forEach((item, index) => {
        item.rankInTier = index + 1;
      });
      
      // İstatistikleri güncelle (optimistic)
      if (this.statistics) {
        this.statistics.totalItems = (this.statistics.totalItems || 0) + 1;
      }
      
      // Change detection'ı tetikle (OnPush için kritik)
      this.cdr.markForCheck();
      
      // Eğer imageUrl yoksa, arka planda listeyi yeniden yükle
      if (!newItem.imageUrl) {
        setTimeout(() => {
          this.loadList();
        }, 300);
      }
    } else if (!addedItem) {
      // Item bilgisi yoksa, sadece istatistikleri güncelle
      if (this.statistics) {
        this.statistics.totalItems = (this.statistics.totalItems || 0) + 1;
      }
      // Change detection'ı tetikle
      this.cdr.markForCheck();
      // Arka planda liste güncellemesini yap (kullanıcı görmez)
      setTimeout(() => {
        this.loadList();
        this.loadStatistics();
      }, 500);
    }
  }

  removeItem(itemId: number) {
    // Optimistic update: Item'ı önce listeden kaldır
    let removedItem: any = null;
    if (this.list) {
      for (const tier of this.list.tiers) {
        const itemIndex = tier.items.findIndex(item => item.id === itemId);
        if (itemIndex !== -1) {
          removedItem = tier.items[itemIndex];
          tier.items.splice(itemIndex, 1);
          // rankInTier değerlerini güncelle
          tier.items.forEach((item, index) => {
            item.rankInTier = index + 1;
          });
          break;
        }
      }
      
      // İstatistikleri güncelle (optimistic)
      if (this.statistics) {
        this.statistics.totalItems = Math.max(0, (this.statistics.totalItems || 0) - 1);
      }
    }

    this.animeListService.removeItem(itemId).subscribe({
      next: (response) => {
        if (isResponseSuccessful(response)) {
          // Başarılı - zaten optimistic update yaptık
        } else {
          // Hata durumunda geri al ve sayfayı yenile
          if (removedItem && this.list) {
            const tier = this.list.tiers.find(t => t.items.some(item => item.id === itemId || !t.items.find(i => i.id === removedItem.id)));
            if (tier) {
              tier.items.push(removedItem);
              tier.items.forEach((item, index) => {
                item.rankInTier = index + 1;
              });
            }
          }
          this.loadList();
          this.loadStatistics();
        }
      },
      error: () => {
        // Hata durumunda geri al ve sayfayı yenile
        if (removedItem && this.list) {
          const tier = this.list.tiers.find(t => !t.items.find(i => i.id === removedItem.id));
          if (tier) {
            tier.items.push(removedItem);
            tier.items.forEach((item, index) => {
              item.rankInTier = index + 1;
            });
          }
        }
        this.loadList();
        this.loadStatistics();
      }
    });
  }

  openAddTierModal() {
    this.editingTier = null;
    this.newTierTitle = '';
    this.newTierColor = '#FFFFFF';
    this.showTierModal = true;
  }

  openEditTierModal(tier: any) {
    this.editingTier = tier;
    this.newTierTitle = tier.title;
    this.newTierColor = tier.color;
    this.showTierModal = true;
  }

  saveTier() {
    if (!this.newTierTitle.trim()) {
      Swal.fire({
        icon: 'error',
        title: 'Hata',
        text: 'Tier başlığı gereklidir'
      });
      return;
    }

    if (this.editingTier) {
      // Güncelle
      const request: UpdateTierRequest = {
        tierId: this.editingTier.id,
        title: this.newTierTitle,
        color: this.newTierColor
      };
      this.animeListService.updateTier(request).subscribe({
        next: (response) => {
          if (isResponseSuccessful(response)) {
            this.showTierModal = false;
            this.loadList();
          }
        }
      });
    } else {
      // Yeni tier ekle
      const request: AddTierRequest = {
        listId: this.listId,
        title: this.newTierTitle,
        color: this.newTierColor
      };
      this.animeListService.addTier(request).subscribe({
        next: (response) => {
          if (isResponseSuccessful(response)) {
            this.showTierModal = false;
            this.loadList();
            this.loadStatistics();
          }
        }
      });
    }
  }

  removeTier(tierId: number) {
    Swal.fire({
      title: 'Emin misiniz?',
      text: 'Bu tier\'ı silmek istediğinize emin misiniz? Tier içindeki animeler de silinecek.',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'Evet, Sil',
      cancelButtonText: 'İptal'
    }).then((result) => {
      if (result.isConfirmed) {
        const request: RemoveTierRequest = { tierId };
        this.animeListService.removeTier(request).subscribe({
          next: (response) => {
            if (isResponseSuccessful(response)) {
              Swal.fire('Silindi!', 'Tier silindi.', 'success');
              this.loadList();
              this.loadStatistics();
            }
          }
        });
      }
    });
  }

  convertToFusion() {
    Swal.fire({
      title: 'Emin misiniz?',
      text: 'Liste Fusion moduna çevrilecek. Bu işlem geri alınamaz.',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'Evet, Çevir',
      cancelButtonText: 'İptal'
    }).then((result) => {
      if (result.isConfirmed) {
        this.animeListService.convertToFusion(this.listId).subscribe({
          next: (response) => {
            if (isResponseSuccessful(response)) {
              Swal.fire('Başarılı!', 'Liste Fusion moduna çevrildi.', 'success');
              this.loadList();
            }
          }
        });
      }
    });
  }

  convertToRanked() {
    Swal.fire({
      title: 'Emin misiniz?',
      text: 'Liste Ranked moduna çevrilecek. Tier yapısı kaybolacak.',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'Evet, Çevir',
      cancelButtonText: 'İptal'
    }).then((result) => {
      if (result.isConfirmed) {
        this.animeListService.convertToRanked(this.listId).subscribe({
          next: (response) => {
            if (isResponseSuccessful(response)) {
              Swal.fire('Başarılı!', 'Liste Ranked moduna çevrildi.', 'success');
              this.loadList();
            }
          }
        });
      }
    });
  }

  // TrackBy fonksiyonları - performans optimizasyonu için
  trackByTierId(index: number, tier: any): number {
    return tier.id;
  }

  trackByItemId(index: number, item: any): number {
    return item.id || item.animeMalId || index;
  }

  // Drag and Drop handlers - Sadece DOM'u günceller, API çağrısı yapmaz
  dropItem(event: CdkDragDrop<any[]>, tierId: number) {
    if (!this.list) return;

    const tier = this.list.tiers.find(t => this.getTierId(t.id) === event.container.id);
    if (!tier) return;

    if (event.previousContainer === event.container) {
      // Aynı tier içinde sıralama değişti - sadece DOM'u güncelle
      moveItemInArray(tier.items, event.previousIndex, event.currentIndex);
      // rankInTier değerlerini local olarak güncelle (API çağrısı yok)
      this.recalculateRanks(tier.id);
    } else {
      // Farklı tier'e taşındı
      const previousTier = this.list.tiers.find(t => this.getTierId(t.id) === event.previousContainer.id);
      if (!previousTier) return;

      transferArrayItem(
        event.previousContainer.data,
        event.container.data,
        event.previousIndex,
        event.currentIndex
      );

      // Her iki tier'in rankInTier değerlerini local olarak güncelle (API çağrısı yok)
      this.recalculateRanks(previousTier.id);
      this.recalculateRanks(tier.id);
    }
    
    // Change detection'ı manuel tetikle (OnPush için)
    this.cdr.detectChanges();
    
    // API çağrısı yapılmıyor - sadece "Kaydet" butonuna basıldığında kaydedilecek
  }

  dropRankedItem(event: CdkDragDrop<any[]>) {
    if (!this.list || this.list.mode !== 'Ranked' || this.list.tiers.length === 0) return;

    const tier = this.list.tiers[0];
    // Sadece DOM'u güncelle - API çağrısı yok
    moveItemInArray(tier.items, event.previousIndex, event.currentIndex);
    
    // rankInTier değerlerini local olarak güncelle (API çağrısı yok)
    this.recalculateRanks(tier.id);
    
    // Change detection'ı manuel tetikle (OnPush için)
    this.cdr.detectChanges();
    
    // API çağrısı yapılmıyor - sadece "Kaydet" butonuna basıldığında kaydedilecek
  }

  private recalculateRanks(tierId: number) {
    if (!this.list) return;
    const tier = this.list.tiers.find(t => t.id === tierId);
    if (tier) {
      tier.items.forEach((item, index) => {
        item.rankInTier = index + 1;
      });
    }
  }

  getTierId(tierId: number): string {
    return `tier-${tierId}`;
  }

  getConnectedTierIds(): string[] {
    if (!this.list) return [];
    return this.list.tiers.map(t => this.getTierId(t.id));
  }

  onImageFileSelected(event: any) {
    const file = event.target.files[0];
    if (!file) return;

    // Dosya validasyonu
    const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp'];
    if (!allowedTypes.includes(file.type)) {
      Swal.fire({
        icon: 'error',
        title: 'Hata',
        text: 'Sadece resim dosyaları kabul edilir! (jpg, jpeg, png, gif, webp)'
      });
      return;
    }

    if (file.size > 5 * 1024 * 1024) {
      Swal.fire({
        icon: 'error',
        title: 'Hata',
        text: 'Dosya boyutu 5MB\'dan küçük olmalıdır!'
      });
      return;
    }

    this.selectedImageFile = file;
    const reader = new FileReader();
    reader.onload = (e: any) => {
      this.previewImageUrl = e.target.result;
    };
    reader.readAsDataURL(file);

    // Otomatik yükle
    this.uploadListImage();
  }

  uploadListImage() {
    if (!this.selectedImageFile || !this.listId) return;

    this.uploadingImage = true;
    this.animeListService.uploadListImage(this.listId, this.selectedImageFile).subscribe({
      next: (response) => {
        this.uploadingImage = false;
        if (isResponseSuccessful(response) && response.response) {
          Swal.fire({
            icon: 'success',
            title: 'Başarılı',
            text: 'Liste görseli yüklendi!',
            timer: 2000,
            showConfirmButton: false
          });
          
          // Listeyi yeniden yükle
          this.loadList();
          this.selectedImageFile = null;
          this.previewImageUrl = null;
        } else {
          Swal.fire({
            icon: 'error',
            title: 'Hata',
            text: (response as any).errorMessage || 'Liste görseli yüklenirken bir hata oluştu'
          });
        }
      },
      error: (error) => {
        this.uploadingImage = false;
        Swal.fire({
          icon: 'error',
          title: 'Hata',
          text: error.error?.errorMessage || 'Liste görseli yüklenirken bir hata oluştu'
        });
      }
    });
  }

  removeListImage() {
    Swal.fire({
      title: 'Görseli Kaldır',
      text: 'Liste görselini kaldırmak istediğinizden emin misiniz?',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'Evet, Kaldır',
      cancelButtonText: 'İptal',
      confirmButtonColor: '#d33'
    }).then((result) => {
      if (result.isConfirmed) {
        // Backend'de görsel kaldırma endpoint'i yoksa, boş bir dosya yükleyebiliriz
        // Veya backend'e özel bir endpoint eklenebilir
        // Şimdilik sadece frontend'den kaldırıyoruz
        this.previewImageUrl = null;
        this.listImageLink = null;
      }
    });
  }

  getListImageUrl(listImageLink: string | null | undefined): string {
    // Önce component'teki listImageLink'i kontrol et
    const imageLink = listImageLink || this.listImageLink;
    
    if (!imageLink) {
      return 'https://via.placeholder.com/800x400/2a2a2a/808080?text=Görsel+Yok';
    }
    
    // Backend'den gelen imageLink direkt kullanılabilir (tam URL)
    if (imageLink.startsWith('http://') || imageLink.startsWith('https://')) {
      return imageLink;
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

  goToDashboard() {
    this.router.navigate(['/dashboard']);
  }
}

