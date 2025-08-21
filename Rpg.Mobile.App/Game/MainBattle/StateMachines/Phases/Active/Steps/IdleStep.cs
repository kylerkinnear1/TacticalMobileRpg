using Rpg.Mobile.Api;
using Rpg.Mobile.Api.Battles.Data;
using Rpg.Mobile.App.Game.MainBattle.Components;
using Rpg.Mobile.App.Game.UserInterface;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Utilities;

namespace Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases.Active.Steps;

public class IdleStep : ActivePhase.IStep
{
    private readonly MenuComponent _menu;
    private readonly IEventBus _bus;
    private readonly BattleData _data;
    private readonly MainBattleComponent _mainBattle;

    private ISubscription[] _subscriptions = [];
    
    public IdleStep(MenuComponent menu, IEventBus bus, BattleData data, MainBattleComponent mainBattle)
    {
        _menu = menu;
        _bus = bus;
        _data = data;
        _mainBattle = mainBattle;
    }

    public void Enter()
    {
        _menu.SetButtons(
            new("Attack", _ => _bus.Publish(new ActivePhase.AttackClickedEvent())),
            new("Magic", _ => _bus.Publish(new ActivePhase.MagicClickedEvent())),
            new("Wait", _ => _bus.Publish(new ActivePhase.WaitClickedEvent())));

        _subscriptions =
        [
            _bus.Subscribe<GridComponent.TileClickedEvent>(TileClicked)
        ];
    }
    
    public void Execute(float deltaTime)
    {
        var walkShadows = _data.Active.WalkableTiles.Select(x => new RectF(
            x.X * MainBattleComponent.TileWidth, 
            x.Y * MainBattleComponent.TileWidth, 
            MainBattleComponent.TileWidth, 
            MainBattleComponent.TileWidth));
        _mainBattle.MoveShadow.Shadows.Set(walkShadows);
    }
    
    public void Leave()
    {
        _mainBattle.MoveShadow.Shadows.Clear();
        _subscriptions.DisposeAll();
    }
    
    private void TileClicked(GridComponent.TileClickedEvent evnt)
    {
        if (!_data.Active.WalkableTiles.Contains(evnt.Tile))
        {
            return;
        }
        
        _data.UnitCoordinates[_data.CurrentUnit()] = evnt.Tile;
        var finalTarget = _mainBattle.GetPositionForTile(evnt.Tile, _mainBattle.CurrentUnit.Unit.Bounds.Size);
        _mainBattle.Units[_data.CurrentUnit()].MoveTo(finalTarget, speed: 500f);
    }
}