using Rpg.Mobile.GameSdk.StateManagement;

namespace Rpg.Mobile.Server.Battles;

public record TileClickedEvent(int PlayerId, Point Tile) : IEvent;
