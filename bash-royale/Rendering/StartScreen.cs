using System;
using SadConsole;
using SadConsole.UI;
using SadConsole.UI.Controls;
using SadRogue.Primitives;

namespace bash_royale.Scenes
{
    public class StartScreen : ControlsConsole
    {
        public StartScreen() : base(GameSettings.GAME_WIDTH, GameSettings.GAME_HEIGHT)
        {
            // 1. Title Label
            var title = new Label("BASH ROYALE")
            {
                Position = new Point((Width - 11) / 2, 5), 
                TextColor = Color.Cyan
            };
            Controls.Add(title); // <-- Changed here

            // 2. Host Button
            var hostBtn = new Button(10)
            {
                Text = "Host",
                Position = new Point((Width - 10) / 2, 10)
            };
            hostBtn.Click += OnHostClicked;
            Controls.Add(hostBtn); // <-- Changed here

            // 3. IP Address Text Box
            var ipInput = new TextBox(16)
            {
                Text = "127.0.0.1",
                Position = new Point((Width - 16) / 2, 15)
            };
            Controls.Add(ipInput); // <-- Changed here

            // 4. Join Button
            var joinBtn = new Button(10)
            {
                Text = "Join",
                Position = new Point((Width - 10) / 2, 17)
            };
            joinBtn.Click += (s, e) => OnJoinClicked(ipInput.Text);
            Controls.Add(joinBtn); // <-- Changed here
        }

        private void OnHostClicked(object? sender, EventArgs e)
        {
            LaunchGame(isHost: true, ipAddress: "");
        }

        private void OnJoinClicked(string ip)
        {
            LaunchGame(isHost: false, ipAddress: ip);
        }

        private void LaunchGame(bool isHost, string ipAddress)
        {
            var battleScreen = new BattleRenderer();
            Game.Instance.Screen = battleScreen;
            battleScreen.IsFocused = true;
        }
    }
}