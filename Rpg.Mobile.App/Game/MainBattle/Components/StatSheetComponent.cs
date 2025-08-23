using Rpg.Mobile.Api.Battles.Data;
using Rpg.Mobile.App.Game.UserInterface;
using Rpg.Mobile.GameSdk.StateManagement;

namespace Rpg.Mobile.App.Game.MainBattle.Components;

public class StatSheetComponent : TextboxComponent
{
    private readonly BattleData _data;
    private readonly IEventBus _bus;
    
    public StatSheetComponent(
        BattleData data,
        IEventBus bus,
        RectF bounds) 
        : base(bounds, "")
    {
        _data = data;
        _bus = bus;
        
        BackColor = Colors.Aqua;
        TextColor = Colors.Black;
        FontSize = 12f;

        _bus.Subscribe<GridComponent.TileHoveredEvent>(TileHovered);
    }

    private void TileHovered(GridComponent.TileHoveredEvent evnt)
    {
        var unit = _data.UnitCoordinates.ContainsValue(evnt.Tile)
            ? _data.Units.Single(x => x.UnitId == _data.UnitCoordinates.First(x => x.Value == evnt.Tile).Key)
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

