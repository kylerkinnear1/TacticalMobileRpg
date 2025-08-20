using Rpg.Mobile.App.Game.Lobby;
using Rpg.Mobile.App.Game.MainBattle;
using Rpg.Mobile.App.Game.MainBattle.Components;
using Rpg.Mobile.App.Windows;
using Rpg.Mobile.GameSdk.Core;

namespace Rpg.Mobile.App;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();

        // TODO: Inject
        var gameLoopFactory = new GameLoopFactory();

        // TODO: Inject
        var mouse = new MouseWindowsUser32();

        // TODO: Inject mouse into scene
        // TODO: figure out how to dispose from this form (probably DI?)
        var lobby = new LobbyScene();
        var battleScene = new BattleGridScene(mouse);
        var game = gameLoopFactory.Create(GameView, lobby, mouse);
        game.Start();
    }
}

