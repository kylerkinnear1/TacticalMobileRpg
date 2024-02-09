using Rpg.Mobile.App.Battling.Units;
using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App;

internal class GameSceneView : GraphicsView
{
    public GameSceneView()
    {
        BackgroundColor = Colors.Transparent;

        var scene = new BattleScene(this);
        Drawable = scene;

        var loop = new GameLoop(Dispatcher, scene);
        loop.Start();
    }
}
