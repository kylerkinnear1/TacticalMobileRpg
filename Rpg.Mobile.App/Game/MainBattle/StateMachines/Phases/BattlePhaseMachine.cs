namespace Rpg.Mobile.Server.Battles.StateMachines.Phases;

public class BattlePhaseMachineClient
{
    private void ShowMessage(string message)
    {
        _context.Main.Message.Position = new(_context.Main.Map.Bounds.Left, _context.Main.Map.Bounds.Top - 10f);
        _context.Main.Message.Play(message);
    }
}