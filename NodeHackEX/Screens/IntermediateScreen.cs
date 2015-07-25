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
    class IntermediateScreen : SingleControlScreen
    {

        #region Fields

        ContentManager content;
        Texture2D levelWinTexture;
        Texture2D backgroundTexture;
        Texture2D taptoContinueTexture;

        string levelWinTexturePath;
        string taptoContinueTexturePath;
        string soundPath;

        Vector2 tapToContinueLocation;


        CursorText levelwinText;
        Vector2 levelWinTextDrawLocation;
        Vector2 originalLevelWinTextDrawLocation;

        int wave;
        UInt64 score;

        SoundEffect splashSound;
        SoundEffect raiseSound;
        bool startedDraw = false;

        float DelayToRaiseText = 3.0f;
        float RaiseTextTimeMax = 0.75f;
        float RaiseTextTime = 0.75f;
        Vector2 levelWinTextRaiseLocation;

        #endregion

        public IntermediateScreen(string titleTexturePath, string tapToContinueTexturePath, string splashSoundPath, int currentWave, UInt64 currentscore)
        {
            // menus generally only need Tap for menu selection
            EnabledGestures = GestureType.Tap;
            levelWinTexturePath = titleTexturePath;
            taptoContinueTexturePath = tapToContinueTexturePath;
            soundPath = splashSoundPath;
            wave = currentWave;
            score = currentscore;
        }

        public override void LoadContent()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");

            backgroundTexture = content.Load<Texture2D>("sprites\\Background");
            levelWinTexture = content.Load<Texture2D>(levelWinTexturePath);
            taptoContinueTexture = content.Load<Texture2D>(taptoContinueTexturePath);

            levelwinText = new CursorText(levelWinTexture, 1.5f, 1.0f, content);
            levelWinTextDrawLocation = new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2 - levelWinTexture.Width / 2,
                ScreenManager.GraphicsDevice.Viewport.Height / 2 - levelWinTexture.Height / 2);
            originalLevelWinTextDrawLocation = levelWinTextDrawLocation;

            tapToContinueLocation = new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2 - taptoContinueTexture.Width / 2,
                ScreenManager.GraphicsDevice.Viewport.Height - 100);

            levelWinTextRaiseLocation = new Vector2(levelWinTextDrawLocation.X, 60);

            splashSound = content.Load<SoundEffect>(soundPath);
            raiseSound = content.Load<SoundEffect>("Sounds\\Thump");

            AddAccountingControl();
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

            if (DelayToRaiseText > 0)
            {
                DelayToRaiseText -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            else if (RaiseTextTime > 0)
            {
                RaiseTextTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (RaiseTextTime <= 0)
                {
                    RaiseTextTime = 0;
                    levelWinTextDrawLocation = levelWinTextRaiseLocation;
                    //AddAccountingControl();
                    if(RootControl != null)
                    RootControl.Visible = true;
                    raiseSound.Play();
                }
                else
                {
                    levelWinTextDrawLocation = new Vector2(MathHelper.Lerp(levelWinTextRaiseLocation.X, originalLevelWinTextDrawLocation.X, RaiseTextTime / RaiseTextTimeMax),
                        MathHelper.Lerp(levelWinTextRaiseLocation.Y, originalLevelWinTextDrawLocation.Y, RaiseTextTime / RaiseTextTimeMax));
                }

            }

            levelwinText.Update(gameTime);
        }

        private void AddAccountingControl()
        {
            WaveAccountingTable table = ((Game1)(ScreenManager.Game)).GetAccounting();
            RootControl = new AccountingPanel(content, table, wave, score);
            RootControl.Visible = false;
        }

        public override void HandleInput(InputState input)
        {
            // look for any taps that occurred and select any entries that were tapped
            foreach (GestureSample gesture in input.Gestures)
            {
                if (gesture.GestureType == GestureType.Tap)
                {
                    //well, we're going to the game screen!
                    ExitSelf();
                }
            }
            base.HandleInput(input);
        }

        public override void OnBackButton()
        {
            ExitSelf();
        }

        protected virtual void ExitSelf()
        {
            ExitScreen();
        }



        /// <summary>
        /// Draws the game over screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            Rectangle fullscreen = new Rectangle(0, 0, viewport.Width, viewport.Height);

            
            
            if (!startedDraw)
            {
                //this is our first draw, play the sound
                splashSound.Play();
                startedDraw = true;
            }

            spriteBatch.Begin();

            spriteBatch.Draw(backgroundTexture, fullscreen,
                             new Color(TransitionAlpha, TransitionAlpha, TransitionAlpha));

            levelwinText.DrawSelf(spriteBatch, levelWinTextDrawLocation, new Color(TransitionAlpha, TransitionAlpha, TransitionAlpha));

            if (RootControl.Visible == true)
            {
                spriteBatch.Draw(taptoContinueTexture, tapToContinueLocation, new Color(TransitionAlpha, TransitionAlpha, TransitionAlpha));
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
        #endregion
    }
}
