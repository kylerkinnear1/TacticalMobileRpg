using Rpg.Mobile.Api;
using Rpg.Mobile.Api.Battles.Calculators;
using Rpg.Mobile.Api.Battles.Data;
using Rpg.Mobile.App.Game.MainBattle.Components;
using Rpg.Mobile.App.Game.UserInterface;
using Rpg.Mobile.GameSdk.StateManagement;

namespace Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases.Active.Steps;

public class SelectingMagicTargetStep : ActivePhase.IStep
{
    private readonly MainBattleComponent _mainBattle;
    private readonly MenuComponent _menu;
    private readonly BattleData _data;
    private readonly IEventBus _bus;
    private readonly ISelectingMagicTargetCalculator _magicTargetCalculator;
    
    private ISubscription[] _subscriptions = [];
    
    public void Enter()
    {
        _mainBattle.AttackTargetHighlight.Range = _data.Active.CurrentSpell.Aoe;
        
        var attackTiles = _data.Active.SpellTargetTiles
            .Select(x => 
                new RectF(
                    _mainBattle.GetPositionForTile(x, MainBattleComponent.TileSize), 
                    MainBattleComponent.TileSize));
        
        _mainBattle.AttackShadow.Shadows.Set(attackTiles);
        _menu.SetButton(new("Back", _ => _bus.Publish(new ActivePhase.BackClickedEvent())));
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
        if (!_magicTargetCalculator.IsValidMagicTargetTile(evnt.Tile, _data))
        {
            _mainBattle.AttackTargetHighlight.Visible = false;
            return;
        }

        _mainBattle.AttackTargetHighlight.Center = evnt.Tile;
        _mainBattle.AttackTargetHighlight.Visible = true;
    }
}