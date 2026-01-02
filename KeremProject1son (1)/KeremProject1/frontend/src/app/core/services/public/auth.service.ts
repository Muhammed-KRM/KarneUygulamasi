import { Injectable, PLATFORM_ID, Inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { BehaviorSubject, Observable } from 'rxjs';
import { HttpHeaderService } from './http-header.service';
import { AuthApiService } from '../api/auth.service';
import { User } from '../../models/entities/user.model';
import { UserRole } from '../../models/enums/user-role.enum';
import { RegisterRequest, LoginRequest } from '../../models/requests/auth-requests.model';
import { BaseResponse } from '../../models/responses/base-response.model';
import { LoginResponse, RegisterResponse } from '../../models/responses/auth-responses.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  user: User | null = null;
  private authStateSubject = new BehaviorSubject<User | null>(null);
  authState$ = this.authStateSubject.asObservable();

  constructor(
    @Inject(PLATFORM_ID) private platformId: Object,
    private authApiService: AuthApiService,
    private httpHeaderService: HttpHeaderService
  ) {
    if (isPlatformBrowser(this.platformId)) {
      const userStr = localStorage.getItem('user');
      const token = localStorage.getItem('token');
      const userId = localStorage.getItem('userId');
      
      if (userStr) {
        this.user = JSON.parse(userStr) as User;
      } else if (token && userId) {
        // Token varsa ama user yoksa, user bilgilerini getir
        this.user = {
          id: parseInt(userId),
          username: '',
          role: UserRole.User,
          token: token
        } as User;
      }
      
      if (token) {
        this.httpHeaderService.setToken(token);
      }

      if (this.user) {
        this.user.role = this.normalizeRole(this.user.role);
        if (!this.user.token) {
          this.user.token = token || undefined;
        }
        this.authStateSubject.next(this.user);
      }
    }
  }

  login(request: LoginRequest): Observable<BaseResponse<LoginResponse>> {
    return this.authApiService.login(request);
  }

  register(request: RegisterRequest): Observable<BaseResponse<RegisterResponse>> {
    return this.authApiService.register(request);
  }

  getToken(): string {
    if (typeof window !== 'undefined') {
      return localStorage.getItem('token') || this.user?.token || '';
    }
    return this.user?.token || '';
  }

  getUserId(): number | null {
    if (typeof window !== 'undefined') {
      const userId = localStorage.getItem('userId');
      if (userId) return parseInt(userId);
    }
    return this.user?.id || null;
  }

  isAuthenticated(): boolean {
    const token = this.getToken();
    return !!token;
  }

  persistAuthUser(response: LoginResponse, fallbackUsername?: string): User {
    const user: User = {
      id: response.id ?? response.userId ?? this.user?.id ?? 0,
      username: response.username ?? fallbackUsername ?? this.user?.username ?? '',
      role: this.normalizeRole(response.role),
      malUsername: response.malUsername ?? undefined,
      token: response.token,
      userImageLink: response.userImageLink ?? undefined
    };

    this.setUser(user, response.token);
    return user;
  }

  setUser(user: User, token: string): void {
    const normalizedRole = this.normalizeRole(user.role);
    this.user = {
      ...user,
      role: normalizedRole,
      token
    };

    if (isPlatformBrowser(this.platformId)) {
      localStorage.setItem('user', JSON.stringify(this.user));
      localStorage.setItem('token', token);
      localStorage.setItem('userId', this.user.id.toString());
      this.httpHeaderService.setToken(token);
    }

    this.authStateSubject.next(this.user);
  }

  logout(): void {
    this.user = null;
    if (isPlatformBrowser(this.platformId)) {
      localStorage.removeItem('user');
      localStorage.removeItem('token');
      localStorage.removeItem('userId');
      this.httpHeaderService.clearToken();
    }

    this.authStateSubject.next(null);
  }

  AdminSignUp(data: any): Observable<any> {
    return this.authApiService.adminSignUp(data);
  }

  getAllConfigurationDatas(): any {
    if (isPlatformBrowser(this.platformId)) {
      const configData = localStorage.getItem('configurationData');
      if (configData) {
        try {
          return JSON.parse(configData);
        } catch (e) {
          return null;
        }
      }
    }
    return null;
  }

  private normalizeRole(role?: string | UserRole): UserRole {
    if (!role) {
      return UserRole.User;
    }

    const normalized = role.toString().toLowerCase();
    if (normalized.includes('admin')) {
      return UserRole.Admin;
    }

    return UserRole.User;
  }
}
