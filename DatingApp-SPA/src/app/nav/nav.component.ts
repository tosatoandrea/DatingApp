import { AlertifyService } from './../_services/alertify.service';
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

  constructor(private authervice: AuthService, private alertify: AlertifyService) { }

  ngOnInit() {
  }

  login() {
    this.authervice.login(this.model).subscribe(next => {
      this.alertify.success('Loged in successfully');
      console.log('Loged in successfully');
    }, error => {
      this.alertify.error(error);
      console.log(error);
    });
  }

  loggedIn() {
    return this.authervice.loggedIn();
  }

  logOut() {
    localStorage.removeItem('token');
    this.alertify.message('Logged out');
    console.log('Logged out');
    this.model.username = '';
    this.model.password = '';
  }

}