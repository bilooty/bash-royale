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
        [ProjectileType.WizardBoom] = new EntityDisplay(
            [[new ColoredGlyph(Color.Orange, Color.Yellow, '*')]], isTransparent:false, flashing:true),
        [ProjectileType.FireBallBoom] = new EntityDisplay(
            [[new ColoredGlyph(Color.Orange, Color.Yellow, '*')]], isTransparent:false, flashing:true),
        [ProjectileType.DragonBoom] = new EntityDisplay(
            [[new ColoredGlyph(Color.Orange, Color.Yellow, '*')]], isTransparent:false, flashing:true),
        [ProjectileType.Arrow] = new EntityDisplay(
            [[new ColoredGlyph(Color.Black, Color.White, '^'), new ColoredGlyph(Color.White, Color.White, 'o')]], isTransparent:true, flashing:false),
        [ProjectileType.CannonBall] = new EntityDisplay(
            [[new ColoredGlyph(Color.Black, Color.White, 'o'), new ColoredGlyph(Color.White, Color.White, 'o')]], isTransparent:true, flashing:false),
        [ProjectileType.WizardBall] = new EntityDisplay(
            [[new ColoredGlyph(Color.Orange, Color.White, 'o'), new ColoredGlyph(Color.White, Color.White, 'o')]], isTransparent:true, flashing:false),
        [ProjectileType.FireBallSummon] = new EntityDisplay(
            [[new ColoredGlyph(Color.Orange, Color.White, 'O'), new ColoredGlyph(Color.White, Color.White, 'o')]], isTransparent:true, flashing:false),
        [ProjectileType.FireBall] = new EntityDisplay(
            [[new ColoredGlyph(Color.Orange, Color.Red, 'o'), new ColoredGlyph(Color.White, Color.White, 'o')]], isTransparent:false, flashing:false),
        [ProjectileType.DragonBall] = new EntityDisplay(
            [[new ColoredGlyph(Color.Orange, Color.White, 'o'), new ColoredGlyph(Color.White, Color.White, 'o')]], isTransparent:true, flashing:false),
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
                    new ColoredGlyph(Color.Beige, Color.DarkGray, 'G'),
                    new ColoredGlyph(Color.Red, Color.DarkGray, 'G')
                ],
                [
                    new ColoredGlyph(Color.Red, Color.DarkGray, 'G'),
                    new ColoredGlyph(Color.Red, Color.DarkGray, 'G')
                ]
            ]
        ),
        [UnitType.Berserker] = new EntityDisplay(
        [
            [new ColoredGlyph(Color.Orange, Color.DarkGray, 'B')]]),
        [UnitType.Balloon] = new EntityDisplay(
        [
            [new ColoredGlyph(Color.Yellow, Color.DarkGray, 'L')]]),
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
        [UnitType.Brawler] = new EntityDisplay(
            [
                [new ColoredGlyph(Color.Lime, Color.DarkGray, 'G')]
            ]
        ),
        [UnitType.Wizard] = new EntityDisplay(
            [
                [new ColoredGlyph(Color.Magenta, Color.DarkGray, 'W')]
            ]
        ),
        [UnitType.Cannon] = new EntityDisplay(
            [
                [new ColoredGlyph(Color.Silver, Color.DarkGray, 'C')]
            ]
        ),
        [UnitType.HogRider] = new EntityDisplay(
            [
                [new ColoredGlyph(Color.Orange, Color.DarkGray, 'h')]
            ]
        ),

        [UnitType.Barbarian] = new EntityDisplay(
            [
                [new ColoredGlyph(Color.Yellow, Color.DarkGray, 'B')]
            ]
        ),
        [UnitType.EBarbs] = new EntityDisplay(
            [
                [new ColoredGlyph(Color.Silver, Color.DarkGray, 'B')]
            ]
        ),
        [UnitType.GoblinCage] = new EntityDisplay(
            [
                [new ColoredGlyph(Color.LightYellow, Color.DarkGray, 'G')]
            ]
        ),
        [UnitType.Musketeer] = new EntityDisplay(
            [
                [new ColoredGlyph(Color.LightBlue, Color.DarkGray, 'M')]
            ]
        ),
        [UnitType.MiniPekka] = new EntityDisplay(
            [
                [new ColoredGlyph(Color.MediumPurple, Color.DarkGray, 'p')]
            ]
        ),
        [UnitType.Pekka] = new EntityDisplay(
            [
                [new ColoredGlyph(Color.MediumPurple, Color.DarkGray, 'P')]
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