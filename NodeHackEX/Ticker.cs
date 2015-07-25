using System;
using System.Collections.Generic;

using System.Text;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.IO;
using NodeDefinition;
using Pathfinding;
using Particles2DPipelineSample;


namespace HackPrototype
{

    class HackGameBoard_Ticker
    {

        const float tickerCharWidth = 10.0f;
        const float tickerSpeed = 0.05f;
        float tickerTimer;
        float currentTickerPos = 0;
        bool active = false;
        bool newlyactive = false;
        float newActiveTime = 0;

        float stringDrawLength;

        const float drawLocationY = 670;

        bool overriding = false; //whether the string is being overridden
        


        string currentTicker = "..DEFAULT STRING..";
        string overrideString = "..OVERRIDE STRING..";
        

        public void DrawSelf(HackNodeGameBoardMedia drawing, SpriteBatch spriteBatch, GraphicsDevice GraphicsDevice, HackGameAgent_Player player)
        {
            if (newlyactive)
            {
                newlyactive = false;
                active = true;
                currentTickerPos = GraphicsDevice.Viewport.Width - tickerCharWidth;
                stringDrawLength = drawing.Ticker_Font.MeasureString(currentTicker).X;
                if (!overriding && newActiveTime <= 0)
                {
                    drawing.StartMessageSound();
                }
            }
            if (active)
            {
                //Draw the ticker
                spriteBatch.DrawString(drawing.Ticker_Font, currentTicker, new Vector2(currentTickerPos, drawLocationY), Color.Red);
            }
            
        }

        public void SetNewTickerString(string stringval, float delay)
        {
            if (!overriding)
            {
                InternalSetNewTickerString(stringval, delay);
            }
        }

        private void InternalSetNewTickerString(string stringval, float delay)
        {
            active = false;
            currentTicker = stringval;
            newActiveTime = delay;
            if (newActiveTime <= 0)
            {
                newlyactive = true;
            }
        }

        public void SetOverride(string overridestring)
        {
            overriding = true;
            overrideString = overridestring;
            InternalSetNewTickerString(overridestring, 0);
        }

        public void ClearOverride()
        {
            overriding = false; //just let it expire.
        }

        public void UpdateSelf(GameTime time)
        {
            if (active)
            {
                float drawXEnd = -1 * stringDrawLength;
                tickerTimer -= (float)time.ElapsedGameTime.TotalSeconds;
                if (tickerTimer <= 0)
                {
                    currentTickerPos -= tickerCharWidth;
                    tickerTimer = tickerSpeed;
                }

                if (currentTickerPos < drawXEnd)
                {
                    active = false;
                    if (overriding)
                    {
                        InternalSetNewTickerString(overrideString, 0);
                    }
                }
            }
            else if (newActiveTime > 0)
            {
                newActiveTime -= (float)time.ElapsedGameTime.TotalSeconds;
                if (newActiveTime <= 0)
                {
                    newlyactive = true;
                }
            }
        }
    }

    public enum BackgroundTextDrawDirection
    {
        BackgroundTextDrawDirection_UpToDown,
        BackgroundTextDrawDirection_DownToUp
    };

    class HackGameBoard_BackgroundText
    {
        Queue<HackGameBoard_BackgroundTextItem> currentItems = new Queue<HackGameBoard_BackgroundTextItem>();
        Queue<HackGameBoard_BackgroundTextItem> upcomingItems = new Queue<HackGameBoard_BackgroundTextItem>();



        BackgroundTextDrawDirection direction = BackgroundTextDrawDirection.BackgroundTextDrawDirection_UpToDown;

        SpriteFont BG_Font;

        public Color defaultStartColor = new Color(.3f, .6f, .2f);
        public Color defaultEndColor = new Color(.2f, .4f, .1f);
        public float defaultColorFadeTime = 1.0f;
        
        public Color emergencyStartColor = new Color(.6f, .3f, .2f);
        //public Color emergencyEndColor = new Color(.4f, .2f, .1f); //RED
        public Color emergencyEndColor = new Color(.2f, .4f, .1f); // GREEN
        public float emergencyColorFadeTime = 3.0f;

        public Color awardStartColor = new Color(.8f, .9f, .6f);
        public Color awardEndColor = new Color(.2f, .4f, .1f);
        public float awardColorFadeTime = 3.0f;

        const float carriageReturnSpeed = 1.0f;
        const float carriageReturnHeight = 40.0f;
        int maxItemsOnScreen = 16;
        Vector2 drawLocation = Vector2.Zero;

        HackGameForwardLerpDrawHelper alphaLerp = null;
        HackGameForwardLerpDrawHelper colorLerp = null;

        float carriageReturnT = 0.0f;
        bool startCarriageReturn = false;

        public void FadeIn(float seconds)
        {
            Color oldAlpha = alphaLerp != null ? alphaLerp.CurrentColor() : Color.White;
            alphaLerp = new HackGameForwardLerpDrawHelper(seconds, 1.0f, 1.0f, seconds, oldAlpha, new Color(1f, 1f, 1f, 1f), seconds, Vector2.Zero, Vector2.Zero, seconds);
        }

        public void FadeOut(float seconds)
        {
            Color oldAlpha = alphaLerp != null ? alphaLerp.CurrentColor() : Color.White;
            alphaLerp = new HackGameForwardLerpDrawHelper(seconds, 1.0f, 1.0f, seconds, oldAlpha, new Color(0, 0, 0, 0), seconds, Vector2.Zero, Vector2.Zero, seconds);
        }

        public void ToColor(Color color, float seconds)
        {
            Color oldColor = colorLerp != null ? colorLerp.CurrentColor() : defaultStartColor;
            colorLerp = new HackGameForwardLerpDrawHelper(seconds, 1.0f, 1.0f, seconds, oldColor, color, seconds, Vector2.Zero, Vector2.Zero, seconds);
        }

        public void LoadContent(ContentManager content)
        {
            BG_Font = content.Load<SpriteFont>("Fonts\\bgfontsheet");
        }

        public void UpdateSelf(GameTime t)
        {
            float floatT = (float)t.ElapsedGameTime.TotalSeconds;
            //if upcoming expires, feed into current, start push up motion
            if (IsCarriageReturning())
            {
                carriageReturnT += carriageReturnSpeed * floatT;
                if (carriageReturnT >= 1.0f)
                {
                    carriageReturnT = 0;
                    DoCarriageReturnComplete();
                }
            }
            else if(upcomingItems.Count > 0) //update just the first item
            {
                HackGameBoard_BackgroundTextItem item = upcomingItems.Peek();
                item.timeUntilActivate -= floatT;
                if (item.timeUntilActivate <= 0)
                {
                    upcomingItems.Dequeue();

                    //add to latest
                    currentItems.Enqueue(item);
                    //if # items > max
                    if (currentItems.Count > maxItemsOnScreen)
                    {
                        DoCarriageReturnStart();
                    }
                }
            }
            foreach (HackGameBoard_BackgroundTextItem item in currentItems)
            {
                item.UpdateWhileActive(t);
            }
            if (alphaLerp != null)
            {
                alphaLerp.Update(t);
            }
            if (colorLerp != null)
            {
                colorLerp.Update(t);
            }
        }

        private void DoCarriageReturnStart()
        {
            startCarriageReturn = true;
            carriageReturnT = 0.0f;

            //temp temp
            DoCarriageReturnComplete();
        }

        public void AddItem(HackGameBoard_BackgroundTextItem item)
        {
            //put the item in the upcoming queue
            upcomingItems.Enqueue(item);
        }

        private void DoCarriageReturnComplete()
        {
            //pop off the topmost item
            currentItems.Dequeue();
            startCarriageReturn = false;
            carriageReturnT = 0.0f;
        }

        public bool IsCarriageReturning()
        {
            if (carriageReturnT > 0 || startCarriageReturn)
            {
                return true;
            }
            return false;
        }

        public void SetDrawDirection(BackgroundTextDrawDirection drawDirection)
        {
            direction = drawDirection;
        }

        public void ClearAllUpcoming()
        {
            upcomingItems.Clear();
        }

        public void SetDrawLocation(Vector2 location)
        {
            drawLocation = location;
        }

        public void SetMaxLines(int maxlines)
        {
            maxItemsOnScreen = maxlines;
            upcomingItems.Clear();
            currentItems.Clear();
        }

        public void DrawSelf(SpriteBatch sb, GraphicsDevice gd)
        {
            Color boardColor = colorLerp != null ? colorLerp.CurrentColor() : Color.White;
            boardColor.A = (byte)(alphaLerp != null ? alphaLerp.CurrentColor().A : (byte)255);
            Color finalColor = Color.White;
            Vector4 finalColorVec = Vector4.Zero;

            Queue<HackGameBoard_BackgroundTextItem>.Enumerator e = currentItems.GetEnumerator();
            Vector2 currentDraw = drawLocation;


            for (int i = 0; i < currentItems.Count; i++)
            {
                e.MoveNext();
                HackGameBoard_BackgroundTextItem item = e.Current;

                finalColorVec = item.GetCurrentColor().ToVector4() * boardColor.ToVector4();
                finalColorVec *= boardColor.ToVector4().W; //scale all colors down by A
                finalColor = new Color(finalColorVec);

                if (direction == BackgroundTextDrawDirection.BackgroundTextDrawDirection_DownToUp)
                {
                    currentDraw.Y = drawLocation.Y + (i * carriageReturnHeight);
                }
                else
                {
                    currentDraw.Y = drawLocation.Y + ((maxItemsOnScreen * carriageReturnHeight) - (i * carriageReturnHeight)); 
                }

                

                sb.DrawString(BG_Font, item.GetString(), currentDraw, finalColor, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 1.0f);
                
            }
        }

        public int GetMaxLines()
        {
            return maxItemsOnScreen;
        }
    }

    class HackGameBoard_BackgroundTextItem
    {
        StringBuilder textString;
        public float timeUntilActivate;
        HackGameForwardLerpDrawHelper lerpHelper;

        public HackGameBoard_BackgroundTextItem(StringBuilder stringToPrint, float timeAfterLast, Color startColor, Color endColor, float colorTime)
        {
            textString = stringToPrint;
            timeUntilActivate = timeAfterLast;
            lerpHelper = new HackGameForwardLerpDrawHelper(colorTime, 1.0f, 1.0f, colorTime, startColor, endColor, colorTime, Vector2.Zero, Vector2.Zero, colorTime);
        }

        public Color GetCurrentColor()
        {
            return lerpHelper.CurrentColor();
        }

        public StringBuilder GetString()
        {
            return textString;
        }

        public void UpdateWhileActive(GameTime t)
        {
            lerpHelper.Update(t);
        }
    }
}
