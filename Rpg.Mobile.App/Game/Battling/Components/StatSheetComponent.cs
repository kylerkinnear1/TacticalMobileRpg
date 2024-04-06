using Rpg.Mobile.App.Game.Common;

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
            $"PlayerId: {unit.State.PlayerId}",
            $"Unit Type: {unit.State.Stats.UnitType}",
            $"HP: {unit.State.RemainingHealth}/{unit.State.Stats.MaxHealth}",
            $"MP: {unit.State.RemainingMp}/{unit.State.Stats.MaxMp}",
            $"Range: {unit.State.Stats.AttackMinRange}-{unit.State.Stats.AttackMaxRange}",
            $"Attack: {unit.State.Stats.Attack}",
            $"Defense: {unit.State.Stats.Defense}",
            $"Movement: {unit.State.Stats.Movement}",
            $"Spells: {string.Join(", ", unit.State.Spells
                .Select(x => x.Name)
                .DefaultIfEmpty("N/A"))}");
}
