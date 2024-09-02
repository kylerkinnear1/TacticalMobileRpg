namespace Rpg.Mobile.GameSdk.Core;

// TODO: Look at IUpdateComponent
public interface IBehavior : IUpdateComponent
{
    void Update(float deltaTime);
}
