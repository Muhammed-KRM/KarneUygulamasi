export interface Notification {
  id: number;
  type: 'like' | 'comment' | 'follow' | 'mention';
  message: string;
  relatedListId?: number;
  relatedUserId?: number;
  relatedUsername?: string;
  isRead: boolean;
  createdAt: string;
}

