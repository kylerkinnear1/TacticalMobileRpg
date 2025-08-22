using System.Text.Json;
using System.Text.Json.Serialization;
using Rpg.Mobile.Api.Battles.Data;

namespace Rpg.Mobile.App;

public record GameSettings(string GameId, BattleUnitType[] Team);

public class SettingsLoader
{
    public async Task<GameSettings> LoadSettingsAsync()
    {
        using var stream = await FileSystem.OpenAppPackageFileAsync("Settings.json");
        using var reader = new StreamReader(stream);
        var json = await reader.ReadToEndAsync();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        options.Converters.Add(new JsonStringEnumConverter());
        var settings = JsonSerializer.Deserialize<GameSettings>(json, options);
        
        return settings!;
    }
}