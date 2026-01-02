import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { HttpHeaderService } from '../public/http-header.service';
import { BaseResponse } from '../../models/responses/base-response.model';
import { SyncMalRequest } from '../../models/requests/sync-requests.model';
import { mapListModeToApi } from '../../utils/list-mode.util';
import { ListMode } from '../../models/enums/list-mode.enum';

@Injectable({
  providedIn: 'root'
})
export class SyncService {
  private basePath: string;

  constructor(
    private httpClient: HttpClient,
    private httpHeaderService: HttpHeaderService
  ) {
    this.basePath = this.httpHeaderService.path;
  }

  syncMalList(listId: number, mode: ListMode, replaceExisting: boolean): Observable<BaseResponse<{ updatedItemCount: number }>> {
    const request: SyncMalRequest = { listId, mode, replaceExisting };
    const apiRequest = {
      ...request,
      mode: mapListModeToApi(request.mode)
    };
    return this.httpClient.post<BaseResponse<{ updatedItemCount: number }>>(
      `${this.basePath}/sync/mal`,
      apiRequest,
      { headers: this.httpHeaderService.getHeaders() }
    );
  }
}

