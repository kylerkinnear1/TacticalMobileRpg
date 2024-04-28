﻿using System.Text.Json;
using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App.Game.Battling.Backend;

public record Coordinate(int X, int Y);
public record MapJson(
    int RowCount,
    int ColumnCount,
    List<Coordinate> Player1Origins,
    List<Coordinate> Player2Origins,
    List<Coordinate> RockPositions);

public record LoadStateClickedEvent : IEvent;
public record SaveStateClickedEvent : IEvent;

public class MapLoader
{
    public MapState Load(string path)
    {
        var mapJson = LoadMap(path);

        var state = MapState.New(mapJson.ColumnCount, mapJson.RowCount);
        var player1Units = StatPresets.All.Shuffle(Rng.Instance).ToList();
        var player2Units = StatPresets.All.Shuffle(Rng.Instance).ToList();
        player2Units.ForEach(x => x.PlayerId = 1);

        foreach (var (unit, point) in player1Units.Zip(mapJson.Player1Origins))
            state.UnitCoordinates[unit] = new(point.X, point.Y);

        foreach (var (unit, point) in player2Units.Zip(mapJson.Player2Origins))
            state.UnitCoordinates[unit] = new(point.X, point.Y);

        mapJson.RockPositions.ForEach(x => state.Tiles[x.X, x.Y].Type = TerrainType.Rock);

        var allUnits = player1Units.Concat(player2Units).Shuffle(Rng.Instance).ToList();
        state.TurnOrder.AddRange(allUnits);

        return state;
    }

    private static MapJson LoadMap(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Could not find map: {path}");

        var mapText = File.ReadAllText(path);
        return JsonSerializer.Deserialize<MapJson>(mapText)!;
    }
}
