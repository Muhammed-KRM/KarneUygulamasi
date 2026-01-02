import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { HttpHeaderService } from '../public/http-header.service';
import { BaseResponse } from '../../models/responses/base-response.model';
import { CommentDto } from '../../models/responses/comment-responses.model';

@Injectable({
  providedIn: 'root'
})
export class CommentService {
  private basePath: string;

  constructor(
    private httpClient: HttpClient,
    private httpHeaderService: HttpHeaderService
  ) {
    this.basePath = this.httpHeaderService.path;
  }

  getComments(listId: number, page?: number, limit?: number): Observable<BaseResponse<CommentDto[]>> {
    let params = new HttpParams();
    if (page) params = params.set('page', page.toString());
    if (limit) params = params.set('limit', limit.toString());

    return this.httpClient.get<BaseResponse<CommentDto[]>>(
      `${this.basePath}/comment/${listId}`,
      { params }
    );
  }

  addComment(listId: number, content: string): Observable<BaseResponse<CommentDto>> {
    const request = { listId, content };
    return this.httpClient.post<BaseResponse<CommentDto>>(
      `${this.basePath}/comment/add`,
      request,
      { headers: this.httpHeaderService.getHeaders() }
    );
  }

  deleteComment(commentId: number): Observable<BaseResponse> {
    return this.httpClient.delete<BaseResponse>(
      `${this.basePath}/comment/delete`,
      {
        headers: this.httpHeaderService.getHeaders(),
        body: { commentId }
      }
    );
  }
}

