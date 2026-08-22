namespace bash_royale.Rendering;



public class EntityDisplay(ColoredGlyph[][] glyphs, bool isTransparent = false, bool flashing = false)
{
    public ColoredGlyph[][] Glyphs => glyphs;
    public bool isFlashing => flashing;
    public bool IsTransparent => isTransparent;
    public static Dictionary<ProjectileType, EntityDisplay> Projectiles { get; } = new()
    {
        [ProjectileType.ZapEffect] = new EntityDisplay(
            [[new ColoredGlyph(Color.LightBlue, Color.White, 'Z'), new ColoredGlyph(Color.White, Color.White, '#')]], isTransparent:false, flashing:true),
        [ProjectileType.CannonBall] = new EntityDisplay(
            [[new ColoredGlyph(Color.Black, Color.White, '^'), new ColoredGlyph(Color.White, Color.White, 'o')]], isTransparent:true, flashing:false)
    };
    public static Dictionary<UnitType, EntityDisplay> Displays = new Dictionary<UnitType, EntityDisplay>
    {
        [UnitType.Knight] = new EntityDisplay(
            [
                [new ColoredGlyph(Color.White, Color.DarkGray, 'K')]
            ]
        ),
        [UnitType.Giant] = new EntityDisplay(
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
        [UnitType.Castle] = new EntityDisplay(
            [
            [new ColoredGlyph(Color.White, Color.DarkGray, 'C')]]),
        [UnitType.Tower] = new EntityDisplay(
        [
            [new ColoredGlyph(Color.White, Color.DarkGray, 'T')]]),
        [UnitType.Archer] = new EntityDisplay(
            [
                [new ColoredGlyph(Color.Yellow, Color.DarkGray, 'A')]
            ]
        ),
        [UnitType.Goblin] = new EntityDisplay(
            [
                [new ColoredGlyph(Color.Lime, Color.DarkGray, 'g')]
            ]
        ),
        [UnitType.Wizard] = new EntityDisplay(
            [
                [new ColoredGlyph(Color.Magenta, Color.DarkGray, 'W')]
            ]
        ),
        [UnitType.HogRider] = new EntityDisplay(
            [
                [new ColoredGlyph(Color.Orange, Color.DarkGray, 'h')]
            ]
        ),

        [UnitType.Barbarian] = new EntityDisplay(
            [
                [new ColoredGlyph(Color.Brown, Color.DarkGray, 'B')]
            ]
        ),
        [UnitType.Musketeer] = new EntityDisplay(
            [
                [new ColoredGlyph(Color.LightBlue, Color.DarkGray, 'M')]
            ]
        ),
        [UnitType.MiniPekka] = new EntityDisplay(
            [
                [new ColoredGlyph(Color.Silver, Color.DarkGray, 'P')]
            ]
        ),
        [UnitType.Valkyrie] = new EntityDisplay(
            [
                [new ColoredGlyph(Color.Pink, Color.DarkGray, 'V')]
            ]
        ),
        [UnitType.Skeleton] = new EntityDisplay(
            [
                [new ColoredGlyph(Color.White, Color.DarkGray, 's')]
            ]
        ),
        [UnitType.Dragon] = new EntityDisplay(
            [
                [new ColoredGlyph(Color.LightGreen, Color.DarkGray, 'D')]
            ]
        ),

    };
}