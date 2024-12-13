import { Component, inject, OnInit } from '@angular/core';
import { MembersService } from '../../_services/members.service';
import { MemberCardComponent } from '../member-card/member-card.component';
import { PaginationModule } from 'ngx-bootstrap/pagination'
import { ButtonsModule } from 'ngx-bootstrap/buttons'
import { AccountService } from '../../_services/account.service';
import { UserParams } from '../../_models/user-params';
import { FormsModule } from '@angular/forms';
@Component({
  selector: 'app-member-list',
  standalone: true,
  imports: [MemberCardComponent, PaginationModule, FormsModule, ButtonsModule],
  templateUrl: './member-list.component.html',
  styleUrl: './member-list.component.css'
})
export class MemberListComponent implements OnInit {
  membersService = inject(MembersService);
  genderList = [{ value: 'male', display: 'Males' }, { value: 'female', display: 'Females' }]
  sortList = [{ value: 'created', display: 'Member since' }, { value: 'lastActive', display: 'Last Active' }]

  ngOnInit(): void {
    if (!this.membersService.paginatedResults()) {
      this.loadMembers();
    }
  }

  loadMembers() {
    this.membersService.getMembers();
  }

  resetFilters() {
    this.membersService.resetUserParams();
    this.loadMembers();
  }

  pageChanged(event: any) {
    if (this.membersService.userParams().pageNumber !== event.page) {
      this.membersService.userParams().pageNumber = event.page;
      this.loadMembers();
    }
  }
}
