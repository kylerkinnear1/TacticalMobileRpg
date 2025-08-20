using System.Drawing;

namespace Rpg.Mobile.Api.Battles;

public interface IBattleCommandApi
{
    Task UnitMovementStarted(string gameId, Point targetTile);
}