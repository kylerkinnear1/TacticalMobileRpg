using Rpg.Mobile.Api.Battles.Data;
using Rpg.Mobile.Server.Utils;

namespace Rpg.Mobile.Server.Battles;

public interface IMapLoader
{
    BattleData LoadBattleData();
}

public class MapLoader : IMapLoader
{
    public BattleData LoadBattleData()
    {
        var mapPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "map.json");
        var jsonLoader = new FileClient();
        var mapJson = jsonLoader.ReadJson<MapJson>(mapPath);
        var mapState = mapJson.ToState();
        var battleData = BattleData.FromMap(mapState);
        return battleData;
    }
}