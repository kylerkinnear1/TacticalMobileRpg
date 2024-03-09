using Rpg.Mobile.App.Game.Menu;

namespace Rpg.Mobile.App.Game.Battling.Components;

public class StatSheetComponent : TextboxComponent
{
    public BattleUnitComponent? Unit { get; private set; }

    public StatSheetComponent(RectF bounds, BattleUnitComponent? unit = null) : base(bounds, unit is not null ? Format(unit) : "")
    {
        Unit = unit;
        BackColor = Colors.Aqua;
        TextColor = Colors.Black;
    }

    public void ChangeUnit(BattleUnitComponent? unit)
    {
        Unit = unit;
        Label = unit is not null ? Format(unit) : "";
    }

    private static string Format(BattleUnitComponent unit) =>
        string.Join(Environment.NewLine,
            $"PlayerId: {unit.PlayerId}",
            $"Unit Type: {unit.State.UnitType}",
            $"HP: {unit.State.RemainingHealth}/{unit.State.MaxHealth}",
            $"MP: {unit.State.RemainingMp}/{unit.State.MaxMp}",
            $"Range: {unit.State.AttackMinRange}-{unit.State.AttackMaxRange}",
            $"Attack: {unit.State.Attack}",
            $"Defense: {unit.State.Defense}",
            $"Movement: {unit.State.Movement}");
}
