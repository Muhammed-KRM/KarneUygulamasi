import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { HttpHeaderService } from '../public/http-header.service';
import { BaseResponse } from '../../models/responses/base-response.model';
import {
  UpdateUserRequest,
  UpdateProfileRequest,
  ChangePasswordRequest,
  DeleteUserRequest,
  GetAllUsersRequest,
  SearchUsersRequest
} from '../../models/requests/user-requests.model';
import { GetUserResponse, UserListResponse, UploadImageResponse } from '../../models/responses/user-responses.model';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private basePath: string;

  constructor(
    private httpClient: HttpClient,
    private httpHeaderService: HttpHeaderService
  ) {
    this.basePath = this.httpHeaderService.path;
  }

  uploadUserImage(file: File): Observable<BaseResponse<UploadImageResponse>> {
    const formData = new FormData();
    formData.append('file', file);

    return this.httpClient.post<BaseResponse<UploadImageResponse>>(
      `${this.basePath}/user/upload-image`,
      formData,
      { headers: this.httpHeaderService.getHeaders(false) }
    );
  }

  getUser(userId: number): Observable<BaseResponse<GetUserResponse>> {
    const headers = this.httpHeaderService.getHeaders();
    return this.httpClient.get<BaseResponse<GetUserResponse>>(
      `${this.basePath}/user/${userId}`,
      { headers }
    );
  }

  getMyProfile(): Observable<BaseResponse<GetUserResponse>> {
    return this.httpClient.get<BaseResponse<GetUserResponse>>(
      `${this.basePath}/user/me`,
      { headers: this.httpHeaderService.getHeaders() }
    );
  }

  getAllUsers(request: GetAllUsersRequest): Observable<BaseResponse<UserListResponse>> {
    let params = new HttpParams();
    if (request.page) params = params.set('page', request.page.toString());
    if (request.limit) params = params.set('limit', request.limit.toString());
    if (request.searchQuery) params = params.set('searchQuery', request.searchQuery);
    if (request.isActive !== undefined) params = params.set('isActive', request.isActive.toString());

    return this.httpClient.get<BaseResponse<UserListResponse>>(
      `${this.basePath}/user/all`,
      { params, headers: this.httpHeaderService.getHeaders() }
    );
  }

  searchUsers(request: SearchUsersRequest): Observable<BaseResponse<UserListResponse>> {
    const headers = this.httpHeaderService.getHeaders();
    return this.httpClient.post<BaseResponse<UserListResponse>>(
      `${this.basePath}/user/search`,
      request,
      { headers }
    );
  }

  updateUser(request: UpdateUserRequest): Observable<BaseResponse> {
    return this.httpClient.put<BaseResponse>(
      `${this.basePath}/user/update`,
      request,
      { headers: this.httpHeaderService.getHeaders() }
    );
  }

  changePassword(currentPassword: string, newPassword: string): Observable<BaseResponse> {
    const request: ChangePasswordRequest = { currentPassword, newPassword };
    return this.httpClient.post<BaseResponse>(
      `${this.basePath}/user/change-password`,
      request,
      { headers: this.httpHeaderService.getHeaders() }
    );
  }

  updateProfile(request: UpdateProfileRequest): Observable<BaseResponse> {
    return this.httpClient.put<BaseResponse>(
      `${this.basePath}/user/profile`,
      request,
      { headers: this.httpHeaderService.getHeaders() }
    );
  }

  deleteUser(userId: number, hardDelete: boolean, password?: string): Observable<BaseResponse> {
    const request: DeleteUserRequest = { userId, hardDelete, password };
    return this.httpClient.delete<BaseResponse>(
      `${this.basePath}/user`,
      {
        headers: this.httpHeaderService.getHeaders(),
        body: request
      }
    );
  }
}

