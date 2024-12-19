import { Component, inject, OnInit } from '@angular/core';
import { AdminService } from '../../_services/admin.service';
import { Photo } from '../../_models/photo';
import { FormsModule } from '@angular/forms';
import { TypeaheadModule } from 'ngx-bootstrap/typeahead';
import { MembersService } from '../../_services/members.service';
import { CommonModule } from '@angular/common';
import { BsModalRef, BsModalService, ModalOptions } from 'ngx-bootstrap/modal';
import { ViewModerationImageComponent } from '../../modals/view-moderation-image/view-moderation-image.component';

@Component({
  selector: 'app-photo-management',
  imports: [FormsModule, TypeaheadModule, CommonModule],
  templateUrl: './photo-management.component.html',
  styleUrl: './photo-management.component.css'
})
export class PhotoManagementComponent implements OnInit {
  photos: Photo[] = [];
  members: string[] = [];
  selectedMembers: string = "";

  private adminService = inject(AdminService);
  private modalService = inject(BsModalService);
  bsModalRef: BsModalRef<ViewModerationImageComponent> = new BsModalRef<ViewModerationImageComponent>();

  ngOnInit(): void {
    this.getPhotosForApproval();
  }

  getPhotosForApproval() {
    this.adminService.getPhotosForApproval().subscribe({
      next: (photos) => {
        this.photos = photos;
        this.members = photos.map((photo) => {
          return photo.username ? photo.username : 'no user'
        })
      }
    })
  }

  approvePhoto(photoId: number) {
    return this.adminService.approvePhoto(photoId).subscribe({
      next: () => this.removePhotoFromList(photoId)
    });
  }

  rejectPhoto(photoId: number) {
    return this.adminService.rejectPhoto(photoId).subscribe({
      next: () => this.removePhotoFromList(photoId)
    });
  }

  viewPhoto(photo: Photo) {
    const initialState: ModalOptions = {
      class: 'modal-lg',
      initialState: {
        photo: photo,
        approvalExecuted: false
      }
    }

    this.bsModalRef = this.modalService.show(ViewModerationImageComponent, initialState);
    this.bsModalRef.onHide?.subscribe({
      next: () => {
        if (this.bsModalRef.content && this.bsModalRef.content.approvalExecuted) {
          this.removePhotoFromList(photo.id);
        }
      }
    })
  }

  private removePhotoFromList(photoId: number) {
    this.photos.splice(this.photos.findIndex(p => p.id === photoId), 1);
  }
}
