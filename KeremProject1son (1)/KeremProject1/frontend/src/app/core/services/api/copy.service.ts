import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { HttpHeaderService } from '../public/http-header.service';
import { BaseResponse } from '../../models/responses/base-response.model';
import { CopyListRequest } from '../../models/requests/copy-requests.model';
import { GenerateListResponse } from '../../models/responses/list-generator-responses.model';

@Injectable({
  providedIn: 'root'
})
export class CopyService {
  private basePath: string;

  constructor(
    private httpClient: HttpClient,
    private httpHeaderService: HttpHeaderService
  ) {
    this.basePath = this.httpHeaderService.path;
  }

  copyList(sourceListId: number, newTitle: string): Observable<BaseResponse<GenerateListResponse>> {
    const request: CopyListRequest = { sourceListId, newTitle };
    return this.httpClient.post<BaseResponse<GenerateListResponse>>(
      `${this.basePath}/copy/list`,
      request,
      { headers: this.httpHeaderService.getHeaders() }
    );
  }
}

