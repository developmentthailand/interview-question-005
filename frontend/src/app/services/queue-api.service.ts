import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

export interface QueueState {
  currentNumber: string;
  nextNumber: string;
}

export interface QueueTicket {
  id: number;
  number: string;
  createdAtUtc: string;
}

@Injectable({ providedIn: 'root' })
export class QueueApiService {
  private readonly http = inject(HttpClient);

  state(): Observable<QueueState> {
    return this.http.get<QueueState>('/api/queue/state');
  }

  takeNext(): Observable<QueueTicket> {
    return this.http.post<QueueTicket>('/api/queue/tickets/next', null);
  }

  reset(): Observable<QueueState> {
    return this.http.post<QueueState>('/api/queue/reset', null);
  }
}
