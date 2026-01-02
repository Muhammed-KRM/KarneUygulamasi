export interface AnimeSearchResult {
  malId: number;
  title: string;
  imageUrl: string;
  score?: number;
  year?: number;
  genres: string[];
  synopsis?: string;
}

export interface SearchAnimeResponse {
  results: AnimeSearchResult[];
  totalResults?: number;
  totalCount?: number;
  page: number;
  totalPages?: number;
  limit?: number;
}
