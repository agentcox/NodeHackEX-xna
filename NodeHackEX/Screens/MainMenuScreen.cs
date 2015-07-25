#region File Description
//-----------------------------------------------------------------------------
// MainMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using HackPrototype;
using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System.IO.IsolatedStorage;
using HackPrototype;
using System.Xml.Serialization;
using Microsoft.Phone.Tasks;
using Microsoft.Phone;
#endregion

namespace GameStateManagement
{



    /// <summary>
    /// The main menu screen is the first thing displayed when the game starts up.
    /// </summary>
    class MainMenuScreen : MenuScreen
    {
        #region Initialization

        bool continuingGame; //do we need to be displaying information about a continuing game?
        CurrencyStringer currentMoneyStringer = null;
        string currentWaveString = null;
        bool startedDraw = false;

        /// <summary>
        /// Constructor fills in the menu contents.
        /// </summary>
        public MainMenuScreen(bool isContinuing)
            : base("")
        {
            continuingGame = isContinuing;
            
        }

        protected override void HandleTap(GestureSample tap)
        {
            if (tap.Position.X > 350 && tap.Position.Y > 700)
            {

                //he hit the CNC - go to the website
                WebBrowserTask wbt = new WebBrowserTask();
                wbt.URL = "http://www.nodehackgame.com";
                wbt.Show();
            }

            base.HandleTap(tap);
        }

        void ContinueGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            LoadingScreen.Load(ScreenManager, true, PlayerIndex.One, new GameplayScreen());
            ScreenManager.AddScreen(new LevelWinScreen(((Game1)(ScreenManager.Game)).GetCurrentWave() - 1, ((Game1)(ScreenManager.Game)).GetFinalScore()), PlayerIndex.One);
            ExitScreen();
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Event handler for when the Play Game menu entry is selected.
        /// </summary>
        void PlayGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.AddScreen(new LevelSelectScreen(this), PlayerIndex.One);           
        }

        /// <summary>
        /// Event handler for when the High Scores menu entry is selected.
        /// </summary>
        void HighScoresMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.AddScreen(new HighScoreScreenDynamic(0,0), PlayerIndex.One);
        }


        /// <summary>
        /// Event handler for when the Options menu entry is selected.
        /// </summary>
        void OptionsMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.AddScreen(new OptionsMenuScreen(), e.PlayerIndex);
        }

        public override void LoadContent()
        {

            //Special if game is considered "continuing"
            MenuEntry continueGameMenuEntry = new MenuEntry("Continue Wave " + ((Game1)(ScreenManager.Game)).GetCurrentWave());
            continueGameMenuEntry.Selected += ContinueGameMenuEntrySelected;

            if (continuingGame)
            {
                MenuEntries.Add(continueGameMenuEntry);
            }

            // Create our menu entries.
            MenuEntry playGameMenuEntry = new MenuEntry(continuingGame?"Restart Game":"Play Game");

            MenuEntry highScoresMenuEntry = new MenuEntry("High Scores");
            //MenuEntry optionsMenuEntry = new MenuEntry("Options");
            MenuEntry howToPlayEntry = new MenuEntry("How to Play");
            MenuEntry shareWithFriendMenuEntry = new MenuEntry("Tell a Friend");

            // Hook up menu event handlers.
            playGameMenuEntry.Selected += PlayGameMenuEntrySelected;
            
            highScoresMenuEntry.Selected += HighScoresMenuEntrySelected;
            //optionsMenuEntry.Selected += OptionsMenuEntrySelected;
            howToPlayEntry.Selected += howToPlayEntrySelected;

            shareWithFriendMenuEntry.Selected += shareWithFriendMenuEntrySelected;

            // Add entries to the menu.
            MenuEntries.Add(playGameMenuEntry);

            MenuEntries.Add(highScoresMenuEntry);
           // MenuEntries.Add(optionsMenuEntry);
            MenuEntries.Add(howToPlayEntry);
            MenuEntries.Add(shareWithFriendMenuEntry);
            
            //check if we are continuing. if so, preconstruct the score and wave strings.
            if (continuingGame)
            {
                Game1 ourGame = (Game1)ScreenManager.Game;
                currentMoneyStringer = new CurrencyStringer(ourGame.GetFinalScore());
                currentWaveString = "Wave: " + ourGame.GetCurrentWave() + "\nScore: " + currentMoneyStringer.outputstring;
            }

            ((Game1)(ScreenManager.Game)).SetBackgroundSubtitle("Sprites\\Titles\\MainMenu", 1.0f);
            
            base.LoadContent();



        }

        void howToPlayEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.AddScreen(new TutorialScreen(), PlayerIndex.One);
        }

        void shareWithFriendMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            ((Game1)(ScreenManager.Game)).ShareWithFriend(0);
        }


        /// <summary>
        /// When the user cancels the main menu, we exit the game.
        /// </summary>
        protected override void OnCancel(PlayerIndex playerIndex)
        {
            ScreenManager.Game.Exit();
        }


        public override void Draw(GameTime gameTime)
        {

            //draw base first
            base.Draw(gameTime);
            /*
            if (currentWaveString != null)
            {

                SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
                Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
                Rectangle fullscreen = new Rectangle(0, 0, viewport.Width, viewport.Height);

                SpriteFont font = ScreenManager.Font;

                Vector2 level_origin = new Vector2(0, font.LineSpacing / 2);
                Vector2 level_position = new Vector2(0f, 500.0f);

                if (!startedDraw)
                {
                    startedDraw = true;
                }

                spriteBatch.Begin();

                level_position.X = (int)(ScreenManager.GraphicsDevice.Viewport.Width / 2 - font.MeasureString(currentWaveString).X / 2);


                spriteBatch.DrawString(font, currentWaveString, level_position, Color.Yellow, 0,
                                       level_origin, 1.0f, SpriteEffects.None, 0);

                spriteBatch.End();
            }
             * 
             */
        }

        
        #endregion
    }
}
