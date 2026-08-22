using System.Runtime.CompilerServices;
using bash_royale.Networking;
using SadConsole.Input;
using System;
namespace bash_royale.Scenes;

// UI/BattleRenderer.cs
public class BattleRenderer : SadConsole.ScreenSurface
{
    private static readonly Keys[] HandSlotKeys = { Keys.D1, Keys.D2, Keys.D3, Keys.D4 };
    private Color p1Color = Color.DarkBlue;
    private Color p2Color = Color.DarkRed;
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
    private int? _selectedHandIdx = null;
    private Vector2Int? _hoverCell = null;
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
        // Purely visual overlays. SadConsole walks children top-down and stops at the first
        // one that handles the mouse, so leaving these on would swallow every arena click.
        _unitLayer.UseMouse = false;
        _guiLayer = new ScreenSurface(ArenaMap.Width, 8);
        _guiLayer.Surface.DefaultBackground = Color.Transparent;
        _guiLayer.UseMouse = false;
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
        UseMouse = true;
        IsFocused = true;
        UseMouse = true; 
    }

    public override bool ProcessKeyboard(Keyboard keyboard)
    {
        // Prevent selecting a new card while one is already waiting to be sent
        if (_pendingLocalAction != null && _pendingLocalAction.Action != ActionType.NoAction)
            return base.ProcessKeyboard(keyboard);

        for (int i = 0; i < HandSlotKeys.Length; i++)
        {
            if (keyboard.IsKeyPressed(HandSlotKeys[i]))
            {
                // Arm this card for deployment; the next left click on the arena places it there.
                _selectedHandIdx = (_selectedHandIdx == i) ? null : i;
                return true;
            }
        }

        return base.ProcessKeyboard(keyboard);
    }

    public override bool ProcessMouse(MouseScreenObjectState state)
    {
        _hoverCell = state.IsOnScreenObject
            ? new Vector2Int(state.CellPosition.X, state.CellPosition.Y)
            : null;

        if (_selectedHandIdx is int handIdx
            && state.Mouse.LeftClicked
            && state.IsOnScreenObject
            && (_pendingLocalAction == null || _pendingLocalAction.Action == ActionType.NoAction))
        {
            // CellPosition is where the click landed on screen; the sim works in world space.
            Vector2Int deployPosition = Flip(new Vector2Int(state.CellPosition.X, state.CellPosition.Y));
            PlayerState player = _isHost ? _gameState.PlayerOne : _gameState.PlayerTwo;
            if (handIdx >= player.Hand.Count) return base.ProcessMouse(state);
            CardInfo card = CardInfos.GetCardInfo(player.Hand[handIdx]);

            if (IsValidDeploySpot(deployPosition, card.ValidLocation))
            {
                // Just save the intent. The Update loop assigns the Tick and PlayerId.
                _pendingLocalAction = new NetworkAction
                {
                    Action = ActionType.DeployCard,
                    CardIdx = (byte)handIdx,
                    X = (byte)deployPosition.X,
                    Y = (byte)deployPosition.Y,
                };
                _selectedHandIdx = null;
                return true;
            }
        }

        return base.ProcessMouse(state);
    }

    // The client views the arena rotated 180 degrees so its own side sits at the bottom.
    // A point reflection is its own inverse, so this one helper converts both ways:
    // screen -> world for input, world -> screen for rendering.
    private Vector2Int Flip(Vector2Int p) =>
        _isHost ? p : new Vector2Int(ArenaMap.Width - 1 - p.X, ArenaMap.Height - 1 - p.Y);

    private bool IsValidDeploySpot(Vector2Int position, ValidLocation validLocation)
    {
        if (position.X < 0 || position.X >= ArenaMap.Width) return false;
        if (position.Y < 0 || position.Y >= ArenaMap.Height) return false;
        if (!ArenaMap.IsPassable(position, MovementLayer.Ground)) return false;

        if (validLocation == ValidLocation.BothSides) return true;

        // Units can only be deployed on your own side of the river until you've crossed it.
        return _isHost ? position.Y > ArenaMap.RiverEndRow : position.Y < ArenaMap.RiverStartRow;
    }
    
    // Highlights the cell under the cursor while a card is armed: white if the spot is legal,
    // dark red if it isn't. Draw this after DrawUnits so it sits on top.
    private void DrawDeployCursor()
    {
        if (_selectedHandIdx is not int handIdx) return;
        if (_hoverCell is not Vector2Int cell) return;
        if (cell.X < 0 || cell.X >= _unitLayer.Surface.Width) return;
        if (cell.Y < 0 || cell.Y >= _unitLayer.Surface.Height) return;

        PlayerState player = _isHost ? _gameState.PlayerOne : _gameState.PlayerTwo;
        if (handIdx >= player.Hand.Count) return;

        CardInfo card = CardInfos.GetCardInfo(player.Hand[handIdx]);
        // cell is a screen coordinate: validate in world space, but draw where the cursor is.
        bool valid = IsValidDeploySpot(Flip(cell), card.ValidLocation);

        _unitLayer.Surface[cell.X, cell.Y].Background = valid ? Color.White : Color.DarkRed;
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
        Color teamColor = (player.Id == PlayerId.One) == _isHost ? p1Color : p2Color;
        foreach (UnitState unit in player.Units)
        {
            Vector2Int pos = unit.Position;
            EntityDisplay display = EntityDisplay.Displays[unit.Type];
            Vector2Int size = UnitInfos.GetUnitInfo(unit.Type).Size;
            for (int x = 0; x < size.X; x++)
            {
                for (int y = 0; y < size.Y; y++)
                {
                    ColoredGlyph glyph = display.Glyphs[y][x];
                    Vector2Int render = Flip(new Vector2Int(pos.X + x, pos.Y + y));
                    int renderX = render.X;
                    int renderY = render.Y;

                    _unitLayer.Surface[renderX, renderY].Foreground = glyph.Foreground;
                    _unitLayer.Surface[renderX, renderY].Background = teamColor;
                    _unitLayer.Surface[renderX, renderY].GlyphCharacter = glyph.GlyphCharacter;

                    if ((unit.Ticks - unit.LastAttackTick) < 2)
                    {
                        _unitLayer.Surface[renderX, renderY].GlyphCharacter = ' ';
                    }

                    if ((unit.Ticks - unit.LastDamageTick) < 2)
                    {
                        _unitLayer.Surface[renderX, renderY].Background = Color.Red;
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
        DrawDeployCursor();
        DrawGUI();
        // roughly we need a timer also timer need sto swap to overtime after 2 mins
        
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
        DrawDeployCursor();
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

                // No flip needed here: the map is vertically symmetric (30 rows, water on
                // 14-15, which maps onto itself), so the terrain looks the same either way.
                Surface.SetCellAppearance(x, y, cellAppearance);
            }
        }
    }

    private void DrawGUI()
    {
        // we want a timer, that swaps to overtime after isOvertime
        // we want a victory screen for active player if winner
        // we want a defeat screen for opposite player
        // if ishost and win = player 1 then victory else lose
        // if isclient and win = player 2 then victory else lose
        
        int cardWidth = 5;
        int cardHeight = 3;
        int spacing = 2;
        int startX = 1;
        int startY = 4;

        PlayerState player = _isHost ? _gameState.PlayerOne : _gameState.PlayerTwo;
        for (int i = 0; i < player.Hand.Count; i++)
        {
            CardInfo card = CardInfos.GetCardInfo(player.Hand[i]);
            bool isSelected = _selectedHandIdx == i;
            Color color = isSelected ? Color.White : (player.Elixir >= card.Cost ? Color.Cyan : Color.Gray);
            int cardX = startX + (i * (cardWidth + spacing));
            int cardY = startY;
            string label = card.Id switch
            {
                CardId.Knight  => "KNGHT",
                CardId.Giant   => "GIANT",
                CardId.Archer  => "ARCHR",
                CardId.Goblin  => "GOBLN",
                CardId.Wizard  => "WIZRD",
                CardId.Hog   => "RIDER",
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
            if (card is UnitCard unitCard && EntityDisplay.Displays.TryGetValue(unitCard.UnitType, out var display))
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
        
        _guiLayer.Surface.Print(0, 2, "=========== HAND ===========", Color.Yellow);
        int barY = 1;
        string barlabel = $" {player.Elixir:0}/{GameSettings.MAX_ELIXIR:0}";
        int totalWidth = ArenaMap.Width;
        int barWidth = totalWidth - barlabel.Length -1;
        int filled = barWidth * player.Elixir / GameSettings.MAX_ELIXIR;
        for (int i = 0; i < barWidth; i++)
        {
            if (i < filled)
                _guiLayer.Surface.SetGlyph(1 + i, barY, 219, Color.Magenta);
            else
                _guiLayer.Surface.SetGlyph(1 + i, barY, 176, Color.DarkMagenta);
        }
        _guiLayer.Surface.Print(1 + barWidth, barY, barlabel, Color.Magenta);

        // Row 2 is the only line left: row 1 is the elixir bar, row 3 the HAND banner
        // and rows 4-6 the card boxes.
        if (_selectedHandIdx is int sel && sel < player.Hand.Count)
            _guiLayer.Surface.Print(1, 2, $"{player.Hand[sel]} -> CLICK ARENA", Color.White);
        else
            _guiLayer.Surface.Print(0, 2, "==Press 1-4 to pick a card==", Color.Yellow);
    }
    private void SetupTestBattle()
    {
        PlayerState p1 = _gameState.PlayerOne;
        PlayerState p2 = _gameState.PlayerTwo;

        // Player Two defends the top, Player One the bottom.
        p2.Units.Add(new UnitState(UnitType.Castle, PlayerId.Two, new Vector2Int(13, 3)));
        p2.Units.Add(new UnitState(UnitType.Tower,  PlayerId.Two, new Vector2Int(4, 6)));
        p2.Units.Add(new UnitState(UnitType.Tower,  PlayerId.Two, new Vector2Int(22, 6)));

        p1.Units.Add(new UnitState(UnitType.Castle, PlayerId.One, new Vector2Int(13, 27)));
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