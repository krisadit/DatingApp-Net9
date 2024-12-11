import { CanActivateFn, Router } from '@angular/router';
import { AccountService } from '../_services/account.service';
import { inject } from '@angular/core';
import { ToastrService } from 'ngx-toastr';

export const authGuard: CanActivateFn = (route, state) => {
  const accountService = inject(AccountService);
  const toastr = inject(ToastrService);
  const router = inject(Router)

  if (accountService.currentUser()) {
    return true;
  } else {
    router.navigateByUrl('/').then(_ => toastr.error("You shall not pass!"));
  }
  return false;
};
