import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../auth.service';

@Component({
  selector: 'app-login-page',
  imports: [ReactiveFormsModule],
  templateUrl: './login-page.component.html',
  styleUrl: './login-page.component.scss',
})
export class LoginPageComponent {
  private auth = inject(AuthService);
  private router = inject(Router);
  private fb = inject(FormBuilder);

  errorMessage = signal('');
  submitting = signal(false);

  form = this.fb.nonNullable.group({
    userName: ['', Validators.required],
    password: ['', Validators.required],
  });

  submit(): void {
    if (this.form.invalid) return;
    this.errorMessage.set('');
    this.submitting.set(true);
    const { userName, password } = this.form.getRawValue();
    this.auth.login(userName, password).subscribe({
      next: () => {
        this.submitting.set(false);
        this.router.navigateByUrl('/');
      },
      error: (err) => {
        this.submitting.set(false);
        this.errorMessage.set(
          err.status === 401 ? 'Incorrect username or password.' : 'Login failed. Try again.',
        );
      },
    });
  }
}
