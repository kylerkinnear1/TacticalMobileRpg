using Microsoft.AspNetCore.SignalR.Client;
using Rpg.Mobile.Api.Battles.Calculators;
using Rpg.Mobile.Api.Battles.Data;
using Rpg.Mobile.Api.Lobby;
using Rpg.Mobile.App.Game;
using Rpg.Mobile.App.Game.Lobby;
using Rpg.Mobile.App.Game.MainBattle;
using Rpg.Mobile.App.Windows;
using Rpg.Mobile.GameSdk.Core;
using Rpg.Mobile.GameSdk.StateManagement;

namespace Rpg.Mobile.App;

public partial class MainPage : ContentPage
{
    private readonly SceneManager _scenes;
    private readonly LobbyNetwork _lobbyNetwork;
    
    public MainPage(HubConnection hub)
    {
        InitializeComponent();
        
        var gameLoopFactory = new GameLoopFactory();
        var mouse = new MouseWindowsUser32();
        
        var bus = new EventBus();
        var lobby = new LobbyScene(bus, new GameSettings("game001"));
        var battleScene = new BattleGridScene(mouse, new BattleData(), bus, new PathCalculator());
        var game = gameLoopFactory.Create(GameView, lobby, mouse);
        
        _scenes = new SceneManager(lobby, battleScene, game, bus);
        _lobbyNetwork = new(new LobbyClient(hub), bus, game);
        game.Start();
    }

    protected override void OnAppearing()
    {
        _scenes.Subscribe();
        _lobbyNetwork.Connect();
        base.OnAppearing();
    }

    protected override void OnDisappearing()
    {
        _scenes.Unsubscribe();
        _lobbyNetwork.Disconnect();
        base.OnDisappearing();
    }
}

