using Rpg.Mobile.Api.Battles.Data;

namespace Rpg.Mobile.Server.Battles;

public interface IBattleProvider
{
    Task TileClicked(GameHub hub, string gameId, Point tile);
    Task AttackClicked(GameHub hub, string gameId);
    Task MagicClicked(GameHub gameHub, string gameId);
    Task SpellSelected(GameHub gameHub, string gameId, SpellType spellType);
    Task WaitClicked(GameHub gameHub, string gameId);
}

public class BattleProvider : IBattleProvider
{
    public Task TileClicked(GameHub hub, string gameId, Point tile)
    {
        throw new NotImplementedException();
    }

    public Task AttackClicked(GameHub hub, string gameId)
    {
        throw new NotImplementedException();
    }

    public Task MagicClicked(GameHub gameHub, string gameId)
    {
        throw new NotImplementedException();
    }

    public Task SpellSelected(GameHub gameHub, string gameId, SpellType spellType)
    {
        throw new NotImplementedException();
    }

    public Task WaitClicked(GameHub gameHub, string gameId)
    {
        throw new NotImplementedException();
    }
}