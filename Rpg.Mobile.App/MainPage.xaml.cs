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
        var scene = new BattleGridScene(new MouseWindowsUser32(), GameView);
        var game = gameLoopFactory.Create(GameView, scene);
        game.Start();
    }
}

