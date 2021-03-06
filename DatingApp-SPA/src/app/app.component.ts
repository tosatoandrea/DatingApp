import { User } from './_models/user';
import { AuthService } from './_services/auth.service';
import { Component, OnInit } from '@angular/core';
import { JwtHelperService } from '@auth0/angular-jwt';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  jwtHelper = new JwtHelperService();

  constructor(private authService: AuthService) {}

  ngOnInit(): void {
    // serve per riassegnare il decodedToken nel servizio nel caso di refresh del browser,
    // altrimenti in alto a dx resterebbe scritto solo Welcome, senza il nome dell'utente
    this.authService.setDecodedToken();

    // recupero l'utente dal localstorage se presente
    const user: User = JSON.parse(localStorage.getItem('user'));
    if (user) {
      this.authService.currentUser = user;
      this.authService.changeMemberPhotoUrl(user.photoUrl);
    }
  }
}
