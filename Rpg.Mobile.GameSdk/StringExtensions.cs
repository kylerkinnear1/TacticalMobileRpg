﻿namespace Rpg.Mobile.GameSdk;

public static class StringExtensions
{
    public static bool IsBlank(this string? value) => string.IsNullOrWhiteSpace(value);
    public static bool IsEmpty(this string? value) => string.IsNullOrEmpty(value);
}
