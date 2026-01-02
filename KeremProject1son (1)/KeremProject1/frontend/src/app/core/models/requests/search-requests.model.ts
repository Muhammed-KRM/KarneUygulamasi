export interface SearchAnimeRequest {
  query?: string;
  genre?: string;
  year?: number;
  minScore?: number;
  maxScore?: number;
  page?: number;
  limit?: number;
}

