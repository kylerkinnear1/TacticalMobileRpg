using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rpg.Mobile.App.Infrastructure;

public class JsonFileReader
{
    public T ReadFromFile<T>(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Could not find map: {path}");

        //var width = 10;
        //var height = 12;
        //var map = new MapJson(
        //    height,
        //    width,
        //    StatPresets.All.Append(StatPresets.Warrior).Select(x => x.Stats.UnitType).ToList(),
        //    StatPresets.All.Append(StatPresets.Warrior).Select(x => x.Stats.UnitType).ToList(),
        //    Enumerable.Range(0, 2)
        //        .SelectMany(x => Enumerable.Range(0, height).Select(y => new Coordinate(x, y)))
        //        .ToList(),
        //    Enumerable.Range(8, 2)
        //        .SelectMany(x => Enumerable.Range(0, height).Select(y => new Coordinate(x, y)))
        //        .ToList(),
        //    new(),
        //    StatPresets.All.Select(x => x.Stats).ToList());

        //File.WriteAllText(path, JsonSerializer.Serialize(map, new JsonSerializerOptions
        //{
        //    Converters =
        //    {
        //        new JsonStringEnumConverter()
        //    },
        //    WriteIndented = true
        //}));
        var jsonText = File.ReadAllText(path);
        return JsonSerializer.Deserialize<T>(jsonText, new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() }
        })!;
    }
}

