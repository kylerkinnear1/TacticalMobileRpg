using Microsoft.VisualBasic.CompilerServices;
using Rpg.Mobile.Api;

namespace Rpg.Mobile.Server.Battles;

public class MapLoaderServer
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