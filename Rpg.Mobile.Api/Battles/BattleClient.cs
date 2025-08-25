using System.Drawing;
using Microsoft.AspNetCore.SignalR.Client;
using Rpg.Mobile.Api.Battles.Data;
using static Rpg.Mobile.Api.Battles.IBattleClient;

namespace Rpg.Mobile.Api.Battles;

public interface IBattleClient
{
    Task TileClicked(string gameId, Point tile);
    Task AttackClicked(string gameId);
    Task MagicClicked(string gameId);
    Task WaitClicked(string gameId);
    Task BackClicked(string gameId);
    Task SpellSelected(string gameId, SpellType spell);
    
    public delegate void SetupPhaseStartedHandler(string gameId, List<BattleUnitData> units, BattleSetupPhaseData data);
    event SetupPhaseStartedHandler? SetupStarted;
    
    public delegate void UnitPlacedHandler(string gameId, int unitId, int currentPlaceOrderIndex, Point tile);
    event UnitPlacedHandler? UnitPlaced;
    
    public delegate void NewRoundStartedHandler(string gameId, List<int> turnOrderIds, int activeUnitIndex);
    event NewRoundStartedHandler? NewRoundStarted;

    public delegate void ActivePhaseStartedHandler(string gameId, BattleActivePhaseData activePhaseData);
    event ActivePhaseStartedHandler? ActivePhaseStarted;

    public delegate void IdleStepStartedHandler(string gameId, List<Point> walkableTiles);
    event IdleStepStartedHandler? IdleStepStarted;

    public delegate void IdleStepEndedHandler(string gameId, int unitId);
    event IdleStepEndedHandler? IdleStepEnded;

    public delegate void UnitMovedHandler(string gameId, int unitId, Point tile);
    event UnitMovedHandler? UnitMoved;

    public delegate void SelectingAttackTargetStartedHandler(string gameId, List<Point> attackTargetTiles);
    event SelectingAttackTargetStartedHandler? SelectingAttackTargetStarted;

    public delegate void SelectingMagicTargetStartedHandler(string gameId, SpellData spell, List<Point> magicTargetTiles);
    event SelectingMagicTargetStartedHandler? SelectingMagicTargetStarted;
    
    public delegate void SelectingSpellStartedHandler(string gameId, List<SpellData> spells);
    event SelectingSpellStartedHandler? SelectingSpellStarted;
    
    public delegate void UnitsDamagedHandler(string gameId, UnitsDamagedData evnt);
    event UnitsDamagedHandler? UnitsDamaged;
}

public class BattleClient : IBattleClient
{
    private readonly HubConnection _hub;

    public BattleClient(HubConnection hub)
    {
        _hub = hub;
        SetupEventHandlers();
    }
    public async Task TileClicked(string gameId, Point tile)
    {
        await _hub.InvokeAsync(nameof(IBattleCommandApi.TileClicked), gameId, tile);
    }

    public async Task AttackClicked(string gameId)
    {
        await _hub.InvokeAsync(nameof(IBattleCommandApi.AttackClicked), gameId);
    }

    public async Task MagicClicked(string gameId)
    {
        await _hub.InvokeAsync(nameof(IBattleCommandApi.MagicClicked), gameId);
    }

    public async Task WaitClicked(string gameId)
    {
        await _hub.InvokeAsync(nameof(IBattleCommandApi.WaitClicked), gameId);
    }

    public async Task BackClicked(string gameId)
    {
        await _hub.InvokeAsync(nameof(IBattleCommandApi.BackClicked), gameId);
    }

    public async Task SpellSelected(string gameId, SpellType spell)
    {
        await _hub.InvokeAsync(nameof(IBattleCommandApi.SpellSelected), gameId, spell);
    }

    public event SetupPhaseStartedHandler? SetupStarted;
    public event UnitPlacedHandler? UnitPlaced;
    public event NewRoundStartedHandler? NewRoundStarted;
    public event ActivePhaseStartedHandler? ActivePhaseStarted;
    public event IdleStepStartedHandler? IdleStepStarted;
    public event IdleStepEndedHandler? IdleStepEnded;
    public event UnitMovedHandler? UnitMoved;
    public event SelectingAttackTargetStartedHandler? SelectingAttackTargetStarted;
    public event SelectingMagicTargetStartedHandler? SelectingMagicTargetStarted;
    public event SelectingSpellStartedHandler? SelectingSpellStarted;
    public event UnitsDamagedHandler? UnitsDamaged;

    private void SetupEventHandlers()
    {
        _hub.On<string, List<BattleUnitData>, BattleSetupPhaseData>(nameof(IBattleEventApi.SetupStarted),
            (gameId, units, setupData) => SetupStarted?.Invoke(gameId, units, setupData));

        _hub.On<string, int, int, Point>(nameof(IBattleEventApi.UnitPlaced),
            (gameId, unitId, currentPlaceOrderIndex, tile) => UnitPlaced?.Invoke(gameId, unitId, currentPlaceOrderIndex, tile));
        
        _hub.On<string, List<int>, int>(nameof(IBattleEventApi.NewRoundStarted),
            (gameId, turnOrderIds, activeUnitIndex) => NewRoundStarted?.Invoke(gameId, turnOrderIds, activeUnitIndex));

        _hub.On<string, BattleActivePhaseData>(nameof(IBattleEventApi.ActivePhaseStarted),
            (gameId, activePhaseData) => ActivePhaseStarted?.Invoke(gameId, activePhaseData));

        _hub.On<string, List<Point>>(nameof(IBattleEventApi.IdleStepStarted),
            (gameId, walkableTiles) => IdleStepStarted?.Invoke(gameId, walkableTiles));

        _hub.On<string, int>(nameof(IBattleEventApi.IdleStepEnded),
            (gameId, unitId) => IdleStepEnded?.Invoke(gameId, unitId));

        _hub.On<string, int, Point>(nameof(IBattleEventApi.UnitMoved),
            (gameId, unitId, tile) => UnitMoved?.Invoke(gameId, unitId, tile));

        _hub.On<string, List<Point>>(nameof(IBattleEventApi.SelectingAttackTargetStarted),
            (gameId, attackTargetTiles) => SelectingAttackTargetStarted?.Invoke(gameId, attackTargetTiles));

        _hub.On<string, SpellData, List<Point>>(nameof(IBattleEventApi.SelectingMagicTargetStarted),
            (gameId, spell, magicTargetTiles) => SelectingMagicTargetStarted?.Invoke(gameId, spell, magicTargetTiles));
        
        _hub.On<string, List<SpellData>>(nameof(IBattleEventApi.SelectingSpellStarted),
            (gameId, spells) => SelectingSpellStarted?.Invoke(gameId, spells));

        _hub.On<string, UnitsDamagedData>(nameof(IBattleEventApi.UnitsDamaged),
            (gameId, evnt) => UnitsDamaged?.Invoke(gameId, evnt));
    }
}