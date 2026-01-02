import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { UserService } from '../../../../core/services/api/user.service';
import { UserSummaryDto } from '../../../../core/models/responses/user-responses.model';
import { GetAllUsersRequest, UpdateUserRequest, DeleteUserRequest } from '../../../../core/models/requests/user-requests.model';
import { isResponseSuccessful } from '../../../../core/utils/api-response.util';
import { UserManagementComponentUpdateModal } from './user-management.component-update-modal';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-user-management',
  templateUrl: './user-management.component.html',
  standalone: true,
  imports: [CommonModule, FormsModule, UserManagementComponentUpdateModal],
  styleUrls: ['./user-management.component.scss']
})
export class UserManagementComponent implements OnInit {
  errorMessage = '';
  Message = '';
  users: UserSummaryDto[] = [];
  selectedUser: UserSummaryDto | null = null;
  loading = false;
  
  // Pagination
  page = 1;
  limit = 20;
  totalCount = 0;
  totalPages = 0;
  
  // Filters
  searchQuery = '';
  isActive: boolean | undefined = undefined;

  constructor(private userService: UserService) {}

  ngOnInit() {
    this.loadUsers();
  }

  loadUsers() {
    this.loading = true;
    const request: GetAllUsersRequest = {
      page: this.page,
      limit: this.limit,
      searchQuery: this.searchQuery || undefined,
      isActive: this.isActive
    };

    this.userService.getAllUsers(request).subscribe({
      next: (response) => {
        this.loading = false;
        if (isResponseSuccessful(response) && response.response) {
          this.users = response.response.users;
          this.totalCount = response.response.totalCount;
          this.totalPages = response.response.totalPages;
          this.page = response.response.page;
        } else {
          this.errorMessage = response.errorMessage || 'Kullanıcılar yüklenemedi';
        }
      },
      error: (error) => {
        this.loading = false;
        this.errorMessage = 'Kullanıcılar yüklenirken hata oluştu: ' + (error.error?.errorMessage || error.message);
      }
    });
  }

  search() {
    this.page = 1;
    this.loadUsers();
  }

  clearFilters() {
    this.searchQuery = '';
    this.isActive = undefined;
    this.page = 1;
    this.loadUsers();
  }

  selectUserForUpdate(user: UserSummaryDto) {
    this.selectedUser = { ...user };
  }

  closeUpdateModal() {
    this.selectedUser = null;
    this.loadUsers(); // Refresh list after update
  }

  deleteUser(user: UserSummaryDto) {
    Swal.fire({
      title: 'Kullanıcıyı Sil',
      text: `"${user.username}" kullanıcısını silmek istediğinizden emin misiniz?`,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'Evet, Sil',
      cancelButtonText: 'İptal',
      confirmButtonColor: '#d33',
      cancelButtonColor: '#3085d6'
    }).then((result) => {
      if (result.isConfirmed) {
        Swal.fire({
          title: 'Silme Türü',
          text: 'Kalıcı olarak mı silmek istiyorsunuz?',
          icon: 'question',
          showCancelButton: true,
          confirmButtonText: 'Kalıcı Sil (Hard Delete)',
          cancelButtonText: 'Pasif Yap (Soft Delete)',
          confirmButtonColor: '#d33'
        }).then((deleteResult) => {
          const hardDelete = deleteResult.isConfirmed;
          
          this.userService.deleteUser(user.id, hardDelete).subscribe({
            next: (response) => {
              if (isResponseSuccessful(response)) {
                Swal.fire({
                  icon: 'success',
                  title: 'Başarılı',
                  text: (response as any).errorMessage || 'Kullanıcı başarıyla silindi',
                  timer: 2000,
                  showConfirmButton: false
                });
                this.loadUsers();
              } else {
                Swal.fire({
                  icon: 'error',
                  title: 'Hata',
                  text: (response as any).errorMessage || 'Kullanıcı silinirken bir hata oluştu'
                });
              }
            },
            error: (error) => {
              Swal.fire({
                icon: 'error',
                title: 'Hata',
                text: error.error?.errorMessage || 'Kullanıcı silinirken bir hata oluştu'
              });
            }
          });
        });
      }
    });
  }

  goToPage(page: number) {
    if (page >= 1 && page <= this.totalPages) {
      this.page = page;
      this.loadUsers();
    }
  }

  getRoleName(role: number): string {
    const roles: { [key: number]: string } = {
      0: 'User',
      1: 'Admin',
      2: 'AdminAdmin'
    };
    return roles[role] || 'Unknown';
  }
}
