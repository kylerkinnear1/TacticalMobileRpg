using Rpg.Mobile.Api.Battles.Data;
using Rpg.Mobile.Server.Utils;

namespace Rpg.Mobile.Server.Battles;

public interface IMapLoader
{
    BattleData LoadBattleData(List<BattleUnitType> team0, List<BattleUnitType> team1);
}

public class MapLoader : IMapLoader
{
    public BattleData LoadBattleData(List<BattleUnitType> team0, List<BattleUnitType> team1)
    {
        var mapPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "map.json");
        var jsonLoader = new FileClient();
        var mapJson = jsonLoader.ReadJson<MapJson>(mapPath);
        var mapState = mapJson.ToState();
        var data = new BattleData
        {
            Map = mapState,
            Team0 = team0,
            Team1 = team1
        };
        return data;
    }
}