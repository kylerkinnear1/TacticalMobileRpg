namespace Rpg.Mobile.GameSdk.Utilities;

public class Array2d<T>
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

    public void Each(Action<int, int> stub)
    {
        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Height; y++)
            stub(x, y);
    }

    public IEnumerable<KeyValuePair<(int X, int Y), T>> Where(Func<int, int, bool> stub)
    {
        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Height; y++)
        {
            if (stub(x, y))
                yield return new((x, y), this[x, y]);
        }
    }

    public IEnumerable<KeyValuePair<(int X, int Y), T>> Where(Func<T, bool> stub)
    {
        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Height; y++)
        {
            var value = this[x, y];
            if (stub(value))
                yield return new((x, y), value);
        }
    }

    public IEnumerable<KeyValuePair<(int X, int Y), T>> Where(Func<int, int, T, bool> stub)
    {
        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Height; y++)
        {
            var value = this[x, y];
            if (stub(x, y, value))
                yield return new((x, y), value);
        }
    }

    public IEnumerable<TResult> Select<TResult>(Func<int, int, TResult> converter)
    {
        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Height; y++)
        {
            yield return converter(x, y);
        }
    }

    public IEnumerable<TResult> Select<TResult>(Func<int, int, T, TResult> converter)
    {
        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Height; y++)
        {
            yield return converter(x, y, this[x, y]);
        }
    }
}
