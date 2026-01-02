import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { HttpHeaderService } from '../public/http-header.service';
import { BaseResponse } from '../../models/responses/base-response.model';
import { RegisterRequest, LoginRequest } from '../../models/requests/auth-requests.model';
import { LoginResponse, RegisterResponse } from '../../models/responses/auth-responses.model';

@Injectable({
  providedIn: 'root'
})
export class AuthApiService {
  private basePath: string;

  constructor(
    private httpClient: HttpClient,
    private httpHeaderService: HttpHeaderService
  ) {
    this.basePath = this.httpHeaderService.path;
  }

  register(request: RegisterRequest): Observable<BaseResponse<RegisterResponse>> {
    return this.httpClient.post<BaseResponse<RegisterResponse>>(
      `${this.basePath}/auth/register`,
      request,
      { headers: this.httpHeaderService.getHeaders() }
    );
  }

  login(request: LoginRequest): Observable<BaseResponse<LoginResponse>> {
    return this.httpClient.post<BaseResponse<LoginResponse>>(
      `${this.basePath}/auth/login`,
      request,
      { headers: this.httpHeaderService.getHeaders() }
    );
  }

  createAdmin(request: { username: string; password: string }): Observable<BaseResponse<any>> {
    return this.httpClient.post<BaseResponse<any>>(
      `${this.basePath}/Auth/create-admin`,
      request,
      { headers: this.httpHeaderService.getHeaders() }
    );
  }

  // Eski method - geriye dönük uyumluluk için
  adminSignUp(data: any): Observable<any> {
    return this.createAdmin({
      username: data.userName || data.username,
      password: data.password || data.passwordHash
    });
  }
}

