export interface ShareLinkResponse {
  shareUrl: string;
  shareToken: string;
}

export interface PublicListInfo {
  id: number;
  title: string;
  mode?: string;
  authorUsername: string;
  authorId: number;
  viewCount: number;
  likeCount: number;
  createdAt: string;
  isLiked: boolean;
  listImageLink?: string | null; // Liste görseli linki
}

export interface PublicListsResponse {
  lists: PublicListInfo[];
  totalCount: number;
  page: number;
  totalPages: number;
}

