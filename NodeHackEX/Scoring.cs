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

    class CurrencyStringer
    {
        public StringBuilder outputstring;
        private StringBuilder workingstring;

        public CurrencyStringer(UInt64 startingamount)
        {
            outputstring = new StringBuilder();
            workingstring = new StringBuilder();
            UpdateString(startingamount);
        }

        public void UpdateString(UInt64 amount)
        {
            workingstring.Remove(0, workingstring.Length);
            workingstring.Append(amount);

            int oldlength = workingstring.Length;

            int numcommas = (oldlength - 1) / 3;

            int newlength = oldlength + numcommas;

            char[] old_workingarray = new char[oldlength];
            //workingstring.CopyTo(0, old_workingarray, 0, oldlength); //NOT SUPPORTED ON WP7

            for (int i = 0; i < oldlength; i++)
            {
                old_workingarray[i] = workingstring[i];
            }


            char[] new_workingarray = new char[newlength];
            if (oldlength > 4)
            {
                int old_counter = oldlength - 1;
                int commacounter = 1;
                for (int i = newlength - 1; i >= 0; i--, commacounter++)
                {
                    if (commacounter % 4 == 0)
                    {
                        new_workingarray[i] = ',';
                    }
                    else
                    {
                        new_workingarray[i] = old_workingarray[old_counter];
                        old_counter--;
                    }
                }
            }
            else
            {
                //workingstring.CopyTo(0, new_workingarray, 0, oldlength); //NOT SUPPORTED ON WP7
                new_workingarray = new char[oldlength];
                for (int i = 0; i < oldlength; i++)
                {
                    new_workingarray[i] = workingstring[i];
                }
            }
            outputstring.Remove(0, outputstring.Length);
            outputstring.Append('$');
            outputstring.Append(new_workingarray);
        }
    }

    public class WaveAccountingEntry
    {
        public int c_wave;
        public UInt64 c_score;

        public WaveAccountingEntry()
        {
            c_wave = 0;
            c_score = 0;
        }

        public WaveAccountingEntry(int wave, UInt64 score)
        {
            c_wave = wave;
            c_score = score;
        }
    }

    public class WaveAccountingTable
    {
        List<WaveAccountingEntry> entries;
        int maxentries;

        public WaveAccountingTable(int maxWaves)
        {
            maxentries = maxWaves;
            entries = new List<WaveAccountingEntry>();
            FillWithEmpty();
        }

        public void FillWithEmpty()
        {
            entries.Clear();
            for(int i = 0; i < maxentries; i++)
            {
                entries.Add(new WaveAccountingEntry(i + 1, 0));
            }
        }

        public List<WaveAccountingEntry> GetEntries()
        {
            return entries;
        }

        public bool ModifyEntry(int wave, UInt64 score)
        {
            if (wave > 0 && wave <= maxentries)
            {
                //find it
                for (int i = 0; i < maxentries; i++)
                {
                    if (entries[i].c_wave == wave)
                    {
                        entries[i].c_score = score;
                        return true;
                    }
                }
                return false;
            }
            else
                return false;
        }

        public void Load(List<WaveAccountingEntry> list)
        {
            this.entries.Clear();
            this.maxentries = list.Count;
            FillWithEmpty();
            for (int i = 0; i < list.Count; i++)
            {
                entries[i] = list[i];
            }
        }
    }

    class HackGameBoard_Scoring
    {

        //ACTUAL SCORE METRICS
        UInt64 score = 0;

        UInt64 scoreLeftToAdd = 0;
        float scoreToAddPerSecond = 0.0f;

        float score_UpdateTimeLeft = 0.0f;
        float score_UpdateTimeMax = 2.0f;

        HackGameBoard board;
        CurrencyStringer scorestring = new CurrencyStringer(0);

        public UInt64 GetScore()
        {
            return score;
        }

        public HackGameBoard_Scoring(HackGameBoard ourBoard)
        {
            board = ourBoard;
        }


        public enum HackGameBoard_Scoring_DisplayState
        {
            HackGameBoard_Scoring_DisplayState_Up,
            HackGameBoard_Scoring_DisplayState_PoppingUp,
            HackGameBoard_Scoring_DisplayState_PoppingDown,
            HackGameBoard_Scoring_DisplayState_Down
        }

        public HackGameBoard_Scoring_DisplayState state = HackGameBoard_Scoring_DisplayState.HackGameBoard_Scoring_DisplayState_Down;

        public Vector2 DrawLocation_Shell = new Vector2(0, 380);
        public Vector2 DrawLocation_AlertShell = new Vector2(402, 382);
        public Vector2 DrawLocation_BonusShell = new Vector2(502, 382);
        public Vector2 DrawLocation_ScoreText = new Vector2(13, 393);
        public Vector2 DrawLocation_TargetSlices = new Vector2(402, 382);

        public Vector2 DrawLocation_AlertLightOne = new Vector2(405, 430);
        public Vector2 DrawLocation_AlertLightTwo = new Vector2(435, 430);
        public Vector2 DrawLocation_AlertLightThree = new Vector2(465, 430);

        public Vector2 Offset_Up = new Vector2(0, 0);
        public Vector2 Offset_Down = new Vector2(0, 99);

        public float offsetT = 0.0f;

        public float popUpSpeed = 1.4f;
        public float popDownSpeed = 1.4f;

        FlashingElement AlertLightOne_Flash = new FlashingElement(alertFlashSpeed, false, FlashingElement.FlashingElement_OperationType.FlashingElement_OperationType_StayOff);
        FlashingElement AlertLightTwo_Flash = new FlashingElement(alertFlashSpeed, false, FlashingElement.FlashingElement_OperationType.FlashingElement_OperationType_StayOff);
        FlashingElement AlertLightThree_Flash = new FlashingElement(alertFlashSpeed, false, FlashingElement.FlashingElement_OperationType.FlashingElement_OperationType_StayOff);

        int currentAlertLevel = 0;

        public const float alertFlashSpeed = 0.50f;
        public const float alertFlashLength = 3.0f;

        public void AddScore(int scoreToAddOn)
        {
            if (scoreToAddOn <= 0)
                return;

            board.GetMedia().StartMoneyLoopSound();

            if (score_UpdateTimeLeft <= 0.0f)
            {
                score_UpdateTimeLeft = score_UpdateTimeMax;
            }

            else
            {
                score_UpdateTimeLeft += score_UpdateTimeMax;
            }

            scoreLeftToAdd += (UInt64)scoreToAddOn;
            scoreToAddPerSecond = (float)scoreLeftToAdd / score_UpdateTimeMax;
        }



        public int GetCurrentAlertLevel()
        {
            return currentAlertLevel;
        }

        public void SetCurrentAlertLevel(int level)
        {
            //can't be greater than 3 or less than zero
            if (level > 3)
                level = 3;
            if (level < 0)
                level = 0;
            //first, check delta between current and new
            if (currentAlertLevel == level)
                return; //no change

            else if (currentAlertLevel > level)
            {
                //flash only the lowest level
                if (level == 0)
                {
                    AlertLightOne_Flash.Reset(alertFlashSpeed, false, FlashingElement.FlashingElement_OperationType.FlashingElement_OperationType_StayOff);
                    AlertLightTwo_Flash.Reset(alertFlashSpeed, false, FlashingElement.FlashingElement_OperationType.FlashingElement_OperationType_StayOff);
                    AlertLightThree_Flash.Reset(alertFlashSpeed, false, FlashingElement.FlashingElement_OperationType.FlashingElement_OperationType_StayOff);
                }
                else if (level == 1)
                {
                    AlertLightOne_Flash.Reset(alertFlashSpeed, false, FlashingElement.FlashingElement_OperationType.FlashingElement_OperationType_Normal);
                    AlertLightOne_Flash.ChangeToModeAfter(alertFlashLength, FlashingElement.FlashingElement_OperationType.FlashingElement_OperationType_StayOn);

                    AlertLightTwo_Flash.Reset(alertFlashSpeed, false, FlashingElement.FlashingElement_OperationType.FlashingElement_OperationType_StayOff);
                    AlertLightThree_Flash.Reset(alertFlashSpeed, false, FlashingElement.FlashingElement_OperationType.FlashingElement_OperationType_StayOff);
                }
                else if (level == 2)
                {
                    AlertLightOne_Flash.Reset(alertFlashSpeed, false, FlashingElement.FlashingElement_OperationType.FlashingElement_OperationType_StayOn);

                    AlertLightTwo_Flash.Reset(alertFlashSpeed, false, FlashingElement.FlashingElement_OperationType.FlashingElement_OperationType_Normal);
                    AlertLightTwo_Flash.ChangeToModeAfter(alertFlashLength, FlashingElement.FlashingElement_OperationType.FlashingElement_OperationType_StayOn);

                    AlertLightThree_Flash.Reset(alertFlashSpeed, false, FlashingElement.FlashingElement_OperationType.FlashingElement_OperationType_StayOff);
                }

                else
                {
                    AlertLightOne_Flash.Reset(alertFlashSpeed, false, FlashingElement.FlashingElement_OperationType.FlashingElement_OperationType_StayOn);
                    AlertLightTwo_Flash.Reset(alertFlashSpeed, false, FlashingElement.FlashingElement_OperationType.FlashingElement_OperationType_StayOn);

                    AlertLightThree_Flash.Reset(alertFlashSpeed, false, FlashingElement.FlashingElement_OperationType.FlashingElement_OperationType_Normal);
                    AlertLightThree_Flash.ChangeToModeAfter(alertFlashLength, FlashingElement.FlashingElement_OperationType.FlashingElement_OperationType_StayOn);
                }
            }

            else
            {
                //don't touch other levels, let them do what they do.
                if (level == 1)
                {
                    AlertLightOne_Flash.Reset(alertFlashSpeed, false, FlashingElement.FlashingElement_OperationType.FlashingElement_OperationType_Normal);
                    AlertLightOne_Flash.ChangeToModeAfter(alertFlashLength, FlashingElement.FlashingElement_OperationType.FlashingElement_OperationType_StayOn);
                }
                else if (level == 2)
                {
                    AlertLightTwo_Flash.Reset(alertFlashSpeed, false, FlashingElement.FlashingElement_OperationType.FlashingElement_OperationType_Normal);
                    AlertLightTwo_Flash.ChangeToModeAfter(alertFlashLength, FlashingElement.FlashingElement_OperationType.FlashingElement_OperationType_StayOn);
                }

                else
                {
                    AlertLightThree_Flash.Reset(alertFlashSpeed, false, FlashingElement.FlashingElement_OperationType.FlashingElement_OperationType_Normal);
                    AlertLightThree_Flash.ChangeToModeAfter(alertFlashLength, FlashingElement.FlashingElement_OperationType.FlashingElement_OperationType_StayOn);
                }
            }


            //and finally - set it.
            currentAlertLevel = level;
        }

        public void DrawSelf(HackNodeGameBoardMedia drawing, SpriteBatch spriteBatch, GraphicsDevice GraphicsDevice, HackGameAgent_Player player)
        {
            Vector2 offset = GetCurrentOffset();

            spriteBatch.Draw(drawing.LowerUI_Shell, DrawLocation_Shell + offset, Color.White);
            //spriteBatch.Draw(drawing.LowerUI_AlertShell, DrawLocation_AlertShell + offset, Color.White);
            //spriteBatch.Draw(drawing.LowerUI_BonusShell, DrawLocation_BonusShell + offset, Color.White);

            //Draw the numbers
            spriteBatch.DrawString(drawing.LowerUI_Score_Font, scorestring.outputstring, DrawLocation_ScoreText + offset, Color.Red);

            //Draw the alert lights
            //spriteBatch.Draw(AlertLightOne_Flash.IsOn() ? drawing.LowerUI_Alert_Light_On : drawing.LowerUI_Alert_Light_Off, DrawLocation_AlertLightOne + offset, Color.White);
            //spriteBatch.Draw(AlertLightTwo_Flash.IsOn() ? drawing.LowerUI_Alert_Light_On : drawing.LowerUI_Alert_Light_Off, DrawLocation_AlertLightTwo + offset, Color.White);
            //spriteBatch.Draw(AlertLightThree_Flash.IsOn() ? drawing.LowerUI_Alert_Light_On : drawing.LowerUI_Alert_Light_Off, DrawLocation_AlertLightThree + offset, Color.White);

            //Draw the target slice
            float currentTargetCompletion = 0.0f;
            if (board != null && board.GetTargetCashToExit() > 0)
            {
                currentTargetCompletion = (float)((double)(GetScore()) / (double)(board.GetTargetCashToExit()));
            }

            Texture2D chosenTargetSprite;
            if (currentTargetCompletion >= 1.0f)
            {
                chosenTargetSprite = drawing.TargetSlice_100_Percent;
            }
            else if (currentTargetCompletion >= 0.75f)
            {
                chosenTargetSprite = drawing.TargetSlice_75_Percent;
            }
            else if (currentTargetCompletion >= 0.50f)
            {
                chosenTargetSprite = drawing.TargetSlice_50_Percent;
            }
            else if (currentTargetCompletion >= 0.25f)
            {
                chosenTargetSprite = drawing.TargetSlice_25_Percent;
            }
            else
            {
                chosenTargetSprite = drawing.TargetSlice_0_Percent;
            }

            if (chosenTargetSprite != null)
            {
                spriteBatch.Draw(chosenTargetSprite, DrawLocation_TargetSlices + offset, Color.White);
            }

        }

        public void UpdateSelf(GameTime time)
        {
            /*
            switch (state)
            {
                case HackGameBoard_Scoring_DisplayState.HackGameBoard_Scoring_DisplayState_Up:
                    currentStayUpTime -= (float)time.ElapsedGameTime.TotalSeconds;
                    if (currentStayUpTime <= 0.0f)
                    {
                        offsetT = 0.0f;
                        state = HackGameBoard_Scoring_DisplayState.HackGameBoard_Scoring_DisplayState_PoppingDown;
                    }
                    break;
                case HackGameBoard_Scoring_DisplayState.HackGameBoard_Scoring_DisplayState_PoppingDown:
                    offsetT += popDownSpeed * (float)time.ElapsedGameTime.TotalSeconds;
                    if (offsetT >= 1.0f)
                    {
                        state = HackGameBoard_Scoring_DisplayState.HackGameBoard_Scoring_DisplayState_Down;
                    }
                    break;
                case HackGameBoard_Scoring_DisplayState.HackGameBoard_Scoring_DisplayState_PoppingUp:
                    offsetT += popUpSpeed * (float)time.ElapsedGameTime.TotalSeconds;
                    if (offsetT >= 1.0f)
                    {
                        state = HackGameBoard_Scoring_DisplayState.HackGameBoard_Scoring_DisplayState_Up;
                    }
                    break;
             
            }*/

            //update any score left to add
            if (scoreLeftToAdd > 0)
            {
                score_UpdateTimeLeft -= (float)time.ElapsedGameTime.TotalSeconds;
                UInt64 scoreDelta = (UInt64)MathHelper.Min((scoreToAddPerSecond * (float)time.ElapsedGameTime.TotalSeconds), (float)scoreLeftToAdd);
                score += scoreDelta;
                scorestring.UpdateString(score);
                scoreLeftToAdd -= scoreDelta;
                if (scoreLeftToAdd <= 0)
                {
                    scoreLeftToAdd = 0;
                    score_UpdateTimeLeft = 0.0f;
                    //stop the sound
                    board.GetMedia().StopMoneyLoopSound();
                }
            }

            //update all blinkenlights
            AlertLightOne_Flash.Update(time);
            AlertLightTwo_Flash.Update(time);
            AlertLightThree_Flash.Update(time);
        }

        public Vector2 GetCurrentOffset()
        {
            /*
            Vector2 returnVec = new Vector2();
            switch (state)
            {
                case HackGameBoard_Scoring_DisplayState.HackGameBoard_Scoring_DisplayState_Up:
                    returnVec = Offset_Up;
                    break;
                case HackGameBoard_Scoring_DisplayState.HackGameBoard_Scoring_DisplayState_Down:
                    returnVec = Offset_Down;
                    break;
                case HackGameBoard_Scoring_DisplayState.HackGameBoard_Scoring_DisplayState_PoppingDown:
                    returnVec.X = Offset_Up.X + (Offset_Down.X - Offset_Up.X) * offsetT;
                    returnVec.Y = Offset_Up.Y + (Offset_Down.Y - Offset_Up.Y) * offsetT;
                    break;
                case HackGameBoard_Scoring_DisplayState.HackGameBoard_Scoring_DisplayState_PoppingUp:
                    returnVec.X = Offset_Down.X + (Offset_Up.X - Offset_Down.X) * offsetT;
                    returnVec.Y = Offset_Down.Y + (Offset_Up.Y - Offset_Down.Y) * offsetT;
                    break;
            }*/


            Vector2 returnVec = Offset_Up;
            //BUGBUG: temporary addition hack to correct for portrait mode
            returnVec.Y += 320.0f;

            return returnVec;
        }

    }
}
