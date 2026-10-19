using Example.Com.InterviewQuestion005.Data;
using Example.Com.InterviewQuestion005.Entities;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddDbContext<QueueDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Queue") ?? "Data Source=queue.db;Default Timeout=30"));
builder.Services.AddOpenApi();

var app = builder.Build();
await using (var scope = app.Services.CreateAsyncScope())
{
    var database = scope.ServiceProvider.GetRequiredService<QueueDbContext>();
    await database.Database.EnsureCreatedAsync();
    if (!await database.States.AnyAsync())
    {
        database.States.Add(new QueueState());
        await database.SaveChangesAsync();
    }
}

if (app.Environment.IsDevelopment()) app.MapOpenApi();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }
