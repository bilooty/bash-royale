using SadConsole.Configuration;

Settings.WindowTitle = "My SadConsole Game";

Builder
    .GetBuilder()
    .SetWindowSizeInCells(40, 40)
    .SetStartingScreen<bash_royale.Scenes.StartScreen>()
    .IsStartingScreenFocused(true)
    .ConfigureFonts(true)
    .Run();
