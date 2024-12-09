import { Component, inject, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { UsersService } from './users.service';
import { AppUser } from '../models/AppUser';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    RouterOutlet
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  userService = inject(UsersService);
  title = 'DatingApp';
  users: AppUser[] = [];
  errMsg = '';

  ngOnInit(): void {
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
