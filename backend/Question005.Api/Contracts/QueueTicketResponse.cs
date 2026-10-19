namespace Example.Com.InterviewQuestion005.Contracts;

public sealed record QueueTicketResponse(int Id, string Number, DateTimeOffset CreatedAtUtc);
