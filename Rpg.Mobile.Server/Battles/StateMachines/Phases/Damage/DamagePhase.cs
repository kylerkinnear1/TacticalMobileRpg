using Rpg.Mobile.Api.Battles.Calculators;
using Rpg.Mobile.Api.Battles.Data;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.Server.Battles.Calculators;
using Rpg.Mobile.Server.Battles.StateMachines.Phases.Active.Steps;
using static Rpg.Mobile.Server.Battles.StateMachines.Phases.Active.Steps.SelectingMagicTargetStep;

namespace Rpg.Mobile.Server.Battles.StateMachines.Phases.Damage;

public class DamagePhase(
    BattleData _data,
    IPathCalculator _path,
    IAttackDamageCalculator _attackDamageCalc,
    IMagicDamageCalculator _magicDamageCalc,
    IEventBus _bus) : IBattlePhase
{
    public record CompletedEvent(BattleUnitData Unit) : IEvent;

    public record UnitsDamaged(
        List<(BattleUnitData Unit, int Damage)> DamagedUnits,
        List<BattleUnitData> DefeatedUnits) : IEvent;
    
    public void Enter()
    {
    }

    public void Execute(float deltaTime)
    {
    }

    public void Leave()
    {
    }
    
    public void PerformAttack(SelectingAttackTargetStep.AttackTargetSelectedEvent evnt)
    {
        var damage = _attackDamageCalc.CalcDamage(_data.CurrentUnit().Stats.Attack, evnt.Target.Stats.Defense);
        ApplyDamage(new[] { evnt.Target }, damage);
    }
    
    public void CastSpell(MagicTargetSelectedEvent evnt)
    {
        var spell = _data.Active.CurrentSpell!;
        var hits = _path
            .CreateFanOutArea(evnt.Target, _data.Map.Corner(), spell.MinRange - 1, spell.MaxRange - 1).ToHashSet();

        var units = _data.UnitCoordinates.Where(x => hits.Contains(x.Value));
        var targets = units
            .Where(x =>
                x.Key.PlayerId == _data.CurrentUnit().PlayerId && spell.TargetsFriendlies ||
                x.Key.PlayerId != _data.CurrentUnit().PlayerId && spell.TargetsEnemies)
            .ToList();

        _data.CurrentUnit().RemainingMp -= spell.MpCost;
        
        // TODDO: Vary by unit
        var damage = _magicDamageCalc.CalcDamage(spell);
        ApplyDamage(targets.Select(x => x.Key), damage);
    }
    
    private void ApplyDamage(IEnumerable<BattleUnitData> targets, int damage)
    {
        var defeatedUnits = new List<BattleUnitData>();
        var damagedUnits = new List<(BattleUnitData Unit, int Damage)>();

        foreach (var target in targets)
        {
            target.RemainingHealth = damage >= 0
                ? Math.Max(target.RemainingHealth - damage, 0)
                : Math.Min(target.RemainingHealth - damage, target.Stats.MaxHealth);
            
            damagedUnits.Add((target, damage));

            if (target.RemainingHealth <= 0)
            {
                defeatedUnits.Add(target);

                // TODO: Don't remove. Just mark as dead.
                _data.Active.TurnOrder.Remove(target);
                _data.UnitCoordinates.Remove(target);
            }
        }

        if (_data.Active.ActiveUnitIndex >= _data.Active.TurnOrder.Count)
            _data.Active.ActiveUnitIndex = 0;
        
        _bus.Publish(new UnitsDamaged(damagedUnits, defeatedUnits));
    }
}