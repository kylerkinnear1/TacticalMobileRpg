using Rpg.Mobile.App.Game.MainBattle.Data;
using Rpg.Mobile.App.Game.MainBattle.Events;
using Rpg.Mobile.App.Utils;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Tweening;
using Rpg.Mobile.GameSdk.Utilities;
using static Rpg.Mobile.App.Game.MainBattle.Components.MainBattleComponent;
using static Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases.BattlePhaseMachine;

namespace Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases.Active.Steps;

public class IdleStep(Context _context) : ActivePhase.IStep
{
    public record CompletedEvent(BattleUnitData CurrentUnit) : IEvent;
    
    private ISubscription[] _subscriptions = [];

    public void Enter()
    {
        _subscriptions = [Bus.Global.Subscribe<TileClickedEvent>(TileClicked)];

        var walkableTiles = _context.Path
            .CreateFanOutArea(_context.Data.ActiveUnitStartPosition, _context.Data.Map.Corner, _context.Data.CurrentUnit.Stats.Movement)
            .Where(x => 
                x == _context.Data.ActiveUnitStartPosition ||
                !_context.Data.UnitCoordinates.ContainsValue(x) &&
                _context.Data.Map.Tiles[x.X, x.Y].Type != TerrainType.Rock)
            .ToList();
        _context.Data.WalkableTiles = walkableTiles;

        _context.Menu.SetButtons(
            new("Attack", _ => Bus.Global.Publish(new ActivePhase.AttackClickedEvent())),
            new("Magic", _ => Bus.Global.Publish(new ActivePhase.MagicClickedEvent())),
            new("Wait", _ => Bus.Global.Publish(new CompletedEvent(_context.Data.CurrentUnit))));
    }

    public void Execute(float deltaTime)
    {
        // TODO: Remove and use state machine.
        if (_context.Main.CurrentUnitTween is not null)
        {
            _context.Main.CurrentUnit.Unit.Position = _context.Main.CurrentUnitTween.Advance(deltaTime);
            _context.Main.CurrentUnitTween = _context.Main.CurrentUnitTween.IsComplete ? null : _context.Main.CurrentUnitTween;
        }

        var walkShadows = _context.Data.WalkableTiles.Select(x => new RectF(x.X * TileSize, x.Y * TileSize, TileSize, TileSize));
        _context.Main.MoveShadow.Shadows.Set(walkShadows);
    }

    public void Leave()
    {
        _context.Data.WalkableTiles.Clear();
        _context.Main.MoveShadow.Shadows.Clear();

        _subscriptions.DisposeAll();
    }

    private void TileClicked(TileClickedEvent evnt)
    {
        if (!_context.Data.WalkableTiles.Contains(evnt.Tile))
        {
            return;
        }

        _context.Data.UnitCoordinates[_context.Data.CurrentUnit] = evnt.Tile;
        var finalTarget = _context.Main.GetPositionForTile(evnt.Tile, _context.Main.CurrentUnit.Unit.Bounds.Size);
        _context.Main.CurrentUnitTween = _context.Main.CurrentUnit.Unit.Position.SpeedTween(500f, finalTarget);
    }
}
