using Microsoft.Maui.Graphics;

namespace Rpg.Mobile.GameSdk.Extensions;

public static class StringExtensions
{
    public static bool IsBlank(this string? value) => string.IsNullOrWhiteSpace(value);
    public static bool IsEmpty(this string? value) => string.IsNullOrEmpty(value);
}
