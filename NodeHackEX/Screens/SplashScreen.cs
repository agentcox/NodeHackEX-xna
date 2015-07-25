#region Using Statments
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using HackPrototype;
#endregion

namespace GameStateManagement
{
    class SplashScreen : GameScreen
    {

        #region Fields

        SoundEffect splashSound;
        bool startedDraw = false;

        ContentManager content;
        Texture2D splashTexture;


       
                   
            
        float TimeToAutoClose = 2.0f;
        float currentTimer = 0.0f;

        #endregion

        public SplashScreen()
        {
            // menus generally only need Tap for menu selection
            EnabledGestures = GestureType.Tap;
            IsPopup = true;
        }

        public override void LoadContent()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");

            splashTexture = content.Load<Texture2D>("cncsplash");
            splashSound = content.Load<SoundEffect>("Sounds\\Splash_Sound");
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

            currentTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (currentTimer > TimeToAutoClose)
            {
                ExitSelf();
            }

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
        }

        private void ExitSelf()
        {
            ExitScreen();
            ScreenManager.AddScreen(new MainMenuScreen(((Game1)ScreenManager.Game).IsContinuing()), null);
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

            spriteBatch.Draw(splashTexture, fullscreen,
                             new Color(TransitionAlpha, TransitionAlpha, TransitionAlpha));

            spriteBatch.End();
        }
        #endregion
    }
}
