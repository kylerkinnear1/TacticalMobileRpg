using Rpg.Mobile.App.Battling.Scenes;
using Rpg.Mobile.GameSdk;
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace Rpg.Mobile.App;

public partial class MainPage : ContentPage
{
    private readonly BattleScene _scene;

    public MainPage()
    {
        InitializeComponent();

        var gameLoopFactory = new GameLoopFactory();
        var game = gameLoopFactory.Create(GameView);

        _scene = new BattleScene(game);
        game.Start();
    }
}

