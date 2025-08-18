using System.Drawing;
using Rpg.Mobile.GameSdk.StateManagement;

namespace Rpg.Mobile.Api;

public record UnitPlacedEvent(
    Point PlacementPoint, 
    BattleUnitData Unit) : IEvent;
    
public record ActivePhaseBackClickedEvent : IEvent;