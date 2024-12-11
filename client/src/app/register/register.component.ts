import { Component, inject, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AccountService } from '../_services/account.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent {

  private accountService = inject(AccountService);
  private toastr = inject(ToastrService)

  cancelRegister = output<boolean>();

  model: any = {}

  register() {
    this.accountService.register(this.model).subscribe({
      next:
        (result) => {
          console.log(result);
          this.cancel();
        },
      error: err => this.toastr.error(err.error)
    });
  }

  cancel() {
    this.cancelRegister.emit(false);
  }
}
