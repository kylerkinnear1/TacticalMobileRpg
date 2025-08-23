using Microsoft.AspNetCore.SignalR.Client;
using Rpg.Mobile.Api.Battles;
using Rpg.Mobile.Api.Battles.Calculators;
using Rpg.Mobile.Api.Battles.Data;
using Rpg.Mobile.Api.Lobby;
using Rpg.Mobile.App.Game;
using Rpg.Mobile.App.Game.Lobby;
using Rpg.Mobile.App.Game.MainBattle;
using Rpg.Mobile.App.Game.Splash;
using Rpg.Mobile.App.Windows;
using Rpg.Mobile.GameSdk.Core;
using Rpg.Mobile.GameSdk.StateManagement;

namespace Rpg.Mobile.App;

public partial class MainPage : ContentPage
{
    private readonly SceneManager _scenes;
    private readonly LobbyNetwork _lobbyNetwork;
    private readonly BattleNetwork _battleNetwork;
    
    public MainPage()
    {
        InitializeComponent();
        
        var gameLoopFactory = new GameLoopFactory();
        var mouse = new MouseWindowsUser32();
        
        var bus = new EventBus();
        var settings = DiContainer.Services!.GetRequiredService<GameSettings>();

        var splash = new SplashScene();
        var game = gameLoopFactory.Create(GameView, splash, mouse);

        var lobby = new LobbyScene(bus, game, settings);
        _scenes = new SceneManager(lobby, game, bus, mouse, new PathCalculator());

        var hub = DiContainer.Services!.GetRequiredService<HubConnection>();
        _lobbyNetwork = new(new LobbyClient(hub), bus, game);
        _battleNetwork = new(new BattleClient(hub), bus, game);
        game.Start();
    }

    protected override void OnAppearing()
    {
        _scenes.Start();
        _lobbyNetwork.Connect();
        _battleNetwork.Connect();
        
        base.OnAppearing();
    }

    protected override void OnDisappearing()
    {
        _scenes.Stop();
        _lobbyNetwork.Disconnect();
        _battleNetwork.Disconnect();
        base.OnDisappearing();
    }
}

