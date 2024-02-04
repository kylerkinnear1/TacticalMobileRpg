using Microsoft.Maui.Graphics;
using System.Reflection;

namespace Rpg.Mobile.GameSdk;

public interface IImageLoader
{
    IImage Load(string path);
}

public class EmbeddedResourceImageLoader : IImageLoader
{
    private readonly Assembly _assembly;

    public EmbeddedResourceImageLoader(Assembly assembly) => _assembly = assembly;

    public IImage Load(string path)
    {
        var names = _assembly.GetManifestResourceNames();
        using var stream = _assembly.GetManifestResourceStream(path);
        return Microsoft.Maui.Graphics.Platform.PlatformImage.FromStream(stream);
    }
}
