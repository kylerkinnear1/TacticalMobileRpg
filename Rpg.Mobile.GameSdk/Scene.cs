namespace Rpg.Mobile.GameSdk;

public interface IScene
{
    void Update(TimeSpan delta);
    void Render();
}
