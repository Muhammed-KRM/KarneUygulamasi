export interface AuthUserResponse {
  id?: number;
  userId?: number;
  username: string;
  role?: string;
  token: string;
  malUsername?: string | null;
  userImageLink?: string | null;
}

export type LoginResponse = AuthUserResponse;

export type RegisterResponse = AuthUserResponse;

