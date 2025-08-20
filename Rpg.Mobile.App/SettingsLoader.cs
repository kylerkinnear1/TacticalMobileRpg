using System.Text.Json;

namespace Rpg.Mobile.App;

public record GameSettings(string GameId);

public class SettingsLoader
{
    public async Task<GameSettings> LoadSettingsAsync()
    {
        using var stream = await FileSystem.OpenAppPackageFileAsync("Settings.json");
        using var reader = new StreamReader(stream);
        var json = await reader.ReadToEndAsync();
        
        var settings = JsonSerializer.Deserialize<GameSettings>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        return settings ?? new GameSettings("default-game");
    }
}