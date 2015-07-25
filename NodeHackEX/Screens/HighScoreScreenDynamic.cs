//-----------------------------------------------------------------------------
// HighScoreScreenDynamic.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Input;
using HackPrototype;
using Microsoft.Phone;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input.Touch;
using System.IO.IsolatedStorage;
using System.IO;
using System.Xml.Serialization;

namespace GameStateManagement
{

    public class HighScoreEntry
    {
        public UInt64 score = 0;
        public int wave = 0;
        public string name = "Default";

        public HighScoreEntry(UInt64 c_score, int c_wave, string c_name)
        {
            score = c_score;
            wave = c_wave;
            name = c_name;
        }

        public HighScoreEntry()
        {
        }
    }

    public struct HighScoreStruct
    {
        public List<HighScoreEntry> highScores;
        public int highestWaveReached;
    }

    public class HighScoreTable
    {
        HighScoreStruct highScoreStruct;
        public List<string> highScoreStrings;
        public int maxScores = 17;
        const string fileName = "highscores.xml";
        public int currentHighScoreIndex = -1;


        public HighScoreTable()
        {
            highScoreStruct.highScores = new List<HighScoreEntry>();
        }

        public List<HighScoreEntry> GetHighScores()
        {
            return highScoreStruct.highScores;
        }

        public int GetHighestWave()
        {
            if (highScoreStruct.highestWaveReached > 0)
                return highScoreStruct.highestWaveReached;
            else
            {
                return 1;
            }
        }

        public bool Load(ContentManager content, UInt64 newHighScore, int waveReached)
        {
            IsolatedStorageFile isoFile = IsolatedStorageFile.GetUserStoreForApplication();
            IsolatedStorageFileStream isoFileStream;
            if (isoFile.FileExists(fileName))
            {
                isoFileStream = isoFile.OpenFile(fileName, System.IO.FileMode.Open, FileAccess.ReadWrite);
            }
            else
            {
                isoFileStream = isoFile.CreateFile(fileName);
                isoFileStream.Close();
                //write in new data
                CreateDefaultScores();
                WriteScores();

                isoFileStream.Close();

                //read it in again
                isoFileStream = isoFile.OpenFile(fileName, System.IO.FileMode.Open);
            }

            XmlSerializer reader = new XmlSerializer(typeof(HighScoreStruct));
            try
            {
                highScoreStruct = (HighScoreStruct)reader.Deserialize(isoFileStream);
            }
            catch (Exception e)
            {
                CreateDefaultScores();
            }

            isoFileStream.Close();
            WriteScores();
            GenerateNewStrings();

            if (newHighScore > 0)
            {
                //check if higher than any score on list.
                for (int i = 0; i < highScoreStruct.highScores.Count; i++)
                {
                    if (newHighScore > highScoreStruct.highScores[i].score)
                    {
                        return true;
                    }
                }
                return false;
            }
            else
            {
                return false;
            }

        }

        public void WriteInNewHighScore(HighScoreEntry newHighScore)
        {

            if (newHighScore.score > 0)
            {
                //check if higher than any score on list.
                for (int i = 0; i < highScoreStruct.highScores.Count; i++)
                {
                    if (newHighScore.score > highScoreStruct.highScores[i].score)
                    {
                        highScoreStruct.highScores.Insert(i, newHighScore);
                        currentHighScoreIndex = i;
                        int amountOver = highScoreStruct.highScores.Count - maxScores;
                        if (amountOver > 0)
                        {
                            highScoreStruct.highScores.RemoveRange(maxScores, amountOver);
                        }
                        //stop looking.
                        break;
                    }
                }
                highScoreStruct.highScores.TrimExcess();

                //did we get a higher wave than the highest wave?
                if (newHighScore.wave > highScoreStruct.highestWaveReached)
                {
                    highScoreStruct.highestWaveReached = newHighScore.wave;
                }

                WriteScores();
                GenerateNewStrings();
            }
        }

        private void GenerateNewStrings()
        {
            highScoreStrings = new List<string>();
            for (int i = 0; i < highScoreStruct.highScores.Count; i++)
            {
                highScoreStrings.Add(highScoreStruct.highScores[i].name + "\\" + new CurrencyStringer(highScoreStruct.highScores[i].score).outputstring + "\\" + highScoreStruct.highScores[i].wave.ToString());
            }
        }

        private void CreateDefaultScores()
        {
            UInt64 scoreStep = 1000;
            highScoreStruct.highScores.Clear();
            for (int i = 0; i < maxScores; i++)
            {
                highScoreStruct.highScores.Add(new HighScoreEntry(scoreStep * (UInt64)(maxScores - i), 1, "Anonymous"));
            }
            highScoreStruct.highestWaveReached = 1;
        }

        private bool WriteScores()
        {
            IsolatedStorageFile isoFile = IsolatedStorageFile.GetUserStoreForApplication();
            IsolatedStorageFileStream isoFileStream;
            if (isoFile.FileExists(fileName))
            {
                isoFileStream = isoFile.OpenFile(fileName, System.IO.FileMode.Create, FileAccess.Write);
                XmlSerializer serializer = new XmlSerializer(typeof(HighScoreStruct));
                serializer.Serialize(isoFileStream, highScoreStruct);
                isoFileStream.Close();
                return true;
            }
            else
            {
                return false;
            }
        }
    }
    /// <summary>
    /// LeaderboardScreen is a GameScreen that creates a single PageFlipControl containing
    /// a collection of LeaderboardPanel controls, which display a game's leaderboards.
    /// 
    /// You will need to customize the LoadContent() method of this class to create the
    /// appropriate list of leaderboards to match your game configuration.
    /// </summary>
    public class HighScoreScreenDynamic : SingleControlScreen
    {

        bool startedDraw = false;

        IAsyncResult kbResult;
        string typedText = "";

        UInt64 newScoreToPost;
        int maxWaveToPost;

        HighScoreTable highScores;

        float TimeToAutoClose = 5.0f;
        float currentTimer = 0.0f;

        bool madeHighScore = false;
        
        Texture2D tapForMainMenuTexture;
        Vector2 tapForMainMenuTextureLocation;

        public HighScoreScreenDynamic(UInt64 newScore, int maxWave) : base()
        {

            newScoreToPost = newScore;
            maxWaveToPost = maxWave;

            EnabledGestures = GestureType.Tap | GestureType.Hold;
        }
        
        public override void LoadContent()
        {
            ContentManager content = ScreenManager.Game.Content;

            ((Game1)(ScreenManager.Game)).SetBackgroundSubtitle("Sprites\\Titles\\HighScores", 1.0f);

            //check for isoStore version of highScores

            highScores = new HighScoreTable();
            madeHighScore = highScores.Load(content, newScoreToPost, maxWaveToPost);

            if (madeHighScore)
            {
                kbResult = Guide.BeginShowKeyboardInput(PlayerIndex.One,
"You Placed in the High Scores!", "Enter Your Name (Max 10 Characters)",
((typedText == null) ? "" : typedText),
GetTypedChars, null);

                tapForMainMenuTexture = content.Load<Texture2D>("Sprites\\Titles\\TapHoldShareTapMainMenu");
            }

            else
            {
                tapForMainMenuTexture = content.Load<Texture2D>("Sprites\\Titles\\TapForMainMenu");
            }

            tapForMainMenuTextureLocation = new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2 - tapForMainMenuTexture.Width / 2,
                ScreenManager.GraphicsDevice.Viewport.Height - 180);

            RootControl = new HighScorePanel(content, highScores);

            base.LoadContent();
        }

       
        protected void GetTypedChars(IAsyncResult r)
        {
            typedText = Guide.EndShowKeyboardInput(r);

            if (typedText == null || typedText == "")
            {
                typedText = "Anonymous";
            }

            if (typedText.Length > 10)
            {
                typedText = typedText.Substring(0, 10);
            }

            highScores.WriteInNewHighScore(new HighScoreEntry(newScoreToPost, maxWaveToPost, typedText));

            RootControl = new HighScorePanel(ScreenManager.Game.Content, highScores);
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void HandleInput(InputState input)
        {
            // look for any taps that occurred and select any entries that were tapped
            foreach (GestureSample gesture in input.Gestures)
            {

                if (gesture.GestureType == GestureType.Hold && madeHighScore == true)
                {
                    //share this score with a friend!
                    ((Game1)(ScreenManager.Game)).ShareWithFriend(newScoreToPost);
                }

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
            ((Game1)(ScreenManager.Game)).SetBackgroundSubtitle(null, 0);
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            Rectangle fullscreen = new Rectangle(0, 0, viewport.Width, viewport.Height);
            spriteBatch.Begin();

        
            if (RootControl.Visible == true)
            {
                spriteBatch.Draw(tapForMainMenuTexture, tapForMainMenuTextureLocation, new Color(TransitionAlpha, TransitionAlpha, TransitionAlpha));
            }

            spriteBatch.End();
            
            base.Draw(gameTime);
        }
    }
}
