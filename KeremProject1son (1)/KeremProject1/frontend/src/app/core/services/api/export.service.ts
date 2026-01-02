import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { HttpHeaderService } from '../public/http-header.service';
import { BaseResponse } from '../../models/responses/base-response.model';
import { ExportImageResponse, EmbedCodeResponse } from '../../models/responses/export-responses.model';

@Injectable({
  providedIn: 'root'
})
export class ExportService {
  private basePath: string;

  constructor(
    private httpClient: HttpClient,
    private httpHeaderService: HttpHeaderService
  ) {
    this.basePath = this.httpHeaderService.path;
  }

  exportListAsImage(listId: number): Observable<BaseResponse<ExportImageResponse>> {
    return this.httpClient.post<BaseResponse<ExportImageResponse>>(
      `${this.basePath}/export/image/${listId}`,
      {},
      { headers: this.httpHeaderService.getHeaders() }
    );
  }

  getEmbedCode(listId: number): Observable<BaseResponse<EmbedCodeResponse>> {
    return this.httpClient.get<BaseResponse<EmbedCodeResponse>>(
      `${this.basePath}/export/embed/${listId}`
    );
  }
}

