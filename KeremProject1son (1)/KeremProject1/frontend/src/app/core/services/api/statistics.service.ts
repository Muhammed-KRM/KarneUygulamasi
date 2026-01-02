import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { HttpHeaderService } from '../public/http-header.service';
import { BaseResponse } from '../../models/responses/base-response.model';
import { UserStatisticsDto } from '../../models/responses/statistics-responses.model';
import { ListStatisticsDto } from '../../models/responses/anime-list-responses.model';

@Injectable({
  providedIn: 'root'
})
export class StatisticsService {
  private basePath: string;

  constructor(
    private httpClient: HttpClient,
    private httpHeaderService: HttpHeaderService
  ) {
    this.basePath = this.httpHeaderService.path;
  }

  getUserStatistics(userId: number): Observable<BaseResponse<UserStatisticsDto>> {
    const headers = this.httpHeaderService.getHeaders();
    return this.httpClient.get<BaseResponse<UserStatisticsDto>>(
      `${this.basePath}/statistics/user/${userId}`,
      { headers }
    );
  }

  getMyStatistics(): Observable<BaseResponse<UserStatisticsDto>> {
    return this.httpClient.get<BaseResponse<UserStatisticsDto>>(
      `${this.basePath}/statistics/me`,
      { headers: this.httpHeaderService.getHeaders() }
    );
  }

  getListStatistics(listId: number): Observable<BaseResponse<ListStatisticsDto>> {
    return this.httpClient.get<BaseResponse<ListStatisticsDto>>(
      `${this.basePath}/list/${listId}/statistics`,
      { headers: this.httpHeaderService.getHeaders() }
    );
  }
}

