import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { HttpHeaderService } from '../public/http-header.service';
import { BaseResponse } from '../../models/responses/base-response.model';
import { MoveItemRequest, ReorderItemsRequest } from '../../models/requests/dragdrop-requests.model';

@Injectable({
  providedIn: 'root'
})
export class DragdropService {
  private basePath: string;

  constructor(
    private httpClient: HttpClient,
    private httpHeaderService: HttpHeaderService
  ) {
    this.basePath = this.httpHeaderService.path;
  }

  moveItem(itemId: number, targetTierId: number, newRankInTier: number): Observable<BaseResponse> {
    const request: MoveItemRequest = { itemId, targetTierId, newRankInTier };
    return this.httpClient.post<BaseResponse>(
      `${this.basePath}/dragdrop/move-item`,
      request,
      { headers: this.httpHeaderService.getHeaders() }
    );
  }

  reorderItems(tierId: number, items: Array<{ itemId: number; rankInTier: number }>): Observable<BaseResponse> {
    const request: ReorderItemsRequest = { tierId, items };
    return this.httpClient.post<BaseResponse>(
      `${this.basePath}/dragdrop/reorder-items`,
      request,
      { headers: this.httpHeaderService.getHeaders() }
    );
  }
}

