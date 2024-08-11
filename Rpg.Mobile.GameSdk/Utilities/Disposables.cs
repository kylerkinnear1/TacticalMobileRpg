public static class DisposableEnumerableExtensions
{
    public static void DisposeAll(this IEnumerable<IDisposable> disposables)
    {
        foreach (var d in disposables)
            d.Dispose();
    }
}
