import { Component, OnInit } from '@angular/core';
import { UserService } from 'src/app/_services/user.service';
import { User } from 'src/app/_models/user';

@Component({
  selector: 'app-group',
  templateUrl: './group.component.html',
  styleUrls: ['./group.component.css']
})
export class GroupComponent implements OnInit {
users: any;
newUsers: {};

  constructor(private userService: UserService) { }

  ngOnInit() {
    this.userService.groupGetAllUsers().subscribe(res => {
        this.users = res;
        console.log(this.users.group);
      });
  }

}
