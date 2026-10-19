namespace Example.Com.InterviewQuestion005.Entities;

public sealed class QueueTicket
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; }
}
