﻿using Rpg.Mobile.App.Game.Common;
using Rpg.Mobile.App.Game.MainBattle.Systems.Data;
using Rpg.Mobile.GameSdk.StateManagement;
using static Rpg.Mobile.App.Game.MainBattle.MainBattleStateMachine;

namespace Rpg.Mobile.App.Game.MainBattle.States;

public class SelectingSpellState : IBattleState
{
    private readonly Context _context;

    public SelectingSpellState(Context context) => _context = context;

    public void Enter()
    {
        var buttons = _context.Data.CurrentUnit.Spells
            .Select(x => new ButtonData(x.Name, _ => Bus.Global.Publish(new SpellSelected(x))))
            .ToArray();
        _context.Menu.SetButtons(buttons);
    }

    public void Execute(float deltaTime)
    {
    }

    public void Leave()
    {
    }
}

public record SpellSelected(SpellData Spell) : IEvent;
