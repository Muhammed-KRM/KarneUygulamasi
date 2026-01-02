export interface Comment {
  id: number;
  listId: number;
  userId: number;
  username: string;
  content: string;
  createdAt: string;
  modTime?: string;
}

