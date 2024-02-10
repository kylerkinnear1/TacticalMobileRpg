using Microsoft.Extensions.Logging;
using Rpg.Mobile.GameSdk.Infrastructure;

namespace Rpg.Mobile.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddFactory<GenericUnitGameObject>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
