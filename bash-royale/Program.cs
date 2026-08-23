using SadConsole.Configuration;

Settings.WindowTitle = "Console";

Builder
    .GetBuilder()
    .SetWindowSizeInCells(GameSettings.GAME_WIDTH + 30, GameSettings.GAME_HEIGHT)
    .SetStartingScreen<bash_royale.Rendering.StartScreen>()
    .IsStartingScreenFocused(true)
    .ConfigureFonts(true)
    .Run();
