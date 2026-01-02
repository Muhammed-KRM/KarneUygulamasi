import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, BehaviorSubject } from 'rxjs';
import { HttpHeaderService } from '../public/http-header.service';

@Injectable({
  providedIn: 'root'
})
export class UserManagementService {
  private path: string;
  constructor(private httpClient: HttpClient, private httpHeaderService: HttpHeaderService) {
    this.path = `${this.httpHeaderService.path}/User`;
  }

  private selectedUserSubject = new BehaviorSubject<User | null>(null);
  selectedUser$ = this.selectedUserSubject.asObservable();

  setSelectedUser(user: User | null) {
    this.selectedUserSubject.next(user);
  }

  getAllUser(): Observable<any> {
    return this.httpClient.post(
      `${this.path}/GetAllUser`,
      {},
      { headers: this.httpHeaderService.getHeaders() }
    );
  }

  updateUser(data: any): Observable<any> {
    return this.httpClient.post(
      `${this.path}/UpdateUser`,
      data,
      { headers: this.httpHeaderService.getHeaders() }
    );
  }

  deleteUser(data: any): Observable<any> {
    return this.httpClient.post(
      `${this.path}/DeleteUser`,
      data,
      { headers: this.httpHeaderService.getHeaders() }
    );
  }
}

export type User = {
  id: number;
  userName: string;
  password: string;
  userImageLink: string;
  userRoleinAuthorization: number;
  state: boolean;
  token: string;
};

