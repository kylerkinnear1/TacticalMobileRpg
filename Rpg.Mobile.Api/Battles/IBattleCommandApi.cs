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
    Task UnitMoved(string gameId, Point tile);
}