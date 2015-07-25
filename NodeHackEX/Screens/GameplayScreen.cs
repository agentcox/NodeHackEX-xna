#region File Description
//-----------------------------------------------------------------------------
// GameplayScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Threading;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using System.IO;
using HackPrototype;
using System.Xml.Serialization;
using System.Xml;


#endregion

namespace GameStateManagement
{
    /// <summary>
    /// This screen implements the actual game logic. It is just a
    /// placeholder to get the idea across: you'll probably want to
    /// put some more interesting gameplay in here!
    /// </summary>
    /// 


    class Camera
    {
        Vector2 cameraOffset = Vector2.Zero;
        Vector2 lastCameraOffset = Vector2.Zero;

        float cameraZoom = 1.0f;
        float lastcameraZoom = 1.0f;

        bool locked = false;


        public void SetCameraOffsetAndZoom(Vector2 newcameraOffset, float newZoom, HackGameBoard board)
        {
            lastcameraZoom = cameraZoom;
            lastCameraOffset = cameraOffset;
            
            cameraOffset.X = newcameraOffset.X;
            cameraOffset.Y = newcameraOffset.Y;

            cameraZoom = newZoom;

            BoundCameraOffsetAndZoom(board);
        }

        public Vector2 GetOffsetDelta()
        {
            return lastCameraOffset - cameraOffset;
        }

        public float GetZoomDelta()
        {
            return lastcameraZoom - cameraZoom;
        }

        public Vector2 GetCameraOffset()
        {
            return cameraOffset;
        }

        public float GetCameraZoom()
        {
            return cameraZoom;
        }

        public void BoundCameraOffsetAndZoom(HackGameBoard board)
        {

            if (cameraOffset.X > 0.0f)
            {
                cameraOffset.X = 0.0f;
            }
            if (cameraOffset.Y > 0.0f)
            {
                cameraOffset.Y = 0.0f;
            }

            Vector2 boardmax = board.GetMaxCameraOffsetBottomRight(cameraZoom, board.GetGame().GraphicsDevice);

            if (cameraOffset.X < boardmax.X)
            {
                cameraOffset.X = boardmax.X;
            }

            if (cameraOffset.Y < boardmax.Y)
            {
                cameraOffset.Y = boardmax.Y;
            }

            float minZoom = board.GetMinZoom(board.GetGame().GraphicsDevice);

            if (cameraZoom < minZoom)
            {
                cameraZoom = minZoom;
            }
            if (cameraZoom > 1.5f)
            {
                cameraZoom = 1.5f;
            }
        }


        public void Update()
        {
            lastCameraOffset = cameraOffset;
            lastcameraZoom = cameraZoom;
        }

        internal void LerpToCameraOffsetAndZoom(Vector2 newCam, float zoom, HackGameBoard hackGameBoard)
        {
            //TODO!
            throw new NotImplementedException();
        }

        public void Lock()
        {
            locked = true;
        }

        public void Unlock()
        {
            locked = false;
        }

        public bool IsLocked()
        {
            return locked;
        }
    }

    class GameplayScreen : GameScreen
    {
        #region Fields

        ContentManager content;
        SpriteFont gameFont;

        //SpriteBatch spriteBatch;
        HackNodeGameBoardMedia drawing;
        
        
        FlickHandler flickHandler = new FlickHandler();

        //STUFF THAT GOES IN GAME STATE
        Camera cam = new Camera();
        HackGameBoard board;
        HackGameAgent_Player player;
        HackGameTimer gameOverTimer;
        HackGameTimer exitOutTimer;
        bool allOver = false;
        //END STUFF THAT GOES IN GAME STATE

        bool firstFrame = true;
        Game1 ourGame;

        bool paused = true;


        #endregion

        #region Initialization

        

        /// <summary>
        /// Constructor.
        /// </summary>
        public GameplayScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);




            this.EnabledGestures = GestureType.FreeDrag | GestureType.Pinch | GestureType.Tap | GestureType.Flick;


        }


        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void LoadContent()
        {


            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");

            ourGame = ((Game1)(ScreenManager.Game));

            gameFont = content.Load<SpriteFont>("gamefont");

            drawing = new HackNodeGameBoardMedia(ScreenManager.Game, content);
            board = new HackGameBoard((Game1)ScreenManager.Game, this, drawing);


            //USE YOUR CURRENT WAVE TO LOAD UP A NEW MAP.

            board.LoadWave(ourGame);         
            player = new HackGameAgent_Player(board);
            board.AddAgent(player);

            int maxtrails = 50;
            float maxAlpha = 0.85f;
            float minAlpha = 0.00f;
            for (int i = 0; i < maxtrails; i++)
            {
                HackGameAgent_Trail t = new HackGameAgent_Trail(board, MathHelper.Lerp(minAlpha, maxAlpha, (float)(maxtrails-i)/maxtrails), drawing.PlayerTexture);
                if (i % 2 != 0 || i < 5)
                {
                    t.SetCurrentState(HackGameAgent.HackGameAgent_State.HackGameAgent_State_Inactive);
                }
                player.AddTrail(t);
                board.AddAgent(t);
            }
            ScreenManager.Game.ResetElapsedTime();

            ourGame.LoadStopTime = DateTime.Now;
            ourGame.LoadTime = (float)(ourGame.LoadStopTime.Ticks - ourGame.LoadStartTime.Ticks) / (float)TimeSpan.TicksPerSecond;

        }


        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void UnloadContent()
        {

            content.Unload();
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the state of the game. This method checks the GameScreen.IsActive
        /// property, so the game will stop updating when the pause menu is active,
        /// or if you tab away to a different application.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            if (IsActive)
            {
                if (paused)
                {
                    //not paused anymore!
                    paused = false;
                    board.GetMedia().UnpauseAllSounds();
                }

                if (firstFrame)
                {
                    firstFrame = false;
                    ourGame.PlayGameSong();
                }
            
                    board.UpdateState(gameTime, drawing);
            

            if (player.GetCurrentState() == HackGameAgent.HackGameAgent_State.HackGameAgent_State_BeingKilled)
            {
                if (gameOverTimer == null) //we just entered this state.
                {
                    gameOverTimer = new HackGameTimer(5.0f);
                    board.FreezeCollapseTimer();
                }
                gameOverTimer.Update(gameTime);
                if (!gameOverTimer.IsAlive() && !allOver)
                {
                    allOver = true;
                    ourGame.SetLevelScore(GetScore());
                    ourGame.SetFinalScore(ourGame.GetFinalScore() + GetScore());
                    
                    ExitScreen();
                    ScreenManager.AddScreen(new GameOverScreen(ourGame.GetCurrentWave(), ourGame.GetFinalScore()), PlayerIndex.One);

                }
            }

            else if (player.GetCurrentState() == HackGameAgent.HackGameAgent_State.HackGameAgent_State_Exited)
            {
                if (exitOutTimer == null) //we just entered this state.
                {
                    exitOutTimer = new HackGameTimer(5.0f);
                    board.EndCollapse();
                }
                exitOutTimer.Update(gameTime);

            if (!exitOutTimer.IsAlive() && !allOver)
            {
                allOver = true;
                ourGame.SetLevelScore(GetScore());
                ourGame.SetFinalScore(ourGame.GetFinalScore() + GetScore());
                //reset the random seed
                ourGame.ResetRandomBoardSeed();
                board.GetGame().StopMusic();

                ExitScreen();
                
                //level advance!

                int i;
                for(i = ourGame.GetCurrentWave() + 1; !ourGame.DoesWaveExist(i) && i <= ourGame.GetMaxWave(); i++)
                {
                }
                //did we win the game?
                if (i > ourGame.GetMaxWave())
                {
                    //FIX FIX, MAKE IT A WIN EVERYTHING SCREEN
                    ScreenManager.AddScreen(new GameWinScreen(ourGame.GetMaxWave(), ourGame.GetFinalScore()), PlayerIndex.One);
                    //reset game state to 0 score, wave 1

                }
                else
                {
                    int oldWave = ourGame.GetCurrentWave();
                    //roll right along into the next level.
                    ourGame.SetCurrentWave(i);

                    LoadingScreen.Load(ScreenManager, true, PlayerIndex.One, new GameplayScreen());
                    ScreenManager.AddScreen(new LevelWinScreen(oldWave, ourGame.GetFinalScore()), PlayerIndex.One);
                }
                //


            }
            }
        }
        }

        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(InputState input)
        {
            cam.Update();

            foreach (GestureSample gesture in input.Gestures)
            {
                GestureSample gs = gesture;
                if (gs.GestureType == GestureType.FreeDrag)
                {
                    if (!cam.IsLocked())
                    {
                        cam.SetCameraOffsetAndZoom(cam.GetCameraOffset() + gs.Delta, cam.GetCameraZoom(), board);
                    }
                }

                if (gs.GestureType == GestureType.Tap)
                {

                    //ONLY IF PLAYER ACTIVE!
                    if (player.IsActive())
                    {

                        //player.setCurrentBoardLocation(board.GetBoardLocationAtTouchLocation(gs.Position, cameraOffset, cameraZoom, ScreenManager.GraphicsDevice), board);
                        Point location = board.GetBoardLocationAtTouchLocation(gs.Position, cam.GetCameraOffset(), cam.GetCameraZoom(), ScreenManager.GraphicsDevice);

                        if (player.TryDestination(location, board))
                        {
                            drawing.PlayerLockLocationSound.Play();
                        }
                    }

                }

                else if (gs.GestureType == GestureType.Pinch)
                {
                    // get the current and previous locations of the two fingers
                    Vector2 a = gs.Position;
                    Vector2 aOld = gs.Position - gs.Delta;
                    Vector2 b = gs.Position2;
                    Vector2 bOld = gs.Position2 - gs.Delta2;

                    // figure out the distance between the current and previous locations
                    float d = Vector2.Distance(a, b);
                    float dOld = Vector2.Distance(aOld, bOld);

                    // calculate the difference between the two and use that to alter the scale
                    float scaleChange = (d - dOld) * .002f;

                    //find the center of the pinch point
                    Vector2 pinchCenter = new Vector2(Math.Abs((gs.Position2.X + gs.Position.X) / 2), Math.Abs((gs.Position2.Y + gs.Position.Y) / 2));
                    //find the center of the screen
                    Vector2 screenCenter = new Vector2(this.ourGame.GraphicsDevice.Viewport.Width / 2, this.ourGame.GraphicsDevice.Viewport.Height / 2);

                    Vector2 directionToPinchTarget = screenCenter - pinchCenter;
                    directionToPinchTarget.Normalize();


                    if (!cam.IsLocked())
                    {
                        if (scaleChange > 0)
                        {
                            cam.SetCameraOffsetAndZoom(cam.GetCameraOffset() - Vector2.One * 7.0f, cam.GetCameraZoom() + scaleChange, board);
                        }
                        else
                        {
                            cam.SetCameraOffsetAndZoom(cam.GetCameraOffset() + Vector2.One * 7.0f, cam.GetCameraZoom() + scaleChange, board);
                        }
                        //cam.SetCameraOffsetAndZoom(cam.GetCameraOffset() + directionToPinchTarget * 5.0f, cam.GetCameraZoom() + scaleChange, board);
                    }
                }

                    /*
                else if (gs.GestureType == GestureType.Flick)
                {
                    switch (flickHandler.getFlickDir(gs))
                    {
                        case FlickHandler.DirType.DirTypeUp:
                            board.PopUpScoreboard(5.0f);
                            break;
                        case FlickHandler.DirType.DirTypeDown:
                            board.PopDownScoreboard();
                            break;
                    }
                }*/
            }

            KeyboardState ks = Keyboard.GetState();
            if (ks.IsKeyDown(Keys.Q))
            {
                cam.SetCameraOffsetAndZoom(cam.GetCameraOffset(), cam.GetCameraZoom() + 0.02f, board);
            }

            if (ks.IsKeyDown(Keys.E))
            {
                cam.SetCameraOffsetAndZoom(cam.GetCameraOffset(), cam.GetCameraZoom() - 0.02f, board);
            }
            /*
            if (player.IsActive())
            {
                if (ks.IsKeyDown(Keys.W))
                {

                    Point newpt = board.getPointInDirection(player.getCurrentBoardLocation(), HackGameAgent.MovementDirection.MovementDirection_North);
                    newpt.Y -= 1;
                    if (player.TryDestination(newpt, board))
                    {
                        drawing.PlayerLockLocationSound.Play();
                    }
                }

                if (ks.IsKeyDown(Keys.E))
                {

                    Point newpt = board.getPointInDirection(player.getCurrentBoardLocation(), HackGameAgent.MovementDirection.MovementDirection_East);
                    newpt.X += 1;
                    if (player.TryDestination(newpt, board))
                    {
                        drawing.PlayerLockLocationSound.Play();
                    }
                }

            }
            */
            drawing.UpdatePanAndZoomDeltas(cam.GetOffsetDelta(), cam.GetZoomDelta());

            base.HandleInput(input);
        }

        public override void OnBackButton()
        {
            Pause();

            BackgroundScreen pauseBg = new BackgroundScreen("Sprites\\Titles\\GamePaused");
            ScreenManager.AddScreen(pauseBg, PlayerIndex.One);
            ScreenManager.AddScreen(new PauseScreen(pauseBg), PlayerIndex.One);
        }

        private void Pause()
        {
            paused = true;
            board.GetMedia().PauseAllSounds();
        }

        private void DrawGameBoardForeground(GameTime gameTime, SpriteBatch spriteBatch)
        {
            board.DrawScoring(drawing, spriteBatch, ScreenManager.GraphicsDevice, player);
            board.DrawTicker(drawing, spriteBatch, ScreenManager.GraphicsDevice, player);
            board.DrawExitEffect(drawing, cam.GetCameraZoom(), spriteBatch, ScreenManager.GraphicsDevice, player);
        }

        private void DrawGameBoard(GameTime gameTime, SpriteBatch spriteBatch)
        {
            //spriteBatch.Begin();
            //spriteBatch.Draw(drawing.NodeBoxtexture, Vector2.Zero, Color.White);
            board.DrawGameBoard(drawing, spriteBatch, cam.GetCameraOffset(), cam.GetCameraZoom(), ScreenManager.GraphicsDevice, player);

            
            //board.DrawDebugText(drawing, spriteBatch, ScreenManager.GraphicsDevice, player);
            //spriteBatch.End();
        }

        private void DrawGameBoardAdditive(GameTime gameTime, SpriteBatch spriteBatch)
        {
            board.DrawGameBoardAdditive(drawing, spriteBatch, cam.GetCameraOffset(), cam.GetCameraZoom(), ScreenManager.GraphicsDevice, player);
        }

        private void DrawGameBoardBackground(GameTime gameTime, SpriteBatch spriteBatch)
        {
            board.DrawBackgroundText(drawing, spriteBatch, ScreenManager.GraphicsDevice, player);
        }

        public UInt64 GetScore()
        {
            return board.GetScore();
        }

        public Camera GetCamera()
        {
            return cam;
        }

        public void LockCamera()
        {
            cam.Lock();
        }

        public void UnlockCamera()
        {
            cam.Unlock();
        }
       
        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // This game has a blue background. Why? Because!
            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target,
                                               Color.Black, 0, 0);

            // Our player and enemy are both actually just text strings.
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            spriteBatch.Begin();
            DrawGameBoardBackground(gameTime, spriteBatch);
            spriteBatch.End();

            spriteBatch.Begin();
            DrawGameBoard(gameTime, spriteBatch);
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
            DrawGameBoardAdditive(gameTime, spriteBatch);
            spriteBatch.End();

            spriteBatch.Begin();
            DrawGameBoardForeground(gameTime, spriteBatch);
            spriteBatch.End();

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0)
                ScreenManager.FadeBackBufferToBlack(1f - TransitionAlpha);
        }


        #endregion

    }

    class FlickHandler
    {

        public enum DirType
        {

            DirTypeUp,
            DirTypeDown,
            DirTypeLeft,
            DirTypeRight,
            DirTypeUnknown
        };

        public DirType getFlickDir(GestureSample gs)
        {
            if (gs.Delta.Y > 0 && (Math.Abs(gs.Delta.Y) >= Math.Abs(gs.Delta.X)))
            {
                return DirType.DirTypeDown;
            }

            if (gs.Delta.Y < 0 && (Math.Abs(gs.Delta.Y) >= Math.Abs(gs.Delta.X)))
            {
                return DirType.DirTypeUp;
            }

            if (gs.Delta.X > 0 || (Math.Abs(gs.Delta.Y) < Math.Abs(gs.Delta.X)))
            {
                return DirType.DirTypeRight;
            }

            if (gs.Delta.X > 0 || (Math.Abs(gs.Delta.Y) > Math.Abs(gs.Delta.X)))
            {
                return DirType.DirTypeLeft;
            }
            return DirType.DirTypeUnknown;
        }
    }
}
