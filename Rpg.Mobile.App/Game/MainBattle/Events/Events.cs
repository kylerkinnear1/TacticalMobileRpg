using Rpg.Mobile.App.Game.MainBattle.Systems.Data;
using Rpg.Mobile.GameSdk.StateManagement;

namespace Rpg.Mobile.App.Game.MainBattle.Events;

public record SpellSelectedEvent(SpellData Spell) : IEvent;
public record TileHoveredEvent(Point Tile) : IEvent;
public record TileClickedEvent(Point Tile) : IEvent;
public record MiniMapClickedEvent(PointF Position) : IEvent;
public record UnitTurnEndedEvent(BattleUnitData Unit) : IEvent;
public record UnitDamageAssignedEvent(IEnumerable<BattleUnitData> Units, int Damage) : IEvent;
public record BackClickedEvent : IEvent;
public record AttackClickedEvent : IEvent;
public record MagicClickedEvent : IEvent;

public record NotEnoughMpEvent(SpellData Spell) : IEvent;
