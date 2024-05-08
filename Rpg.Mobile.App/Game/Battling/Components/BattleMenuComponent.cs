using Rpg.Mobile.App.Game.Battling.Gamemaster;
using Rpg.Mobile.App.Game.Common;
using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App.Game.Battling.Components;

public class BattleMenuComponent : MenuComponent
{
    private readonly BattleStateService _battleService;
    private readonly BattleState _state;

    public BattleMenuComponent(BattleStateService battleService, BattleState state, RectF bounds) : base(bounds)
    {
        _state = state;
        _battleService = battleService;
        Bus.Global.Subscribe<BattleStepChangedEvent>(BattleStepChanged);
    }

    private void BattleStepChanged(BattleStepChangedEvent evnt)
    {
        SetButtons(evnt.Step switch
        {
            BattleStep.Moving => new ButtonState[] {
                new("Attack", _ => _battleService.ChangeBattleState(BattleStep.SelectingAttackTarget)),
                new("Magic", _ => _battleService.ChangeBattleState(BattleStep.SelectingSpell)),
                new("Wait", _ => _battleService.AdvanceToNextUnit()),
                new("Re-roll", _ => _battleService.RerollUnit())
            },
            BattleStep.SelectingAttackTarget or BattleStep.SelectingMagicTarget => new ButtonState[] {
                new ("Back", _ => _battleService.ChangeBattleState(BattleStep.Moving))
            },
            BattleStep.SelectingSpell =>
                _state.CurrentUnit.Spells
                    .Select(x => new ButtonState(x.Name, _ => _battleService.TargetSpell(x)))
                    .Append(new("Back", _ => _battleService.ChangeBattleState(BattleStep.Moving)))
                    .ToArray(),
            _ => Array.Empty<ButtonState>()
        });
    }
}
