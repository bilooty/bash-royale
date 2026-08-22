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
                    new ColoredGlyph(Color.Red, Color.DarkGray, 'G'),
                    new ColoredGlyph(Color.Red, Color.DarkGray, 'G')
                ],
                [
                    new ColoredGlyph(Color.Red, Color.DarkGray, 'G'),
                    new ColoredGlyph(Color.Red, Color.DarkGray, 'G')
                ]
            ]
        ),
        [UnitType.Castle] = new UnitDisplay(
            [
            [new ColoredGlyph(Color.White, Color.DarkGray, 'C')]]),
        [UnitType.Tower] = new UnitDisplay(
        [
            [new ColoredGlyph(Color.White, Color.DarkGray, 'T')]]),
        [UnitType.Archer] = new UnitDisplay(
            [
                [new ColoredGlyph(Color.Yellow, Color.DarkGray, 'A')]
            ]
        ),
        [UnitType.Goblin] = new UnitDisplay(
            [
                [new ColoredGlyph(Color.Green, Color.DarkGray, 'g')]
            ]
        ),
        [UnitType.Wizard] = new UnitDisplay(
            [
                [new ColoredGlyph(Color.Purple, Color.DarkGray, 'W')]
            ]
        ),
        [UnitType.HogRider] = new UnitDisplay(
            [
                [new ColoredGlyph(Color.Orange, Color.DarkGray, 'h')]
            ]
        ),

    };
}