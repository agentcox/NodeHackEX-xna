using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using System.Text;
using System.IO.IsolatedStorage;
using System.IO;
using GameStateManagement;
using NodeDefinition;
using WP7MusicManagement;
using Microsoft.Phone.Tasks;
using Microsoft.Devices;

using System.Xml.Serialization;

using System.Globalization;


namespace HackPrototype
{

    public class GameState
    {
        public UInt64 currentTotalScore = 0;
        public int currentWave = 1;
        public int boardSeed = 0;
        public bool loaded = false;
        public List<WaveAccountingEntry> accountingentries = new List<WaveAccountingEntry>();
    }

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
         #region Fields

        public GameState state = new GameState();

        GraphicsDeviceManager graphics;
        ScreenManager screenManager;
        UInt64 finalscore = 0;
        UInt64 levelscore = 0;

        WaveAccountingTable currentGameAccounting;

        int currentwave = 1;
        int maxwave = 1;
        int boardSeed = 0;
        BoardWaveDefines wavedefines;

        Song MainMenuSong;
        Song GameSong;

        BackgroundScreen background;

        public float LoadTime = 0.0f;
        public DateTime LoadStartTime;
        public DateTime LoadStopTime;
        public float DensityDelta = 99999.0f;

        BackgroundMusicManager backgroundMusicManager;

        CultureInfo ci;

        #endregion

        #region Initialization

        /// <summary>
        /// The main game constructor.
        /// </summary>
        public Game1()
        {
            Content.RootDirectory = "Content";

            graphics = new GraphicsDeviceManager(this);
            graphics.IsFullScreen = true;
  graphics.PreparingDeviceSettings += new EventHandler<PreparingDeviceSettingsEventArgs>(graphics_PreparingDeviceSettings);
  
  // Frame rate is 60 fps
  IsFixedTimeStep = false;
  //TargetElapsedTime = TimeSpan.FromTicks(166667);


#if WINDOWS
            graphics.IsFullScreen = false;
#endif
            
            // you can choose whether you want a landscape or portait
            // game by using one of the two helper functions.
            InitializePortraitGraphics();
            //InitializeLandscapeGraphics();

            //Get CI
            ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            ci.NumberFormat.NumberDecimalSeparator = ".";
            ci.NumberFormat.NumberGroupSeparator = ",";


            //Load the waves
            wavedefines = Content.Load<BoardWaveDefines>("Maps\\Waves");
            maxwave = wavedefines.GetMaxWave();

            // Create the accounting table
            currentGameAccounting = new WaveAccountingTable(maxwave);

            // Create the screen manager component.
            screenManager = new ScreenManager(this);

            Components.Add(screenManager);

            //attempt to deserialize overall gameplay state.
            bool loaded;
            loaded = DeserializeState();

            if (loaded == true)
            {
                boardSeed = state.boardSeed;
                currentwave = state.currentWave;
                finalscore = state.currentTotalScore;
                currentGameAccounting.Load(state.accountingentries);
            }

            // attempt to deserialize the screen manager from disk. if that
            // fails, we add our default screens.
            //if (!screenManager.DeserializeState())
            //{
                // Activate the first screens.

            background = new BackgroundScreen("sprites\\Titles\\MainMenu");
            background.StartCursor(2.0f);

            screenManager.AddScreen(background, null);
            screenManager.AddScreen(new SplashScreen(), null);
            //screenManager.AddScreen(new GameOverScreen(), PlayerIndex.One);
            //screenManager.AddScreen(new GameplayScreen(), null);
            //}

              
               //music
               backgroundMusicManager = new BackgroundMusicManager(this);
               MainMenuSong = Content.Load<Song>("Sounds\\Music\\jm_menu_loop");
               GameSong = Content.Load<Song>("Sounds\\Music\\jm_main_loop");
        }

        public void SetBackgroundSubtitle(string subtitleTexturePath, float cursorDelay)
        {
            if (background != null)
            {
                background.SetSubtitleTexture(subtitleTexturePath, cursorDelay);
            }
        }

        public CultureInfo GetCI()
        {
            return ci;
        }

        void graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            e.GraphicsDeviceInformation.PresentationParameters.PresentationInterval = PresentInterval.One;
        }



        public void ShareWithFriend(UInt64 score)
        {
            ShareLinkTask shareLinkTask = new ShareLinkTask();

            shareLinkTask.Title = "Node.Hack EX for Windows Phone";
            shareLinkTask.LinkUri = new Uri("http://www.nodehackgame.com", UriKind.Absolute);
            if (score == 0)
            {
                shareLinkTask.Message = "I just played Node.Hack EX, a hacking action/strategy game for Windows Phone. Try it out!";
            }
            else
            {
                CurrencyStringer stringer = new CurrencyStringer(score);
                shareLinkTask.Message = "I just got " + stringer.outputstring.ToString() + " in Node.Hack EX, a hacking game for Windows Phone.";
            }

            shareLinkTask.Show();

        }

        public void SetBackgroundScreen(BackgroundScreen screen)
        {
            background = screen;
        }

        public bool IsContinuing()
        {
            if (currentwave > 1 || finalscore > 0)
                return true;
            return false;
        }

        public int GetRandomBoardSeed()
        {
            return boardSeed;
        }

        public void ResetRandomBoardSeed()
        {
            boardSeed = 0;
            state.boardSeed = 0;
        }

        public void SetRandomBoardSeed(int newRandomSeed)
        {
            state.boardSeed = newRandomSeed;
            boardSeed = newRandomSeed;
        }

        public void PlayGameSong()
        {
            backgroundMusicManager.Play(GameSong);
        }

        public void PlayMainMenuSong()
        {
            backgroundMusicManager.Play(MainMenuSong);
        }

        public void StopMusic()
        {
            backgroundMusicManager.Stop();
        }

        public void SetLevelScore(UInt64 score)
        {
            levelscore = score;
            currentGameAccounting.ModifyEntry(currentwave, score);
        }

        public UInt64 GetLevelScore(UInt64 score)
        {
            return levelscore;
        }

        public void SetFinalScore(UInt64 score)
        {
            finalscore = score;
        }

        public UInt64 GetFinalScore()
        {
            return finalscore;
        }

        public int GetCurrentWave()
        {
            return currentwave;
        }

        public bool SetCurrentWave(int wave)
        {
            if (wave < 1 || wave > maxwave)
            {
                return false;
            }
            currentwave = wave;
            return true;
        }

        public BoardWaveDefine GetWave(int wave)
        {
            return wavedefines.GetWave(wave);
        }

        protected override void OnExiting(object sender, System.EventArgs args)
        {
            //serialize overall state
            SerializeState();
            
            // serialize the screen manager whenever the game exits
            screenManager.SerializeState();

            base.OnExiting(sender, args);
        }

        public void Vibrate(float secondsLength)
        {
            if (secondsLength > 5.0 || secondsLength < 0)
                return;
            VibrateController vc = VibrateController.Default;
            vc.Start(TimeSpan.FromSeconds(secondsLength));
        }

        /// <summary>
        /// Helper method to the initialize the game to be a portrait game.
        /// </summary>
        private void InitializePortraitGraphics()
        {
            graphics.PreferredBackBufferWidth = 480;
            graphics.PreferredBackBufferHeight = 800;
        }

        /// <summary>
        /// Helper method to initialize the game to be a landscape game.
        /// </summary>
        private void InitializeLandscapeGraphics()
        {
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 480;
        }

        #endregion

        #region Draw

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.Black);

            // The real drawing happens inside the screen manager component.
            base.Draw(gameTime);
        }

        #endregion


        public void ResetAll()
        {
            SetFinalScore(0);
            SetCurrentWave(1);
            ResetRandomBoardSeed();
            currentGameAccounting.FillWithEmpty();

            //write to state so it can be serialized out
            SetState();
        }

        public int GetMaxWave()
        {
            return wavedefines.GetMaxWave();
        }

        public bool DoesWaveExist(int wave)
        {
            return wavedefines.DoesWaveExist(wave);
        }

        public WaveAccountingTable GetAccounting()
        {
            return this.currentGameAccounting;
        }

        public void SetState()
        {
            //make sure everything is represented in game state
            state.boardSeed = boardSeed;
            state.currentTotalScore = finalscore;
            state.currentWave = currentwave;
            state.accountingentries = currentGameAccounting.GetEntries();
        }

        /// <summary>
        /// Informs the screen manager to serialize its state to disk.
        /// </summary>
        public void SerializeState()
        {

            SetState();

            // open up isolated storage
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                // if our screen manager directory already exists, delete the contents
                if (storage.DirectoryExists("Game1"))
                {
                    DeleteState(storage);
                }

                // otherwise just create the directory
                else
                {
                    storage.CreateDirectory("Game1");
                }

                // create a file we'll use to store the list of screens in the stack
                using (IsolatedStorageFileStream stream = storage.CreateFile("Game1\\Game1.dat"))
                {
                    XmlSerializer writer = new XmlSerializer(typeof(GameState));
                    writer.Serialize(stream, this.state);
                }

            }
        }

        public bool DeserializeState()
        {
            // open up isolated storage
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                // see if our saved state directory exists
                if (storage.DirectoryExists("Game1"))
                {
                    try
                    {
                        // see if we have a screen list
                        if (storage.FileExists("Game1\\Game1.dat"))
                        {
                            // load the list of screen types
                            using (IsolatedStorageFileStream stream = storage.OpenFile("Game1\\Game1.dat", FileMode.Open, FileAccess.Read))
                            {
                                XmlSerializer reader = new XmlSerializer(typeof(GameState));
                                state = (GameState)(reader.Deserialize(stream));
                            }
                        }

                        return true;
                    }
                    catch (Exception e)
                    {
                        string wut = e.Message;
                        // if an exception was thrown while reading, odds are we cannot recover
                        // from the saved state, so we will delete it so the game can correctly
                        // launch.
                        DeleteState(storage);
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Deletes the saved state files from isolated storage.
        /// </summary>
        private void DeleteState(IsolatedStorageFile storage)
        {
            // get all of the files in the directory and delete them
            string[] files = storage.GetFileNames("ScreenManager\\*");
            foreach (string file in files)
            {
                storage.DeleteFile(Path.Combine("ScreenManager", file));
            }
        }

        public void HighScoresAndRestartGame()
        {
            UInt64 score = GetFinalScore();
            int wave = GetCurrentWave();


            //reset everything to wave 1 and score 0
            ResetAll();

            //high score screen.
            BackgroundScreen screen = new BackgroundScreen("Sprites\\Titles\\HighScores");
            screenManager.AddScreen(screen, PlayerIndex.One);
            screenManager.AddScreen(new MainMenuScreen(IsContinuing()), PlayerIndex.One);
            screenManager.AddScreen(new HighScoreScreenDynamic(score, wave), PlayerIndex.One);
        }

        public void ExitCurrentLevel()
        {
            //find the gameplayScreen.
            GameScreen[] screens = screenManager.GetScreens();
            foreach (GameScreen screen in screens)
            {
                if (screen is GameplayScreen)
                {
                    ((GameplayScreen)(screen)).ExitScreen();
                }
            }

            BackgroundScreen bgscreen = new BackgroundScreen("Sprites\\Titles\\MainMenu");
            screenManager.AddScreen(bgscreen, PlayerIndex.One);
            screenManager.AddScreen(new MainMenuScreen(IsContinuing()), PlayerIndex.One);

        }
    }

    
}
