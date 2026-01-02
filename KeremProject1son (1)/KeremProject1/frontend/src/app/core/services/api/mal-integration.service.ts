import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { HttpHeaderService } from '../public/http-header.service';
import { BaseResponse } from '../../models/responses/base-response.model';
import { MalAuthUrlResponse, MalAnimeListResponse } from '../../models/responses/mal-responses.model';

@Injectable({
  providedIn: 'root'
})
export class MalIntegrationService {
  private basePath: string;

  constructor(
    private httpClient: HttpClient,
    private httpHeaderService: HttpHeaderService
  ) {
    this.basePath = this.httpHeaderService.path;
  }

  getAuthUrl(): Observable<BaseResponse<MalAuthUrlResponse>> {
    return this.httpClient.get<BaseResponse<MalAuthUrlResponse>>(
      `${this.basePath}/mal/get-auth-url`,
      { headers: this.httpHeaderService.getHeaders() }
    );
  }

  handleCallback(code: string, codeVerifier: string): Observable<BaseResponse> {
    return this.httpClient.post<BaseResponse>(
      `${this.basePath}/mal/callback`,
      { code, codeVerifier },
      { headers: this.httpHeaderService.getHeaders() }
    );
  }

  getMyList(): Observable<BaseResponse<MalAnimeListResponse>> {
    return this.httpClient.get<BaseResponse<MalAnimeListResponse>>(
      `${this.basePath}/mal/get-my-list`,
      { headers: this.httpHeaderService.getHeaders() }
    );
  }

  getListByUsername(username: string, status: number = 2): Observable<BaseResponse<MalAnimeListResponse>> {
    return this.httpClient.post<BaseResponse<MalAnimeListResponse>>(
      `${this.basePath}/mal/get-list-by-username`,
      { username, status },
      { headers: this.httpHeaderService.getHeaders() }
    );
  }

  getAdvancedListByUsername(username: string, status: number = 2): Observable<BaseResponse<any>> {
    return this.httpClient.post<BaseResponse<any>>(
      `${this.basePath}/mal/get-advanced-list-by-username`,
      { username, status },
      { headers: this.httpHeaderService.getHeaders() }
    );
  }
}

