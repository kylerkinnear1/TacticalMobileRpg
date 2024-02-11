using Rpg.Mobile.App.Battling.Scenes;
using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App;

internal class GameSceneView : GraphicsView
{
    // TODO: Fix circular reference.
    private readonly BattleScene _scene;

    public GameSceneView()
    {
        BackgroundColor = Colors.Transparent;

        _scene = new BattleScene(this);
        Drawable = _scene;

        var loop = new GameLoop(Dispatcher, _scene);
        loop.Start();
    }

    public void OnClickDown(TouchEventArgs touchEventArgs) => _scene.OnClickDown(touchEventArgs);

    public void OnClickUp(TouchEventArgs touchEventArgs) => _scene.OnClickUp(touchEventArgs);
}
