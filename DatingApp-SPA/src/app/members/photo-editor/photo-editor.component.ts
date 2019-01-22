import { AlertifyService } from './../../_services/alertify.service';
import { UserService } from './../../_services/user.service';
import { AuthService } from './../../_services/auth.service';
import { Photo } from './../../_models/photo';
import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { FileUploader } from 'ng2-file-upload';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-photo-editor',
  templateUrl: './photo-editor.component.html',
  styleUrls: ['./photo-editor.component.css']
})
export class PhotoEditorComponent implements OnInit {
  @Input() photos: Photo[];
  @Output() mainPhotoChange = new EventEmitter<string>();
  uploader: FileUploader;
  hasBaseDropZoneOver = false;
  baseUrl = environment.apiUrl;
  currentMainPhoto: Photo;

  constructor(private authService: AuthService, private userService: UserService, private alertify: AlertifyService) { }

  ngOnInit() {
    this.initializeUploader();
  }

  fileOverBase(e: any): void {
    this.hasBaseDropZoneOver = e;
  }

  initializeUploader() {
    this.uploader = new FileUploader({
      url: this.baseUrl + 'users/' + this.authService.decodedToken.nameid + '/photos',
      authToken: 'Bearer ' + localStorage.getItem('token'),
      isHTML5: true,
      allowedFileType: ['image'],
      removeAfterUpload: true,
      autoUpload: false,
      maxFileSize: 10 * 1024 * 1024 // 10MB
    });

    this.uploader.onAfterAddingFile = (file) => { file.withCredentials = false; };

    this.uploader.onSuccessItem = (item, response, status, headers) => {
      if (response) {
        const res: Photo = JSON.parse(response);
        // nel corso viene creato un oggetto photo ed usato quello per aggiungere la foto all'array,
        // tuttavia funziona anche usano l'oggetto photo creato dal json
        // const photo: Photo = {
        //   id: res.id,
        //   url: res.url,
        //   dateAdded: res.dateAdded,
        //   description: res.description,
        //   isMain: res.isMain
        // };
        this.photos.push(res);
      }
    };
  }

  setMainPhoto(photo: Photo) {
    console.log('setMainPhoto()');
    this.userService.setMainPhoto(this.authService.decodedToken.nameid, photo.id).subscribe(() => {
      console.log('main photo udated');
      this.currentMainPhoto = this.photos.filter(p => p.isMain === true)[0];
      this.currentMainPhoto.isMain = false;
      photo.isMain = true;
      // this.mainPhotoChange.emit(photo.url);
      // this.authService.currentUser.photoUrl = photo.url;
      this.authService.changeMemberPhotoUrl(photo.url);
      localStorage.setItem('user', JSON.stringify(this.authService.currentUser));
    }, error => {
      this.alertify.error(error);
    });
  }

  deletePhoto(photo: Photo) {
    this.alertify.confirm('Are you sure you want to delete this photo?', () => {
      console.log('deletePhoto()');
      this.userService.deletePhoto(this.authService.decodedToken.nameid, photo.id).subscribe(() => {
        this.photos.splice(this.photos.findIndex(x => x.id === photo.id), 1);
        this.alertify.success('Photo has been deleted');
      }, error => {
        this.alertify.error('Fail to delete the photo');
      });
    });
  }

}
