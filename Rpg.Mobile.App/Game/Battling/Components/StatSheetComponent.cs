using Rpg.Mobile.App.Game.Battling.Systems.Data;
using Rpg.Mobile.App.Game.Common;
using Rpg.Mobile.GameSdk.StateManagement;

namespace Rpg.Mobile.App.Game.Battling.Components;

public class StatSheetComponent : TextboxComponent
{
    public BattleUnitState? Unit { get; private set; }

    public StatSheetComponent(RectF bounds, BattleUnitState? unit = null) : base(bounds, unit is not null ? Format(unit) : "")
    {
        Unit = unit;
        BackColor = Colors.Aqua;
        TextColor = Colors.Black;
        FontSize = 12f;

        Bus.Global.Subscribe<BattleTileHoveredEvent>(x => ChangeUnit(x.Unit));
    }

    public void ChangeUnit(BattleUnitState? unit)
    {
        Unit = unit;
        Label = unit is not null ? Format(unit) : "";
        BackColor = unit?.PlayerId == 0 ? Colors.Aqua : Colors.Orange;
        Visible = unit is not null;
    }

    private static string Format(BattleUnitState unit) =>
        string.Join(Environment.NewLine,
            $"PlayerId: {unit.PlayerId}",
            $"Unit Type: {unit.Stats.UnitType}",
            $"HP: {unit.RemainingHealth}/{unit.Stats.MaxHealth}",
            $"MP: {unit.RemainingMp}/{unit.Stats.MaxMp}",
            $"Range: {unit.Stats.AttackMinRange}-{unit.Stats.AttackMaxRange}",
            $"Attack: {unit.Stats.Attack}",
            $"Defense: {unit.Stats.Defense}",
            $"Movement: {unit.Stats.Movement}{Environment.NewLine}",
            $"Spells:{Environment.NewLine}" +
            $"{string.Join(Environment.NewLine, unit.Spells
                .Select(x => $"{x.Name}")
                .DefaultIfEmpty("N/A"))}");
}

