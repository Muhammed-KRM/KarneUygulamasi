export interface UserStatisticsDto {
  totalLists: number;
  publicLists: number;
  totalLikes: number;
  totalFollowers: number;
  totalFollowing: number;
  totalAnimeWatched: number;
  averageScore: number;
  scoreDistribution: { [score: number]: number };
  yearDistribution: { [year: number]: number };
  genreDistribution: { [genre: string]: number };
}

