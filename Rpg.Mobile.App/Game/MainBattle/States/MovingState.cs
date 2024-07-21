using Rpg.Mobile.App.Game.MainBattle.Components;
using Rpg.Mobile.App.Game.MainBattle.Events;
using Rpg.Mobile.App.Game.MainBattle.Systems.Data;
using Rpg.Mobile.App.Utils;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Tweening;
using static Rpg.Mobile.App.Game.MainBattle.MainBattleComponent;
using static Rpg.Mobile.App.Game.MainBattle.MainBattleStateMachine;

namespace Rpg.Mobile.App.Game.MainBattle.States;

public class MovingState : IBattleState
{
    private readonly Context _context;

    public MovingState(Context context) => _context = context;

    public void Enter()
    {
        Bus.Global.Subscribe<TileClickedEvent>(TileClicked);

        var walkableTiles = _context.Path
            .CreateFanOutArea(_context.Data.ActiveUnitStartPosition, _context.Data.Map.Corner, _context.Data.CurrentUnit.Stats.Movement)
            .Where(x => x == _context.Data.ActiveUnitStartPosition ||
                        !_context.Data.UnitCoordinates.ContainsValue(x) &&
                        _context.Data.Map.Tiles[x.X, x.Y].Type != TerrainType.Rock)
            .ToList();
        _context.Data.WalkableTiles = walkableTiles;

        _context.Menu.SetButtons(
            new("Attack", _ => Bus.Global.Publish(new AttackClickedEvent())),
            new("Magic", _ => Bus.Global.Publish(new MagicClickedEvent())),
            new("Wait", _ => Bus.Global.Publish(new UnitTurnEndedEvent(_context.Data.CurrentUnit))));
    }

    public void Execute(float deltaTime)
    {
        if (_context.Main.CurrentUnitTween is not null)
        {
            _context.Main.CurrentUnit.Position = _context.Main.CurrentUnitTween.Advance(deltaTime);
            _context.Main.CurrentUnitTween = _context.Main.CurrentUnitTween.IsComplete ? null : _context.Main.CurrentUnitTween;
        }

        var walkShadows = _context.Data.WalkableTiles.Select(x => new RectF(x.X * TileSize, x.Y * TileSize, TileSize, TileSize));
        _context.Main.MoveShadow.Shadows.Set(walkShadows);
    }

    public void Leave()
    {
        _context.Data.WalkableTiles.Clear();
        _context.Main.MoveShadow.Shadows.Clear();

        Bus.Global.Unsubscribe<TileClickedEvent>(TileClicked);
    }

    private void TileClicked(TileClickedEvent evnt)
    {
        if (!_context.Data.WalkableTiles.Contains(evnt.Tile))
        {
            return;
        }

        _context.Data.UnitCoordinates[_context.Data.CurrentUnit] = evnt.Tile;
        var finalTarget = _context.Main.GetPositionForTile(evnt.Tile, _context.Main.CurrentUnit.Bounds.Size);
        _context.Main.CurrentUnitTween = _context.Main.CurrentUnit.Position.SpeedTween(500f, finalTarget);
    }
}
