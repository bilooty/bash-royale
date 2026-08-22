using System.Runtime.CompilerServices;
using bash_royale.Networking;
using SadConsole.Input;
using System;
namespace bash_royale.Scenes;

// UI/BattleRenderer.cs
public class BattleRenderer : SadConsole.ScreenSurface
{
    private static readonly Keys[] HandSlotKeys = { Keys.D1, Keys.D2, Keys.D3, Keys.D4 };
    private Color p1Color = Color.Blue;
    private Color p2Color = Color.Red;
    private GameState _gameState;
    private ScreenSurface _unitLayer;
    private ScreenSurface _guiLayer;
    private double _timer = 0.05f;
    private string _ipAddress = "127.0.0.1";
    private bool _isHost;
    
    private int _executionTick = 0;
    private int _inputTick = 0;
    private const int COMMAND_DELAY = 10;

    private bool _isPrimed = false;
    private Dictionary<int, NetworkAction> _localInputs = new();
    private int tick = 0;
    private bool _hasSentAction = false;
    private NetworkManager _networkManager;
    private NetworkAction? _pendingLocalAction;
    public BattleRenderer(string ipAddress, bool isHost) : base(GameSettings.GAME_WIDTH, GameSettings.GAME_HEIGHT)
    {
        _isHost = isHost;
        _ipAddress = ipAddress;
        
        _networkManager =  new NetworkManager();
        if (isHost)
        {
            _networkManager.StartHost(9050);
        }
        else
        {
            _networkManager.StartClient(ipAddress, 9050);
        }
        
        // 1. Initialize your deterministic engine
        _gameState = GameState.CreateNew();
        SetupTestBattle();
        _unitLayer = new ScreenSurface(GameSettings.GAME_WIDTH, GameSettings.GAME_HEIGHT);
        _unitLayer.Surface.DefaultBackground = Color.Transparent;
        _guiLayer = new ScreenSurface(28, 8);
        _guiLayer.Surface.DefaultBackground = Color.Transparent;
        _guiLayer.Position = new Point(0, ArenaMap.Height); 
        Children.Add(_guiLayer);
        
        
        PlayerState p1 = _gameState.PlayerOne;
        // p1.Units.Add(new UnitState(UnitType.Castle, PlayerId.Two, new Vector2Int(10, 4)));
        // p1.Units.Add(new UnitState(UnitType.Knight, PlayerId.One, new Vector2Int(5, 5)));
        PlayerState p2 = _gameState.PlayerTwo;
        
        
         _gameState.PlayerOne = p1;
         _gameState.PlayerTwo = p2;
        Children.Add(_unitLayer);
        // 3. Draw the static map onto the base surface once
        DrawArena();

        UseKeyboard = true;
        IsFocused = true;
    }

    public override bool ProcessKeyboard(Keyboard keyboard)
    {
        // Prevent queuing a second card if one is already waiting to be sent
        if (_pendingLocalAction != null && _pendingLocalAction.Action != ActionType.NoAction)
            return base.ProcessKeyboard(keyboard);

        for (int i = 0; i < HandSlotKeys.Length; i++)
        {
            if (keyboard.IsKeyPressed(HandSlotKeys[i]))
            {
                Vector2Int deployPosition = _isHost ? new Vector2Int(ArenaMap.Width / 2, ArenaMap.Height - 5) : new Vector2Int(ArenaMap.Width / 2, 5);
                
                // Just save the intent. The Update loop assigns the Tick and PlayerId.
                _pendingLocalAction = new NetworkAction
                {
                    Action = ActionType.DeployCard,
                    CardIdx = (byte)i,
                    X = (byte)deployPosition.X,
                    Y = (byte)deployPosition.Y,
                };
                return true;
            }
        }

        return base.ProcessKeyboard(keyboard);
    }
    
    private bool ShouldDrawSprout(int x, int y)
    {
        // Use large prime numbers to create a chaotic but repeatable hash
        uint hash = (uint)x * 374761393u ^ (uint)y * 668265263u;
    
        // Mix the bits further to eliminate any remaining geometric patterns
        hash = (hash ^ (hash >> 13)) * 1274126177u;
    
        // Use modulo to set the density. 
        // Example: hash % 100 < 15 means a ~15% chance for a sprout.
        return (hash % 100) < 15; 
    }
    

    public void DrawUnits(PlayerState player)
    {
        foreach (UnitState unit in player.Units)
        {
            Vector2Int pos = unit.Position;
            UnitDisplay display = UnitDisplay.Displays[unit.Type];
            Vector2Int size = UnitInfos.GetUnitInfo(unit.Type).Size;
            for (int x = 0; x < size.X; x++)
            {
                for (int y = 0; y < size.Y; y++)
                {
                    ColoredGlyph glyph = display.Glyphs[y][x];
                    _unitLayer.Surface[pos.X, pos.Y].Background = glyph.Background;
                    _unitLayer.Surface[pos.X, pos.Y].Foreground = player.Id == PlayerId.One ? p1Color : p2Color;
                    //_unitLayer.Surface[pos.X, pos.Y].Foreground = glyph.Foreground;
                    _unitLayer.Surface[pos.X, pos.Y].GlyphCharacter = glyph.GlyphCharacter;
                    //System.Console.WriteLine("[" + glyph.GlyphCharacter + "] ticks: " + unit.Ticks + " last tick:" + unit.LastAttackTick);
                    if ((unit.Ticks - unit.LastAttackTick) < 5)
                    {
                        _unitLayer.Surface[pos.X, pos.Y].GlyphCharacter = ' ';
                    }
                    if ((unit.Ticks - unit.LastDamageTick) < 5)
                    {

                        _unitLayer.Surface[pos.X, pos.Y].Background = Color.Red;
                    }
                }
            }
        }
    }
public override void Update(TimeSpan delta)
    {
        _networkManager.PollEvents();
    
        _unitLayer.Surface.Clear();
        _guiLayer.Surface.Clear();
        DrawUnits(_gameState.PlayerOne);
        DrawUnits(_gameState.PlayerTwo);
        DrawGUI();

        if (!_networkManager.IsConnected)
        {
            _guiLayer.Surface.Print(2, 6, "Waiting for opponent...", Color.Yellow, Color.Black);
            base.Update(delta);
            return; 
        }        

        // --- PRIME THE PUMP ---
        // The moment we connect, send 10 future ticks of NoAction so both 
        // clients have a buffer to start playing immediately without freezing.
        if (!_isPrimed)
        {
            for (int i = 0; i < COMMAND_DELAY; i++)
            {
                var blankAction = new NetworkAction { Tick = _inputTick, PlayerId = (_isHost) ? (byte)0 : (byte)1, Action = ActionType.NoAction };
                _localInputs[_inputTick] = blankAction;
                _networkManager.SendAction(blankAction);
                _inputTick++;
            }
            _isPrimed = true;
        }

        _timer -= delta.TotalSeconds;
    
        if (_timer <= 0f)
        {
            // === THE BUFFERED LOCKSTEP GATE ===
            // We only look at _executionTick (Tick 0, 1, 2...). 
            // We do NOT care if future packets haven't arrived yet!
            if (!_networkManager.RemoteInputs.ContainsKey(_executionTick))
            {
                _timer = 0f; // A packet took longer than 0.5 seconds! Stutter!
            }
            else
            {
                // 1. GET BOTH ACTIONS FOR CURRENT TICK
                NetworkAction remoteAction = _networkManager.RemoteInputs[_executionTick];
                NetworkAction localAction = _localInputs[_executionTick];
                
                if (remoteAction.Action != ActionType.NoAction)
                    System.Console.WriteLine("Received: pid: " + remoteAction.PlayerId + " x" + remoteAction.X + " y" + remoteAction.Y);

                // 2. ADVANCE THE ENGINE
                if (_isHost)
                    _gameState = GameSim.Update(_gameState, localAction, remoteAction);
                else
                    _gameState = GameSim.Update(_gameState, remoteAction, localAction);
              
                // 3. CLEAN UP EXECUTED TICK
                _networkManager.RemoteInputs.Remove(_executionTick);
                _localInputs.Remove(_executionTick);
                _executionTick++;

                // 4. GENERATE AND SEND THE FUTURE INPUT TICK (Tick + 10)
                NetworkAction nextInput = _pendingLocalAction ?? new NetworkAction { Action = ActionType.NoAction };
                nextInput.Tick = _inputTick;
                nextInput.PlayerId = _isHost ? (byte)0 : (byte)1;
                
                _localInputs[_inputTick] = nextInput;
                _networkManager.SendAction(nextInput);
                
                _pendingLocalAction = null; // Clear the keyboard buffer
                _inputTick++;
                tick++;
                _timer = 0.05;
            }
        }
    
        DrawUnits(_gameState.PlayerOne);
        DrawUnits(_gameState.PlayerTwo);
        base.Update(delta);
    }
    private void DrawArena()
    {
        for (int y = 0; y < ArenaMap.Height; y++)
        {
            for (int x = 0; x < ArenaMap.Width; x++)
            {
                TileType tile = ArenaMap.Grid[x, y];

                ColoredGlyph cellAppearance;

                switch (tile)
                {
                    case TileType.Grass:
                        ColoredGlyph baseGrass = new ColoredGlyph(Color.LightGreen, Color.LightGreen, ' ');
                        ColoredGlyph grassSprout = new ColoredGlyph(Color.LightSeaGreen, Color.LightGreen, '"');
                        cellAppearance = baseGrass;
                        
                        // Your modulus logic for random sprouts
                        if (ShouldDrawSprout(x,y))
                        {
                            cellAppearance = grassSprout;
                        }
                        break;
                
                    case TileType.Water:
                        // You can use a similar trick here to make the water look animated or textured later!
                        cellAppearance = new ColoredGlyph(Color.Cyan, Color.Aquamarine, ' ');
                        break;
                
                    case TileType.Bridge:
                        cellAppearance = new ColoredGlyph(Color.BurlyWood, Color.BurlyWood, ' ');
                        break;
                        
                    default:
                        cellAppearance = new ColoredGlyph(Color.White, Color.Black, ' ');
                        break;
                }

                // Apply the visual state to the specific grid coordinate on the screen
                Surface.SetCellAppearance(x, y, cellAppearance);
            }
        }
    }

    private void DrawGUI()
    {
        _guiLayer.Surface.Clear();
        _guiLayer.Surface.DrawBox(
            new Rectangle(0, 0, _guiLayer.Surface.Width, _guiLayer.Surface.Height),
            ShapeParameters.CreateBorder(new ColoredGlyph(Color.Black, Color.Gray)));
        
        int cardWidth = 5;
        int cardHeight = 3;
        int spacing = 2;
        int startX = 1;
        int startY = 4;

        PlayerState player = _gameState.PlayerOne;
        for (int i = 0; i < player.Hand.Count; i++)
        {
            CardInfo card = CardInfos.GetCardInfo(player.Hand[i]);
            Color color = player.Elixir >= card.Cost ? Color.Cyan : Color.Gray;
            int cardX = startX + (i * (cardWidth + spacing));
            int cardY = startY;
            string label = card.Id switch
            {
                CardId.Knight  => "KNGHT",
                CardId.Giant   => "GIANT",
                CardId.Archer  => "ARCHR",
                CardId.Goblin  => "GOBLN",
                CardId.Wizard  => "WIZRD",
                CardId.Horde   => "HORDE",
                CardId.FireBall => "FRBAL",
                _ => card.Id.ToString()[..5],
                };
            _guiLayer.Surface.SetGlyph(cardX, cardY, 218 , color);
            _guiLayer.Surface.SetGlyph(cardX+cardWidth-1, cardY, 191 , color);

            _guiLayer.Surface.SetGlyph(cardX, cardY + cardHeight-1,192, color);
            _guiLayer.Surface.SetGlyph(cardX+cardWidth-1, cardY + cardHeight - 1,217, color);

            for (int row = 1; row < cardHeight - 1; row++)
            {
                _guiLayer.Surface.SetGlyph(cardX, cardY + row,179, color);
                _guiLayer.Surface.SetGlyph(cardX + cardWidth - 1, cardY + row, 179, color);
            }

            for (int col = 1; col < cardWidth - 1; col++)
            {
                _guiLayer.Surface.SetGlyph(cardX +col, cardY,196, color);
                _guiLayer.Surface.SetGlyph(cardX + col, cardY+cardHeight-1, 196, color);
            }
            _guiLayer.Surface.Print(cardX, cardY-1, label, color);


            _guiLayer.Surface.Print(cardX+cardWidth-1, cardY+cardHeight-1, card.Cost.ToString(), Color.Magenta);
            int centerX = cardX + (cardWidth / 2);
            int centerY = cardY + (cardHeight / 2);
            if (card is UnitCard unitCard && UnitDisplay.Displays.TryGetValue(unitCard.UnitType, out var display))
            {
                ColoredGlyph g = display.Glyphs[0][0];
                _guiLayer.Surface[centerX, centerY].GlyphCharacter = g.GlyphCharacter;
                _guiLayer.Surface[centerX, centerY].Foreground = g.Foreground;
            }
            else
            {
                _guiLayer.Surface.Print(centerX, centerY, card.Cost.ToString(), Color.Red);
            }
        }
        
        _guiLayer.Surface.Print(2, 2, "=== HAND ===", Color.Yellow);
        _guiLayer.Surface.Print(2, 1, $"Elixir: {player.Elixir:0.0} / {GameSettings.MAX_ELIXIR:0}", Color.Magenta);
        
    }
    private void SetupTestBattle()
    {
        PlayerState p1 = _gameState.PlayerOne;
        PlayerState p2 = _gameState.PlayerTwo;

        // Player Two defends the top, Player One the bottom.
        p2.Units.Add(new UnitState(UnitType.Castle, PlayerId.Two, new Vector2Int(13, 2)));
        p2.Units.Add(new UnitState(UnitType.Tower,  PlayerId.Two, new Vector2Int(4, 6)));
        p2.Units.Add(new UnitState(UnitType.Tower,  PlayerId.Two, new Vector2Int(22, 6)));

        p1.Units.Add(new UnitState(UnitType.Castle, PlayerId.One, new Vector2Int(13, 29)));
        p1.Units.Add(new UnitState(UnitType.Tower,  PlayerId.One, new Vector2Int(4, 25)));
        p1.Units.Add(new UnitState(UnitType.Tower,  PlayerId.One, new Vector2Int(22, 25)));

        // Five knights a side, spread across the width so none share a spawn cell.
        // Well back from the river so you can watch them route to the bridges.
        // for (int i = 0; i < 5; i++)
        // {
        //     int x = 4 + i * 5;
        //     p1.Units.Add(new UnitState(UnitType.Knight, PlayerId.One, new Vector2Int(x, 21)));
        //     p2.Units.Add(new UnitState(UnitType.Knight, PlayerId.Two, new Vector2Int(x, 10)));
        // }

        _gameState.PlayerOne = p1;
        _gameState.PlayerTwo = p2;
    }
    private void DrawState()
    {
        Surface.Clear();

        // Draw Arena (River, Bridges, Towers)
        
        
    }
}