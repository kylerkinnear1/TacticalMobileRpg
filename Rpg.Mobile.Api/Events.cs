using System.Drawing;
using Rpg.Mobile.Api.Battles.Data;
using Rpg.Mobile.GameSdk.StateManagement;

namespace Rpg.Mobile.Api;

public record UnitPlacedEvent(
    Point PlacementPoint, 
    BattleUnitData Unit) : IEvent;
    
public record ActivePhaseBackClickedEvent : IEvent;

public record TileHoveredEvent(Point Tile) : IEvent;
public record TileClickedEvent(Point Tile) : IEvent;
public record MiniMapClickedEvent(PointF Position) : IEvent;