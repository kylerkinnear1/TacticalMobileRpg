using Rpg.Mobile.Api.Battles.Data;
using Rpg.Mobile.GameSdk.StateManagement;

namespace Rpg.Mobile.App.Game.MainBattle;

public class BattleNetwork
{
    public record UnitsDamagedEvent(
        List<(BattleUnitData Unit, int Damage)> DamagedUnits,
        List<BattleUnitData> DefeatedUnits) : IEvent;
}