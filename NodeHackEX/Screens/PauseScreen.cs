#region File Description
//-----------------------------------------------------------------------------
// PauseScreen.cs
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
using Microsoft.Xna.Framework.GamerServices;
#endregion

namespace GameStateManagement
{



    /// <summary>
    /// The main menu screen is the first thing displayed when the game starts up.
    /// </summary>
    class PauseScreen : MenuScreen
    {
        #region Initialization

        BackgroundScreen backgroundScreen;

        /// <summary>
        /// Constructor fills in the menu contents.
        /// </summary>
        public PauseScreen(BackgroundScreen bg)
            : base("")
        {
            backgroundScreen = bg;
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


        #endregion

        #region Handle Input


        /// <summary>
        /// Event handler for when the Play Game menu entry is selected.
        /// </summary>
        void PlayGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            ExitSelf();
        }

        /// <summary>
        /// Event handler for when the High Scores menu entry is selected.
        /// </summary>
        void HighScoresMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.AddScreen(new HighScoreScreenDynamic(0,0), e.PlayerIndex);
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
            ((Game1)(ScreenManager.Game)).PlayMainMenuSong();

            backgroundScreen.SetSubtitleTexture("Sprites\\Titles\\GamePaused", 1.0f);

            // Create our menu entries.
            MenuEntry playGameMenuEntry = new MenuEntry("Resume Game");

            MenuEntry highScoresMenuEntry = new MenuEntry("High Scores");
            //MenuEntry optionsMenuEntry = new MenuEntry("Options");
            MenuEntry howToPlayEntry = new MenuEntry("How to Play");
            MenuEntry shareWithFriendMenuEntry = new MenuEntry("Tell a Friend");
            MenuEntry exitGameMenuEntry = new MenuEntry("Abandon Game");

            // Hook up menu event handlers.
            playGameMenuEntry.Selected += PlayGameMenuEntrySelected;
            
            highScoresMenuEntry.Selected += HighScoresMenuEntrySelected;
            //optionsMenuEntry.Selected += OptionsMenuEntrySelected;
            howToPlayEntry.Selected += howToPlayEntrySelected;

            shareWithFriendMenuEntry.Selected += shareWithFriendMenuEntrySelected;

            exitGameMenuEntry.Selected += exitGameMenuEntrySelected;

            // Add entries to the menu.
            MenuEntries.Add(playGameMenuEntry);

            MenuEntries.Add(highScoresMenuEntry);
           // MenuEntries.Add(optionsMenuEntry);
            MenuEntries.Add(howToPlayEntry);
            MenuEntries.Add(shareWithFriendMenuEntry);
            MenuEntries.Add(exitGameMenuEntry);
            base.LoadContent();
        }

        void exitGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            ((Game1)(ScreenManager.Game)).ExitCurrentLevel();
            ExitSelf();
        }



        void howToPlayEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.AddScreen(new TutorialScreen(), PlayerIndex.One);
        }

        void shareWithFriendMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            ((Game1)(ScreenManager.Game)).ShareWithFriend(0);
        }

        protected override void OnCancel(PlayerIndex playerIndex)
        {
            ExitSelf();
        }

        private void ExitSelf()
        {
            if (backgroundScreen != null)
                ScreenManager.RemoveScreen(backgroundScreen);

            ((Game1)(ScreenManager.Game)).PlayGameSong();
            ((Game1)(ScreenManager.Game)).SetBackgroundSubtitle(null, 0);
            ExitScreen();
        }

        public override void Draw(GameTime gameTime)
        {

            //draw base last
            base.Draw(gameTime);
        }


        #endregion
    }
}
