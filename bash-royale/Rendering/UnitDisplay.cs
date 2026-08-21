namespace bash_royale.Scenes;

public class UnitDisplay(ColoredGlyph[][] glyphs)
{
    public ColoredGlyph[][] Glyphs => glyphs;
    public static Dictionary<UnitType, UnitDisplay> Displays = new Dictionary<UnitType, UnitDisplay>
    {
        [UnitType.Knight] = new UnitDisplay(
            [
                [new ColoredGlyph(Color.White, Color.DarkGray, 'K')]
            ]
        ),
        [UnitType.Giant] = new UnitDisplay(
            [
                [
                    new ColoredGlyph(Color.White, Color.DarkGray, 'G'),
                    new ColoredGlyph(Color.White, Color.DarkGray, 'G')
                ],
                [
                    new ColoredGlyph(Color.White, Color.DarkGray, 'G'),
                    new ColoredGlyph(Color.White, Color.DarkGray, 'G')
                ]
            ]
        ),

    };
}