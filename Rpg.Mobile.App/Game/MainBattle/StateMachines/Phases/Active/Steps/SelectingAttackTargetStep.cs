using Rpg.Mobile.Api;
using Rpg.Mobile.Api.Battles.Calculators;
using Rpg.Mobile.Api.Battles.Data;
using Rpg.Mobile.App.Game.MainBattle.Components;
using Rpg.Mobile.App.Game.UserInterface;
using Rpg.Mobile.GameSdk.StateManagement;

namespace Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases.Active.Steps;

public class SelectingAttackTargetStep : ActivePhase.IStep
{
    private readonly MainBattleComponent _mainBattle;
    private readonly MenuComponent _menu;
    private readonly IEventBus _bus;
    private readonly BattleData _data;
    private readonly ISelectingAttackTargetCalculator _attackTargetCalculator;

    private ISubscription[] _subscriptions = [];

    public SelectingAttackTargetStep(
        MainBattleComponent mainBattle,
        MenuComponent menu,
        IEventBus bus,
        BattleData data,
        ISelectingAttackTargetCalculator attackTargetCalculator)
    {
        _mainBattle = mainBattle;
        _menu = menu;
        _bus = bus;
        _data = data;
        _attackTargetCalculator = attackTargetCalculator;
    }

    public void Enter()
    {
        _mainBattle.AttackTargetHighlight.Range = 1;

        var attackTiles = _data.Active.AttackTargetTiles
            .Select(x => 
                new RectF(
                    _mainBattle.GetPositionForTile(x, MainBattleComponent.TileSize),
                    MainBattleComponent.TileSize));
        
        _mainBattle.AttackShadow.Shadows.Set(attackTiles);
        _menu.SetButtons(new ButtonData("Back", _ => _bus.Publish(new ActivePhase.BackClickedEvent())));

        _subscriptions = 
        [
            _bus.Subscribe<GridComponent.TileHoveredEvent>(TileHovered)
        ];
    }

    public void Execute(float deltaTime)
    {
    }
    
    public void Leave()
    {
        _mainBattle.AttackTargetHighlight.Visible = false;
        _mainBattle.AttackShadow.Shadows.Clear();
    }
    
    private void TileHovered(GridComponent.TileHoveredEvent evnt)
    {
        if (!_attackTargetCalculator.IsValidAttackTargetTile(evnt.Tile, _data)) 
            return;

        _mainBattle.AttackTargetHighlight.Center = evnt.Tile;
        _mainBattle.AttackTargetHighlight.Range = 1;
        _mainBattle.AttackTargetHighlight.Visible = true;
    }
}