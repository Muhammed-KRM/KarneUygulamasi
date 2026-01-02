import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../core/services/public/auth.service';
import Swal from 'sweetalert2';
import { LoginRequest } from '../../../core/models/requests/auth-requests.model';
import { isResponseSuccessful } from '../../../core/utils/api-response.util';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  username: string = '';
  password: string = '';
  returnUrl: string = '/dashboard';
  loading: boolean = false;

  constructor(
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/dashboard';
  }

  onLogin(): void {
    if (!this.username || !this.password) {
      Swal.fire({
        icon: "error",
        title: "Hata",
        text: "Kullanıcı adı ve şifre gereklidir"
      });
      return;
    }

    this.loading = true;
    
    const request: LoginRequest = {
      username: this.username,
      password: this.password // Backend'de hash'lenecek veya frontend'de hash'lenebilir
    };

    this.authService.login(request).subscribe({
      next: (response) => {
        this.loading = false;
        if (isResponseSuccessful(response) && response.response) {
          const loginData = response.response;
          this.authService.persistAuthUser(loginData, this.username);
          
          // Başarı mesajı
          Swal.fire({
            icon: "success",
            title: "Başarılı",
            text: "Giriş yapıldı"
          }).then(() => {
            this.router.navigate([this.returnUrl]);
          });
        } else {
          Swal.fire({
            icon: "error",
            title: "Hata",
            text: response.message || response.errorMessage || "Giriş başarısız"
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