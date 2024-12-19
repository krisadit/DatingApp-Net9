import { Component, inject } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { Photo } from '../../_models/photo';
import { AdminService } from '../../_services/admin.service';

@Component({
  selector: 'app-view-moderation-image',
  imports: [],
  templateUrl: './view-moderation-image.component.html',
  styleUrl: './view-moderation-image.component.css'
})
export class ViewModerationImageComponent {
  bsModalRef = inject(BsModalRef);
  private adminService = inject(AdminService);
  approvalExecuted = false;
  photo?: Photo;
  invalidTextShow = false;

  approvePhoto() {
    return this.adminService.approvePhoto(this.photo!.id).subscribe({
      next: () => {
        this.approvalExecuted = true;
        this.bsModalRef.hide();
      }
    });
  }

  rejectPhoto() {
    return this.adminService.rejectPhoto(this.photo!.id).subscribe({
      next: () => {
        this.approvalExecuted = true;
        this.bsModalRef.hide();
      }
    });
  }

  getTitle() {
    if (this.photo && this.photo.username) {
      return `${this.photo.username}'s Photo`;
    }
    return 'Image from unknown source!';
  }

  invalidPhoto() {
    this.invalidTextShow = true;
  }
}
