import { DatePipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { extractErrorMessage } from '../../../shared/http-error.util';
import { LocationDto } from '../../locations/location.model';
import { LocationsApiService } from '../../locations/locations-api.service';
import { TillSessionDto } from '../../till-sessions/till-session.model';
import { TillSessionsApiService } from '../../till-sessions/till-sessions-api.service';

/**
 * The single most important screen in FarmApp: card-only checkout, meant to run on a laptop at a
 * real market stall (doc 01 Module 4). Kept as one component rather than split into a multi-route
 * wizard (unlike stock-takes' start/detail split) - a cashier mid-sale needs the till-session
 * state, product grid, basket, and checkout all reachable without a navigation, per the task
 * brief's "big-button, touch-friendly... used with dirty hands in a hurry" framing.
 *
 * This first slice is just the till-session gate (task brief step 1): find or open a TillSession
 * at a chosen Location before anything else is reachable. The grid/basket/checkout land in
 * follow-up commits.
 */
@Component({
  selector: 'app-pos-page',
  imports: [DatePipe, FormsModule],
  templateUrl: './pos-page.component.html',
  styleUrl: './pos-page.component.scss',
})
export class PosPageComponent implements OnInit {
  private tillSessionsApi = inject(TillSessionsApiService);
  private locationsApi = inject(LocationsApiService);

  gateLoading = signal(true);
  gateError = signal('');

  // Every currently-open till session, across all locations - usually zero or one, but
  // TillSessionService allows one per location at a time, so a second cashier at a different
  // location could legitimately have one open too.
  openSessions = signal<TillSessionDto[]>([]);
  locations = signal<LocationDto[]>([]);
  locationsById = computed(() => new Map(this.locations().map((l) => [l.locationId, l])));

  tillSession = signal<TillSessionDto | null>(null);

  openLocationId = signal<number | null>(null);
  opening = signal(false);
  openError = signal('');

  ngOnInit(): void {
    this.locationsApi.getAll().subscribe((rows) => this.locations.set(rows));
    this.loadGate();
  }

  private loadGate(): void {
    this.gateLoading.set(true);
    this.gateError.set('');
    this.tillSessionsApi.getAll(null, true).subscribe({
      next: (sessions) => {
        this.gateLoading.set(false);
        this.openSessions.set(sessions);
        // Exactly one open session anywhere - the overwhelmingly common real case (one cashier,
        // one stall) - skips straight to the sell screen with no extra tap.
        if (sessions.length === 1) this.tillSession.set(sessions[0]);
      },
      error: (err: HttpErrorResponse) => {
        this.gateLoading.set(false);
        this.gateError.set(extractErrorMessage(err));
      },
    });
  }

  locationName(id: number): string {
    return this.locationsById().get(id)?.name ?? `#${id}`;
  }

  useSession(session: TillSessionDto): void {
    this.tillSession.set(session);
  }

  openSession(): void {
    const locationId = this.openLocationId();
    if (!locationId) return;

    this.opening.set(true);
    this.openError.set('');
    this.tillSessionsApi.open({ locationId }).subscribe({
      next: (session) => {
        this.opening.set(false);
        this.tillSession.set(session);
      },
      error: (err: HttpErrorResponse) => {
        this.opening.set(false);
        this.openError.set(extractErrorMessage(err));
      },
    });
  }

  // Lets a cashier back out of the sell screen to switch sessions (e.g. picked the wrong location)
  // without a page reload - re-checks what's actually open rather than trusting stale state.
  changeSession(): void {
    this.tillSession.set(null);
    this.loadGate();
  }
}
