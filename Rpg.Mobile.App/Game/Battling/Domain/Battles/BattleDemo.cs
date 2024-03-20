namespace Rpg.Mobile.App.Game.Battling.Domain.Battles;

public static partial class Battles
{
    public static readonly MapState Demo = CreateDemo();

    private static MapState CreateDemo()
    {
        var state = MapState.New(10, 12);
        var player1Units = StatPresets.All.ToList();
        var player2Units = StatPresets.All.ToList();
        player2Units.ForEach(x => x.PlayerId = 1);

        // Pseudo random for now.
        var allUnits = player1Units.Concat(player2Units).OrderBy(x => Guid.NewGuid()).ToList();
        foreach (var (unit, index) in allUnits.Where(x => x.PlayerId == 0).Select((x, i) => (x, i)))
        {
            state.UnitTiles[unit] = new(1, (index * 2) + 1);
        }

        foreach (var (unit, index) in allUnits.Where(x => x.PlayerId == 1).Select((x, i) => (x, i)))
        {
            state.UnitTiles[unit] = new(8, (index * 2) + 1);
        }

        state.Tiles[3, 5].Type = TerrainType.Rock;
        state.Tiles[6, 4].Type = TerrainType.Rock;
        state.TurnOrder.AddRange(allUnits);

        return state;
    }
}
