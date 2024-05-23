using Rpg.Mobile.App.Game.Battling.Systems.Data;
using Rpg.Mobile.App.Infrastructure;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Utilities;

namespace Rpg.Mobile.App.Game.Battling.Components.MainBattle.States;

public class MainBattleStateMachine : StateMachine<IBattleState>
{
    private readonly BattleData _data;
    private readonly MainBattleComponent _component;

    public MainBattleStateMachine(BattleData data, MainBattleComponent component)
    {
        _data = data;
        _component = component;

        Bus.Global.Subscribe<UnitTurnEndedEvent>(UnitTurnEnded);
        Bus.Global.Subscribe<UnitDamageAssignedEvent>(UnitDamageCalculated);
        Bus.Global.Subscribe<NotEnoughMpEvent>(_ => ShowMessage("Not enough MP."));
    }

    private void UnitTurnEnded(UnitTurnEndedEvent evnt)
    {
        _component.Units[_data.CurrentUnit].HealthBar.HasGone = true;
        _data.ActiveUnitIndex += 1 % _data.TurnOrder.Count;

        if (_data.ActiveUnitIndex == 0)
        {
            StartNewTurn();
        }
    }

    private void StartNewTurn()
    {
        _data.TurnOrder.Set(_data.TurnOrder.Shuffle(Rng.Instance).ToList());
        _component.Units.Values.ToList().ForEach(x => x.HealthBar.HasGone = false);
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
                _data.TurnOrder.Remove(unit);
                _data.UnitCoordinates.Remove(unit);
            }
        }

        if (_data.ActiveUnitIndex >= _data.TurnOrder.Count)
            _data.ActiveUnitIndex = 0;

        var positions = damagedUnits
            .Select(x => (_component.Units[x.Unit].Position, Data: x.Damage))
            .ToList();

        _component.DamageIndicator.SetDamage(positions);

        var defeatedComponents = defeatedUnits.Select(x => _component.Units[x]).ToList();
        foreach (var unit in defeatedComponents)
        {
            unit.Visible = false;
        }

        Bus.Global.Publish(new UnitDamagedEvent(damagedUnits));
        Bus.Global.Publish(new UnitsDefeatedEvent(defeatedUnits));
        Bus.Global.Publish(new UnitTurnEndedEvent(_data.CurrentUnit));
    }

    private void ShowMessage(string message)
    {
        _component.Message.Position = new(_component.Map.Bounds.Left, _component.Map.Bounds.Top - 10f);
        _component.Message.Play(message);
    }
}

public interface IBattleState : IState { }

public record UnitTurnEndedEvent(BattleUnitData Unit) : IEvent;

public record UnitDamageAssignedEvent(IEnumerable<BattleUnitData> Units, int Damage) : IEvent;
