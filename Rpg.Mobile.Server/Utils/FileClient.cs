using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rpg.Mobile.Server.Utils;

public class FileClient
{
    public T ReadJson<T>(string path)
    {
        var jsonText = ReadText(path);
        return JsonSerializer.Deserialize<T>(jsonText, new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() },
            PropertyNameCaseInsensitive = true
        })!;
    }

    public string ReadText(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Could not find file: {path}");

        var jsonText = File.ReadAllText(path);
        return jsonText;
    }
}

