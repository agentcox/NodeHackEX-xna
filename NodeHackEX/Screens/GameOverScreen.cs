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
    class GameOverScreen : IntermediateScreen
    {

        public GameOverScreen(int wave, UInt64 score)
            : base("Sprites\\Titles\\GameOver", "Sprites\\Titles\\TapForHighScores", "Sounds\\DoorSlam", wave, score)
        {
        }
   
        protected override void ExitSelf()
        {

            ExitScreen();
            ((Game1)(ScreenManager.Game)).HighScoresAndRestartGame();
        }


 }
}
