using System.Runtime.CompilerServices;
using bash_royale.Networking;
using SadConsole.Input;
using System;
using bash_royale.Emotes;
using bash_royale.Music;
using SadConsole.UI.Controls;

namespace bash_royale.Rendering;

public class BattleRenderer : SadConsole.ScreenSurface
{
    private static readonly Keys[] HandSlotKeys = { Keys.D1, Keys.D2, Keys.D3, Keys.D4 };
    private Color p1Color = Color.DarkBlue;
    private Color p2Color = Color.DarkRed;
    private GameState _gameState;
    private ScreenSurface _unitLayer;
    private ScreenSurface _guiLayer;
    private ScreenSurface _logLayer;
    private SadConsole.UI.ControlsConsole _endScreenLayer;
    private double _timer = 0.05f;
    private string _ipAddress = "127.0.0.1";
    private bool _isHost;
    
    private int _executionTick = 0;
    private int _inputTick = 0;
    private const int COMMAND_DELAY = 10;

    private bool _wasOvertime = false;
    private bool _isPrimed = false;
    private bool _deckSent = false;
    private bool _matchStarted = false;
    private int? _selectedHandIdx = null;
    private Vector2Int? _hoverCell = null;
    private Dictionary<int, NetworkAction> _localInputs = new();
    private int tick = 0;
    private bool _hasSentAction = false;
    private NetworkManager _networkManager;
    private NetworkAction? _pendingLocalAction;
    private EmoteManager _emoteManager = new();
    private bool[,] _groundOccupied;
    private const float ShadowDarkness = 0.95f;
    private const int CountdownSeconds = 3;
    private List<ActionLog> _logs = new();

    private record ActionLog(PlayerId Player, CardId Card);

    public BattleRenderer(string ipAddress, bool isHost) : base(GameSettings.GAME_WIDTH + 30, GameSettings.GAME_HEIGHT)
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
        
        _gameState = GameState.CreateNew();
        SetupTestBattle();
        _unitLayer = new ScreenSurface(GameSettings.GAME_WIDTH, GameSettings.GAME_HEIGHT);
        _unitLayer.Surface.DefaultBackground = Color.Transparent;
        _unitLayer.UseMouse = false;
        _groundOccupied = new bool[GameSettings.GAME_WIDTH, GameSettings.GAME_HEIGHT];
        _guiLayer = new ScreenSurface(ArenaMap.Width, 8);
        _guiLayer.Surface.DefaultBackground = Color.Transparent;
        _guiLayer.UseMouse = false;
        _guiLayer.Position = new Point(0, ArenaMap.Height); 
        Children.Add(_guiLayer);
        _logLayer = new ScreenSurface(30, GameSettings.GAME_HEIGHT);
        _logLayer.Surface.DefaultBackground = Color.Black;
        _logLayer.Surface.Clear();
        _logLayer.Position = new Point(GameSettings.GAME_WIDTH, 0);
        _logLayer.UseMouse = false;
        Children.Add(_logLayer);

        PlayerState p1 = _gameState.PlayerOne;
        PlayerState p2 = _gameState.PlayerTwo;
        
         _gameState.PlayerOne = p1;
         _gameState.PlayerTwo = p2;
        Children.Add(_unitLayer);
        
        AudioManager.StopMusic();
        AudioManager.PlayBattleMusic();

        _endScreenLayer = new SadConsole.UI.ControlsConsole(GameSettings.GAME_WIDTH, GameSettings.GAME_HEIGHT);
        _endScreenLayer.Surface.DefaultBackground = Color.Transparent;
        _endScreenLayer.Surface.Clear(); 
        _endScreenLayer.IsVisible = false;

        var mainMenubtn = new Button(14)
        {
            Text = "Main Menu",
            Position = new Point(ArenaMap.Width / 2 - 7, ArenaMap.Height / 2 + 6)
        };
        var playAgainbtn = new Button(14)
        {
            Text = "Play Again",
            Position = new Point(ArenaMap.Width / 2 - 7, ArenaMap.Height / 2 + 2)
        };
        
        mainMenubtn.Click += (s, e) => 
        {
            Game.Instance.Screen = new StartScreen();
        };
        playAgainbtn.Click += (s, e) => 
        {
            _networkManager.Stop();
            var battleScreen = new BattleRenderer(_ipAddress, _isHost);
            Game.Instance.Screen = battleScreen;
            battleScreen.IsFocused = true;
        };
        
        _endScreenLayer.Controls.Add(mainMenubtn);
        _endScreenLayer.Controls.Add(playAgainbtn);
        
        Children.Add(_endScreenLayer);

        DrawArena();

        UseKeyboard = true;
        UseMouse = true;
        IsFocused = true;
        UseMouse = true; 
    }

    public override bool ProcessKeyboard(Keyboard keyboard)
    {
        if (_emoteManager.HandleInput(keyboard, out EmoteId? emote))
        {
            if (emote != null)
            {
                _emoteManager.Show(emote.Value, _isHost ? PlayerId.One : PlayerId.Two);
                var emoteAction = new NetworkAction
                {
                    Tick = _inputTick,
                    PlayerId = _isHost ? (byte)0 : (byte)1,
                    Action = ActionType.Emote,
                    EmoteId = (byte)emote.Value,
                };
                _networkManager.SendAction(emoteAction);
            }
            return true;
        }
        if (_pendingLocalAction != null && _pendingLocalAction.Action != ActionType.NoAction)
            return base.ProcessKeyboard(keyboard);
        for (int i = 0; i < HandSlotKeys.Length; i++)
        {
            if (keyboard.IsKeyPressed(HandSlotKeys[i]))
            {
                PlayerId id = _isHost ? PlayerId.One : PlayerId.Two;
                PlayerState state = GameState.GetPlayerState(_gameState, id);
                CardInfo card = CardInfos.GetCardInfo(state.Hand[i]);
                if (state.Elixir >= card.Cost)
                {
                    _selectedHandIdx = (_selectedHandIdx == i) ? null : i;
                    return true;
                }
            }
        }
        return base.ProcessKeyboard(keyboard);
    }

    private void DrawProjectiles()
    {
        foreach (ProjectileState proj in _gameState.Projectiles)
        {
            if (!EntityDisplay.Projectiles.TryGetValue(proj.Type, out EntityDisplay display))
                continue; 
            if (display.isFlashing && tick % 2 == 0) continue;
            ColoredGlyph glyph = display.Glyphs[0][0];
            Vector2Int? sizeHuh = ProjectileState.Infos[proj.Type].Size;

            Vector2Int size = sizeHuh ?? new Vector2Int(1, 1);
            for (int x = 0; x < size.X; x++)
            {
                for (int y = 0; y < size.Y; y++)
                {
                    Vector2Int render = Flip(new Vector2Int(proj.Position.X + x, proj.Position.Y + y));

                    if (render.X < 0 || render.X >= _unitLayer.Surface.Width) continue;
                    if (render.Y < 0 || render.Y >= _unitLayer.Surface.Height) continue;

                    _unitLayer.Surface[render.X, render.Y].Foreground = glyph.Foreground;
                    if (!display.IsTransparent)
                        _unitLayer.Surface[render.X, render.Y].Background = glyph.Background;
                    _unitLayer.Surface[render.X, render.Y].GlyphCharacter = glyph.GlyphCharacter;
                }
            }
        }
    }

    private static List<(Vector2Int topLeft, Vector2Int size)> DeployFootprints(CardInfo card, Vector2Int origin)
    {
        List<(Vector2Int, Vector2Int)> footprints = new();

        switch (card)
        {
            case SpellCard spell:
                footprints.Add((origin + spell.Offset, spell.Size));
                break;

            case SwarmCard swarm:
            {
                Vector2Int size = UnitInfos.GetUnitInfo(swarm.UnitType).Size;
                foreach (Vector2Int offset in swarm.Offsets)
                {
                    footprints.Add((origin + offset, size));
                }
                break;
            }

            case UnitCard unit:
                footprints.Add((origin, UnitInfos.GetUnitInfo(unit.UnitType).Size));
                break;

            default:
                footprints.Add((origin, new Vector2Int(1, 1)));
                break;
        }

        return footprints;
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
            Vector2Int deployPosition = Flip(new Vector2Int(state.CellPosition.X, state.CellPosition.Y));
            PlayerState player = _isHost ? _gameState.PlayerOne : _gameState.PlayerTwo;
            if (handIdx >= player.Hand.Count) return base.ProcessMouse(state);
            CardInfo card = CardInfos.GetCardInfo(player.Hand[handIdx]);

            if (IsValidDeployment(card, deployPosition))
            {
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

    private Vector2Int Flip(Vector2Int p) =>
        _isHost ? p : new Vector2Int(ArenaMap.Width - 1 - p.X, ArenaMap.Height - 1 - p.Y);

    private bool IsValidDeployment(CardInfo card, Vector2Int origin)
    {
        foreach ((Vector2Int topLeft, Vector2Int size) in DeployFootprints(card, origin))
        {
            for (int x = 0; x < size.X; x++)
            {
                for (int y = 0; y < size.Y; y++)
                {
                    if (!IsValidCell(new Vector2Int(topLeft.X + x, topLeft.Y + y), card)) return false;
                }
            }
        }

        return true;
    }

    private bool IsValidCell(Vector2Int position, CardInfo card)
    {
        if (position.X < 0 || position.X >= ArenaMap.Width) return false;
        if (position.Y < 0 || position.Y >= ArenaMap.Height) return false;
 
        if (card.ValidLocation == ValidLocation.BothSides) return true;
 
        if (!ArenaMap.IsPassable(position, MovementLayer.Ground)) return false;
 
        return _isHost ? position.Y > ArenaMap.RiverEndRow : position.Y < ArenaMap.RiverStartRow;
    }

    private void DrawDeployCursor()
    {
        if (_selectedHandIdx is not int handIdx) return;
        if (_hoverCell is not Vector2Int visualCell) return;
        
        if (visualCell.X < 0 || visualCell.X >= _unitLayer.Surface.Width) return;
        if (visualCell.Y < 0 || visualCell.Y >= _unitLayer.Surface.Height) return;

        PlayerState player = _isHost ? _gameState.PlayerOne : _gameState.PlayerTwo;
        if (handIdx >= player.Hand.Count) return;

        CardInfo card = CardInfos.GetCardInfo(player.Hand[handIdx]);

        Vector2Int origin = Flip(visualCell);
        Color highlightColor = IsValidDeployment(card, origin) ? Color.White : Color.DarkRed;

        foreach ((Vector2Int topLeft, Vector2Int size) in DeployFootprints(card, origin))
        {
            for (int x = 0; x < size.X; x++)
            {
                for (int y = 0; y < size.Y; y++)
                {
                    Vector2Int render = Flip(new Vector2Int(topLeft.X + x, topLeft.Y + y));

                    if (render.X < 0 || render.X >= _unitLayer.Surface.Width) continue;
                    if (render.Y < 0 || render.Y >= _unitLayer.Surface.Height) continue;

                    _unitLayer.Surface[render.X, render.Y].Background = highlightColor;
                }
            }
        }
    }


    private bool ShouldDrawSprout(int x, int y)
    {
        uint hash = (uint)x * 374761393u ^ (uint)y * 668265263u;
        hash = (hash ^ (hash >> 13)) * 1274126177u;
        return (hash % 100) < 15; 
    }
    

    public void DrawUnits(PlayerState player, MovementLayer layer)
    {
        Color teamColor = (player.Id == PlayerId.One) == _isHost ? p1Color : p2Color;
        foreach (UnitState unit in player.Units)
        {
            UnitInfo info = UnitInfos.GetUnitInfo(unit.Type);
            if (info.Layer != layer) continue;

            Vector2Int pos = unit.Position;
            EntityDisplay display = EntityDisplay.Displays[unit.Type];
            Vector2Int size = info.Size;
            for (int x = 0; x < size.X; x++)
            {
                for (int y = 0; y < size.Y; y++)
                {
                    ColoredGlyph glyph = display.Glyphs[0][0];
                    Vector2Int render = Flip(new Vector2Int(pos.X + x, pos.Y + y));
                    int renderX = render.X;
                    int renderY = render.Y;

                    if (renderX < 0 || renderX >= _unitLayer.Surface.Width) continue;
                    if (renderY < 0 || renderY >= _unitLayer.Surface.Height) continue;

                    bool coveringGround = layer == MovementLayer.Air && _groundOccupied[renderX, renderY];

                    _unitLayer.Surface[renderX, renderY].Foreground = glyph.Foreground;
                    if (!coveringGround)
                        _unitLayer.Surface[renderX, renderY].Background = teamColor;
                    _unitLayer.Surface[renderX, renderY].GlyphCharacter = glyph.GlyphCharacter;

                    if (layer == MovementLayer.Ground)
                        _groundOccupied[renderX, renderY] = true;

                    if ((unit.Ticks - unit.LastAttackTick) < 1)
                    {
                        _unitLayer.Surface[renderX, renderY].GlyphCharacter = ' ';
                    }

                    if ((unit.Ticks - unit.LastDamageTick) < 1)
                    {
                        _unitLayer.Surface[renderX, renderY].Background = Color.Red;
                    }
                }
            }
        }
    }

    public void DrawShadows(PlayerState player)
    {
        Color shadowColor = (player.Id == PlayerId.One) == _isHost ? p1Color : p2Color;
        foreach (UnitState unit in player.Units)
        {
            UnitInfo info = UnitInfos.GetUnitInfo(unit.Type);
            if (info.Layer != MovementLayer.Air) continue;

            for (int x = 0; x < info.Size.X; x++)
            {
                for (int y = 0; y < info.Size.Y; y++)
                {
                    Vector2Int render = Flip(new Vector2Int(unit.Position.X + x, unit.Position.Y + y));

                    int shadowX = render.X;
                    int shadowY = render.Y + 1;

                    if (shadowX < 0 || shadowX >= _unitLayer.Surface.Width) continue;
                    if (shadowY < 0 || shadowY >= _unitLayer.Surface.Height) continue;
                    if (_groundOccupied[shadowX, shadowY]) continue;

                    _unitLayer.Surface[shadowX, shadowY].Background =
                        Surface[shadowX, shadowY].Background * ShadowDarkness;
                }
            }
        }
    }

    public void DrawUnitHP(PlayerState player)
    {
        foreach (UnitState unit in player.Units)
        {
            if (unit.Type == UnitType.Tower || unit.Type == UnitType.Castle) continue;

            UnitInfo info = UnitInfos.GetUnitInfo(unit.Type);
            if (unit.Health >= info.MaxHealth) continue;

            float healthPct = (float)unit.Health / info.MaxHealth;

            Color barColor;
            if (healthPct > 0.6f) barColor = Color.LimeGreen;
            else if (healthPct > 0.3f) barColor = Color.Yellow;
            else barColor = Color.Red;

            Vector2Int cornerA = Flip(unit.Position);
            Vector2Int cornerB = Flip(new Vector2Int(
                unit.Position.X + info.Size.X - 1,
                unit.Position.Y + info.Size.Y - 1));

            int barX = Math.Max(cornerA.X, cornerB.X) + 1;
            int topY = Math.Min(cornerA.Y, cornerB.Y);

            int barHeight = info.Size.Y;
            int filled = Math.Max(1, (int)Math.Ceiling(barHeight * healthPct));

            for (int i = 0; i < barHeight; i++)
            {
                int rowY = topY + i;

                if (barX < 0 || barX >= _unitLayer.Surface.Width) continue;
                if (rowY < 0 || rowY >= _unitLayer.Surface.Height) continue;

                _unitLayer.Surface[barX, rowY].GlyphCharacter = (char)221;
                _unitLayer.Surface[barX, rowY].Foreground = i >= barHeight - filled ? barColor : Color.DarkGray;
            }
        }
    }

    private static int CrownTowerCount(List<UnitState> units)
    {
        int count = 0;
        foreach (UnitState unit in units)
        {
            if (unit.Type == UnitType.Tower || unit.Type == UnitType.Castle) count++;
        }
        return count;
    }

    private void DrawPhaseCountdown()
    {
        if (_gameState.IsGameOver) return;

        int ticksLeft;
        string label;

        if (!_gameState.IsOvertime)
        {
            ticksLeft = GameSettings.REGULATION_END_TICK - _gameState.Tick;
            bool level = CrownTowerCount(_gameState.PlayerOne.Units)
                         == CrownTowerCount(_gameState.PlayerTwo.Units);
            label = level ? "OVERTIME IN " : "GAME ENDS IN ";
        }
        else
        {
            ticksLeft = GameSettings.OVERTIME_END_TICK - _gameState.Tick;
            label = "GAME ENDS IN ";
        }

        if (ticksLeft <= 0) return;
        if (ticksLeft > CountdownSeconds * GameSettings.TICKS_PER_SECOND) return;

        int seconds = ticksLeft / GameSettings.TICKS_PER_SECOND;
        string text = label + seconds;

        int row = (ArenaMap.RiverStartRow + ArenaMap.RiverEndRow) / 2;
        if (row < 0 || row >= _unitLayer.Surface.Height) return;

        int startX = Math.Max(0, (ArenaMap.Width - text.Length) / 2);

        _unitLayer.Surface.Print(startX, row, text, Color.White, Color.Black);
    }

    public override void Update(TimeSpan delta)
    {
        _networkManager.PollEvents();
        while (_networkManager.RemoteEmotes.Count > 0)
        {
            var emoteAction = _networkManager.RemoteEmotes.Dequeue();
            _emoteManager.Show((EmoteId)emoteAction.EmoteId,
            _isHost ? PlayerId.Two : PlayerId.One);
        }
        if (_gameState.IsGameOver)
        {
            Redraw();
            base.Update(delta);
            return;
        }   
        if (!_networkManager.IsConnected)
        {
            _deckSent = false;
            Redraw();
            _guiLayer.Surface.Print(2, 6, "Waiting for opponent...", Color.Yellow, Color.Black);
            base.Update(delta);
            return;
        }

        if (!_deckSent)
        {
            _networkManager.SendDeck(Decks.Current);
            _deckSent = true;
        }

        if (!_matchStarted)
        {
            if (_networkManager.RemoteDeck is not List<CardId> remoteDeck)
            {
                _guiLayer.Surface.Print(2, 6, "Exchanging decks...", Color.Yellow, Color.Black);
                base.Update(delta);
                return;
            }

            _gameState = _isHost
                ? GameState.CreateNew(Decks.Current, remoteDeck)
                : GameState.CreateNew(remoteDeck, Decks.Current);
            SetupTestBattle();
            _matchStarted = true;
        }
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
          
            if (!_networkManager.RemoteInputs.ContainsKey(_executionTick))
            {
                _timer = 0f; 
            }
            else
            {
    
                NetworkAction remoteAction = _networkManager.RemoteInputs[_executionTick];
                NetworkAction localAction = _localInputs[_executionTick];
                PlayActionSounds(localAction);
                PlayActionSounds(remoteAction);
                
                if (localAction.Action == ActionType.DeployCard)
                {
                    PlayerState p = _isHost ? _gameState.PlayerOne : _gameState.PlayerTwo;
                    if (localAction.CardIdx < p.Hand.Count)
                        _logs.Add(new ActionLog(p.Id, p.Hand[localAction.CardIdx]));
                }
                if (remoteAction.Action == ActionType.DeployCard)
                {
                    PlayerState p = _isHost ? _gameState.PlayerTwo : _gameState.PlayerOne;
                    if (remoteAction.CardIdx < p.Hand.Count)
                        _logs.Add(new ActionLog(p.Id, p.Hand[remoteAction.CardIdx]));
                }
                
                if (remoteAction.Action == ActionType.Emote)
                    _emoteManager.Show((EmoteId)remoteAction.EmoteId,
                        _isHost ? PlayerId.Two : PlayerId.One);
                
                if (remoteAction.Action != ActionType.NoAction)
                    System.Console.WriteLine("Received: pid: " + remoteAction.PlayerId + " x" + remoteAction.X + " y" + remoteAction.Y);

               
                if (_isHost)
                    _gameState = GameSim.Update(_gameState, localAction, remoteAction);
                else
                    _gameState = GameSim.Update(_gameState, remoteAction, localAction);
                
                _networkManager.RemoteInputs.Remove(_executionTick);
                _localInputs.Remove(_executionTick);
                _executionTick++;
                
                NetworkAction nextInput = _pendingLocalAction ?? new NetworkAction { Action = ActionType.NoAction };
                nextInput.Tick = _inputTick;
                nextInput.PlayerId = _isHost ? (byte)0 : (byte)1;
                
                _localInputs[_inputTick] = nextInput;
                _networkManager.SendAction(nextInput);
                
                _pendingLocalAction = null;
                _inputTick++;
                tick++;
                if (_gameState.IsOvertime && !_wasOvertime)
                {
                    AudioManager.PlayOvertimeMusic();
                    _wasOvertime = true;
                }
                _timer = 0.05;
            }
        }
    
        Redraw();
        base.Update(delta);
        _emoteManager.Update(_unitLayer, _isHost);
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
                        
                        if (ShouldDrawSprout(x,y))
                        {
                            cellAppearance = grassSprout;
                        }
                        break;
                
                    case TileType.Water:
                        cellAppearance = new ColoredGlyph(Color.Cyan, Color.Aquamarine, ' ');
                        break;
                
                    case TileType.Bridge:
                        cellAppearance = new ColoredGlyph(Color.BurlyWood, Color.BurlyWood, ' ');
                        break;
                        
                    default:
                        cellAppearance = new ColoredGlyph(Color.White, Color.Black, ' ');
                        break;
                }

                Surface.SetCellAppearance(x, y, cellAppearance);
            }
        }
    }
    private void PlayActionSounds(NetworkAction action)
    {
        if (action.Action != ActionType.DeployCard) return;
        PlayerState player = action.PlayerId == 0 ? _gameState.PlayerOne : _gameState.PlayerTwo;
        if (action.CardIdx >= player.Hand.Count) return;
    
        CardId cardId = player.Hand[action.CardIdx];
        CardInfo card = CardInfos.GetCardInfo(cardId);
    
        AudioManager.PlaySound(card.DeploySound); 
    }

    private void DrawLog()
    {
        _logLayer.Surface.Clear();
        _logLayer.Surface.DrawBox(new Rectangle(0, 0, _logLayer.Surface.Width, _logLayer.Surface.Height), ShapeParameters.CreateBorder(new ColoredGlyph(Color.DarkGray)));
        
        int ySpacing = 2;
        int maxVisibleLogs = (_logLayer.Surface.Height - 2) / ySpacing;
        
        // Start from the oldest log that can fit on screen, up to the newest one
        int startIndex = Math.Max(0, _logs.Count - maxVisibleLogs);
        
        int startY = 1; // Start drawing at the top of the box
        
        for (int i = startIndex; i < _logs.Count; i++)
        {
            var log = _logs[i];
            
            bool isLocal = (log.Player == PlayerId.One) == _isHost;
            string pName = isLocal ? "[PLAYER 1]" : "[PLAYER 2]";
            Color pColor = isLocal ? p1Color : p2Color;
            
            _logLayer.Surface.Print(1, startY, pName, Color.White, pColor);
            _logLayer.Surface.Print(1 + pName.Length, startY, " played ", Color.LightGray);
            
            string cardName = CardInfos.GetShortLabel(log.Card);
            _logLayer.Surface.Print(1 + pName.Length + 8, startY, cardName, Color.White);
            
            int glyphX = 1 + pName.Length + 8 + cardName.Length + 1;
            ColoredGlyph? g = CardInfos.GetDisplayGlyph(log.Card);
            if (g != null)
            {
                _logLayer.Surface.SetCellAppearance(glyphX, startY, g);
                _logLayer.Surface[glyphX, startY].Background = Color.Black;
            }
            else
            {
                _logLayer.Surface.Print(glyphX, startY, "*", Color.Red);
                _logLayer.Surface[glyphX, startY].Background = Color.Black;
            }
            
            startY += ySpacing; // Move down for the next entry
        }
    }

    private void DrawGUI()
    {
        int cardWidth = 5;
        int cardHeight = 3;
        int spacing = 2;
        int startX = 1;
        int startY = 4;

        PlayerState player = _isHost ? _gameState.PlayerOne : _gameState.PlayerTwo;
        bool isOvertime = _gameState.IsOvertime;

        int phaseEndTick = isOvertime
            ? GameSettings.OVERTIME_END_TICK
            : GameSettings.REGULATION_END_TICK;

        int remaining = Math.Max(0, (phaseEndTick - _gameState.Tick) / GameSettings.TICKS_PER_SECOND);

        string clock = $"{remaining / 60}:{remaining % 60:00}";
        string phase = isOvertime ? "OVERTIME " : "";
        Color clockColor = isOvertime ? Color.Red : Color.White;

        _guiLayer.Surface.Print(ArenaMap.Width - phase.Length - clock.Length - 1, 0, phase + clock, clockColor);

        for (int i = 0; i < player.Hand.Count; i++)
        {
            CardInfo card = CardInfos.GetCardInfo(player.Hand[i]);
            bool isSelected = _selectedHandIdx == i;
            Color color = isSelected ? Color.White : (player.Elixir >= card.Cost ? Color.Cyan : Color.Gray);
            int cardX = startX + (i * (cardWidth + spacing));
            int cardY = startY;
            string label = CardInfos.GetShortLabel(card.Id);
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
            ColoredGlyph? g = CardInfos.GetDisplayGlyph(card.Id);
            if (g is { } gg)
            {
                _guiLayer.Surface.SetCellAppearance(centerX, centerY, gg);
                _guiLayer.Surface[centerX, centerY].Background = Color.Black;
            }
            else
            {
                _guiLayer.Surface.Print(centerX, centerY, "*", Color.Red);
                
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
            {
                _guiLayer.Surface.SetGlyph(1 + i, barY, 219, Color.Magenta);
                if (player.Elixir >= 10)
                {
                    if (tick % 4 == 0)
                    {
                        _guiLayer.Surface.SetGlyph(1 + i, barY, 219, Color.White);
                    }
                }
            }
            else
            {
                _guiLayer.Surface.SetGlyph(1 + i, barY, 176, Color.DarkMagenta);
            }
        }
        _guiLayer.Surface.Print(1 + barWidth, barY, barlabel, Color.Magenta);

        if (_selectedHandIdx is int sel && sel < player.Hand.Count)
            _guiLayer.Surface.Print(1, 2, $"{player.Hand[sel]} -> CLICK ARENA", Color.White);
        else
            _guiLayer.Surface.Print(0, 2, "==Press 1-4 to pick a card==", Color.Yellow);
    }
    private void SetupTestBattle()
    {
        PlayerState p1 = _gameState.PlayerOne;
        PlayerState p2 = _gameState.PlayerTwo;
        
        p2.Units.Add(new UnitState(UnitType.Castle, PlayerId.Two, new Vector2Int(13, 1), 0));
        p2.Units.Add(new UnitState(UnitType.Tower,  PlayerId.Two, new Vector2Int(4, 3), 1));
        p2.Units.Add(new UnitState(UnitType.Tower,  PlayerId.Two, new Vector2Int(22, 3), 2));

        p1.Units.Add(new UnitState(UnitType.Castle, PlayerId.One, new Vector2Int(13, 27), 3));
        p1.Units.Add(new UnitState(UnitType.Tower,  PlayerId.One, new Vector2Int(4, 25),4) );
        p1.Units.Add(new UnitState(UnitType.Tower,  PlayerId.One, new Vector2Int(22, 25),5 ));
        _gameState.NextID = 6;
        _gameState.PlayerOne = p1;
        _gameState.PlayerTwo = p2;
    }
    private void DrawState()
    {
        Surface.Clear();
        
    }
    
    private void DrawEndScreen()
    {
        if (!_gameState.IsGameOver) return;

        PlayerId localPlayer = _isHost ? PlayerId.One : PlayerId.Two;

        string message;
        Color color;

        if (_gameState.IsDraw)
        {
            message = "DRAW";
            color = Color.Yellow;
        }
        else if (_gameState.Winner == localPlayer)
        {
            message = "VICTORY";
            color = Color.Gold;
        }
        else
        {
            message = "DEFEAT";
            color = Color.Red;
        }

        int bannerY = ArenaMap.Height / 2 - 1;
        int bannerX = (ArenaMap.Width - message.Length) / 2;

        for (int x = 0; x < ArenaMap.Width; x++)
        {
            for (int y = bannerY - 1; y <= bannerY + 1; y++)
            {
                _unitLayer.Surface[x, y].Background = Color.Black;
                _unitLayer.Surface[x, y].GlyphCharacter = ' ';
            }
        }

        _unitLayer.Surface.Print(bannerX, bannerY, message, color);
        _endScreenLayer.IsVisible = true;
    }
    
    private void Redraw()
    {
        _unitLayer.Surface.Clear();
        _guiLayer.Surface.Clear();
        Array.Clear(_groundOccupied, 0, _groundOccupied.Length);
        DrawUnits(_gameState.PlayerOne, MovementLayer.Ground);
        DrawUnits(_gameState.PlayerTwo, MovementLayer.Ground);
        DrawShadows(_gameState.PlayerOne);
        DrawShadows(_gameState.PlayerTwo);
        DrawUnits(_gameState.PlayerOne, MovementLayer.Air);
        DrawUnits(_gameState.PlayerTwo, MovementLayer.Air);
        DrawProjectiles();
        DrawBuildingHealth(_gameState.PlayerOne);
        DrawBuildingHealth(_gameState.PlayerTwo);
        DrawUnitHP(_gameState.PlayerOne);
        DrawUnitHP(_gameState.PlayerTwo);
        DrawDeployCursor();
        DrawPhaseCountdown();
        DrawGUI();
        DrawLog();
        DrawEndScreen();
    }


    private void DrawBuildingHealth(PlayerState player)
    {
        Color teamColor = (player.Id == PlayerId.One) == _isHost ? p1Color : p2Color;

        foreach (UnitState unit in player.Units)
        {
            if (unit.Type != UnitType.Tower && unit.Type != UnitType.Castle) continue;

            UnitInfo info = UnitInfos.GetUnitInfo(unit.Type);
            string text = unit.Health.ToString().PadLeft(5);

            int worldCenterX = unit.Position.X + info.Size.X / 2;
            int worldY = unit.Position.Y > ArenaMap.Height / 2
                ? unit.Position.Y + info.Size.Y
                : unit.Position.Y - 1;

            Vector2Int anchor = Flip(new Vector2Int(worldCenterX, worldY));

            int labelX = Math.Clamp(anchor.X - text.Length / 2, 0, ArenaMap.Width - text.Length);
            if (anchor.Y < 0 || anchor.Y >= ArenaMap.Height) continue;

            _unitLayer.Surface.Print(labelX, anchor.Y, text, Color.White, Color.Black);
        }
    }
}