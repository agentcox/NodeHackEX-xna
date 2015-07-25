#region Using Statments
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using HackPrototype;
#endregion

namespace GameStateManagement
{
    class LevelSelectScreen : SingleControlScreen
    {

        #region Fields

        ContentManager content;
        Texture2D level_lock_texture;
        Texture2D[] level_textures;
        SpriteFont menufont;
        HighScoreTable highScores;
        MainMenuScreen menu;

        int rows = 4;
        int columns = 4;
        int totalitems = 15;

        Vector2 panelOffset = new Vector2(15, 110);
        int itemXmargin = 10;
        int itemYmargin = 30;
        int itemspacing = 50;

        PanelControl[] row_control;

        #endregion

        public LevelSelectScreen(MainMenuScreen menuScreen)
        {
            // menus generally only need Tap for menu selection
            EnabledGestures = GestureType.Tap;
            menu = menuScreen;

        }

        public override void LoadContent()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");

            level_lock_texture = content.Load<Texture2D>("sprites\\levelselect_lock");
            level_textures = new Texture2D[totalitems];
            for (int i = 0; i < totalitems; i++)
            {
                level_textures[i] = content.Load<Texture2D>("sprites\\levelselect_" + (i + 1).ToString());
            }
            menufont = content.Load<SpriteFont>("menufont");

            ((Game1)(ScreenManager.Game)).SetBackgroundSubtitle("Sprites\\Titles\\SelectLevel", 1.0f);

            highScores = new HighScoreTable();
            highScores.Load(content, 0, 0);



            AddControls();
        }


        /// <summary>
        /// Unloads graphics content for this screen.
        /// </summary>
        public override void UnloadContent()
        {
            content.Unload();
        }

        #region Update and Draw


        /// <summary>
        /// Updates the tutorial screen.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);


        }

        private void AddControls()
        {
            //get game data to inform the controls
            //WaveAccountingTable table = ((Game1)(ScreenManager.Game)).GetAccounting();
            RootControl = new PanelControl();

            RootControl.Position = panelOffset;

            row_control = new PanelControl[rows];
            int createditems = 0;
            for (int i = 0; i < rows; i++)
            {
                row_control[i] = new PanelControl();
                for (int k = 0; k < columns && createditems < totalitems; k++)
                {
                    row_control[i].AddChild(new LevelSelectControl((highScores.GetHighestWave() >= createditems +1)?level_textures[createditems]:level_lock_texture, Vector2.Zero, createditems+1, this));
                    //TextControl tc = new TextControl((createditems + 1).ToString(), menufont, Color.LawnGreen);
                    //tc.Size = new Vector2(64.0f, 64.0f);
                    //row_control[i].AddChild(tc);
                    createditems++;
                }
                row_control[i].LayoutRow(itemXmargin, itemYmargin, itemspacing);
                RootControl.AddChild(row_control[i]);
            }

            ((PanelControl)(RootControl)).LayoutColumn(itemXmargin, itemYmargin, itemspacing);

            RootControl.Visible = true;
        }

        public override void HandleInput(InputState input)
        {
            // look for any taps that occurred and select any entries that were tapped
            foreach (GestureSample gesture in input.Gestures)
            {
                //check against the panel
            }
            base.HandleInput(input);
        }

        public override void OnBackButton()
        {
            //nothing will change!
            ExitSelf();
        }

        protected virtual void ExitSelf()
        {
            ExitScreen();
        }

        public void OnLevelSelected(int level)
        {
            //if valid
            bool valid = level <= highScores.GetHighestWave();

            if (valid)
            {
                //DANGER! You should really wrap this in a YES/NO confirm box.
                ((Game1)ScreenManager.Game).ResetAll();
                //set wave to the result of the level select screen.
                ((Game1)ScreenManager.Game).SetCurrentWave(level);
                ((Game1)ScreenManager.Game).LoadStartTime = DateTime.Now;
                LoadingScreen.Load(ScreenManager, true, PlayerIndex.One, new GameplayScreen());
                
                ExitScreen();
                menu.ExitScreen();
            }
        }


        #endregion
    }
}
