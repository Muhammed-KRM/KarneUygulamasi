import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpHeaderService } from '../public/http-header.service';
import { BaseResponse } from '../../models/responses/base-response.model';
import { GetUserLogsRequest, GetAdminLogsRequest } from '../../models/requests/log-requests.model';
import { LogListResponse } from '../../models/responses/log-responses.model';

@Injectable({
  providedIn: 'root'
})
export class LogsService {
  private basePath: string;

  constructor(
    private httpClient: HttpClient,
    private httpHeaderService: HttpHeaderService
  ) {
    this.basePath = this.httpHeaderService.path;
  }

  /**
   * Kullanıcının kendi loglarını görüntüler. Admin, başka kullanıcının loglarını da görebilir.
   */
  getUserLogs(request: GetUserLogsRequest): Observable<BaseResponse<LogListResponse>> {
    const body: any = {
      page: request.page || 1,
      limit: request.limit || 20
    };

    if (request.userId !== undefined) body.userId = request.userId;
    if (request.tableName) body.tableName = request.tableName;
    if (request.action) body.action = request.action;
    if (request.startDate) body.startDate = request.startDate;
    if (request.endDate) body.endDate = request.endDate;

    return this.httpClient.post<BaseResponse<LogListResponse>>(
      `${this.basePath}/log/user`,
      body,
      { headers: this.httpHeaderService.getHeaders() }
    );
  }

  /**
   * Admin, sistemdeki tüm logları görüntüleyebilir. Sadece Admin ve AdminAdmin kullanabilir.
   */
  getAdminLogs(request: GetAdminLogsRequest): Observable<BaseResponse<LogListResponse>> {
    const body: any = {
      page: request.page || 1,
      limit: request.limit || 50
    };

    if (request.userId !== undefined) body.userId = request.userId;
    if (request.tableName) body.tableName = request.tableName;
    if (request.action) body.action = request.action;
    if (request.startDate) body.startDate = request.startDate;
    if (request.endDate) body.endDate = request.endDate;

    return this.httpClient.post<BaseResponse<LogListResponse>>(
      `${this.basePath}/log/admin/all`,
      body,
      { headers: this.httpHeaderService.getHeaders() }
    );
  }

  // Eski method - geriye dönük uyumluluk için
  GetSystemLogs(data: any): Observable<any> {
    return this.httpClient.post(
      "https://localhost:7049/General/GetSystemLogs",
      JSON.stringify(data),
      { headers: this.httpHeaderService.getHeaders() }
    );
  }
}
