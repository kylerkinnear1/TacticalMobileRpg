using System.Text.Json;
using System.Text.Json.Serialization;
using Rpg.Mobile.Api.Battles.Data;

namespace Rpg.Mobile.App;

public record GameSettings(string GameId, BattleUnitType[] Team);

public class SettingsLoader
{
    public GameSettings LoadSettings()
    {
        return Task.Run(async () => await LoadSettingsAsync()).GetAwaiter().GetResult();
    }
    
    public async Task<GameSettings> LoadSettingsAsync()
    {
        using var stream = await FileSystem.OpenAppPackageFileAsync("Settings.json").ConfigureAwait(false);
        using var reader = new StreamReader(stream);
        var json = await reader.ReadToEndAsync().ConfigureAwait(false);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        options.Converters.Add(new JsonStringEnumConverter());
        var settings = JsonSerializer.Deserialize<GameSettings>(json, options);
    
        return settings!;
    }
}