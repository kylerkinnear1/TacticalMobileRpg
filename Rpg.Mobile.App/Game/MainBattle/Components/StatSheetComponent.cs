using Rpg.Mobile.App.Game.Common;
using Rpg.Mobile.App.Game.MainBattle.Systems.Data;
using Rpg.Mobile.GameSdk.StateManagement;

namespace Rpg.Mobile.App.Game.MainBattle.Components;

public class StatSheetComponent : TextboxComponent
{
    private readonly BattleData _data;

    public StatSheetComponent(BattleData data, RectF bounds) : base(bounds, "")
    {
        _data = data;
        BackColor = Colors.Aqua;
        TextColor = Colors.Black;
        FontSize = 12f;

        Bus.Global.Subscribe<TileHoveredEvent>(TileHovered);
    }

    private void TileHovered(TileHoveredEvent evnt)
    {
        var unit = _data.UnitCoordinates.ContainsValue(evnt.Tile)
            ? _data.UnitCoordinates.First(x => x.Value == evnt.Tile).Key
        : null;

        Label = unit is not null ? Format(unit) : "";
        BackColor = unit?.PlayerId == 0 ? Colors.Aqua : Colors.Orange;
        Visible = unit is not null;
    }

    private static string Format(BattleUnitData unit) =>
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

