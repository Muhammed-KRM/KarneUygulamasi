import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { HttpHeaderService } from '../public/http-header.service';
import { BaseResponse } from '../../models/responses/base-response.model';
import { SetVisibilityRequest, GenerateLinkRequest } from '../../models/requests/share-requests.model';
import { ShareLinkResponse, PublicListInfo, PublicListsResponse } from '../../models/responses/share-responses.model';
import { AnimeListDto } from '../../models/responses/anime-list-responses.model';

@Injectable({
  providedIn: 'root'
})
export class ShareService {
  private basePath: string;

  constructor(
    private httpClient: HttpClient,
    private httpHeaderService: HttpHeaderService
  ) {
    this.basePath = this.httpHeaderService.path;
  }

  setVisibility(request: SetVisibilityRequest): Observable<BaseResponse> {
    return this.httpClient.post<BaseResponse>(
      `${this.basePath}/share/set-visibility`,
      request,
      { headers: this.httpHeaderService.getHeaders() }
    );
  }

  generateShareLink(request: GenerateLinkRequest): Observable<BaseResponse<ShareLinkResponse>> {
    return this.httpClient.post<BaseResponse<ShareLinkResponse>>(
      `${this.basePath}/share/generate-link`,
      request,
      { headers: this.httpHeaderService.getHeaders() }
    );
  }

  getPublicList(shareToken: string): Observable<BaseResponse<{ list: AnimeListDto; publicInfo: PublicListInfo }>> {
    const headers = this.httpHeaderService.getHeaders();
    return this.httpClient.get<BaseResponse<{ list: AnimeListDto; publicInfo: PublicListInfo }>>(
      `${this.basePath}/share/public/${shareToken}`,
      { headers }
    );
  }

  getPublicLists(page?: number, limit?: number): Observable<BaseResponse<PublicListsResponse>> {
    let params = new HttpParams();
    if (page) params = params.set('page', page.toString());
    if (limit) params = params.set('limit', limit.toString());

    return this.httpClient.get<BaseResponse<PublicListsResponse>>(
      `${this.basePath}/share/public`,
      { params }
    );
  }

  deleteShareLink(listId: number): Observable<BaseResponse> {
    return this.httpClient.delete<BaseResponse>(
      `${this.basePath}/share/link/${listId}`,
      { headers: this.httpHeaderService.getHeaders() }
    );
  }
}

