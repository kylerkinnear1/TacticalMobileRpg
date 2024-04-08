using Rpg.Mobile.App.Game.Battling.Scenes;
using Rpg.Mobile.App.Windows;
using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        var gameLoopFactory = new GameLoopFactory();
        var mouse = new MouseWindowsUser32();
        var scene = new BattleGridScene(mouse);
        var game = gameLoopFactory.Create(GameView, scene, mouse);
        game.Start();
    }
}

