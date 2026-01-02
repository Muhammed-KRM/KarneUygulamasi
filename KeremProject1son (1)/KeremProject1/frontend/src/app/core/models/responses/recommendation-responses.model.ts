export interface RecommendationDto {
  malId: number;
  title: string;
  imageUrl: string;
  score?: number;
  reason: string;
  matchCount: number;
}

export interface TrendingListsResponse {
  lists: Array<{
    id: number;
    title: string;
    authorUsername: string;
    viewCount: number;
    likeCount: number;
    commentCount: number;
    createdAt: string;
    trendingScore: number;
    listImageLink?: string | null; // Liste görseli linki
  }>;
  totalCount: number;
  page: number;
  totalPages: number;
}

