using System.Drawing;

namespace Rpg.Mobile.GameEngine.Scenes.Battling.Rules;

public record UnitId(int Id);
public record Unit(
    UnitId Id,
    PlayerId PlayerId,
    string Name,
    Point Position,
    Modified<UnitStats> Stats,
    int RemainingStamina,
    List<Ability> Abilities);

public record Modified<T>(T Base, T Current);
public record UnitStats(int Stamina, int Attack, int Defense, int Move);
public record AbilityId(int Id);
public record Ability(AbilityId Id);
