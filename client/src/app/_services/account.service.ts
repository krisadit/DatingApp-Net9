import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { StoredUser } from '../_models/stored-user';
import { map } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AccountService {

  private http = inject(HttpClient)
  private baseUrl = environment.apiUrl;
  currentUser = signal<StoredUser | null>(null);

  login(model: any) {
    return this.http.post<StoredUser>(`${this.baseUrl}account/login`, model).pipe(
      map(user => {
        if (user) {
          localStorage.setItem('user', JSON.stringify(user));
          this.currentUser.set(user);
        }
        return user;
      })
    );
  }

  register(model: any) {
    return this.http.post<StoredUser>(`${this.baseUrl}account/register`, model).pipe(
      map(user => {
        if (user) {
          localStorage.setItem('user', JSON.stringify(user));
          this.currentUser.set(user);
        }
        return user;
      })
    );
  }

  logout() {
    localStorage.removeItem('user');
    this.currentUser.set(null);
  }
}