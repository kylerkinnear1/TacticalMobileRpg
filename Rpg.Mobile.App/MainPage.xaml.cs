using Rpg.Mobile.App.Battling.Scenes;
using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App;

public partial class MainPage : ContentPage
{
    private readonly BattleScene _scene;

    public MainPage()
    {
        InitializeComponent();

        var updates = new List<IUpdateGameObject>();
        var renders = new List<IRenderGameObject>();
        _scene = new BattleScene(updates, renders);
        var gameLoopFactory = new GameLoopFactory();
        var game = gameLoopFactory.Create(GameView, _scene, updates, renders);
        game.Start();
    }

    private void GameView_OnStartInteraction(object? sender, TouchEventArgs e) => _scene.OnClickDown(e);

    private void GameView_OnEndInteraction(object? sender, TouchEventArgs e) => _scene.OnClickUp(e);
}

