export interface UserProfileDto {
  id: number;
  username: string;
  userImageLink?: string;
  malUsername?: string;
  totalLists: number;
  totalFollowers: number;
  totalFollowing: number;
  isFollowing: boolean;
  isOwnProfile: boolean;
}

export interface NotificationsResponse {
  notifications: Array<{
    id: number;
    type: string;
    message: string;
    relatedListId?: number;
    relatedUserId?: number;
    relatedUsername?: string;
    isRead: boolean;
    createdAt: string;
  }>;
  unreadCount: number;
  page: number;
  totalPages: number;
}

export interface TemplateDto {
  id: number;
  title: string;
  mode: string;
  authorUsername: string;
  useCount: number;
  createdAt: string;
}

