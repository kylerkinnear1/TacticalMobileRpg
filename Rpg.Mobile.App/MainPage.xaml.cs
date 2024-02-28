using Rpg.Mobile.App.Scenes.BattleGrid;
using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();

        var gameLoopFactory = new GameLoopFactory();
        var scene = new BattleGridScene();
        var game = gameLoopFactory.Create(GameView, scene);
        game.Start();
    }
}

