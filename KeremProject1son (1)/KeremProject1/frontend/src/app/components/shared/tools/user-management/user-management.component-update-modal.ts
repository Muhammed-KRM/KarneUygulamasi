import { CommonModule } from '@angular/common';
import { Component, OnInit, OnChanges, ChangeDetectorRef, Input, Output, EventEmitter, SimpleChanges } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { UserService } from '../../../../core/services/api/user.service';
import { UserSummaryDto } from '../../../../core/models/responses/user-responses.model';
import { UpdateUserRequest } from '../../../../core/models/requests/user-requests.model';
import { isResponseSuccessful } from '../../../../core/utils/api-response.util';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-user-management-update-modal',
  templateUrl: './user-management.component-update-modal.html',
  standalone: true,
  imports: [CommonModule, FormsModule],
  styleUrls: ['./user-management.component.scss']
})
export class UserManagementComponentUpdateModal implements OnInit, OnChanges {
  @Input() user: UserSummaryDto | null = null;
  @Output() userUpdated = new EventEmitter<void>();
  
  errorMessage = '';
  Message = '';
  
  // Form fields
  username = '';
  malUsername = '';
  role: number = 0;
  state: boolean = true;

  constructor(
    private userService: UserService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    if (this.user) {
      this.loadUserData();
    }
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['user'] && this.user) {
      this.loadUserData();
    }
  }

  loadUserData() {
    if (!this.user) return;
    
    this.username = this.user.username;
    this.malUsername = this.user.malUsername || '';
    // Note: role ve state UserSummaryDto'da yok, API'den detaylı bilgi çekmek gerekebilir
    // Şimdilik varsayılan değerler kullanıyoruz
    this.role = 0;
    this.state = true;
  }

  updateUser() {
    if (!this.user) {
      this.errorMessage = 'Kullanıcı seçilmedi';
      return;
    }

    const request: UpdateUserRequest = {
      targetUserId: this.user.id,
      username: this.username || undefined,
      malUsername: this.malUsername || undefined,
      role: this.role,
      state: this.state
    };

    this.userService.updateUser(request).subscribe({
      next: (response) => {
        if (isResponseSuccessful(response)) {
          Swal.fire({
            icon: 'success',
            title: 'Başarılı',
            text: response.errorMessage || 'Kullanıcı başarıyla güncellendi',
            timer: 2000,
            showConfirmButton: false
          });
          this.errorMessage = '';
          this.Message = 'Kullanıcı başarıyla güncellendi';
          // Parent component'e bildir
          setTimeout(() => {
            this.userUpdated.emit();
          }, 500);
        } else {
          const errorMsg = (response as any).errorMessage || 'Kullanıcı güncellenirken bir hata oluştu';
          this.errorMessage = errorMsg;
          Swal.fire({
            icon: 'error',
            title: 'Hata',
            text: errorMsg
          });
        }
      },
      error: (error) => {
        this.errorMessage = error.error?.errorMessage || 'Kullanıcı güncellenirken bir hata oluştu';
        Swal.fire({
          icon: 'error',
          title: 'Hata',
          text: this.errorMessage
        });
      }
    });
  }

  cancel() {
    this.user = null;
    this.errorMessage = '';
    this.Message = '';
  }
}
