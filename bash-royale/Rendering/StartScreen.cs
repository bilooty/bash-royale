using System;
using SadConsole;
using SadConsole.UI;
using SadConsole.UI.Controls;
using SadRogue.Primitives;
using System.Linq;

namespace bash_royale.Rendering
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

            // 5. Deck Button - opens the deck builder
            var deckBtn = new Button(10)
            {
                Text = "Deck",
                Position = new Point((Width - 10) / 2, 19)
            };
            deckBtn.Click += OnDeckClicked;
            Controls.Add(deckBtn);

            // 6. A peek at the deck you will take into the next battle
            var deckLabel = new Label("YOUR DECK")
            {
                Position = new Point((Width - 9) / 2, 22),
                TextColor = Color.Yellow
            };
            Controls.Add(deckLabel);

            // Two rows of four so the labels fit the 40 cell wide screen.
            string[] labels = Decks.Current.Select(CardInfos.GetShortLabel).ToArray();
            for (int row = 0; row * 4 < labels.Length; row++)
            {
                string line = string.Join(" ", labels.Skip(row * 4).Take(4));
                Controls.Add(new Label(line)
                {
                    Position = new Point((Width - line.Length) / 2, 24 + row),
                    TextColor = Color.Cyan
                });
            }
        }

        private void OnDeckClicked(object? sender, EventArgs e)
        {
            var deckScreen = new DeckScreen();
            Game.Instance.Screen = deckScreen;
            deckScreen.IsFocused = true;
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
            var battleScreen = new BattleRenderer(ipAddress, isHost);
            Game.Instance.Screen = battleScreen;
            battleScreen.IsFocused = true;
        }
    }
}