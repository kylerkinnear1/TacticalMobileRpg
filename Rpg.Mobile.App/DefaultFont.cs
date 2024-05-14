namespace Rpg.Mobile.App;

public class DefaultFont
{
    public const string DefaultName = "Arial";
    public static readonly Font Normal = new(DefaultName);
    public static readonly Font ExtraBold = new(DefaultName, FontWeights.ExtraBold);
    public static readonly Font ExtraBoldItalic = new(DefaultName, FontWeights.ExtraBold, FontStyleType.Italic);

    public static Font Create(int weight = 400, FontStyleType style = FontStyleType.Normal) => new(DefaultName, weight, style);
}
