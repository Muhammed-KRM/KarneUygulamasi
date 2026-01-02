import { Injectable } from '@angular/core';
import { HttpHeaders } from '@angular/common/http';
import { environment } from '../../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class HttpHeaderService {
  path = environment.apiUrl;
  token = '';
  
  constructor() {
    if (typeof window !== 'undefined') {
      const storedToken = localStorage.getItem('token');
      if (storedToken) {
        this.token = storedToken;
      }
    }
  }
  
  getHeaders(isJson: boolean = true): HttpHeaders {
    let headers = new HttpHeaders();

    if (isJson) {
      headers = headers.set('Content-Type', 'application/json');
    }
    
    const token = this.token || (typeof window !== 'undefined' ? localStorage.getItem('token') : null);
    if (token) {
      headers = headers.set('Token', token);
    }
    
    return headers;
  }

  setToken(token: string): void {
    this.token = token;
    if (typeof window !== 'undefined') {
      localStorage.setItem('token', token);
    }
  }

  clearToken(): void {
    this.token = '';
    if (typeof window !== 'undefined') {
      localStorage.removeItem('token');
    }
  }
}