import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { HttpHeaderService } from '../public/http-header.service';
import { BaseResponse } from '../../models/responses/base-response.model';
import { FollowRequest, CreateTemplateRequest } from '../../models/requests/social-requests.model';
import { UserProfileDto, NotificationsResponse, TemplateDto } from '../../models/responses/social-responses.model';

@Injectable({
  providedIn: 'root'
})
export class SocialService {
  private basePath: string;

  constructor(
    private httpClient: HttpClient,
    private httpHeaderService: HttpHeaderService
  ) {
    this.basePath = this.httpHeaderService.path;
  }

  likeList(listId: number): Observable<BaseResponse> {
    return this.httpClient.post<BaseResponse>(
      `${this.basePath}/social/like/${listId}`,
      {},
      { headers: this.httpHeaderService.getHeaders() }
    );
  }

  followUser(request: FollowRequest): Observable<BaseResponse> {
    return this.httpClient.post<BaseResponse>(
      `${this.basePath}/social/follow`,
      request,
      { headers: this.httpHeaderService.getHeaders() }
    );
  }

  getUserProfile(userId: number): Observable<BaseResponse<UserProfileDto>> {
    const headers = this.httpHeaderService.getHeaders();
    return this.httpClient.get<BaseResponse<UserProfileDto>>(
      `${this.basePath}/social/profile/${userId}`,
      { headers }
    );
  }

  getNotifications(page?: number, limit?: number): Observable<BaseResponse<NotificationsResponse>> {
    let params = new HttpParams();
    if (page) params = params.set('page', page.toString());
    if (limit) params = params.set('limit', limit.toString());

    return this.httpClient.get<BaseResponse<NotificationsResponse>>(
      `${this.basePath}/social/notifications`,
      { params, headers: this.httpHeaderService.getHeaders() }
    );
  }

  markNotificationAsRead(notificationId: number): Observable<BaseResponse> {
    return this.httpClient.put<BaseResponse>(
      `${this.basePath}/social/notifications/${notificationId}/read`,
      {},
      { headers: this.httpHeaderService.getHeaders() }
    );
  }

  markAllNotificationsAsRead(): Observable<BaseResponse> {
    return this.httpClient.put<BaseResponse>(
      `${this.basePath}/social/notifications/read-all`,
      {},
      { headers: this.httpHeaderService.getHeaders() }
    );
  }

  deleteNotification(notificationId: number): Observable<BaseResponse> {
    return this.httpClient.delete<BaseResponse>(
      `${this.basePath}/social/notification/${notificationId}`,
      { headers: this.httpHeaderService.getHeaders() }
    );
  }

  deleteAllNotifications(): Observable<BaseResponse> {
    return this.httpClient.delete<BaseResponse>(
      `${this.basePath}/social/notifications/all`,
      { headers: this.httpHeaderService.getHeaders() }
    );
  }

  createTemplate(request: CreateTemplateRequest): Observable<BaseResponse> {
    return this.httpClient.post<BaseResponse>(
      `${this.basePath}/social/template/create`,
      request,
      { headers: this.httpHeaderService.getHeaders() }
    );
  }

  getTemplates(page?: number, limit?: number): Observable<BaseResponse<TemplateDto[]>> {
    let params = new HttpParams();
    if (page) params = params.set('page', page.toString());
    if (limit) params = params.set('limit', limit.toString());

    return this.httpClient.get<BaseResponse<TemplateDto[]>>(
      `${this.basePath}/social/templates`,
      { params }
    );
  }

  deleteTemplate(templateId: number): Observable<BaseResponse> {
    return this.httpClient.delete<BaseResponse>(
      `${this.basePath}/social/template/${templateId}`,
      { headers: this.httpHeaderService.getHeaders() }
    );
  }
}

