using Example.Com.InterviewQuestion005.Entities;
using Microsoft.EntityFrameworkCore;

namespace Example.Com.InterviewQuestion005.Data;

public sealed class QueueDbContext(DbContextOptions<QueueDbContext> options) : DbContext(options)
{
    public DbSet<QueueState> States => Set<QueueState>();
    public DbSet<QueueTicket> Tickets => Set<QueueTicket>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<QueueState>().ToTable("QueueStates").HasKey(state => state.Id);
        modelBuilder.Entity<QueueTicket>().ToTable("QueueTickets").HasIndex(ticket => ticket.CreatedAtUtc);
        modelBuilder.Entity<QueueTicket>().Property(ticket => ticket.Number).HasMaxLength(2).IsRequired();
    }
}
