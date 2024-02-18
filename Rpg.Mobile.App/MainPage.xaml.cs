using Rpg.Mobile.App.Battling.Scenes;
using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();

        var gameLoopFactory = new GameLoopFactory();
        var game = gameLoopFactory.Create(GameView);

        _ = new BattleScene(game);
        game.Start();
    }
}

