using Rpg.Mobile.Api.Battles;
using Rpg.Mobile.Api.Battles.Data;
using Rpg.Mobile.Api.Lobby;

namespace Rpg.Mobile.Api;

public interface IEventApi : ILobbyEventApi, IBattleEventApi
{
}