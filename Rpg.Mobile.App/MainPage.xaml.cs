using Rpg.Mobile.App.Battling.Scenes;
using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();

        var gameLoopFactory = new GameLoopFactory();
        var componentList = new List<IComponent>();
        var scene = new BattleScene();
        var game = gameLoopFactory.Create(GameView, scene);
        game.Start();
    }
}

