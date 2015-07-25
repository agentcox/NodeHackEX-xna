//-----------------------------------------------------------------------------
// HighScorePanel.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;
using System.Collections.Generic;
using HackPrototype;


namespace GameStateManagement
{
    /// <remarks>
    /// This class displays a list of waves and associated money.
    /// </remarks>
    public class AccountingPanel : ScrollingPanelControl
    {
        Control resultListControl = null;

        SpriteFont titleFont;
        SpriteFont headerFont;
        SpriteFont detailFont;

        Texture2D separator;

        CurrencyStringer totalStringer;

        public AccountingPanel(ContentManager content, WaveAccountingTable table, int currentWave, UInt64 totalscore)
        {
            titleFont = content.Load<SpriteFont>("Fonts\\MenuTitle");
            headerFont = content.Load<SpriteFont>("Fonts\\MenuHeader");
            detailFont = content.Load<SpriteFont>("Fonts\\MenuDetail");
            
            separator = content.Load<Texture2D>("Sprites\\Titles\\Separator");

            totalStringer = new CurrencyStringer(totalscore);

            AddChild(new TextControl(" ", titleFont));
            AddChild(CreateHeaderControl());
            PopulateTable(table, currentWave, totalscore);
        }

        private void PopulateTable(WaveAccountingTable table, int currentWave, UInt64 totalscore)
        {

            List<WaveAccountingEntry> entries = table.GetEntries();

            PanelControl newList = new PanelControl();

            for (int i = 0; i < entries.Count; i++)
            {
                CurrencyStringer stringer = new CurrencyStringer(entries[i].c_score);
                newList.AddChild(CreateAccountingEntryControl(entries[i].c_wave.ToString(), stringer.outputstring.ToString(), entries[i].c_wave == currentWave));
            }

            newList.LayoutColumn(0, 0, 0);

            if (resultListControl != null)
            {
                RemoveChild(resultListControl);
            }
            resultListControl = newList;
            AddChild(resultListControl);
            LayoutColumn(0, 0, 0);

            AddChild(new ImageControl(separator, new Vector2(0, 610)));
            AddChild(new TextControl("Total: " + totalStringer.outputstring.ToString(), headerFont, Color.Green, new Vector2(50, 630)));
        }

        /*
        private void PopulateWithFakeData()
        {
            PanelControl newList = new PanelControl();
            Random rng = new Random();
            for (int i = 0; i < 10; i++)
            {
                long score = 10000 - i * 10;
                TimeSpan time = TimeSpan.FromSeconds(rng.Next(60, 3600));
                newList.AddChild(CreateLeaderboardEntryControl("player" + i.ToString(), "$4,000,000,000", "Wave 2"));
            }
            newList.LayoutColumn(0, 0, 0);

            if (resultListControl != null)
            {
                RemoveChild(resultListControl);
            }
            resultListControl = newList;
            AddChild(resultListControl);
            LayoutColumn(0, 0, 0);
        }
        */
        protected Control CreateHeaderControl()
        {
            PanelControl panel = new PanelControl();

            panel.AddChild(new TextControl("Wave", headerFont, Color.Green, new Vector2(30, 100)));
            panel.AddChild(new TextControl("Score", headerFont, Color.Green, new Vector2(175, 100)));
            return panel;
        }

        // Create a Control to display one entry in a leaderboard. The content is broken out into a parameter
        // list so that we can easily create a control with fake data when running under the emulator.
        //
        // Note that for time leaderboards, this function interprets the time as a count in seconds. The
        // value posted is simply a long, so your leaderboard might actually measure time in ticks, milliseconds,
        // or microfortnights. If that is the case, adjust this function to display appropriately.
        protected Control CreateAccountingEntryControl(string wavestring, string scorestring, bool isCurrentHighScore)
        {
            Color textColor = isCurrentHighScore?Color.Yellow:Color.Green;

            var panel = new PanelControl();

            // Wave
            panel.AddChild(
                new TextControl
                {
                    Text = wavestring,
                    Font = detailFont,
                    Color = textColor,
                    Position = new Vector2(30, 0)
                });

            // Score
            panel.AddChild(
                new TextControl
                {
                    Text = scorestring,
                    Font = detailFont,
                    Color = textColor,
                    Position = new Vector2(175, 0)
                });


            return panel;
        }
    }
}
