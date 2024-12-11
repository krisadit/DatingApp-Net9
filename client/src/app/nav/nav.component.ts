import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AccountService } from '../_services/account.service';
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-nav',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    BsDropdownModule,
  ],
  templateUrl: './nav.component.html',
  styleUrl: './nav.component.css'
})
export class NavComponent {
  accountService = inject(AccountService);
  navbarCollapsed = true;
  model: any = {};

  login() {
    this.accountService.login(this.model).subscribe({
      next:
        (result) => {
          console.log(result);
        },
      error: (err) => {
        console.log(err);
      }
    })
  }

  logout() {
    this.accountService.logout();
  }

  toggleNavBar() {
    this.navbarCollapsed = !this.navbarCollapsed;
  }
}
