import { Component, inject, OnInit } from '@angular/core';
import { RegisterComponent } from "../register/register.component";
import { AppUser } from '../_models/app-user';
import { UsersService } from '../_services/users.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [RegisterComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent implements OnInit {
  registerMode = false;
  errMsg = '';

  userService = inject(UsersService);
  users: AppUser[] = [];

  ngOnInit(): void {
    this.getUsers();
  }

  registerToggle() {
    this.registerMode = !this.registerMode;
  }

  cancelRegisterMode(event: boolean) {
    this.registerMode = event;
  }

  getUsers(): void {
    this.userService.getUsers().subscribe({
      next:
        (result) => {
          this.users = result;
        },
      error: (err) => {
        console.log(err);
        this.errMsg = err;
      },
      complete: () => {
        console.log('Request complete');
      }
    });
  }
}
