import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from './../../environments/environment';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { User } from '../_models/user';


@Injectable({
  providedIn: 'root'
})
export class UserService {
  baseUrl = environment.apiUrl;

  constructor(private httpClient: HttpClient) { }

  getUsers(): Observable<User[]> {
    return this.httpClient.get<User[]>(this.baseUrl + 'users');
  }

  getUser(id: string): Observable<User> {
    return this.httpClient.get<User>(this.baseUrl + 'users/' + id);
  }

  updateUser(id: number, user: User) {
    return this.httpClient.put(this.baseUrl + 'users/' + id, user);
  }

  setMainPhoto(userId: number, photoId: number) {
    console.log(this.baseUrl + 'users/' + userId + '/photos/' + photoId + '/setmain');
    return this.httpClient.post(this.baseUrl + 'users/' + userId + '/photos/' + photoId + '/setmain', {});
  }

  deletePhoto(userId: number, photoId: number) {
    console.log(this.baseUrl + 'users/' + userId + '/photos/' + photoId);
    return this.httpClient.delete(this.baseUrl + 'users/' + userId + '/photos/' + photoId);
  }


}
