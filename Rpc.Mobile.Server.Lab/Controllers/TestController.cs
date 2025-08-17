using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Client;

namespace Rpg.TestClient.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private static HubConnection? _connection;
    private static readonly List<string> _receivedMessages = new();

    [HttpPost("connect")]
    public async Task<IActionResult> Connect()
    {
        if (_connection != null)
        {
            await _connection.DisposeAsync();
        }

        _connection = new HubConnectionBuilder()
            .WithUrl("https://localhost:5004/battle-hub") // Your SignalR server URL
            .Build();

        // Set up message handlers
        _connection.On<string, string>("ReceiveMessage", (user, message) =>
        {
            var msg = $"[{DateTime.Now:HH:mm:ss}] {user}: {message}";
            _receivedMessages.Add(msg);
            Console.WriteLine($"Received: {msg}");
        });

        _connection.On<string>("UserConnected", (connectionId) =>
        {
            var msg = $"[{DateTime.Now:HH:mm:ss}] User {connectionId} connected";
            _receivedMessages.Add(msg);
            Console.WriteLine(msg);
        });

        _connection.On<string>("UserDisconnected", (connectionId) =>
        {
            var msg = $"[{DateTime.Now:HH:mm:ss}] User {connectionId} disconnected";
            _receivedMessages.Add(msg);
            Console.WriteLine(msg);
        });

        _connection.On<string>("UserJoined", (connectionId) =>
        {
            var msg = $"[{DateTime.Now:HH:mm:ss}] User {connectionId} joined game";
            _receivedMessages.Add(msg);
            Console.WriteLine(msg);
        });

        await _connection.StartAsync();
        return Ok(new
        {
            message = "Connected successfully!",
            connectionId = _connection.ConnectionId,
            state = _connection.State.ToString()
        });
    }

    [HttpPost("disconnect")]
    public async Task<IActionResult> Disconnect()
    {
        if (_connection == null)
        {
            return BadRequest("No active connection");
        }

        await _connection.DisposeAsync();
        _connection = null;
        return Ok("Disconnected successfully");
    }

    [HttpPost("send-message")]
    public async Task<IActionResult> SendMessage(string user, string message)
    {
        if (_connection?.State != HubConnectionState.Connected)
        {
            return BadRequest("Not connected. Call /connect first.");
        }

        await _connection.InvokeAsync("SendMessage", user, message);
        return Ok($"Message sent: {user} said '{message}'");
    }

    [HttpPost("join-game")]
    public async Task<IActionResult> JoinGame(string gameId)
    {
        if (_connection?.State != HubConnectionState.Connected)
        {
            return BadRequest("Not connected. Call /connect first.");
        }

        await _connection.InvokeAsync("JoinGame", gameId);
        return Ok($"Joined game: {gameId}");
    }

    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        return Ok(new
        {
            isConnected = _connection?.State == HubConnectionState.Connected,
            connectionId = _connection?.ConnectionId,
            state = _connection?.State.ToString() ?? "No connection",
            receivedMessagesCount = _receivedMessages.Count
        });
    }

    [HttpGet("messages")]
    public IActionResult GetMessages()
    {
        return Ok(_receivedMessages);
    }

    [HttpDelete("clear-messages")]
    public IActionResult ClearMessages()
    {
        _receivedMessages.Clear();
        return Ok("Messages cleared");
    }
}