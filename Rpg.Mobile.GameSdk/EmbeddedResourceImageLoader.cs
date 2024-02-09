using Microsoft.Maui.Graphics;
using Rpg.Mobile.GameSdk.Extensions;
using System.Reflection;

namespace Rpg.Mobile.GameSdk;

public interface IImageLoader
{
    IImage Load(string path);
}

public class EmbeddedResourceImageLoader : IImageLoader
{
    public record Options(Assembly Assembly, string BasePath = "Rpg.Mobile.App.Resources.EmbeddedResources");

    private readonly Options _options;

    public EmbeddedResourceImageLoader(Options options) => _options = options;

    public IImage Load(string path)
    {
        var combined = Combine(_options.BasePath, path);
        using var stream = _options.Assembly.GetManifestResourceStream(combined);
        return Microsoft.Maui.Graphics.Platform.PlatformImage.FromStream(stream);
    }

    private static string Combine(params string[] paths) => string.Join('.', paths.Where(x => !x.IsEmpty()));
}
