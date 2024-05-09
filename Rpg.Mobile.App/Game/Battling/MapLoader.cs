﻿using System.Text.Json;
using System.Text.Json.Serialization;
using Rpg.Mobile.App.Game.Battling.Gamemaster;
using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App.Game.Battling;

public record Coordinate(int X, int Y);
public record MapJson(
    int RowCount,
    int ColumnCount,
    List<BattleUnitType> Player1Team,
    List<BattleUnitType> Player2Team,
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
        var tiles = new Array2d<TileState>(mapJson.RowCount, mapJson.ColumnCount);
        for (var i = 0; i < tiles.Data.Length; i++)
            tiles[i] = new();

        mapJson.RockPositions.ForEach(x => tiles[x.X, x.Y].Type = TerrainType.Rock);

        var state = new MapState(
            tiles,
            mapJson.Player1Team,
            mapJson.Player2Team,
            mapJson.Player1Origins.Select(x => new Point(x.X, x.Y)).ToList(),
            mapJson.Player2Origins.Select(x => new Point(x.X, x.Y)).ToList());

        return state;
    }

    private static MapJson LoadMap(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Could not find map: {path}");

        // Overwrite map file programmatically.
        //var map = new MapJson(
        //    12,
        //    10,
        //    StatPresets.All.Append(StatPresets.Warrior).Select(x => x.Stats.UnitType).ToList(),
        //    StatPresets.All.Append(StatPresets.Warrior).Select(x => x.Stats.UnitType).ToList(),
        //    Enumerable.Range(0, 2)
        //        .SelectMany(x => Enumerable.Range(0, 10).Select(y => new Coordinate(x, y)))
        //        .ToList(),
        //    Enumerable.Range(8, 2)
        //        .SelectMany(x => Enumerable.Range(0, 10).Select(y => new Coordinate(x, y)))
        //        .ToList(),
        //    new());

        //File.WriteAllText(path, JsonSerializer.Serialize(map, new JsonSerializerOptions
        //{
        //    Converters =
        //    {
        //        new JsonStringEnumConverter()
        //    },
        //    WriteIndented = true
        //}));
        var mapText = File.ReadAllText(path);
        return JsonSerializer.Deserialize<MapJson>(mapText, new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() }
        })!;
    }
}
