import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { extractErrorMessage } from '../../../shared/http-error.util';
import { APP_USER_ROLES, APP_USER_ROLE_DESCRIPTIONS, AppUserRole, CreateUserRequest, UserDto } from '../user.model';
import { UsersApiService } from '../users-api.service';

@Component({
  selector: 'app-user-page',
  imports: [ReactiveFormsModule, FormsModule],
  templateUrl: './user-page.component.html',
  styleUrl: './user-page.component.scss',
})
export class UserPageComponent implements OnInit {
  private api = inject(UsersApiService);
  private fb = inject(FormBuilder);

  roles = APP_USER_ROLES;
  roleDescriptions = APP_USER_ROLE_DESCRIPTIONS;

  users = signal<UserDto[]>([]);
  showInactive = signal(false);

  createForm = this.fb.nonNullable.group({
    userName: ['', [Validators.required, Validators.maxLength(50)]],
    password: ['', [Validators.required, Validators.minLength(8)]],
    role: ['Worker' as AppUserRole, [Validators.required]],
  });
  createError = signal('');

  // Change-role: reveal panel (like Sale's refund confirmation), not plain confirm() - a role
  // change needs a value picked, not just a yes/no.
  changingRoleId = signal<number | null>(null);
  newRole = signal<AppUserRole>('Worker');
  roleError = signal('');

  // Reset-password: same reveal-panel shape - needs a typed value, and is sensitive enough
  // (per the task brief) to warrant a confirm() on top of the explicit "Set password" click.
  resettingPasswordId = signal<number | null>(null);
  newPassword = signal('');
  passwordError = signal('');

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.api.getAll(this.showInactive()).subscribe((rows) => this.users.set(rows));
  }

  toggleShowInactive(): void {
    this.showInactive.update((v) => !v);
    this.load();
  }

  add(): void {
    if (this.createForm.invalid) return;
    this.createError.set('');
    const req: CreateUserRequest = this.createForm.getRawValue();
    this.api.create(req).subscribe({
      next: () => {
        this.createForm.reset({ userName: '', password: '', role: 'Worker' });
        this.load();
      },
      error: (err: HttpErrorResponse) => this.createError.set(extractErrorMessage(err)),
    });
  }

  setActive(user: UserDto, isActive: boolean): void {
    const verb = isActive ? 'Reactivate' : 'Deactivate';
    if (!confirm(`${verb} user "${user.userName}"?`)) return;
    this.api.setActive(user.appUserId, { isActive }).subscribe(() => this.load());
  }

  startChangeRole(user: UserDto): void {
    this.changingRoleId.set(user.appUserId);
    this.newRole.set(user.role as AppUserRole);
    this.roleError.set('');
  }

  cancelChangeRole(): void {
    this.changingRoleId.set(null);
  }

  saveRole(user: UserDto): void {
    const role = this.newRole();
    if (role === user.role) {
      this.changingRoleId.set(null);
      return;
    }
    if (!confirm(`Change "${user.userName}"'s role from ${user.role} to ${role}?`)) return;

    this.roleError.set('');
    this.api.updateRole(user.appUserId, { role }).subscribe({
      next: () => {
        this.changingRoleId.set(null);
        this.load();
      },
      error: (err: HttpErrorResponse) => this.roleError.set(extractErrorMessage(err)),
    });
  }

  startResetPassword(user: UserDto): void {
    this.resettingPasswordId.set(user.appUserId);
    this.newPassword.set('');
    this.passwordError.set('');
  }

  cancelResetPassword(): void {
    this.resettingPasswordId.set(null);
  }

  saveNewPassword(user: UserDto): void {
    const password = this.newPassword();
    if (password.length < 8) {
      this.passwordError.set('Password must be at least 8 characters.');
      return;
    }
    if (!confirm(`Set a new password for "${user.userName}"? They will need to log in again.`)) return;

    this.passwordError.set('');
    this.api.resetPassword(user.appUserId, { newPassword: password }).subscribe({
      next: () => {
        this.resettingPasswordId.set(null);
        this.newPassword.set('');
      },
      error: (err: HttpErrorResponse) => this.passwordError.set(extractErrorMessage(err)),
    });
  }
}
