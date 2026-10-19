import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it } from 'vitest';
import { App } from './app';
import { appConfig } from './app.config';

describe('Queue kiosk flow', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [...(appConfig.providers ?? []), provideHttpClientTesting()],
    }).compileComponents();
  });

  afterEach(() => TestBed.inject(HttpTestingController).verify());

  it('shows the empty queue and next ticket number from the API', () => {
    const { fixture, root, http } = createPage();
    http.expectOne('/api/queue/state').flush({ currentNumber: '00', nextNumber: 'A0' });
    fixture.detectChanges();

    expect(root.textContent).toContain('00');
    expect(root.textContent).toContain('A0');
    expect(root.querySelector<HTMLButtonElement>('.receive-button')?.disabled).toBe(false);
  });

  it('requests the next ticket and opens the ticket display', () => {
    const { fixture, root, http } = createPage();
    http.expectOne('/api/queue/state').flush({ currentNumber: 'A9', nextNumber: 'B0' });
    fixture.detectChanges();
    findButton(root, 'รับบัตรคิว').click();

    const request = http.expectOne('/api/queue/tickets/next');
    expect(request.request.method).toBe('POST');
    request.flush({ id: 10, number: 'B0', createdAtUtc: '2026-10-20T00:15:00Z' });
    http.expectOne('/api/queue/state').flush({ currentNumber: 'B0', nextNumber: 'B1' });
    fixture.detectChanges();

    expect(root.textContent).toContain('หมายเลขคิวของคุณ');
    expect(root.querySelector('.ticket-number')?.textContent?.trim()).toBe('B0');
    expect(root.querySelector<HTMLButtonElement>('.return-button')).not.toBeNull();
  });

  it('clears to 00, then returns to the receive screen', () => {
    const { fixture, root, http } = createPage();
    http.expectOne('/api/queue/state').flush({ currentNumber: 'C4', nextNumber: 'C5' });
    fixture.detectChanges();
    findButton(root, 'ล้างคิว').click();

    const request = http.expectOne('/api/queue/reset');
    expect(request.request.method).toBe('POST');
    request.flush({ currentNumber: '00', nextNumber: 'A0' });
    fixture.detectChanges();

    expect(root.querySelector('.reset-number')?.textContent?.trim()).toBe('00');
    findButton(root, 'กลับหน้ารับบัตรคิว').click();
    fixture.detectChanges();
    expect(root.querySelector('.receive-button')).not.toBeNull();
  });
});

function createPage(): { fixture: ReturnType<typeof TestBed.createComponent<App>>; root: HTMLElement; http: HttpTestingController } {
  const fixture = TestBed.createComponent(App);
  fixture.detectChanges();
  return { fixture, root: fixture.nativeElement as HTMLElement, http: TestBed.inject(HttpTestingController) };
}

function findButton(root: HTMLElement, label: string): HTMLButtonElement {
  const button = Array.from(root.querySelectorAll<HTMLButtonElement>('button')).find((candidate) => candidate.textContent?.includes(label));
  if (!button) throw new Error(`Button with text "${label}" was not rendered.`);
  return button;
}
