import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { HttpHeaderService } from '../public/http-header.service';
import { BaseResponse } from '../../models/responses/base-response.model';
import { ActivityDto } from '../../models/responses/activity-responses.model';

@Injectable({
  providedIn: 'root'
})
export class ActivityService {
  private basePath: string;

  constructor(
    private httpClient: HttpClient,
    private httpHeaderService: HttpHeaderService
  ) {
    this.basePath = this.httpHeaderService.path;
  }

  getUserActivity(userId: number, page?: number, limit?: number): Observable<BaseResponse<ActivityDto[]>> {
    let params = new HttpParams();
    if (page) params = params.set('page', page.toString());
    if (limit) params = params.set('limit', limit.toString());

    const headers = this.httpHeaderService.getHeaders();
    return this.httpClient.get<BaseResponse<ActivityDto[]>>(
      `${this.basePath}/activity/user/${userId}`,
      { params, headers }
    );
  }

  getMyActivity(page?: number, limit?: number): Observable<BaseResponse<ActivityDto[]>> {
    let params = new HttpParams();
    if (page) params = params.set('page', page.toString());
    if (limit) params = params.set('limit', limit.toString());

    return this.httpClient.get<BaseResponse<ActivityDto[]>>(
      `${this.basePath}/activity/me`,
      { params, headers: this.httpHeaderService.getHeaders() }
    );
  }
}

