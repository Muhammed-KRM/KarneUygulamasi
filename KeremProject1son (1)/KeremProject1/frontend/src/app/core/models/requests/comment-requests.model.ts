export interface AddCommentRequest {
  listId: number;
  content: string;
}

export interface UpdateCommentRequest {
  commentId: number;
  content: string;
}

