using System.Collections.Concurrent;
using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

[Authorize] // Now SignalR is going to authorize to this endpoint.
public class NotificationHub : Hub
{
    /*
    But signal R itself doesn't keep track of which email addresses or which user object is connected.
    It just keeps track of a client connection ID, and that's what the browser uses to maintain the connection with our SignalR Service.
    Now, in order for us to keep track of who is connected by their email, then we're going to need to store that inside our hub.
    So, it's going to be stored in memory. And typically for scalability, this might be something that you would want to use Redis for rather than doing it the way I'm about to demonstrate.
    But for simplicity, and because of the fact we're only going to have a single server, then we're going to use a dictionary for this.
    But a much more scalable option would be to use Redis, because then we could have multiple servers.
    And if we're storing our connection IDs and the email addresses inside Redis, then we could have 100 different frontend web servers.
    But because Redis is storing the data for the connection IDs, it doesn't matter which server the user would hit, we'd be able to send them a notification whichever server they are connected to, because we could get the information from there.
    We're not going to do it that way, and I'll demonstrate how we are going to do it.
    We're going to create a property in here, a private static read only field, and we're going to use the concurrent dictionary to store this information.
    */
    private static readonly ConcurrentDictionary<string, string> UserConnections = new();

    public override Task OnConnectedAsync()
    {
        var email = Context.User?.GetEmail();

        if (!string.IsNullOrEmpty(email)) UserConnections[email] = Context.ConnectionId;

        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        var email = Context.User?.GetEmail();

        if (!string.IsNullOrEmpty(email)) UserConnections.TryRemove(email, out _);

        return base.OnDisconnectedAsync(exception);
    }

    public static string? GetConnectionIdByEmail(string email)
    {
        UserConnections.TryGetValue(email, out var connectionId);

        return connectionId;
    }
}
