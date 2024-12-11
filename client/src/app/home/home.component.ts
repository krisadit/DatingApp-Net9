import { Component, inject, OnInit } from '@angular/core';
import { RegisterComponent } from "../register/register.component";
import { AppUser } from '../_models/app-user';
import { UsersService } from '../_services/users.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [RegisterComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent implements OnInit {
  registerMode = false;

  userService = inject(UsersService);
  private toastr = inject(ToastrService);

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
      error: err => this.toastr.error(err.error),
      complete: () => {
        console.log('Request complete');
      }
    });
  }
}
