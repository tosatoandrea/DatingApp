import { Router } from '@angular/router';
import { AlertifyService } from './../_services/alertify.service';
import { AuthService } from './../_services/auth.service';
import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { FormGroup, FormControl, Validators, FormBuilder } from '@angular/forms';
import { BsDatepickerConfig } from 'ngx-bootstrap';
import { User } from '../_models/user';

@Component({
  selector: 'app-register-reactive',
  templateUrl: './registerReactive.component.html',
  styleUrls: ['./registerReactive.component.css']
})
export class RegisterReactiveComponent implements OnInit {
  @Output() cancelRegister = new EventEmitter();
  user: User;
  registerForm: FormGroup;

  // il Partial serve per rendere opzionali (nullable) tutti i membri della classe BsDatepickerConfig,
  // in questo modo non serve assegnarli tutti
  bsConfig: Partial<BsDatepickerConfig>;


  constructor(private authService: AuthService, private alertify: AlertifyService,
                private fb: FormBuilder, private router: Router) { }

  ngOnInit() {
    // this.registerForm = new FormGroup({
    //   username: new FormControl('', Validators.required),
    //   password: new FormControl('', [ Validators.required, Validators.minLength(4), Validators.maxLength(8) ]),
    //   confirmPassword: new FormControl('', Validators.required)
    // }, this.passwordMatchValidator);
    this.bsConfig = {
      containerClass: 'theme-red'
    };
    this.createRegisterForm();
  }

  createRegisterForm() {
    this.registerForm = this.fb.group({
      gender: ['male'],
      username: ['', Validators.required],
      knownAs: ['', Validators.required],
      dateOfBirth: [null, Validators.required],
      city: ['', Validators.required],
      country: ['', Validators.required],
      password: ['', [ Validators.required, Validators.minLength(4), Validators.maxLength(8)]],
      confirmPassword: ['', Validators.required]
    }, { validator: this.passwordMatchValidator });
  }

  passwordMatchValidator(g: FormGroup) {
    return g.get('password').value === g.get('confirmPassword').value ? null : { 'mismatch' : true};
  }

  register() {
    if (this.registerForm.valid) {
      // copia un oggetto in un'altro con le stesse proprietà
      this.user = Object.assign({}, this.registerForm.value);

      // dopo aver registrato esegue il login e reindirizza la navigazione nella pagine /members
      this.authService.register(this.user).subscribe(next => {
        console.log('Registration Successfully');
        this.alertify.success('Registration successfully');
      }, error => {
        console.log(error);
        this.alertify.error(error);
      }, () => {
        this.authService.login(this.user).subscribe( () => {
          this.router.navigate(['/members']);
        });
      });
    }
  }

  cancel() {
    this.cancelRegister.emit(false);
  }

}
