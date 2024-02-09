using Microsoft.Maui.Graphics;

namespace Rpg.Mobile.GameSdk;

public interface IUpdateState<TState>
{
    void Update(TState state);
}

public interface ICalculateState<TState>
{
    TState Calculate(TState state);
}

public interface IRenderState<TState>
{
    void Render(TState state, ICanvas canvas, RectF dirtyRect);
}

public interface IUpdateGameObject<TState> : IUpdateState<TState>, IRenderState<TState> { }
public interface ICalculateGameObject<TState> : ICalculateState<TState>, IRenderState<TState> { }
