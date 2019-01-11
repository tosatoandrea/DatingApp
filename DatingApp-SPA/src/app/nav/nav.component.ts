import { AuthService } from './../_services/auth.service';
import { Component, OnInit } from '@angular/core';
import { modelGroupProvider } from '@angular/forms/src/directives/ng_model_group';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  model: any = {};

  constructor(private authervice: AuthService) { }

  ngOnInit() {
  }

  login() {
    this.authervice.login(this.model).subscribe(next => {
      console.log('Login succesfully');
    }, error => {
      console.log('Login failed');
    });
  }

  loggedIn() {
    const token = localStorage.getItem('token');
    return !!token;
  }

  logOut() {
    localStorage.removeItem('token');
    console.log('logged out');
    this.model.username = null;
    this.model.password = null;
  }

}
