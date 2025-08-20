using Rpg.Mobile.Api.Battles.Data;

namespace Rpg.Mobile.Server.Battles.StateMachines.Phases.Damage;

public class DamagePhase
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