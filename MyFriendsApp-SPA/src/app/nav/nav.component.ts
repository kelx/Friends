import { Component, OnInit } from '@angular/core';
import { AuthService } from '../_services/auth.service';
import { AlertifyService } from '../_services/alertify.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
model: any = {};
photoUrl: string;
myGroups: string[] = [];
currentGroup = 'My Group';

  constructor(private authService: AuthService, private alertify: AlertifyService,
              private router: Router) { }

  ngOnInit() {
    this.authService.photoUrl.subscribe(pUrl => this.photoUrl = pUrl);
    // this.authService.currentMyGroups.subscribe(pGrp => this.myGroups = pGrp);
  }

  login() {
    this.authService.login(this.model).subscribe(next => {
      this.alertify.success('logged in successfully');
      this.myGroups = JSON.parse(localStorage.getItem('user')).myGroups;
    }, error => {
      this.alertify.error(error);
    }, () => {
      this.router.navigate(['/members']);
    });
  }
  loggedIn() {
    if (this.authService.loggedIn()) {
      if (this.myGroups.length < 1 || this.myGroups === undefined) {
        if (JSON.parse(localStorage.getItem('user'))) {
          this.myGroups = JSON.parse(localStorage.getItem('user')).myGroups;
        }
      }
    }
    return this.authService.loggedIn();
  }
  logOut() {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    this.authService.decodedToken = null;
    this.authService.currentUser = null;
    this.alertify.message('logged out');
    this.router.navigate(['/home']);
    this.model = {};
  }
  changeGroupHeading(groupName: string)
  {
    this.currentGroup = ('My Group' + ' (' + groupName + ')').toLowerCase();
  }
}
