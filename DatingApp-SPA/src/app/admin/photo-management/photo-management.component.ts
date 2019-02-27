import { AlertifyService } from './../../_services/alertify.service';
import { AdminService } from './../../_services/admin.service';
import { Component, OnInit } from '@angular/core';
import { Photo } from 'src/app/_models/photo';

@Component({
  selector: 'app-photo-management',
  templateUrl: './photo-management.component.html',
  styleUrls: ['./photo-management.component.css']
})
export class PhotoManagementComponent implements OnInit {
  photos: Photo[];

  constructor(private adminService: AdminService, private alertify: AlertifyService) { }

  ngOnInit() {
    this.getPhotosAwaitingApproval();
  }

  getPhotosAwaitingApproval() {
    this.adminService.photosForModeration().subscribe((photos: Photo[]) => {
      this.photos = photos;
    }, error => {
      console.log(error);
    });
  }

  acceptPhoto(id: number) {
    this.adminService.approvePhoto(id).subscribe(() => {
      this.photos.splice(this.photos.findIndex(x => x.id === id), 1);
      this.alertify.success('Photo approved');
      console.log('photo approved');
    }, error => {
      this.alertify.error(error);
      console.log('photo approved ERROR');
    });
  }

  rejectPhoto(id: number) {
    this.adminService.rejectPhoto(id).subscribe(() => {
      this.photos.splice(this.photos.findIndex(x => x.id === id), 1);
      this.alertify.message('Photo rejected');
    }, error => {
      this.alertify.error(error);
    });
  }
}
