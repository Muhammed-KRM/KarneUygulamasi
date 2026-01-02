export interface ActivityDto {
  id: number;
  type: string;
  userId: number;
  username: string;
  message: string;
  relatedListId?: number;
  relatedListTitle?: string;
  relatedUserId?: number;
  createdAt: string;
}

