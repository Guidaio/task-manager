import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs/operators';
import { AuthService } from '../../core/auth/auth.service';
import { apiErrorMessage } from '../../core/http-api-error';
import { SignalRNotificationsService } from '../../core/realtime/signalr-notifications.service';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly realtime = inject(SignalRNotificationsService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly demoEmail = 'demo@taskmanager.local';
  protected readonly demoPassword = 'Demo_user_12345';

  protected readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
  });

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.loading.set(true);
    this.error.set(null);
    this.auth
      .login(this.form.getRawValue())
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: () => {
          const raw = this.route.snapshot.queryParamMap.get('returnUrl');
          const returnUrl =
            raw !== null && raw.startsWith('/') && !raw.startsWith('//') ? raw : '/tasks';
          void this.router.navigateByUrl(returnUrl);
          // Do not block navigation on SignalR: hub.start() can hang while auth already succeeded.
          void this.realtime.primeConnection();
        },
        error: (err: HttpErrorResponse) => {
          this.error.set(apiErrorMessage(err, 'Login failed.'));
        },
      });
  }
}
