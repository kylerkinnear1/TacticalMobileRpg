using System.Text;
using System.Text.Json;
using Rpg.Mobile.Api.Battles.Data;
using Rpg.Mobile.App.Game.Lobby;
using Rpg.Mobile.App.Game.MainBattle;
using Rpg.Mobile.App.Game.UserInterface;
using Rpg.Mobile.GameSdk.Core;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Utilities;

namespace Rpg.Mobile.App.Game.Networking;

public class NetworkMonitorComponent : ComponentBase, IDisposable
{
    private record NetworkEvent(DateTime Timestamp, string Type, string Payload);
    
    private readonly List<NetworkEvent> _events = new();
    private readonly IEventBus _bus;
    private readonly IGameLoop _game;
    private readonly BattleData _data;
    
    private readonly ButtonComponent _prevButton;
    private readonly ButtonComponent _nextButton;
    private readonly ButtonComponent _copyButton;
    private readonly TextboxComponent _pageInfo;
    private readonly TextboxComponent _statusMessage;
    private ISubscription[] _subscriptions = [];
    
    private int _currentPage = 0;
    private const int EventsPerPage = 5;
    private readonly JsonSerializerOptions _jsonOptions;
    private string _statusText = "";
    private DateTime? _statusMessageTime;
    private const double StatusMessageDurationSeconds = 3.0;

    public Color BackColor { get; set; } = Colors.Black.WithAlpha(0.9f);
    public Color TextColor { get; set; } = Colors.Lime;
    public Color HeaderColor { get; set; } = Colors.Cyan;
    public float FontSize { get; set; } = 10f;
    public float HeaderFontSize { get; set; } = 12f;

    public NetworkMonitorComponent(
        IEventBus bus, 
        IGameLoop game, 
        RectF bounds,
        BattleData data) : base(bounds)
    {
        _bus = bus;
        _game = game;
        _data = data;
        
        IgnoreCamera = true;
        
        _jsonOptions = new JsonSerializerOptions 
        { 
            WriteIndented = true,
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
        };

        // Navigation buttons
        _prevButton = AddChild(new ButtonComponent(
            new RectF(10f, bounds.Height - 40f, 60f, 30f),
            "< Prev",
            OnPrevClick)
        {
            BackColor = Colors.DarkSlateGray,
            TextColor = Colors.White,
            FontSize = 12f
        });

        _nextButton = AddChild(new ButtonComponent(
            new RectF(80f, bounds.Height - 40f, 60f, 30f),
            "Next >",
            OnNextClick)
        {
            BackColor = Colors.DarkSlateGray,
            TextColor = Colors.White,
            FontSize = 12f
        });

        // Copy to clipboard button
        _copyButton = AddChild(new ButtonComponent(
            new RectF(bounds.Width - 110f, bounds.Height - 40f, 100f, 30f),
            "📋 Copy All",
            OnCopyClick)
        {
            BackColor = Colors.DarkGreen,
            TextColor = Colors.White,
            FontSize = 12f
        });

        _pageInfo = AddChild(new TextboxComponent(
            new RectF(150f, bounds.Height - 40f, 100f, 30f))
        {
            BackColor = Colors.Transparent,
            TextColor = Colors.White,
            FontSize = 12f
        });

        // Status message for copy feedback
        _statusMessage = AddChild(new TextboxComponent(
            new RectF(bounds.Width - 220f, bounds.Height - 75f, 210f, 25f))
        {
            BackColor = Colors.DarkGreen.WithAlpha(0.8f),
            TextColor = Colors.White,
            FontSize = 11f,
            Visible = false
        });

        SubscribeToEvents();
    }
    
    private void SubscribeToEvents()
    {
        _subscriptions =
        [
            // Lobby Network Events (from LobbyNetwork)
            _bus.Subscribe<LobbyNetwork.GameStartedEvent>(e => 
                AddEvent("🌐 GameStarted", new { 
                    e.GameId, 
                    MapSize = $"{e.Battle.Map.Width()}x{e.Battle.Map.Height()}",
                    Team0Count = e.Battle.Team0.Count,
                    Team1Count = e.Battle.Team1.Count
                })),
            _bus.Subscribe<LobbyNetwork.GameEndedEvent>(e => 
                AddEvent("🌐 GameEnded", new { e.GameId })),
            
            // Client -> Server events (outgoing)
            _bus.Subscribe<LobbyScene.JoinGameClickedEvent>(e => 
                AddEvent("📤 JoinGame", new { e.Team })),
            
            // Battle Network Events (from BattleNetwork)
            _bus.Subscribe<BattleNetwork.SetupStartedEvent>(e => 
                AddEvent("🌐 SetupStarted", new { 
                    CurrentPlaceOrder = e.SetupData.CurrentPlaceOrder,
                    PlaceOrderCount = e.SetupData.PlaceOrderIds.Count,
                    Units = e.SetupData.PlaceOrderIds
                        .Select(u => _data.Units.Single(x => x.UnitId == u))
                        .Select(u => new {
                        u.PlayerId,
                        u.Stats.UnitType,
                        HP = u.Stats.MaxHealth,
                        MP = u.Stats.MaxMp
                    })
                })),
            
            _bus.Subscribe<BattleNetwork.UnitPlacedEvent>(e => 
                AddEvent("🌐 UnitPlaced", new { 
                    e.Tile,
                    Unit = new { 
                        e.Unit.PlayerId, 
                        e.Unit.Stats.UnitType,
                        HP = $"{e.Unit.RemainingHealth}/{e.Unit.Stats.MaxHealth}",
                        MP = $"{e.Unit.RemainingMp}/{e.Unit.Stats.MaxMp}"
                    }
                })),
            
            _bus.Subscribe<BattleNetwork.UnitsDamagedEvent>(e => 
                AddEvent("🌐 UnitsDamaged", new { 
                    DamagedCount = e.DamagedUnits.Count,
                    DefeatedCount = e.DefeatedUnits.Count,
                    Damages = e.DamagedUnits.Select(d => new { 
                        UnitType = d.Unit.Stats.UnitType,
                        PlayerId = d.Unit.PlayerId,
                        Damage = d.Damage,
                        RemainingHP = d.Unit.RemainingHealth
                    }),
                    Defeated = e.DefeatedUnits.Select(u => new {
                        u.Stats.UnitType,
                        u.PlayerId
                    })
                }))
        ];
    }

    private void AddEvent(string type, object payload)
    {
        try
        {
            var json = JsonSerializer.Serialize(payload, _jsonOptions);
            _events.Add(new NetworkEvent(DateTime.Now, type, json));
            
            // Keep only last 100 events
            if (_events.Count > 100)
                _events.RemoveAt(0);
                
            // Jump to latest page when new event arrives
            _currentPage = Math.Max(0, (_events.Count - 1) / EventsPerPage);
        }
        catch (Exception ex)
        {
            _events.Add(new NetworkEvent(DateTime.Now, type, $"[Serialization Error: {ex.Message}]"));
        }
    }

    private void OnPrevClick(IEnumerable<PointF> touches)
    {
        if (_currentPage > 0)
            _currentPage--;
    }

    private void OnNextClick(IEnumerable<PointF> touches)
    {
        var maxPage = Math.Max(0, (_events.Count - 1) / EventsPerPage);
        if (_currentPage < maxPage)
            _currentPage++;
    }

    private void OnCopyClick(IEnumerable<PointF> touches)
    {
        if (_game == null)
        {
            ShowStatusMessage("Copy requires IGameLoop", Colors.Red);
            return;
        }

        var logContent = BuildFullLog();
        
        // Use the async pattern with ContinueWith and game.Execute
#if WINDOWS || MACCATALYST || IOS || ANDROID
        Microsoft.Maui.ApplicationModel.DataTransfer.Clipboard
            .SetTextAsync(logContent)
            .ContinueWith(task => 
            {
                _game.Execute(() =>
                {
                    if (task.IsCompletedSuccessfully)
                    {
                        ShowStatusMessage("✓ Copied to clipboard!", Colors.DarkGreen);
                    }
                    else
                    {
                        ShowStatusMessage("✗ Copy failed", Colors.Red);
                    }
                });
            });
#else
        ShowStatusMessage("✗ Clipboard not supported on this platform", Colors.Red);
#endif
    }

    private string BuildFullLog()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== Network Events Log ===");
        sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Total Events: {_events.Count}");
        sb.AppendLine();

        foreach (var evt in _events)
        {
            sb.AppendLine($"[{evt.Timestamp:HH:mm:ss.fff}] {evt.Type}");
            sb.AppendLine(evt.Payload);
            sb.AppendLine("---");
        }

        return sb.ToString();
    }

    private void ShowStatusMessage(string message, Color backgroundColor)
    {
        _statusText = message;
        _statusMessage.Label = message;
        _statusMessage.BackColor = backgroundColor.WithAlpha(0.8f);
        _statusMessage.Visible = true;
        _statusMessageTime = DateTime.Now;
    }

    public override void Update(float deltaTime)
    {
        var totalPages = Math.Max(1, (_events.Count + EventsPerPage - 1) / EventsPerPage);
        _pageInfo.Label = $"Page {_currentPage + 1}/{totalPages}";
        
        _prevButton.Visible = _currentPage > 0;
        _nextButton.Visible = _currentPage < totalPages - 1;

        // Hide status message after duration
        if (_statusMessageTime.HasValue && 
            (DateTime.Now - _statusMessageTime.Value).TotalSeconds > StatusMessageDurationSeconds)
        {
            _statusMessage.Visible = false;
            _statusMessageTime = null;
        }
    }

    public override void Render(ICanvas canvas, RectF dirtyRect)
    {
        // Background
        canvas.FillColor = BackColor;
        canvas.FillRoundedRectangle(0, 0, Bounds.Width, Bounds.Height, 5f);
        
        // Border
        canvas.StrokeColor = HeaderColor.WithAlpha(0.5f);
        canvas.StrokeSize = 1f;
        canvas.DrawRoundedRectangle(0, 0, Bounds.Width, Bounds.Height, 5f);
        
        // Title
        canvas.FontColor = HeaderColor;
        canvas.FontSize = HeaderFontSize;
        canvas.Font = DefaultFont.ExtraBold;
        canvas.DrawString("Network Events Monitor", 10f, 10f, Bounds.Width - 20f, 20f, 
            HorizontalAlignment.Left, VerticalAlignment.Top);
        
        // Events
        var startIndex = _currentPage * EventsPerPage;
        var endIndex = Math.Min(startIndex + EventsPerPage, _events.Count);
        var yOffset = 35f;
        
        canvas.Font = DefaultFont.Normal;
        
        for (var i = startIndex; i < endIndex; i++)
        {
            var evt = _events[i];
            
            // Event header with icon
            canvas.FontSize = FontSize;
            canvas.FontColor = HeaderColor;
            var timeStr = evt.Timestamp.ToString("HH:mm:ss.fff");
            canvas.DrawString($"[{timeStr}] {evt.Type}", 
                10f, yOffset, Bounds.Width - 20f, 15f,
                HorizontalAlignment.Left, VerticalAlignment.Top);
            
            // Event payload (truncated for display)
            canvas.FontColor = TextColor;
            canvas.FontSize = FontSize - 1f;
            
            var lines = evt.Payload.Split('\n').Take(3);
            var lineY = yOffset + 15f;
            foreach (var line in lines)
            {
                var displayLine = line.Length > 80 ? line.Substring(0, 77) + "..." : line;
                canvas.DrawString(displayLine, 
                    20f, lineY, Bounds.Width - 30f, 12f,
                    HorizontalAlignment.Left, VerticalAlignment.Top);
                lineY += 12f;
            }
            
            yOffset += 60f;
            
            // Separator
            if (i < endIndex - 1)
            {
                canvas.StrokeColor = TextColor.WithAlpha(0.2f);
                canvas.StrokeSize = 0.5f;
                canvas.DrawLine(10f, yOffset - 5f, Bounds.Width - 10f, yOffset - 5f);
            }
        }
        
        if (_events.Count == 0)
        {
            canvas.FontColor = TextColor.WithAlpha(0.5f);
            canvas.FontSize = FontSize;
            canvas.DrawString("No network events captured yet...",
                10f, 50f, Bounds.Width - 20f, 20f,
                HorizontalAlignment.Center, VerticalAlignment.Top);
        }
    }

    public void Dispose()
    {
        _subscriptions.DisposeAll();
    }
}