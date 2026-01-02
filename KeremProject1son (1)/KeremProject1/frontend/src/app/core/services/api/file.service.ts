import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { HttpHeaderService } from '../public/http-header.service';
import { BaseResponse } from '../../models/responses/base-response.model';
import { FileType } from '../../models/enums/file-type.enum';

@Injectable({
  providedIn: 'root'
})
export class FileService {
  private basePath: string;

  constructor(
    private httpClient: HttpClient,
    private httpHeaderService: HttpHeaderService
  ) {
    this.basePath = this.httpHeaderService.path;
  }

  downloadFile(filename: string, type: FileType, sessionno: string, signature: string): Observable<Blob> {
    let params = new HttpParams();
    params = params.set('filename', filename);
    params = params.set('type', type.toString());
    params = params.set('sessionno', sessionno);
    params = params.set('signature', signature);

    return this.httpClient.get(
      `${this.basePath}/file/download`,
      {
        params,
        responseType: 'blob',
        headers: this.httpHeaderService.getHeaders()
      }
    );
  }

  getFileInfo(filename: string, type: FileType): Observable<BaseResponse> {
    let params = new HttpParams();
    params = params.set('filename', filename);
    params = params.set('type', type.toString());

    const headers = this.httpHeaderService.getHeaders();
    return this.httpClient.get<BaseResponse>(
      `${this.basePath}/file/info`,
      { params, headers }
    );
  }

  generateFileLink(filename: string, type: FileType, userId: number): string {
    // Bu metod backend'den dönen güvenli link formatına göre oluşturulmalı
    // Şimdilik basit bir örnek
    return `${this.basePath}/file/download?filename=${filename}&type=${type}&sessionno=${userId}&signature=...`;
  }

  cleanTempFiles(): Observable<BaseResponse> {
    return this.httpClient.post<BaseResponse>(
      `${this.basePath}/file/clean-temp`,
      {},
      { headers: this.httpHeaderService.getHeaders() }
    );
  }
}

