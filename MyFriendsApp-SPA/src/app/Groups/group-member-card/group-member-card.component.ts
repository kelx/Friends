import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { User } from 'src/app/_models/user';
import { AuthService } from 'src/app/_services/auth.service';
import { UserService } from 'src/app/_services/user.service';
import { AlertifyService } from 'src/app/_services/alertify.service';


@Component({
  selector: 'app-group-member-card',
  templateUrl: './group-member-card.component.html',
  styleUrls: ['./group-member-card.component.css']
})
export class GroupMemberCardComponent implements OnInit {
@Input() user: User;
@Output() selectedUser:  EventEmitter<User> = new EventEmitter();

userGroup: User;
  constructor(private authService: AuthService, private userService: UserService,
     private alertify: AlertifyService) { }

  ngOnInit() {
  }

  addUser(user: User) {
    this.userGroup = user;
    this.selectedUser.emit(this.userGroup);
  }

}
