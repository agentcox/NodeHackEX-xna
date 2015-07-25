#region File Description
//-----------------------------------------------------------------------------
// BackgroundScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using HackPrototype;
using System.Collections.Generic;
using System.Collections;
using System.Text;
#endregion

namespace GameStateManagement
{

    public class CursorText
    {
        Texture2D titleTexture;
        Texture2D cursorTexture;
        float cursorT = 1.0f;
        float cursorSpeed = 1.5f;
        float cursorStartTimer = 0.0f;

        SoundEffect typingSoundEffect;
        float typingSoundEffectDelayMax = 0.05f;
        float typingSoundEffectDelay = 0.0f;

        public CursorText(Texture2D tex, float speed, float delay, ContentManager content)
        {
            cursorStartTimer = delay;
            cursorSpeed = speed;
            titleTexture = tex;
            if (tex == null)
                cursorT = 1.0f;
            else
                cursorT = 0.0f;

            cursorTexture = content.Load<Texture2D>("sprites\\Titles\\Title_Cursor");
            typingSoundEffect = content.Load<SoundEffect>("sounds\\Type");
        }

        public void StartCursor(float delaySeconds)
        {
            cursorStartTimer = delaySeconds;
            cursorT = 0.0f;
        }

        public void DrawSelf(SpriteBatch sb, Vector2 position, Color color)
        {
            //now draw subtitle?
            if (titleTexture != null)
            {
                if (cursorStartTimer > 0)
                {
                    //do nothing
                }

                else if (cursorT < 1.0f)
                {
                    sb.Draw(titleTexture, position, new Rectangle(0, 0, (int)(titleTexture.Width * cursorT), titleTexture.Height),
                             color);

                    sb.Draw(cursorTexture, new Vector2(MathHelper.Lerp(position.X - 2.0f, position.X + titleTexture.Width + 10.0f, cursorT), position.Y - 5.0f),
                        color);
                }

                else
                {
                    sb.Draw(titleTexture, position,
                             color);
                }

            }
        }

        public void Update(GameTime gameTime)
        {
            if (titleTexture != null)
            {
                if (cursorStartTimer > 0)
                {
                    cursorStartTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                }

                else if (cursorT < 1.0f)
                {
                    cursorT += cursorSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    typingSoundEffectDelay -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (typingSoundEffectDelay < 0)
                    {
                        typingSoundEffectDelay = typingSoundEffectDelayMax;
                        typingSoundEffect.Play();
                    }
                }
            }
        }
    }

    /// <summary>
    /// The background screen sits behind all the other menu screens.
    /// It draws a background image that remains fixed in place regardless
    /// of whatever transitions the screens on top of it may be doing.
    /// </summary>
    public class BackgroundScreen : GameScreen
    {
        #region Fields

        ContentManager content;
        Texture2D backgroundTexture;
        Texture2D titleOverlayTexture;

        CursorText subtitleText;

        HackGameBoard_BackgroundText bgText;
        List<HackGameBoard_BackgroundTextItem> bgTextStrings = new List<HackGameBoard_BackgroundTextItem>();

        WorldSpaceUIElement ping_Element;
        Texture2D ping_Texture;
        float ping_Delay_Max = 5.0f;
        float ping_Delay = 0.0f;
        


        

        float subTitleX = 0;
        const float subTitleY = 60.0f;



        float nextBGTextMin = 0.02f;
        float nextBGTextMax = 1.2f;
        float nextBGText = 0.0f;
        Random r = new Random();
        float nextBGClearMax = 15.0f;
        float nextBGClear = 0.0f;


        string subTexturetoLoad = null;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public BackgroundScreen(string subtitleTexture)
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            subTexturetoLoad = subtitleTexture;
        }


        /// <summary>
        /// Loads graphics content for this screen. The background texture is quite
        /// big, so we use our own local ContentManager to load it. This allows us
        /// to unload before going from the menus into the game itself, wheras if we
        /// used the shared ContentManager provided by the Game class, the content
        /// would remain loaded forever.
        /// </summary>
        public override void LoadContent()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");

            ((Game1)(ScreenManager.Game)).PlayMainMenuSong();

            backgroundTexture = content.Load<Texture2D>("sprites\\Background");
            titleOverlayTexture = content.Load<Texture2D>("sprites\\Titles\\MainTitleOverlay");

            LoadSubtexture();




            bgText = new HackGameBoard_BackgroundText();
            bgText.LoadContent(content);
            bgText.SetDrawLocation(new Vector2(10.0f, 100.0f));
            bgText.SetMaxLines(15);
            bgText.SetDrawDirection(BackgroundTextDrawDirection.BackgroundTextDrawDirection_DownToUp);

            bgTextStrings.Add(new HackGameBoard_BackgroundTextItem(new StringBuilder("[WARNING] AI ACTIVATING"), 0.0f, bgText.emergencyStartColor, bgText.emergencyEndColor, bgText.emergencyColorFadeTime));
            bgTextStrings.Add(new HackGameBoard_BackgroundTextItem(new StringBuilder("SENSORS PICKING UP ICE"), 0.0f, bgText.emergencyStartColor, bgText.emergencyEndColor, bgText.emergencyColorFadeTime));
            bgTextStrings.Add(new HackGameBoard_BackgroundTextItem(new StringBuilder("DANGER-DANGER-DANGER"), 0.0f, bgText.emergencyStartColor, bgText.emergencyEndColor, bgText.emergencyColorFadeTime));
            bgTextStrings.Add(new HackGameBoard_BackgroundTextItem(new StringBuilder("HACKNODE PING #0198"), 0.0f, bgText.defaultStartColor, bgText.defaultEndColor, bgText.defaultColorFadeTime));
            bgTextStrings.Add(new HackGameBoard_BackgroundTextItem(new StringBuilder("HACKNODE OK"), 0.0f, bgText.defaultStartColor, bgText.defaultEndColor, bgText.defaultColorFadeTime));
            bgTextStrings.Add(new HackGameBoard_BackgroundTextItem(new StringBuilder("ACTIVATING ALL SYSTEMS"),0, bgText.defaultStartColor, bgText.defaultEndColor, bgText.defaultColorFadeTime));
            bgTextStrings.Add(new HackGameBoard_BackgroundTextItem(new StringBuilder("HACK SENSORS [OK]"),0, bgText.defaultStartColor, bgText.defaultEndColor, bgText.defaultColorFadeTime));
            bgTextStrings.Add(new HackGameBoard_BackgroundTextItem(new StringBuilder("HACK ATTACK [OK]"),0, bgText.defaultStartColor, bgText.defaultEndColor, bgText.defaultColorFadeTime));
            bgTextStrings.Add(new HackGameBoard_BackgroundTextItem(new StringBuilder("HACK UNIT [OK]"),0, bgText.defaultStartColor, bgText.defaultEndColor, bgText.defaultColorFadeTime));
            bgTextStrings.Add(new HackGameBoard_BackgroundTextItem(new StringBuilder("HACK SWEEP [OK]"),0, bgText.defaultStartColor, bgText.defaultEndColor, bgText.defaultColorFadeTime));
            bgTextStrings.Add(new HackGameBoard_BackgroundTextItem(new StringBuilder("HACK TORPEDO [OK]"),0, bgText.defaultStartColor, bgText.defaultEndColor, bgText.defaultColorFadeTime));
            bgTextStrings.Add(new HackGameBoard_BackgroundTextItem(new StringBuilder("HACK SENSORS [OK]"),0, bgText.defaultStartColor, bgText.defaultEndColor, bgText.defaultColorFadeTime));
            bgTextStrings.Add(new HackGameBoard_BackgroundTextItem(new StringBuilder("HACK ATTACK [OK]"),0, bgText.defaultStartColor, bgText.defaultEndColor, bgText.defaultColorFadeTime));
            bgTextStrings.Add(new HackGameBoard_BackgroundTextItem(new StringBuilder("HACK UNIT [OK]"),0, bgText.defaultStartColor, bgText.defaultEndColor, bgText.defaultColorFadeTime));
            bgTextStrings.Add(new HackGameBoard_BackgroundTextItem(new StringBuilder("HACK SWEEP [OK]"),0, bgText.defaultStartColor, bgText.defaultEndColor, bgText.defaultColorFadeTime));

            ping_Texture = content.Load<Texture2D>("Sprites\\ping_effect");
            
        }

        private bool LoadSubtexture()
        {
            if (subTexturetoLoad != null)
            {
                Texture2D subtitleTexture = content.Load<Texture2D>(subTexturetoLoad);
                subTitleX = ScreenManager.GraphicsDevice.Viewport.Width - subtitleTexture.Width - 10.0f;
                subtitleText = new CursorText(subtitleTexture, 1.5f, 5.0f, content);
                return true;
            }

            else
            {
                subtitleText = new CursorText(null, 0, 0, content);
                return false;
            }
        }

        public void SetSubtitleTexture(string subtitleTexturePath, float cursorDelay)
        {
            subTexturetoLoad = subtitleTexturePath;
            if (LoadSubtexture())
            {
                subtitleText.StartCursor(cursorDelay);
            }
        }


        /// <summary>
        /// Unloads graphics content for this screen.
        /// </summary>
        public override void UnloadContent()
        {
            content.Unload();
        }



        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the background screen. Unlike most screens, this should not
        /// transition off even if it has been covered by another screen: it is
        /// supposed to be covered, after all! This overload forces the
        /// coveredByOtherScreen parameter to false in order to stop the base
        /// Update method wanting to transition off.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            subtitleText.Update(gameTime);

            nextBGText -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            nextBGClear -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (nextBGClear <= 0.0f)
            {
                nextBGClear = nextBGClearMax;
                bgText.ClearAllUpcoming();
                for (int i = 0; i < bgText.GetMaxLines(); i++)
                {
                    bgText.AddItem(new HackGameBoard_BackgroundTextItem(new StringBuilder(" "), 0, bgText.defaultStartColor, bgText.defaultEndColor, bgText.defaultColorFadeTime));
                }
                nextBGText = nextBGTextMax;
            }


            if (nextBGText <= 0.0f)
            {
                //throw a bg text line
                nextBGText = MathHelper.Lerp(nextBGTextMin, nextBGTextMax, (float)r.NextDouble());
                HackGameBoard_BackgroundTextItem bgnext = bgTextStrings[r.Next(0, bgTextStrings.Count)];
                bgText.AddItem(bgnext);
            }
            bgText.UpdateSelf(gameTime);
            base.Update(gameTime, otherScreenHasFocus, false);

            ping_Delay -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (ping_Delay <= 0.0f)
            {
                ping_Delay = ping_Delay_Max;
                Vector2 location = new Vector2(MathHelper.Lerp(0, ScreenManager.GraphicsDevice.Viewport.Width, (float)r.NextDouble()),
                    MathHelper.Lerp(0, ScreenManager.GraphicsDevice.Viewport.Height, (float)r.NextDouble()));
                ping_Element = new WorldSpaceUIElement(ping_Texture, 2.0f, location,location, new Color(0.0f, 1.0f, 0, 0.0f), new Color(0.0f, 0, 0, 0), 0.2f, 10.0f, 3.0f);
            }

            if (ping_Element != null)
            {
                ping_Element.UpdateState(gameTime, null);
            }
        }


        /// <summary>
        /// Draws the background screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            Rectangle fullscreen = new Rectangle(0, 0, viewport.Width, viewport.Height);

            spriteBatch.Begin();

            spriteBatch.Draw(backgroundTexture, fullscreen,
                             new Color(TransitionAlpha, TransitionAlpha, TransitionAlpha));


            //draw all the widgets
            bgText.DrawSelf(spriteBatch, ScreenManager.GraphicsDevice);

            if (ping_Element != null)
            {
                ping_Element.DrawSelf(spriteBatch, Vector2.Zero, 1.0f);
            }

            //now draw main overlay
            spriteBatch.Draw(titleOverlayTexture, fullscreen,
                             new Color(TransitionAlpha, TransitionAlpha, TransitionAlpha));

            // draw cursor text
            subtitleText.DrawSelf(spriteBatch, new Vector2(subTitleX, subTitleY), new Color(TransitionAlpha, TransitionAlpha, TransitionAlpha));



            spriteBatch.End();

            base.Draw(gameTime);
        }


        #endregion

        public void StartCursor(float delay)
        {
            if (subtitleText != null)
                subtitleText.StartCursor(delay);
        }
    }
}
