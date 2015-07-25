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
using GameStateManagement;

namespace HackPrototype
{
    class HackGameBoard_CollapseController
    {
        StringBuilder timerString = new StringBuilder();
        HackGameTimer collapseTimer;
        Point boardcenter;

        bool lerpingIn = false;
        bool lerpingOut = false;
        bool firingFlash = false;
        bool playActivationSound = false;

        const float lerpInOutTime = 0.5f;
        
        bool collapsingFinal = false;
        HackGameTimer collapseFinalTimer = new HackGameTimer(1.0f);

        HackGameForwardLerpDrawHelper lerpInOut = new HackGameForwardLerpDrawHelper(0,0,0,0,Color.White,Color.White,0,Vector2.Zero,Vector2.Zero,0);
        HackGameForwardLerpDrawHelper lerpFlash = new HackGameForwardLerpDrawHelper(0, 0, 0, 0, Color.White, Color.White, 0, Vector2.Zero, Vector2.Zero, 0);
        HackGameTimer lerpFlashTimer = new HackGameTimer(0);

        List<HackGameBoard_CollapseTarget> targets = new List<HackGameBoard_CollapseTarget>();
        int max_target_levels;
        int max_targets_value;
        int current_targets_value;
        float timePerTargetLevel = 0;
        float currentTargetTimer = 0;
        List<HackGameBoard_CollapseTarget> targetsToFire = new List<HackGameBoard_CollapseTarget>();

        float setTime;

        bool active = false;
        bool frozen = false;
        bool isCollapsed = false;

        bool stopLoopSound = false;

        public HackGameBoard_CollapseController(float time, HackGameBoard board, Point center)
        {
            setTime = time;

            PopulateTargets(board, center);
        }

        private void PopulateTargets(HackGameBoard board, Point center)
        {
            PathFinder pf = new PathFinder();
            pf.Initialize(board);
            boardcenter = center;

            for (int i = 0; i < board.GetGameBoardSize(); i++)
            {
                for(int k = 0; k < board.GetGameBoardSize(); k++)
                {
                    if (board.board[i, k].type == HackGameBoardElementBaseType.HackGameBoardElementBaseType_Node)
                    {
                        //pathfind to the center.
                        pf.Reset(new Point(k, i), center);
                        bool success = pf.SearchPath();
                        if (success == true)
                        {
                            HackGameBoard_CollapseTarget target = new HackGameBoard_CollapseTarget();
                            target.location = new Point(k, i);
                            target.level = pf.FinalPath().Count - 1;
                            target.delay = (float)board.r.NextDouble();

                            targets.Add(target);
                        }
                    }
                }
            }

            int maxtargets = 0;
            max_target_levels = 0;
            targets.Sort(CompareTargetsByLevel);
            foreach (HackGameBoard_CollapseTarget target in targets)
            {
                if (target.level > maxtargets)
                {
                    maxtargets = target.level;
                    max_target_levels++;
                }
            }
            max_targets_value = maxtargets;


        }

        private static int CompareTargetsByLevel(HackGameBoard_CollapseTarget x, HackGameBoard_CollapseTarget y)
        {
            if (x.level > y.level)
            {
                return 1;
            }
            if (y.level > x.level)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }
        

        private void UpdateTimerString()
        {
            timerString.Remove(0, timerString.Length);

            if (collapseTimer.GetLifeTimeLeft() <= 0)
            {
                timerString.Append("00.0");
            }

            else
            {
                decimal timeleft = (decimal)collapseTimer.GetLifeTimeLeft();
                decimal rounddown = (decimal.Floor(timeleft));

                if (collapseTimer.GetLifeTimeLeft() < 10.0f)
                {
                    timerString.Append('0');
                }

                timerString.Append((int)(rounddown));
                timerString.Append('.');
                timerString.Append((int)(decimal.Floor((timeleft - rounddown) * 10)));
            }
        }

        public void Freeze()
        {
            frozen = true;
        }

        public void Unfreeze()
        {
            frozen = false;
        }

        public bool HasCollapsed()
        {
            return isCollapsed;
        }

        public bool IsActive()
        {
            return active;
        }

        public void Activate(HackGameBoard board)
        {
            if (!active)
            {
                LerpIn();
                playActivationSound = true;
                board.ticker.SetOverride("..NETWORK COLLAPSE..EXIT IMMEDIATELY..NETWORK COLLAPSE..EXIT IMMEDIATELY..");
            }
            active = true;
            ResetTargets();
            decimal timeleft = (decimal)collapseTimer.GetLifeTimeLeft();
            decimal rounddown = (decimal.Floor(timeleft));
            decimal splitsecond = decimal.Floor((timeleft - rounddown) * 10);

            if ((float)splitsecond == 0)
            {
                lerpFlashTimer.Reset(1.0f);
            }
            else
            {
                lerpFlashTimer.Reset((float)splitsecond / 10.0f);
            }

            if (collapseTimer == null)
            {
                collapseTimer = new HackGameTimer(setTime);
                UpdateTimerString();
            }

            //apply bonus
            float nodeValueMultiplier = 2.0f;
            if (board.IsBonusRound())
            {
                nodeValueMultiplier = 4.0f;
            }
            board.ApplyMultiplier(nodeValueMultiplier);
           
            //this needs to be an event with a delay
            
            int multInt = (int)nodeValueMultiplier;
            string multString = "Nodes " + multInt + "x Value!";
            board.AddNewTextEvent(multString, 2.0f);
        }

        public void Deactivate(HackGameBoard board)
        {
            if (active)
            {
                LerpOut();
            }
            board.ticker.ClearOverride();
            active = false;
            stopLoopSound = true;
        }

        private void LerpIn()
        {
            if (!lerpingIn)
            {
                lerpingIn = true;
                if (lerpingOut)
                {
                    lerpInOut.Reset(lerpInOutTime - lerpInOut.GetLifeTimeLeft(), lerpInOut.CurrentScale(), 1.0f, lerpInOutTime - lerpInOut.GetLifeTimeLeft(), Color.White, Color.White, lerpInOutTime, Vector2.Zero, Vector2.Zero, lerpInOutTime);
                    lerpingOut = false;
                }
                else
                {
                    lerpInOut.Reset(lerpInOutTime, 0, 1.0f, lerpInOutTime, Color.White, Color.White, lerpInOutTime, Vector2.Zero, Vector2.Zero, lerpInOutTime);
                }
            }
        }

        private void LerpOut()
        {
            if (!lerpingOut)
            {
                lerpingOut = true;
                if (lerpingIn)
                {
                    lerpInOut.Reset(lerpInOutTime - lerpInOut.GetLifeTimeLeft(), lerpInOut.CurrentScale(), 0.0f, lerpInOutTime - lerpInOut.GetLifeTimeLeft(), Color.White, Color.White, lerpInOutTime, Vector2.Zero, Vector2.Zero, lerpInOutTime);
                    lerpingOut = false;
                }
                else
                {
                    lerpInOut.Reset(lerpInOutTime, 1.0f, 0, lerpInOutTime, Color.White, Color.White, lerpInOutTime, Vector2.Zero, Vector2.Zero, lerpInOutTime);
                }
            }
        }

        private void FireFlash(float nextFlashTime)
        {
            lerpFlash.Reset(0.5f, 1.0f, 1.0f, 0.5f, new Color(0.7f, 0.3f, 0.3f, 0.3f), new Color(0.3f, 0.3f, 0.3f, 0.3f), 0.5f, Vector2.Zero, Vector2.Zero, 0.5f);
            lerpFlashTimer.Reset(nextFlashTime);
            firingFlash = true;
        }

        public void SetTimer(float newtimer)
        {
            setTime = newtimer;
            ResetTargets();
            collapseTimer = new HackGameTimer(setTime);
            UpdateTimerString();
        }

        private void ResetTargets()
        {
            current_targets_value = max_targets_value;
            timePerTargetLevel = (setTime - 10.0f) / max_target_levels; //link up the final collapse time to ten seconds after the last layer has been placed.
            currentTargetTimer = 0; //fire right away
        }

        public void DrawSelf(HackNodeGameBoardMedia drawing, SpriteBatch spriteBatch, GraphicsDevice GraphicsDevice, HackGameAgent_Player player)
        {
            if (active || lerpingOut)
            {
                //standard drawing
                Vector2 fontSize = drawing.Collapse_GiantNumbers_Font.MeasureString(timerString);
                Vector2 finalPos = new Vector2(GraphicsDevice.Viewport.Width / 2 - fontSize.X / 2, GraphicsDevice.Viewport.Height / 2);
                Vector2 finalScale = Vector2.One;

                if (lerpingIn || lerpingOut)
                {
                    finalScale.Y = lerpInOut.CurrentScale();
                }

                if (firingFlash)
                {
                    firingFlash = false;
                    drawing.TimerTickSound.Play();
                }

                if (playActivationSound)
                {
                    playActivationSound = false;
                    drawing.StartWarningLoopSound();
                }

                Color finalColor = lerpFlash.IsAlive() ? lerpFlash.CurrentColor() : new Color(0.3f, 0.3f, 0.3f, 0.3f);
                if(collapsingFinal == true)
                {
                    Vector4 finalColorExtra = finalColor.ToVector4();
                    finalColorExtra.X = MathHelper.Clamp(finalColorExtra.X + (1.0f - collapseFinalTimer.GetLifeTimeLeft()), 0, 1.0f);
                    finalColorExtra.W = MathHelper.Clamp(finalColorExtra.W + (1.0f - collapseFinalTimer.GetLifeTimeLeft()), 0, 1.0f);
                    finalColor = new Color(finalColorExtra);
                }

                spriteBatch.DrawString(drawing.Collapse_GiantNumbers_Font, timerString, finalPos, finalColor, 0, new Vector2(0, fontSize.Y / 2), finalScale, SpriteEffects.None, 0);
            }
            if(stopLoopSound == true)
            {
                drawing.StopWarningLoopSound();
                stopLoopSound = false;
            }
        }
        /*
        public void UpdateSelf(HackGameBoard board, GameTime time)
        {
            if (active && !frozen)
            {
                collapseTimer.Update(time);
                UpdateTimerString();

                lerpFlash.Update(time);

                lerpFlashTimer.Update(time);
                if (!lerpFlashTimer.IsAlive())
                {
                    FireFlash(1.0f);
                }

                if (lerpingIn || lerpingOut)
                {
                    lerpInOut.Update(time);
                    if (!lerpInOut.IsAlive())
                    {
                        lerpingIn = false;
                        lerpingOut = false;
                    }
                }

                if (!collapseTimer.IsAlive())
                {
                    CollapseLevel();
                }
                else
                {
                    currentTargetTimer -= (float)time.ElapsedGameTime.TotalSeconds;
                    if (currentTargetTimer <= 0)
                    {
                        //time to fire off another set of collapsers
                        FireCollapsers();
                        currentTargetTimer = timePerTargetLevel;
                        current_targets_value--;
                    }

                    //fire ready targets

                    List<HackGameBoard_CollapseTarget> dead = new List<HackGameBoard_CollapseTarget>();
                    for (int i = 0; i < targetsToFire.Count; i++ )
                    {
                        targetsToFire[i].delay -= (float)time.ElapsedGameTime.TotalSeconds;
                        if (targetsToFire[i].delay <= 0)
                        {
                            HackGameAgent_Collapser collapser = new HackGameAgent_Collapser(board, MathHelper.Lerp(05.0f, 10.0f,(float)board.r.NextDouble()), targetsToFire[i].location);
                            collapser.SpawnIn();
                            board.AddAgent(collapser);
                            dead.Add(targetsToFire[i]);
                        }
                    }

                    for (int i = 0; i < dead.Count; i++)
                    {
                        targetsToFire.Remove(dead[i]);
                    }
                }
            }
        }
        */


        public void UpdateSelf(HackGameBoard board, GameTime time)
        {
            if (collapsingFinal == true)
            {
                collapseFinalTimer.Update(time);
                if (!collapseFinalTimer.IsAlive())
                {
                    FinalCollapse();
                }
            }

            lerpInOut.Update(time);

            if (lerpingIn || lerpingOut)
            {
                if (!lerpInOut.IsAlive())
                {
                    lerpingIn = false;
                    lerpingOut = false;
                }
            }

            if (active && !frozen && !lerpingIn)
            {
                collapseTimer.Update(time);
                UpdateTimerString();

                lerpFlash.Update(time);

                lerpFlashTimer.Update(time);
                if (!lerpFlashTimer.IsAlive())
                {
                    FireFlash(1.0f);
                }

                UpdateCollapsers(board, time);
            }

        }

        private void UpdateCollapsers(HackGameBoard board, GameTime time)
        {
            if (!collapseTimer.IsAlive())
            {
                CollapseLevel(board);
            }
            else
            {
                currentTargetTimer -= (float)time.ElapsedGameTime.TotalSeconds;
                if (currentTargetTimer <= 0)
                {
                    //time to fire off another set of collapsers
                    FireCollapsers();
                    currentTargetTimer = timePerTargetLevel;
                    current_targets_value--;
                }

                //fire ready targets

                List<HackGameBoard_CollapseTarget> dead = new List<HackGameBoard_CollapseTarget>();
                for (int i = 0; i < targetsToFire.Count; i++)
                {
                    targetsToFire[i].delay -= (float)time.ElapsedGameTime.TotalSeconds;
                    if (targetsToFire[i].delay <= 0)
                    {
                        HackGameAgent_Collapser collapser = new HackGameAgent_Collapser(board, MathHelper.Lerp(05.0f, 10.0f, (float)board.r.NextDouble()), targetsToFire[i].location);
                        collapser.SpawnIn();
                        board.AddAgent(collapser);
                        dead.Add(targetsToFire[i]);
                    }
                }

                for (int i = 0; i < dead.Count; i++)
                {
                    targetsToFire.Remove(dead[i]);
                }
            }
        }

        private void FireCollapsers()
        {
            //we're at level x. get to level y which is either the next highest level that has at least one representative in the list or is => max
            int i;
            bool found = false;
            for (i = current_targets_value; i > 0; i--)
            {
                foreach (HackGameBoard_CollapseTarget target in targets)
                {
                    if (target.level == i)
                    {
                        found = true;
                        //fire this!
                        FireCollapserItem(target);
                    }
                }
                if (found)
                {
                    current_targets_value = i;
                    return;
                }
            }
                //we dropped off the end of the list.
                //bummer.

        }

        private void FireCollapserItem(HackGameBoard_CollapseTarget target)
        {
            targetsToFire.Add(target);
        }

        private void CollapseLevel(HackGameBoard board)
        {
            
            HackGameAgent_Collapser collapser = new HackGameAgent_Collapser(board, 1.0f, boardcenter);
            collapser.SpawnIn();
            board.AddAgent(collapser);

            collapsingFinal = true;

            Freeze();
        }

        private void FinalCollapse()
        {
            isCollapsed = true;
            stopLoopSound = true;
        }
    }

    public class HackGameBoard_CollapseTarget
    {
        public Point location;
        public int level;
        public float delay;
        public float timer;
    }
}
