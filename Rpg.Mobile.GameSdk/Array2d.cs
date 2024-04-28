using System.Collections;

namespace Rpg.Mobile.GameSdk;

public class Array2d<T> : IEnumerable<T>
{
    public readonly T[] Data;
    public readonly int Width;
    public readonly int Height;

    public Array2d(int width, int height)
    {
        Width = width;
        Height = height;
        Data = new T[Width * Height];
    }

    public T this[int x, int y]
    {
        get => Data[y * Width + x];
        set => Data[y * Width + x] = value;
    }

    public T this[int yPlusX]
    {
        get => Data[yPlusX];
        set => Data[yPlusX] = value;
    }

    public IEnumerable<T> GetColumn(int x)
    {
        for (var y = 0; y < Height; y++)
            yield return Data[y * Width + x];
    }

    public IEnumerable<T> GetRow(int y)
    {
        for (var x = 0; x < Width; x++)
            yield return Data[y * Width + x];
    }

    public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)Data).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => Data.GetEnumerator();

    public void Each(Action<int, int> stub)
    {
        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Height; y++)
            stub(x, y);
    }
}
