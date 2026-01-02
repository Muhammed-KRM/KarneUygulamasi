import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { UserService } from '../../../core/services/api/user.service';
import { MalIntegrationService } from '../../../core/services/api/mal-integration.service';
import { AuthService } from '../../../core/services/public/auth.service';
import { ThemeService } from '../../../core/services/public/theme.service';
import { GetUserResponse } from '../../../core/models/responses/user-responses.model';
import Swal from 'sweetalert2';
import { isResponseSuccessful } from '../../../core/utils/api-response.util';

type SettingsTab = 'profile' | 'password' | 'connections' | 'appearance' | 'account';

@Component({
  selector: 'app-profile-settings',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './profile-settings.component.html',
  styleUrl: './profile-settings.component.scss'
})
export class ProfileSettingsComponent implements OnInit {
  activeTab: SettingsTab = 'profile';
  user: GetUserResponse | null = null;
  
  // Profile tab
  username: string = '';
  malUsername: string = '';
  selectedFile: File | null = null;
  previewImage: string | null = null;
  
  // Password tab
  currentPassword: string = '';
  newPassword: string = '';
  confirmPassword: string = '';
  passwordStrength: number = 0;
  passwordStrengthLabel: string = '';
  
  // Appearance tab
  currentTheme: 'dark' | 'light' = 'dark';
  notificationsEnabled: boolean = true;
  emailNotifications: boolean = false;
  
  // Account tab
  deletePassword: string = '';
  hardDelete: boolean = false;
  showDeleteConfirm: boolean = false;
  
  loading = false;
  uploading = false;

  constructor(
    private userService: UserService,
    private malIntegrationService: MalIntegrationService,
    private authService: AuthService,
    private themeService: ThemeService,
    private router: Router
  ) {}

  ngOnInit() {
    console.log('ProfileSettingsComponent initialized');
    this.loadProfile();
    this.currentTheme = this.themeService.getTheme();
    this.themeService.theme$.subscribe(theme => {
      this.currentTheme = theme;
    });
    
    // LocalStorage'dan ayarları yükle
    if (typeof window !== 'undefined') {
      const savedNotifications = localStorage.getItem('notificationsEnabled');
      if (savedNotifications !== null) {
        this.notificationsEnabled = savedNotifications === 'true';
      }
      const savedEmailNotifications = localStorage.getItem('emailNotifications');
      if (savedEmailNotifications !== null) {
        this.emailNotifications = savedEmailNotifications === 'true';
      }
    }
  }

  loadProfile() {
    this.loading = true;
    this.userService.getMyProfile().subscribe({
      next: (response) => {
        this.loading = false;
        if (isResponseSuccessful(response) && response.response) {
          this.user = response.response;
          this.username = response.response.username;
          this.malUsername = response.response.malUsername || '';
        }
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  setTab(tab: SettingsTab) {
    this.activeTab = tab;
  }

  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
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

      this.selectedFile = file;
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.previewImage = e.target.result;
      };
      reader.readAsDataURL(file);
    }
  }

  uploadImage() {
    if (!this.selectedFile) return;

    this.uploading = true;
    this.userService.uploadUserImage(this.selectedFile).subscribe({
      next: (response) => {
        this.uploading = false;
        if (isResponseSuccessful(response)) {
          Swal.fire({
            icon: 'success',
            title: 'Başarılı',
            text: 'Profil resmi güncellendi!',
            timer: 2000,
            showConfirmButton: false
          });
          this.selectedFile = null;
          this.loadProfile();
        }
      },
      error: (error) => {
        this.uploading = false;
        Swal.fire({
          icon: 'error',
          title: 'Hata',
          text: error.error?.errorMessage || 'Profil resmi yüklenirken bir hata oluştu'
        });
      }
    });
  }

  updateProfile() {
    if (!this.username || this.username.length < 3) {
      Swal.fire({
        icon: 'error',
        title: 'Hata',
        text: 'Kullanıcı adı en az 3 karakter olmalıdır!'
      });
      return;
    }

    this.loading = true;
    this.userService.updateProfile({
      username: this.username,
      malUsername: this.malUsername || undefined
    }).subscribe({
      next: (response) => {
        this.loading = false;
        if (isResponseSuccessful(response)) {
          Swal.fire({
            icon: 'success',
            title: 'Başarılı',
            text: 'Profil güncellendi!',
            timer: 2000,
            showConfirmButton: false
          });
          this.loadProfile();
        } else {
          Swal.fire({
            icon: 'error',
            title: 'Hata',
            text: (response as any).errorMessage || 'Profil güncellenirken bir hata oluştu'
          });
        }
      },
      error: (error) => {
        this.loading = false;
        Swal.fire({
          icon: 'error',
          title: 'Hata',
          text: error.error?.errorMessage || 'Profil güncellenirken bir hata oluştu'
        });
      }
    });
  }

  onPasswordChange() {
    this.calculatePasswordStrength(this.newPassword);
  }

  calculatePasswordStrength(password: string): void {
    if (!password) {
      this.passwordStrength = 0;
      this.passwordStrengthLabel = '';
      return;
    }

    let strength = 0;
    
    // Uzunluk kontrolü
    if (password.length >= 8) strength += 1;
    if (password.length >= 12) strength += 1;
    
    // Küçük harf
    if (/[a-z]/.test(password)) strength += 1;
    
    // Büyük harf
    if (/[A-Z]/.test(password)) strength += 1;
    
    // Rakam
    if (/[0-9]/.test(password)) strength += 1;
    
    // Özel karakter
    if (/[^a-zA-Z0-9]/.test(password)) strength += 1;

    this.passwordStrength = Math.min(strength, 5);
    
    const labels = ['Çok Zayıf', 'Zayıf', 'Orta', 'Güçlü', 'Çok Güçlü'];
    this.passwordStrengthLabel = labels[Math.min(this.passwordStrength - 1, 4)] || '';
  }

  getPasswordStrengthColor(): string {
    if (this.passwordStrength <= 1) return '#ef4444'; // Kırmızı
    if (this.passwordStrength <= 2) return '#f59e0b'; // Turuncu
    if (this.passwordStrength <= 3) return '#eab308'; // Sarı
    if (this.passwordStrength <= 4) return '#22c55e'; // Yeşil
    return '#10b981'; // Koyu yeşil
  }

  changePassword() {
    if (this.newPassword.length < 6) {
      Swal.fire({
        icon: 'error',
        title: 'Hata',
        text: 'Yeni şifre en az 6 karakter olmalıdır!'
      });
      return;
    }

    if (this.newPassword !== this.confirmPassword) {
      Swal.fire({
        icon: 'error',
        title: 'Hata',
        text: 'Şifreler eşleşmiyor!'
      });
      return;
    }

    this.loading = true;
    this.userService.changePassword(this.currentPassword, this.newPassword).subscribe({
      next: (response) => {
        this.loading = false;
        if (isResponseSuccessful(response)) {
          Swal.fire({
            icon: 'success',
            title: 'Başarılı',
            text: 'Şifre değiştirildi!',
            timer: 2000,
            showConfirmButton: false
          });
          this.currentPassword = '';
          this.newPassword = '';
          this.confirmPassword = '';
          this.passwordStrength = 0;
          this.passwordStrengthLabel = '';
        } else {
          const errorMsg = (response as any).errorMessage || 'Şifre değiştirilirken bir hata oluştu';
          if ((response as any).returnValue === 2015) {
            Swal.fire({
              icon: 'error',
              title: 'Hata',
              text: 'Mevcut şifre hatalı!'
            });
          } else {
            Swal.fire({
              icon: 'error',
              title: 'Hata',
              text: errorMsg
            });
          }
        }
      },
      error: (error) => {
        this.loading = false;
        const errorMsg = error.error?.errorMessage || 'Şifre değiştirilirken bir hata oluştu';
        if (error.error?.returnValue === 2015) {
          Swal.fire({
            icon: 'error',
            title: 'Hata',
            text: 'Mevcut şifre hatalı!'
          });
        } else {
          Swal.fire({
            icon: 'error',
            title: 'Hata',
            text: errorMsg
          });
        }
      }
    });
  }

  connectMAL() {
    this.malIntegrationService.getAuthUrl().subscribe({
      next: (response) => {
        if (isResponseSuccessful(response) && response.response) {
          localStorage.setItem('mal_code_verifier', response.response.codeVerifier);
          window.location.href = response.response.authUrl;
        }
      },
      error: (error) => {
        Swal.fire({
          icon: 'error',
          title: 'Hata',
          text: error.error?.errorMessage || 'MAL bağlantısı kurulurken bir hata oluştu'
        });
      }
    });
  }

  disconnectMAL() {
    Swal.fire({
      title: 'MAL Bağlantısını Kaldır',
      text: 'MAL bağlantısını kaldırmak istediğinizden emin misiniz?',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'Evet, Kaldır',
      cancelButtonText: 'İptal',
      confirmButtonColor: '#d33'
    }).then((result) => {
      if (result.isConfirmed) {
        this.loading = true;
        this.userService.updateProfile({
          username: this.username,
          malUsername: ''
        }).subscribe({
          next: (response) => {
            this.loading = false;
            if (isResponseSuccessful(response)) {
              Swal.fire({
                icon: 'success',
                title: 'Başarılı',
                text: 'MAL bağlantısı kaldırıldı!',
                timer: 2000,
                showConfirmButton: false
              });
              this.loadProfile();
            }
          },
          error: () => {
            this.loading = false;
          }
        });
      }
    });
  }

  toggleTheme() {
    this.themeService.toggleTheme();
  }

  onNotificationsToggle() {
    if (typeof window !== 'undefined') {
      localStorage.setItem('notificationsEnabled', this.notificationsEnabled.toString());
    }
  }

  onEmailNotificationsToggle() {
    if (typeof window !== 'undefined') {
      localStorage.setItem('emailNotifications', this.emailNotifications.toString());
    }
  }

  deleteAccount() {
    if (!this.showDeleteConfirm) {
      this.showDeleteConfirm = true;
      return;
    }

    if (this.hardDelete && !this.deletePassword) {
      Swal.fire({
        icon: 'error',
        title: 'Hata',
        text: 'Kalıcı silme için şifre gereklidir!'
      });
      return;
    }

    Swal.fire({
      title: this.hardDelete ? 'Hesabı Kalıcı Olarak Sil' : 'Hesabı Devre Dışı Bırak',
      text: this.hardDelete 
        ? 'Bu işlem geri alınamaz! Tüm verileriniz kalıcı olarak silinecektir.'
        : 'Hesabınız pasif hale getirilecektir. İstediğiniz zaman tekrar aktif edebilirsiniz.',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: this.hardDelete ? 'Evet, Kalıcı Olarak Sil' : 'Evet, Devre Dışı Bırak',
      cancelButtonText: 'İptal',
      confirmButtonColor: '#d33',
      input: this.hardDelete ? 'password' : undefined,
      inputPlaceholder: this.hardDelete ? 'Şifrenizi girin' : undefined,
      inputValidator: (value: string) => {
        if (this.hardDelete && !value) {
          return 'Şifre gereklidir!';
        }
        return null;
      }
    }).then((result) => {
      if (result.isConfirmed) {
        const password = this.hardDelete ? (result.value || this.deletePassword) : undefined;
        const userId = this.user?.id || this.authService.user?.id || 0;
        
        if (!userId) {
          Swal.fire({
            icon: 'error',
            title: 'Hata',
            text: 'Kullanıcı ID bulunamadı!'
          });
          return;
        }

        this.loading = true;
        this.userService.deleteUser(userId, this.hardDelete, password).subscribe({
          next: (response) => {
            this.loading = false;
            if (isResponseSuccessful(response)) {
              Swal.fire({
                icon: 'success',
                title: 'Başarılı',
                text: this.hardDelete 
                  ? 'Hesabınız kalıcı olarak silindi.'
                  : 'Hesabınız devre dışı bırakıldı.',
                timer: 3000
              }).then(() => {
                this.authService.logout();
                this.router.navigate(['/login']);
              });
            } else {
              Swal.fire({
                icon: 'error',
                title: 'Hata',
                text: (response as any).errorMessage || 'Hesap silinirken bir hata oluştu'
              });
            }
          },
          error: (error) => {
            this.loading = false;
            Swal.fire({
              icon: 'error',
              title: 'Hata',
              text: error.error?.errorMessage || 'Hesap silinirken bir hata oluştu'
            });
          }
        });
      }
    });
  }

  getPreviewAvatar(): string {
    if (this.previewImage) {
      return this.previewImage;
    }

    if (this.user?.userImageLink) {
      return this.user.userImageLink;
    }

    const name = this.username || this.user?.username || 'User';
    const encoded = encodeURIComponent(name);
    return `https://ui-avatars.com/api/?background=1f2937&color=ffffff&name=${encoded}`;
  }
}
