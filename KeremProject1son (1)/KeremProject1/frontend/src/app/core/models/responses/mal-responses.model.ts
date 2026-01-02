export interface MalAuthUrlResponse {
  authUrl: string;
  codeVerifier: string;
}

export interface MalAnimeListResponse {
  data: Array<{
    node: {
      id: number;
      title: string;
      main_picture: {
        medium: string;
        large: string;
      };
    };
    list_status: {
      status: string;
      score: number;
    };
  }>;
}

