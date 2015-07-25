
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using HackPrototype;
using Microsoft.Xna.Framework.Input.Touch;

namespace GameStateManagement
{
    class LevelSelectControl : ImageControl
    {
        private LevelSelectScreen levelScreen;
        int myWave;

        public LevelSelectControl(Texture2D texture, Vector2 position, int wave, LevelSelectScreen ownerScreen)
            : base(texture, position)
        {
            myWave = wave;
            levelScreen = ownerScreen;
        }

        public override void HandleInput(InputState input)
        {
            foreach(GestureSample gs in input.Gestures)
            {
                if (gs.GestureType == GestureType.Tap)
                {
                    Vector2 finalPosition = GetFinalPosition();
                    Rectangle location = new Rectangle((int)finalPosition.X, (int)finalPosition.Y, (int)Size.X, (int)Size.Y);
                    if(location.Contains(new Point((int)gs.Position.X, (int)gs.Position.Y)))
                    {
                        levelScreen.OnLevelSelected(myWave);
                    }
                }
            }
        }

        private Vector2 GetFinalPosition()
        {
            return Position + GetSubFinalPosition(Parent);
        }

        private Vector2 GetSubFinalPosition(Control current)
        {
            if (current.Parent != null)
            {
                return current.Position + GetSubFinalPosition(current.Parent);
            }
            return current.Position;
        }
    }
}
