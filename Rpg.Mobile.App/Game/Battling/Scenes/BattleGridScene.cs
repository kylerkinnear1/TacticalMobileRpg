﻿using Rpg.Mobile.App.Game.Battling.Components;
using Rpg.Mobile.App.Game.Menu;
using Rpg.Mobile.GameSdk;
using static Rpg.Mobile.App.Game.Sprites;

namespace Rpg.Mobile.App.Game.Battling.Scenes;

public class BattleGridScene : SceneBase
{
    private readonly MapComponent _map;
    private readonly GridComponent _grid;
    private readonly MiniMapComponent _miniMap;
    private readonly ButtonComponent _attackButton;
    private readonly ButtonComponent _waitButton;
    private readonly TileShadowComponent _moveShadow;

    private readonly List<BattleUnitComponent> _battleUnits;
    private ITween<PointF>? _movingUnit;
    private SpeedTween? _cameraMove;

    public BattleGridScene()
    {
        _grid = new GridComponent(GridClicked, 10, 15);
        _map = Add(new MapComponent(new(0f, 0f, _grid.Bounds.Width, _grid.Bounds.Height)));
        _map.AddChild(_grid);

        _miniMap = Add(new MiniMapComponent(MiniMapClicked, new(1200f, 500f, 100f, 100f))
        {
            IgnoreCamera = true
        });

        var buttonTop = 0f;
        _attackButton = Add(new ButtonComponent(new(1200f, buttonTop += 50f, 100f, 50f), "Attack", OnAttack) { IgnoreCamera = true });
        _waitButton = Add(new ButtonComponent(new(1200f, buttonTop += 60f, 100f, 50f), "Wait", OnWait) { IgnoreCamera = true });
        _moveShadow = Add(new TileShadowComponent(_map.Bounds));

        _battleUnits = new()
        {
            _map.AddChild(new BattleUnitComponent(Images.ArcherIdle01, new(0))),
            _map.AddChild(new BattleUnitComponent(Images.HealerIdle01, new(0))),
            _map.AddChild(new BattleUnitComponent(Images.MageIdle01, new(0))),
            _map.AddChild(new BattleUnitComponent(Images.NinjaIdle01, new(0))),
            _map.AddChild(new BattleUnitComponent(Images.WarriorIdle01, new(0))),
            _map.AddChild(new BattleUnitComponent(Images.ArcherIdle02, new(1))),
            _map.AddChild(new BattleUnitComponent(Images.HealerIdle02, new(1))),
            _map.AddChild(new BattleUnitComponent(Images.MageIdle02, new(1))),
            _map.AddChild(new BattleUnitComponent(Images.NinjaIdle02, new(1))),
            _map.AddChild(new BattleUnitComponent(Images.WarriorIdle02, new(1)))
        };

        foreach (var (unit, index) in _battleUnits.Where(x => x.State.PlayerId == 0).Select((x, i) => (x, i)))
        {
            unit.MoveTo(_grid.GetPositionForTile(1, (index * 2) + 1));
        }

        foreach (var (unit, index) in _battleUnits.Where(x => x.State.PlayerId == 1).Select((x, i) => (x, i)))
        {
            unit.MoveTo(_grid.GetPositionForTile(9, (index * 2) + 1));
        }

        ActiveCamera.Offset = new PointF(-10f, -30f);
    }

    public void OnAttack(IEnumerable<PointF> touches)
    {

    }

    public void OnWait(IEnumerable<PointF> touches)
    {
    }

    public void MiniMapClicked(PointF touch)
    {
        var xPercent = touch.X / _miniMap.Bounds.Width;
        var yPercent = touch.Y / _miniMap.Bounds.Height;
        var target = new PointF(ActiveCamera.Size.Width * xPercent, ActiveCamera.Size.Height * yPercent);
        _cameraMove = ActiveCamera.Offset.TweenTo(target, 100f);
    }

    public void GridClicked(int x, int y)
    {
        var unit = _battleUnits[0];
        var currentY = (int)(unit.Position.Y / _grid.Size);
        var horizontalTarget = _grid.GetPositionForTile(x, currentY);
        var finalTarget = _grid.GetPositionForTile(x, y);

        _movingUnit = _battleUnits[0].Position.TweenTo(200f, horizontalTarget, finalTarget);
    }

    public override void Update(float deltaTime)
    {
        if (_movingUnit is not null)
            _battleUnits[0].MoveTo(_movingUnit.Advance(deltaTime));

        if (_cameraMove is not null)
            ActiveCamera.Offset = _cameraMove.Advance(deltaTime);
    }
}
