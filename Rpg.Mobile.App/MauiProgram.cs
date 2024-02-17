﻿using Microsoft.Extensions.Logging;
using Rpg.Mobile.App.Battling.GameObjects;
using Rpg.Mobile.GameSdk;
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

#if DEBUG
		builder.Logging.AddDebug();
#endif

        builder.Services.AddSingleton<IGameLoopFactory, GameLoopFactory>();
        
        return builder.Build();
    }
}
