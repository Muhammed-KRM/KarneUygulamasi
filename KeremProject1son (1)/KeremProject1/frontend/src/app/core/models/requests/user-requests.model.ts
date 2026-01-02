export interface UpdateUserRequest {
  targetUserId?: number;  // null veya yoksa kendi hesabı, admin için başka kullanıcı ID'si
  username?: string;
  malUsername?: string;
  role?: number;  // Admin için: 0=User, 1=Admin, 2=AdminAdmin
  state?: boolean;  // Admin için: true=aktif, false=pasif
}

export interface UpdateProfileRequest {
  username?: string;
  malUsername?: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

export interface DeleteUserRequest {
  userId: number;
  hardDelete: boolean;
  password?: string;
}

export interface GetAllUsersRequest {
  page?: number;
  limit?: number;
  searchQuery?: string;
  isActive?: boolean;
}

export interface SearchUsersRequest {
  query: string;
  page?: number;
  limit?: number;
}

