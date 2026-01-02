import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/public/auth.service';
import { RegisterRequest } from '../../../core/models/requests/auth-requests.model';
import Swal from 'sweetalert2';
import { isResponseSuccessful } from '../../../core/utils/api-response.util';

@Component({
  selector: 'app-singin',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './singin.component.html',
  styleUrl: './singin.component.scss'
})
export class SinginComponent {
  user = {
    username: '',
    password: '',
    confirmPassword: '',
    agreeToTerms: false
  };
  loading: boolean = false;

  constructor(
    private router: Router,
    private authService: AuthService
  ) {}

  onSubmit() {
    if (!this.user.username || !this.user.password) {
      Swal.fire({
        icon: "error",
        title: "Hata",
        text: "Kullanıcı adı ve şifre gereklidir"
      });
      return;
    }

    if (this.user.password !== this.user.confirmPassword) {
      Swal.fire({ 
        icon: "error", 
        title: "Hata", 
        text: "Şifreler eşleşmiyor!" 
      });
      return;
    }

    if (!this.user.agreeToTerms) {
      Swal.fire({
        icon: "error",
        title: "Hata",
        text: "Lütfen kullanım şartlarını kabul edin"
      });
      return;
    }

    this.loading = true;

    const request: RegisterRequest = {
      username: this.user.username,
      password: this.user.password
    };
    
    this.authService.register(request).subscribe({
      next: (response) => {
        this.loading = false;
        if (isResponseSuccessful(response) && response.response) {
          this.authService.persistAuthUser(response.response, this.user.username);
          
          Swal.fire({
            icon: "success",
            title: "Başarılı!",
            text: "Kayıt işlemi tamamlandı"
          }).then(() => {
            this.router.navigate(['/dashboard']);
          });
        } else {
          Swal.fire({ 
            icon: "error", 
            title: "Hata", 
            text: response.message || response.errorMessage || "Kayıt başarısız" 
          });
        }
      },
      error: (error) => {
        this.loading = false;
        Swal.fire({ 
          icon: "error", 
          title: "Hata", 
          text: error.error?.message || error.message || "Bir hata oluştu" 
        });
      }
    });
  }
}
