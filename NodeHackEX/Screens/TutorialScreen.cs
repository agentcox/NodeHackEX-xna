#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using HackPrototype;
#endregion

namespace GameStateManagement
{
    class TutorialScreen : GameScreen
    {

        #region Fields

        ContentManager content;
        Texture2D backgroundTexture;

        #endregion

        public TutorialScreen()
        {
            // menus generally only need Tap for menu selection
            EnabledGestures = GestureType.Tap;
        }

        public override void LoadContent()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");

            backgroundTexture = content.Load<Texture2D>("tutorial");

            ((Game1)(ScreenManager.Game)).SetBackgroundSubtitle("Sprites\\Titles\\HowToPlay", 1.0f);
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

        public override void HandleInput(InputState input)
        {
            // look for any taps that occurred and select any entries that were tapped
            foreach (GestureSample gesture in input.Gestures)
            {
                if (gesture.GestureType == GestureType.Tap)
                {
                    //out we go.
                    ExitSelf();
                }
            }
            base.HandleInput(input);
        }

        public override void OnBackButton()
        {
            ExitSelf();
        }

        private void ExitSelf()
        {
            ExitScreen();
            ((Game1)(ScreenManager.Game)).SetBackgroundSubtitle(null, 0);
        }


        /// <summary>
        /// Draws the tutorial screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            Rectangle fullscreen = new Rectangle(0, 0, viewport.Width, viewport.Height);

            spriteBatch.Begin();

            spriteBatch.Draw(backgroundTexture, fullscreen,
                             new Color(TransitionAlpha, TransitionAlpha, TransitionAlpha));

            spriteBatch.End();
        }
        #endregion
    }
}
