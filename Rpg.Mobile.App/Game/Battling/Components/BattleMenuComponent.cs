﻿using Rpg.Mobile.App.Game.Battling.Systems;
using Rpg.Mobile.App.Game.Battling.Systems.Data;
using Rpg.Mobile.App.Game.Common;
using Rpg.Mobile.GameSdk.StateManagement;

namespace Rpg.Mobile.App.Game.Battling.Components;

public class BattleMenuComponent : MenuComponent
{
    private readonly BattleStateService _battleService;
    private readonly BattleData _state;

    public BattleMenuComponent(BattleStateService battleService, BattleData state, RectF bounds) : base(bounds)
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
                new("Wait", _ => _battleService.AdvanceToNextUnit())
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
