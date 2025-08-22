using System.Text.Json.Serialization;

namespace Rpg.Mobile.GameSdk.Utilities;

public class Array2d<T>
{
    public T[] Data { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    // Parameterless constructor required for deserialization
    public Array2d()
    {
        Data = Array.Empty<T>();
        Width = 0;
        Height = 0;
    }

    public Array2d(int width, int height)
    {
        Width = width;
        Height = height;
        Data = new T[Width * Height];
    }

    [JsonIgnore]
    public T this[int x, int y]
    {
        get => Data[y * Width + x];
        set => Data[y * Width + x] = value;
    }

    [JsonIgnore]
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