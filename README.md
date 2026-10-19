# Interview Question 05 — Queue Kiosk

A three-state queue kiosk for IT 05-1, IT 05-2, and IT 05-3. Tickets advance from A0 through Z9, display their issue time, and restart at A0 after a reset. The counter increment uses one atomic SQLite update inside a serializable transaction so concurrent requests cannot claim the same sequence.

## Run locally

Requires .NET 10, Node.js 22.22.3+, and npm 10.9.8+.

```powershell
dotnet run --project backend/Question005.Api/Question005.Api.csproj --launch-profile http
```

In a second terminal, run `npm ci` and `npm start` from `frontend`, then open `http://localhost:4204`. Angular proxies `/api` to `http://localhost:5220`. SQLite creates `backend/Question005.Api/queue.db` at first start.

## API

- `GET /api/queue/state` returns the displayed and next ticket numbers.
- `POST /api/queue/tickets/next` atomically claims and stores the next ticket.
- `GET /api/queue/tickets/{id}` returns a stored ticket.
- `POST /api/queue/reset` sets the displayed number to `00` and restarts the sequence at A0.

Reset keeps historical ticket rows; the running counter is reset independently.

## Verify

```powershell
dotnet test tests/backend/Question005.Api.Tests/Question005.Api.Tests.csproj --configuration Release
dotnet build backend/Question005.Api/Question005.Api.csproj --configuration Release
cd frontend
npm ci
npm test
npm run build
```

Results: backend **3 passed** (including 24 parallel ticket requests), Angular **3 passed**, and production Angular bundle **144.95 kB** initial (estimated transfer **42.04 kB**).

Package identity: `@example-com/interview-question-005`; namespace: `Example.Com.InterviewQuestion005`.
