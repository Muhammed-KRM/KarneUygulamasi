import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { HttpHeaderService } from '../public/http-header.service';
import { BaseResponse } from '../../models/responses/base-response.model';
import { GenerateByGenreRequest } from '../../models/requests/list-generator-requests.model';
import { GenerateListResponse } from '../../models/responses/list-generator-responses.model';

@Injectable({
  providedIn: 'root'
})
export class ListGeneratorService {
  private basePath: string;

  constructor(
    private httpClient: HttpClient,
    private httpHeaderService: HttpHeaderService
  ) {
    this.basePath = this.httpHeaderService.path;
  }

  generateByScore(): Observable<BaseResponse<GenerateListResponse>> {
    return this.httpClient.post<BaseResponse<GenerateListResponse>>(
      `${this.basePath}/generate/by-score`,
      {},
      { headers: this.httpHeaderService.getHeaders() }
    );
  }

  generateByYear(): Observable<BaseResponse<GenerateListResponse>> {
    return this.httpClient.post<BaseResponse<GenerateListResponse>>(
      `${this.basePath}/generate/by-year`,
      {},
      { headers: this.httpHeaderService.getHeaders() }
    );
  }

  generateByGenre(request: GenerateByGenreRequest): Observable<BaseResponse<GenerateListResponse>> {
    return this.httpClient.post<BaseResponse<GenerateListResponse>>(
      `${this.basePath}/generate/by-genre`,
      request,
      { headers: this.httpHeaderService.getHeaders() }
    );
  }

  getGenres(): Observable<BaseResponse<string[]>> {
    return this.httpClient.get<BaseResponse<string[]>>(
      `${this.basePath}/generate/genres`
    );
  }
}

