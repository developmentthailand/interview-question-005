using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Example.Com.InterviewQuestion005.Tests;

public sealed class QueueApiTests
{
    [Fact]
    public async Task Next_ticket_rolls_from_A9_to_B0()
    {
        using var factory = new QueueApiFactory();
        using var client = factory.CreateClient();
        string? number = null;

        for (var index = 0; index < 11; index++)
        {
            using var response = await client.PostAsync("/api/queue/tickets/next", content: null);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            number = body.RootElement.GetProperty("number").GetString();
        }

        Assert.Equal("B0", number);
    }

    [Fact]
    public async Task Reset_displays_00_and_next_receive_starts_at_A0()
    {
        using var factory = new QueueApiFactory();
        using var client = factory.CreateClient();
        await client.PostAsync("/api/queue/tickets/next", content: null);

        using var reset = await client.PostAsync("/api/queue/reset", content: null);
        Assert.Equal(HttpStatusCode.OK, reset.StatusCode);
        using var resetBody = JsonDocument.Parse(await reset.Content.ReadAsStringAsync());
        Assert.Equal("00", resetBody.RootElement.GetProperty("currentNumber").GetString());

        using var next = await client.PostAsync("/api/queue/tickets/next", content: null);
        using var nextBody = JsonDocument.Parse(await next.Content.ReadAsStringAsync());
        Assert.Equal("A0", nextBody.RootElement.GetProperty("number").GetString());
    }

    [Fact]
    public async Task Concurrent_receives_get_distinct_ticket_numbers()
    {
        using var factory = new QueueApiFactory();
        var requests = Enumerable.Range(0, 24).Select(async _ =>
        {
            using var client = factory.CreateClient();
            using var response = await client.PostAsync("/api/queue/tickets/next", content: null);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var ticket = await response.Content.ReadFromJsonAsync<TicketDto>();
            Assert.NotNull(ticket);
            return ticket.Number;
        });

        var numbers = await Task.WhenAll(requests);

        Assert.Equal(24, numbers.Distinct(StringComparer.Ordinal).Count());
    }

    private sealed record TicketDto(int Id, string Number, DateTimeOffset CreatedAtUtc);
}
