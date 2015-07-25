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
using Microsoft.Xna.Framework.GamerServices;
using System.IO.IsolatedStorage;
using System.IO;
using HackPrototype;
using System.Xml.Serialization;
#endregion

namespace GameStateManagement
{

    

    

    class HighScoreScreen : GameScreen
    {

        #region Fields

        ContentManager content;
        bool startedDraw = false;

        IAsyncResult kbResult;
        string typedText = "Default";

        UInt64 newScoreToPost;
        int maxWaveToPost;

        HighScoreTable highScores;
            
        float TimeToAutoClose = 5.0f;
        float currentTimer = 0.0f;

        #endregion

        public HighScoreScreen(UInt64 newScore, int maxWave)
        {
            // menus generally only need Tap for menu selection
            EnabledGestures = GestureType.Tap;

            newScoreToPost = newScore;
            maxWaveToPost = maxWave;
        }

        public override void LoadContent()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");

            //check for isoStore version of highScores

            highScores = new HighScoreTable();
            bool madeHighScore = highScores.Load(content,newScoreToPost, maxWaveToPost);
            
            if (madeHighScore)
            {
                kbResult = Guide.BeginShowKeyboardInput(PlayerIndex.One,
"You Placed in the High Scores!", "Enter Your Name!",
((typedText == null) ? "Anonymous" : typedText),
GetTypedChars, null);
            }

        }


        protected void GetTypedChars(IAsyncResult r)
        {
            typedText = Guide.EndShowKeyboardInput(r);
            highScores.WriteInNewHighScore(new HighScoreEntry(newScoreToPost, maxWaveToPost, typedText));
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
                //ExitSelf();
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
            base.HandleInput(input);
        }

        private void ExitSelf()
        {
            ExitScreen();
        }

        public override void OnBackButton()
        {
            ExitSelf();
        }


        /// <summary>
        /// Draws the game over screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            Rectangle fullscreen = new Rectangle(0, 0, viewport.Width, viewport.Height);

            SpriteFont font = ScreenManager.Font;
            
            Vector2 level_origin = new Vector2(0, font.LineSpacing / 2);
            Vector2 level_position = new Vector2(0f, 100.0f);

            Vector2 score_origin = new Vector2(0, font.LineSpacing / 2);
            Vector2 score_position = new Vector2(0f, level_position.Y + 100.0f);
            Vector2 score_offset = new Vector2(0f, 40.0f);

            string level_text = "HIGH SCORES";


            if (!startedDraw)
            {
                startedDraw = true;
            }

            spriteBatch.Begin();

            level_position.X = (int)(ScreenManager.GraphicsDevice.Viewport.Width / 2 - font.MeasureString(level_text).X / 2);


            //spriteBatch.DrawString(font, level_text, level_position, Color.Red, 0,
            //                       level_origin, 1.0f, SpriteEffects.None, 0);

            if (highScores != null && highScores.highScoreStrings != null && highScores.highScoreStrings.Count > 0)
            {
                for (int i = 0; i < highScores.highScoreStrings.Count; i++)
                {
                    score_position.X = (int)(ScreenManager.GraphicsDevice.Viewport.Width / 2 - font.MeasureString(highScores.highScoreStrings[i]).X / 2);
                    spriteBatch.DrawString(font, highScores.highScoreStrings[i], (new Vector2(score_position.X, score_position.Y + score_offset.Y * i)), i == highScores.currentHighScoreIndex?Color.Yellow:Color.Red, 0,
                                           score_origin, 1.0f, SpriteEffects.None, 0);
                }
            }

            spriteBatch.End();
        }
        #endregion
    }
}
