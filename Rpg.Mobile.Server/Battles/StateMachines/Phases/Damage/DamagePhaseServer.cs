using Rpg.Mobile.Api;
using Rpg.Mobile.GameSdk.StateManagement;
using static Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases.Active.Steps.SelectingAttackTargetStepServer;
using static Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases.Active.Steps.SelectingMagicTargetStepServer;

namespace Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases.Damage;

public class DamagePhaseServer(BattlePhaseMachine.Context _context) : IBattlePhase
{
    public record CompletedEvent(BattleUnitData Unit) : IEvent;

    public void PerformAttack(AttackTargetSelectedEvent evnt)
    {
        var damage = _context.AttackDamageCalc.CalcDamage(_context.Data.CurrentUnit().Stats.Attack, evnt.Target.Stats.Defense);
        ApplyDamage(new[] { evnt.Target }, damage);
    }
    
    public void CastSpell(MagicTargetSelectedEvent evnt)
    {
        var spell = _context.Data.Active.CurrentSpell!;
        var hits = _context.Path
            .CreateFanOutArea(evnt.Target, _context.Data.Map.Corner(), spell.MinRange - 1, spell.MaxRange - 1).ToHashSet();

        var units = _context.Data.UnitCoordinates.Where(x => hits.Contains(x.Value));
        var targets = units
            .Where(x =>
                x.Key.PlayerId == _context.Data.CurrentUnit().PlayerId && spell.TargetsFriendlies ||
                x.Key.PlayerId != _context.Data.CurrentUnit().PlayerId && spell.TargetsEnemies)
            .ToList();

        _context.Data.CurrentUnit().RemainingMp -= spell.MpCost;
        
        // TODDO: Vary by unit
        var damage = _context.MagicDamageCalc.CalcDamage(spell);
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
                _context.Data.Active.TurnOrder.Remove(target);
                _context.Data.UnitCoordinates.Remove(target);
            }
        }

        if (_context.Data.Active.ActiveUnitIndex >= _context.Data.Active.TurnOrder.Count)
            _context.Data.Active.ActiveUnitIndex = 0;
    }
}

public class DamagePhaseClient
{
    public void Enter()
    {
        _context.Main.DamageIndicator.Visible = true;
    }
    
    public void Leave()
    {
        _context.Main.DamageIndicator.Visible = false;
    }

    // TODO: Hm... i don't want to wait for the client to finish the damage
    // and then tell the server and hold up the game while waiting for damage?
    // or is that ok?
    public void Execute(float deltaTime)
    {
        if (_context.Main.DamageIndicator.IsPlaying)
            return;
        
        Bus.Global.Publish(new CompletedEvent(_context.Data.CurrentUnit()));
    }
    
    private void ApplyDamage(IEnumerable<BattleUnitData> targets, int damage)
    {
        var positions = damagedUnits
            .Select(x => (_context.Main.Units[x.Unit].Unit.Position, x.Damage))
            .ToList();

        _context.Main.DamageIndicator.SetDamage(positions);
        _context.Main.DamageIndicator.Visible = true;

        var defeatedComponents = defeatedUnits.Select(x => _context.Main.Units[x]).ToList();
        
        foreach (var component in defeatedComponents)
        {
            component.Unit.Visible = false;
        }
    }
}