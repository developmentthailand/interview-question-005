using System.Data;
using System.Globalization;
using Example.Com.InterviewQuestion005.Contracts;
using Example.Com.InterviewQuestion005.Data;
using Example.Com.InterviewQuestion005.Entities;
using Example.Com.InterviewQuestion005.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Example.Com.InterviewQuestion005.Controllers;

[ApiController]
[Route("api/queue")]
public sealed class QueueController(QueueDbContext database) : ControllerBase
{
    [HttpGet("state")]
    public async Task<ActionResult<QueueStateResponse>> GetState(CancellationToken cancellationToken)
    {
        var sequence = await database.States.AsNoTracking()
            .Where(state => state.Id == 1)
            .Select(state => state.CurrentSequence)
            .SingleAsync(cancellationToken);
        var current = sequence < 0 ? "00" : QueueNumber.FromSequence(sequence);
        var next = QueueNumber.FromSequence((sequence + 1 + QueueNumber.SequenceLength) % QueueNumber.SequenceLength);
        return Ok(new QueueStateResponse(current, next));
    }

    [HttpPost("tickets/next")]
    [ProducesResponseType<QueueTicketResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> TakeNext(CancellationToken cancellationToken)
    {
        await using var transaction = await database.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var connection = database.Database.GetDbConnection();
        await using var command = connection.CreateCommand();
        command.Transaction = transaction.GetDbTransaction();
        command.CommandText = "UPDATE QueueStates SET CurrentSequence = (CurrentSequence + 1) % 260 WHERE Id = 1 RETURNING CurrentSequence;";
        var result = await command.ExecuteScalarAsync(cancellationToken);
        if (result is null or DBNull) throw new InvalidOperationException("The queue counter is missing.");

        var sequence = Convert.ToInt32(result, CultureInfo.InvariantCulture);
        var ticket = new QueueTicket
        {
            Number = QueueNumber.FromSequence(sequence),
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        database.Tickets.Add(ticket);
        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return CreatedAtAction(nameof(GetTicket), new { id = ticket.Id }, ToResponse(ticket));
    }

    [HttpPost("reset")]
    public async Task<ActionResult<QueueResetResponse>> Reset(CancellationToken cancellationToken)
    {
        await using var transaction = await database.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        await database.States.Where(state => state.Id == 1)
            .ExecuteUpdateAsync(setters => setters.SetProperty(state => state.CurrentSequence, -1), cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Ok(new QueueResetResponse("00", "A0"));
    }

    [HttpGet("tickets/{id:int}")]
    public async Task<ActionResult<QueueTicketResponse>> GetTicket(int id, CancellationToken cancellationToken)
    {
        var ticket = await database.Tickets.AsNoTracking().SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        return ticket is null ? NotFound() : Ok(ToResponse(ticket));
    }

    private static QueueTicketResponse ToResponse(QueueTicket ticket) =>
        new(ticket.Id, ticket.Number, ticket.CreatedAtUtc);
}
