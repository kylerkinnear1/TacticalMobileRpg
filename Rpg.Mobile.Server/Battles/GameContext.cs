using Rpg.Mobile.Server.Battles.StateMachines.Phases;

namespace Rpg.Mobile.Server.Battles;

public class GameContext
{
    public object Lock { get; } = new();
    public BattlePhaseMachine? BattlePhase { get; set; }
    public string? Player0ConnectionId { get; set; }
    public string? Player1ConnectionId { get; set; }
}