using Rpg.Mobile.App.Game.Battling.Backend;
using Rpg.Mobile.App.Game.Battling.Components;
using Rpg.Mobile.App.Game.Battling.Components.Menus;
using Rpg.Mobile.App.Game.Common;
using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App.Game.Battling.Scenes;

public class BattleGridScene : SceneBase
{
    private readonly BattleComponent _battle;
    private readonly MiniMapComponent _miniMap;
    private readonly MenuComponent _battleMenu;
    private readonly StatSheetComponent _stats;
    private readonly MenuComponent _stateMenu;
    private readonly MouseCoordinateComponent _mouseComponent;
    private readonly TextboxComponent _hoverComponent;

    private ITween<PointF>? _cameraTween;

    public BattleGridScene(IMouse mouse)
    {
        Add(_battleMenu = new(new(900f, 100f, 150f, 200f)));
        Add(_miniMap = new(new(_battleMenu.Bounds.Right + 100f, _battleMenu.Bounds.Bottom + 100f, 200f, 200f)) { IgnoreCamera = true });

        var mapPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "map.json");
        var mapLoader = new MapLoader();
        var map = mapLoader.Load(mapPath);

        Add(_stats = new(new(900f, _battleMenu.Bounds.Bottom + 30f, 150, 300f)) { IgnoreCamera = true });
        Add(_battle = new(_stats, _battleMenu, new(0f, 0f), map));

        Add(_mouseComponent = new(mouse, new(_miniMap.AbsoluteBounds.Left, _miniMap.AbsoluteBounds.Bottom, 300f, 100f))
        {
            IgnoreCamera = true
        });

        
        Add(_hoverComponent = new(new(_stats.Bounds.Left, _stats.Bounds.Bottom + 100f, 300f, 200f), "")
        {
            IgnoreCamera = true,
            BackColor = Colors.DeepSkyBlue
        });

        _stateMenu = new(new(1200f, _battleMenu.Bounds.Bottom + 5f, _battleMenu.Bounds.Width, _battleMenu.Bounds.Height));
        _stateMenu.SetButtons(new("Save State", SaveStateClicked), new("Load State", LoadStateClicked));

        Bus.Global.Subscribe<TileHoveredEvent>(x => _hoverComponent.Label = $"{x.Tile.X}x{x.Tile.Y}");
        Bus.Global.Subscribe<MiniMapClickedEvent>(MiniMapClicked);

        ActiveCamera.Offset = new PointF(80f, 80f);

        _battle.StartBattle();
    }

    public override void Update(float deltaTime)
    {
        if (_cameraTween is not null)
            ActiveCamera.Offset = _cameraTween.Advance(deltaTime);
    }

    private void MiniMapClicked(MiniMapClickedEvent touch)
    {
        var xPercent = touch.Position.X / _miniMap.Bounds.Width;
        var yPercent = touch.Position.Y / _miniMap.Bounds.Height;
        var target = new PointF(ActiveCamera.Size.Width * xPercent * 2, ActiveCamera.Size.Height * yPercent * 2);
        _cameraTween = ActiveCamera.Offset.TweenTo(target, 1000f);
    }

    private void SaveStateClicked(IEnumerable<PointF> touches) => Bus.Global.Publish(new SaveStateClickedEvent());

    private void LoadStateClicked(IEnumerable<PointF> touches) => Bus.Global.Publish(new LoadStateClickedEvent());
}
