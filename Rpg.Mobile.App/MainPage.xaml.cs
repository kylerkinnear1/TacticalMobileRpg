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
        var touchUpHandlers = new List<(Action<TouchEvent> Handler, Func<RectF>? BoundsProvider)>();
        _scene = new BattleScene(updates, renders, touchUpHandlers);
        var gameLoopFactory = new GameLoopFactory();
        var game = gameLoopFactory.Create(GameView, updates, renders, touchUpHandlers);
        game.Start();
    }
}

