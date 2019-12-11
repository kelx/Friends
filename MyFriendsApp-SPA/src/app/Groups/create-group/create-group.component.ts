import { Component, OnInit } from '@angular/core';
import { User } from 'src/app/_models/user';
import { Pagination, PaginatedResult } from 'src/app/_models/pagination';
import { AuthService } from 'src/app/_services/auth.service';
import { UserService } from 'src/app/_services/user.service';
import { ActivatedRoute } from '@angular/router';
import { AlertifyService } from 'src/app/_services/alertify.service';
import { CreateGroup } from 'src/app/_models/createGroup';
import { FileUploader } from 'ng2-file-upload';
import { Photo } from 'src/app/_models/photo';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-create-group',
  templateUrl: './create-group.component.html',
  styleUrls: ['./create-group.component.css']
})
export class CreateGroupComponent implements OnInit {
  users: User[];
  selectedUsers: Array<User> = [];
  pagination: Pagination;
  likesParam: string;
  createGroup: CreateGroup = {};
  // var bsMsgSend: Message = {};

  baseUrl = environment.apiUrl;

  photos: Photo[];
  uploader: FileUploader;
  hasBaseDropZoneOver = false;

  constructor(private authService: AuthService, private userService: UserService,
    private route: ActivatedRoute, private alertify: AlertifyService) { }

    ngOnInit() {
      this.route.data.subscribe(data => {
        this.users = data['users'].result;
        this.pagination = data['users'].pagination;
      });
      this.likesParam = 'likers';
      this.createGroup.groupName = 'Something';
      this.createGroup.groupMembers = [];

      this.InitializeUploader();
    }
    fileOverBase(e: any): void {
      this.hasBaseDropZoneOver = e;
    }

    pageChanged(event: any): void {
      this.pagination.currentPage = event.page;
      this.loadUsers();
    }
    loadUsers() {
      this.userService.getUsers(this.pagination.currentPage, this.pagination.itemsPerPage, null, this.likesParam)
      .subscribe((res: PaginatedResult<User[]>) => {
        this.users = res.result;
        this.pagination = res.pagination;
      }, error => {
        this.alertify.error(error);
      });
    }
    addSelectedUser(user: User) {
      if (user) {
        this.selectedUsers.push(user);
      }
     // this.selectedUsers.push(user);
     //console.log(user);
    }
    createGroupWithSelectedUsers() {
      this.selectedUsers.forEach(element => {
        this.createGroup.groupMembers.push(element.id);
      });
      this.createGroup.userId = this.authService.decodedToken.nameid;
      this.userService.createGroupWithUsers(this.createGroup).subscribe(res => {
        console.log(res);
      });
    }

    // file upload section
    InitializeUploader() {
      this.uploader = new FileUploader({
        url: this.baseUrl + 'users/' + this.authService.decodedToken.nameid + '/photos',
        authToken: 'Bearer ' + localStorage.getItem('token'),
        isHTML5: true,
        allowedFileType: ['image'],
        removeAfterUpload: true,
        autoUpload: false,
        maxFileSize: 5 * 1024 * 1024
      });
      this.uploader.onAfterAddingFile = (kfile) => {
        kfile.withCredentials = false;
      };
      this.uploader.onSuccessItem = (item, response, status, headers) => {
        if(response) {
          const res: Photo = JSON.parse(response);
          const photo = {
            id: res.id,
            url: res.url,
            dateAdded: res.dateAdded,
            description: res.description,
            isMain: res.isMain
          };
          this.photos.push(photo);
          if (photo.isMain) {
            this.authService.changeMemberPhoto(photo.url);
            this.authService.currentUser.photoUrl = photo.url;
            localStorage.setItem('user', JSON.stringify(this.authService.currentUser));
          }
        }
      };
    }


}
