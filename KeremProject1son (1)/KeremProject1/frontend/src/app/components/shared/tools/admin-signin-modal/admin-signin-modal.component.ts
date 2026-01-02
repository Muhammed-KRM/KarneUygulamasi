import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthApiService } from '../../../../core/services/api/auth.service';
import { AuthService } from '../../../../core/services/public/auth.service';
import { isResponseSuccessful } from '../../../../core/utils/api-response.util';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-admin-signin-modal',
  templateUrl: './admin-signin-modal.component.html',
  styleUrl: './admin-signin-modal.component.scss',
  standalone: true,
  imports: [FormsModule, CommonModule]
})
export class AdminSigninModalComponent {
  adminData = {
    username: '',
    password: ''
  };
  errorMessage = '';
  loading = false;

  constructor(
    private authApiService: AuthApiService,
    private authService: AuthService
  ) {}

  onSubmit() {
    if (!this.adminData.username || !this.adminData.password) {
      Swal.fire({
        icon: 'error',
        title: 'Hata',
        text: 'Kullanıcı adı ve şifre gereklidir'
      });
      return;
    }

    // Admin kontrolü - sadece AdminAdmin yapabilir
    const currentUser = this.authService.user;
    if (!currentUser) {
      Swal.fire({
        icon: 'error',
        title: 'Yetki Hatası',
        text: 'Bu işlem için giriş yapmanız gerekiyor'
      });
      return;
    }

    const role = (currentUser as any).role;
    const roleStr = role?.toString().toLowerCase() || '';
    const isAdminAdmin = roleStr.includes('adminadmin') || role === 2;

    if (!isAdminAdmin) {
      Swal.fire({
        icon: 'error',
        title: 'Yetki Hatası',
        text: 'Bu işlemi sadece AdminAdmin kullanıcıları yapabilir'
      });
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    this.authApiService.createAdmin({
      username: this.adminData.username,
      password: this.adminData.password
    }).subscribe({
      next: (response) => {
        this.loading = false;
        if (isResponseSuccessful(response)) {
          Swal.fire({
            icon: 'success',
            title: 'Başarılı',
            text: (response as any).errorMessage || 'Admin kullanıcısı başarıyla oluşturuldu',
            timer: 2000,
            showConfirmButton: false
          });
          // Formu temizle
          this.adminData = {
            username: '',
            password: ''
          };
        } else {
          const errorMsg = (response as any).errorMessage || 'Admin oluşturulurken bir hata oluştu';
          this.errorMessage = errorMsg;
          Swal.fire({
            icon: 'error',
            title: 'Hata',
            text: errorMsg
          });
        }
      },
      error: (error) => {
        this.loading = false;
        const errorMsg = error.error?.errorMessage || error.message || 'Admin oluşturulurken bir hata oluştu';
        this.errorMessage = errorMsg;
        Swal.fire({
          icon: 'error',
          title: 'Hata',
          text: errorMsg
        });
      }
    });
  }
}
