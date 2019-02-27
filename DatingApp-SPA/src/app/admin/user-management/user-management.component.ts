import { map } from 'rxjs/operators';
import { FileUploadModule } from 'ng2-file-upload';
import { User } from './../../_models/user';
import { AdminService } from './../../_services/admin.service';
import { Component, OnInit } from '@angular/core';
import { BsModalService, BsModalRef } from 'ngx-bootstrap';
import { RolesModalComponent } from '../roles-modal/roles-modal.component';

@Component({
  selector: 'app-user-management',
  templateUrl: './user-management.component.html',
  styleUrls: ['./user-management.component.css']
})
export class UserManagementComponent implements OnInit {
  users: User[];
  bsModalRef: BsModalRef;

  constructor(private adminService: AdminService, private modalService: BsModalService) { }

  ngOnInit() {
    this.getUsersWithRoles();
  }

  getUsersWithRoles() {
    this.adminService.getUsersWithRoles().subscribe((users: User[]) => {
      this.users = users;
    }, error => {
      console.log(error);
    });
  }

  editRoles(user: User) {
    const initialState = {
      user,
      roles: this.getRolesArray(user),
      title: 'Roles'
    };
    this.bsModalRef = this.modalService.show(RolesModalComponent, {initialState});
    this.bsModalRef.content.updateSelectedRoles
      .subscribe((values) => {
        const rolesToUpdate = {
          roleNames: [...values.filter(el => el.checked === true).map(el => el.name)]
        };
        console.log(rolesToUpdate);
        if (rolesToUpdate) {
          this.adminService.updateUserRoles(user, rolesToUpdate).subscribe(() => {
            user.roles = [...rolesToUpdate.roleNames];
          }, error => {
            console.log(error);
          });
        }
      });

  }

  private getRolesArray(user: User) {
    const userRoles = user.roles;
    const roles = [
      { checked: false, name: 'Admin', value: 'Admin' },
      { checked: false, name: 'Moderator', value: 'Moderator' },
      { checked: false, name: 'Member', value: 'Member' },
      { checked: false, name: 'VIP', value: 'VIP' }
    ];

    for (let i = 0; i < roles.length; i++) {
      if (userRoles.findIndex(r => r === roles[i].name) >= 0) {
        roles[i].checked = true;
      }
    }
    return roles;
  }
}
