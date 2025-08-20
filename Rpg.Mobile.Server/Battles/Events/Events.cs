using System.Drawing;
using Rpg.Mobile.GameSdk.StateManagement;

namespace Rpg.Mobile.Server.Battles.Events;

public record TileHoveredEvent(Point Tile) : IEvent;
public record TileClickedEvent(Point Tile) : IEvent;
public record MiniMapClickedEvent(PointF Position) : IEvent;
