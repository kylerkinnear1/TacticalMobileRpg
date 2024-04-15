using Rpg.Mobile.App.Game.Battling.Scenes;
using Rpg.Mobile.App.Windows;
using Rpg.Mobile.GameSdk;

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
        var scene = new BattleGridScene(mouse);
        var game = gameLoopFactory.Create(GameView, scene, mouse);
        game.Start();
    }
}

