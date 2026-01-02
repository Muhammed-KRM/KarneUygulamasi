import { User } from '../entities/user.model';

export interface GetUserResponse extends User {}

export interface UserSummaryDto {
  id: number;
  username: string;
  userImageLink?: string;
  malUsername?: string;
  totalLists: number;
  totalFollowers: number;
  modTime: string;
}

export interface UserListResponse {
  users: UserSummaryDto[];
  totalCount: number;
  page: number;
  limit: number;
  totalPages: number;
}

export interface UploadImageResponse {
  imageLink: string;
  fileName: string;
}

