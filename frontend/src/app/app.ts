import { Component, inject, signal } from '@angular/core';
import { finalize } from 'rxjs';
import { QueueApiService, QueueState, QueueTicket } from './services/queue-api.service';

type KioskView = 'receive' | 'ticket' | 'reset';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  private readonly api = inject(QueueApiService);
  protected readonly view = signal<KioskView>('receive');
  protected readonly queueState = signal<QueueState | null>(null);
  protected readonly ticket = signal<QueueTicket | null>(null);
  protected readonly loadingState = signal(true);
  protected readonly busy = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  constructor() {
    this.refreshState();
  }

  protected currentNumber(): string {
    return this.queueState()?.currentNumber ?? '00';
  }

  protected nextNumber(): string {
    return this.queueState()?.nextNumber ?? 'A0';
  }

  protected receiveTicket(): void {
    if (this.busy()) return;
    this.errorMessage.set(null);
    this.busy.set(true);
    this.api
      .takeNext()
      .pipe(finalize(() => this.busy.set(false)))
      .subscribe({
        next: (ticket) => {
          this.ticket.set(ticket);
          this.view.set('ticket');
          this.refreshState();
        },
        error: () => this.errorMessage.set('รับบัตรคิวไม่สำเร็จ กรุณาลองอีกครั้ง'),
      });
  }

  protected clearQueue(): void {
    if (this.busy()) return;
    this.errorMessage.set(null);
    this.busy.set(true);
    this.api
      .reset()
      .pipe(finalize(() => this.busy.set(false)))
      .subscribe({
        next: (state) => {
          this.queueState.set(state);
          this.ticket.set(null);
          this.view.set('reset');
        },
        error: () => this.errorMessage.set('ล้างคิวไม่สำเร็จ กรุณาลองอีกครั้ง'),
      });
  }

  protected returnToReceive(): void {
    this.view.set('receive');
    this.ticket.set(null);
  }

  protected ticketTime(): string {
    const createdAt = this.ticket()?.createdAtUtc;
    if (!createdAt) return '';
    return new Intl.DateTimeFormat('th-TH', { dateStyle: 'long', timeStyle: 'short' }).format(new Date(createdAt));
  }

  private refreshState(): void {
    this.loadingState.set(true);
    this.api
      .state()
      .pipe(finalize(() => this.loadingState.set(false)))
      .subscribe({
        next: (state) => this.queueState.set(state),
        error: () => this.errorMessage.set('ไม่สามารถโหลดสถานะคิวได้ กรุณารีเฟรชหน้า'),
      });
  }
}
