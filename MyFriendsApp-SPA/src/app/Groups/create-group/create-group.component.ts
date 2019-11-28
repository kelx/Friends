import { Component, OnInit } from '@angular/core';
import { User } from 'src/app/_models/user';
import { Pagination, PaginatedResult } from 'src/app/_models/pagination';
import { AuthService } from 'src/app/_services/auth.service';
import { UserService } from 'src/app/_services/user.service';
import { ActivatedRoute } from '@angular/router';
import { AlertifyService } from 'src/app/_services/alertify.service';
// import { CreateComponentOptions } from '@angular/core/src/render3/component';
import { CreateGroup } from 'src/app/_models/createGroup';
import { THIS_EXPR } from '@angular/compiler/src/output/output_ast';
import { empty } from 'rxjs';

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
  //var bsMsgSend: Message = {};

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


}
