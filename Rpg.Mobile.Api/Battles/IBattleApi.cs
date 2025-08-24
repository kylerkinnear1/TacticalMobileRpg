using System.Drawing;
using Rpg.Mobile.Api.Battles.Data;

namespace Rpg.Mobile.Api.Battles;

public interface IBattleCommandApi
{
    Task TileClicked(string gameId, Point tile);
    Task AttackClicked(string gameId);
    Task MagicClicked(string gameId);
    Task SpellSelected(string gameId, SpellType spellType);
    Task WaitClicked(string gameId);
}

public interface IBattleEventApi
{
    Task UnitMoved(string gameId, int unitId, Point tile);
    Task SetupStarted(string gameId, List<BattleUnitData> units, BattleSetupPhaseData data);
    Task UnitPlaced(string gameId, int unitId, int currentPlaceOrderIndex, Point tile);

    Task NewRoundStarted(string gameId, List<int> turnOrderIds, int activeUnitIndex);
    Task ActivePhaseStarted(string gameId, BattleActivePhaseData activePhaseData);
    Task IdleStepStarted(string gameId, List<Point> walkableTiles);
    Task IdleStepEnded(string gameId, int unitId);
}