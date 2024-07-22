using Rpg.Mobile.App.Game.MainBattle.Components;
using Rpg.Mobile.App.Game.MainBattle.Events;
using Rpg.Mobile.App.Game.MainBattle.States;
using Rpg.Mobile.App.Game.MainBattle.States.ActivePhase;
using Rpg.Mobile.App.Game.MainBattle.States.Phases.Setup;
using Rpg.Mobile.App.Game.MainBattle.Systems.Calculators;
using Rpg.Mobile.App.Game.MainBattle.Systems.Data;
using Rpg.Mobile.App.Utils;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Utilities;

namespace Rpg.Mobile.App.Game.MainBattle;

public class BattlePhaseStateMachine : StateMachine<IBattlePhase>
{
    public record Context(
        BattleData Data,
        MainBattleComponent Main,
        BattleMenuComponent Menu,
        IPathCalculator Path);

    private readonly Context _context;

    public BattlePhaseStateMachine(Context context)
    {
        _context = context;

        Bus.Global.Subscribe<SetupEvent.UnitPlacementCompleted>(_ => StartNewTurn());

        Bus.Global.Subscribe<UnitTurnEndedEvent>(UnitTurnEnded);
        Bus.Global.Subscribe<UnitDamageAssignedEvent>(UnitDamageCalculated);
        Bus.Global.Subscribe<NotEnoughMpEvent>(_ => ShowMessage("Not enough MP."));
        Bus.Global.Subscribe<BackClickedEvent>(_ => Change(new MovingStep(_context)));
        Bus.Global.Subscribe<AttackClickedEvent>(_ => Change(new SelectingAttackTargetPhase(_context)));
        Bus.Global.Subscribe<MagicClickedEvent>(_ => Change(new SelectingSpellPhase(_context)));
        Bus.Global.Subscribe<SpellSelectedEvent>(SpellSelected);
    }

    private void UnitTurnEnded(UnitTurnEndedEvent evnt)
    {
        _context.Main.Units[_context.Data.CurrentUnit].HealthBar.HasGone = true;
        _context.Data.ActiveUnitIndex = (_context.Data.ActiveUnitIndex + 1) % _context.Data.TurnOrder.Count;

        if (_context.Data.ActiveUnitIndex != 0)
            Change(new MovingStep(_context));
        else
            StartNewTurn();
    }

    private void StartNewTurn()
    {
        // TODO: Force other player if last 2 were for player 1.
        _context.Data.TurnOrder.Set(_context.Data.TurnOrder.Shuffle(Rng.Instance).ToList());
        _context.Main.Units.Values.ToList().ForEach(x => x.HealthBar.HasGone = false);
        _context.Data.ActiveUnitIndex = 0;
        _context.Data.ActiveUnitStartPosition = _context.Data.UnitCoordinates[_context.Data.CurrentUnit];
        Change(new MovingStep(_context));
    }

    private void UnitDamageCalculated(UnitDamageAssignedEvent evnt)
    {
        var defeatedUnits = new List<BattleUnitData>();
        var damagedUnits = new List<(BattleUnitData Unit, int Damage)>();
        foreach (var unit in evnt.Units)
        {
            unit.RemainingHealth = evnt.Damage >= 0
                ? Math.Max(unit.RemainingHealth - evnt.Damage, 0)
                : Math.Min(unit.Stats.MaxHealth, unit.RemainingHealth - evnt.Damage);

            damagedUnits.Add((unit, evnt.Damage));

            if (unit.RemainingHealth <= 0)
            {
                defeatedUnits.Add(unit);
                _context.Data.TurnOrder.Remove(unit);
                _context.Data.UnitCoordinates.Remove(unit);
            }
        }

        if (_context.Data.ActiveUnitIndex >= _context.Data.TurnOrder.Count)
            _context.Data.ActiveUnitIndex = 0;

        var positions = damagedUnits
            .Select(x => (_context.Main.Units[x.Unit].Position, Data: x.Damage))
            .ToList();

        _context.Main.DamageIndicator.SetDamage(positions);

        var defeatedComponents = defeatedUnits.Select(x => _context.Main.Units[x]).ToList();
        foreach (var unit in defeatedComponents)
        {
            unit.Visible = false;
        }
        
        Bus.Global.Publish(new UnitTurnEndedEvent(_context.Data.CurrentUnit));
    }

    private void SpellSelected(SpellSelectedEvent evnt)
    {
        _context.Data.CurrentSpell = evnt.Spell;
        Change(new SelectingMagicTargetPhase(_context));
    }

    private void ShowMessage(string message)
    {
        _context.Main.Message.Position = new(_context.Main.Map.Bounds.Left, _context.Main.Map.Bounds.Top - 10f);
        _context.Main.Message.Play(message);
    }
}

public interface IBattlePhase : IState { }
