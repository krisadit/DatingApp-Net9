import { inject, Injectable } from '@angular/core';
import { environment } from '../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AppUser } from '../models/AppUser';

@Injectable({
  providedIn: 'root'
})
export class UsersService {

  private apiUrl = `${environment.apiUrl}/users`
  private http = inject(HttpClient);

  getUsers(): Observable<AppUser[]> {
    return this.http.get<AppUser[]>(this.apiUrl);
  }

  getUserById(id: number): Observable<AppUser> {
    return this.http.get<AppUser>(`${this.apiUrl}/${id}`);
  }
}
