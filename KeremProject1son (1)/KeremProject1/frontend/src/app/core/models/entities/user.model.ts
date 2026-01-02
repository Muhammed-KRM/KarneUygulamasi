import { UserRole } from '../enums/user-role.enum';

export interface User {
  id: number;
  username: string;
  role: UserRole;
  malUsername?: string;
  userImageLink?: string;
  modTime?: string;
  token?: string;
}

