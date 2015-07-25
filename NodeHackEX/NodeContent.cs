using System;
using System.Collections.Generic;

using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace HackPrototype
{
    abstract class HackGameBoardNodeContent
    {
        public HackGameBoardNodeContent() { }

        abstract public void DrawSelf(SpriteBatch sb, HackGameBoardElement_Node node, HackNodeGameBoardMedia gameboarddrawing, Vector2 Nodedrawpos, float zoom);
        abstract public void UpdateState(GameTime time, HackGameBoard board, HackGameBoardElement_Node node);
        abstract public void OnAgentEnter(HackGameAgent agent, HackGameBoard board, HackGameBoardElement_Node node);
        abstract public void OnAgentStay(HackGameAgent agent, HackGameBoard board, HackGameBoardElement_Node node);
        abstract public void OnAgentExit(HackGameAgent agent, HackGameBoard board, HackGameBoardElement_Node node);
    }

    class HackGameBoardNodeContent_Loot : HackGameBoardNodeContent
    {
        /*
        public enum HackGameBoardNodeContent_Loot_Level
        {
            HackGameBoardNodeContent_Loot_Level_One,
            HackGameBoardNodeContent_Loot_Level_Two,
            HackGameBoardNodeContent_Loot_Level_Three,
            HackGameBoardNodeContent_Loot_Level_Four
        };
        */
        public enum HackGameBoardNodeContent_Loot_Color
        {
            HackGameBoardNodeContent_Loot_Color_Yellow,
            HackGameBoardNodeContent_Loot_Color_Blue,
            HackGameBoardNodeContent_Loot_Color_Black
        };
        

        //string currentValueString; //the string representation of what it's worth
        float valueMultiplier = 1.0f; //what it's worth due to any multipliers
        int baseValue; //what it's normally worth
        int drawOffsetX; //offset of the draw to center it lengthwise

        HackGameForwardLerpDrawHelper lerp;
        CurrencyStringer valuestring = new CurrencyStringer(0);

        HackGameReversibleLerpDrawHelper pulseEffect = new HackGameReversibleLerpDrawHelper(0.3f, 0.02f, 0.9f, 1.0f, Color.White, Color.White, Vector2.Zero, Vector2.Zero);


        bool Empty = false;
        HackGameAgent_Player PlayerHacking = null;
        StringBuilder hackbackgroundstringbuilder;
        float HackTimerRemaining = 0.0f;
        float HackTimerMax = 0.0f;
        int HackBackgroundMaxDrawDots = 40;
        float HackBackgroundTextUpdateTimer = 0.0f;
        float HackBackgroundTextUpdateTimerMax = 0.1f;
        HackGameBoardNodeContent_Loot_Color LootColor = HackGameBoardNodeContent_Loot_Color.HackGameBoardNodeContent_Loot_Color_Blue;
        //HackGameBoardNodeContent_Loot_Level LootLevel = HackGameBoardNodeContent_Loot_Level.HackGameBoardNodeContent_Loot_Level_One;

        /*
        public HackGameBoardNodeContent_Loot_Level GetLevel()
        {
            return LootLevel;
        }
        */
        public HackGameBoardNodeContent_Loot_Color GetColor()
        {
            return LootColor;
        }

        public int GetFinalValue()
        {
            return (int)((float)baseValue * valueMultiplier);
        }

        public void ApplyMultiplier(float multiplier)
        {
            if (valueMultiplier != multiplier)
            {
                valueMultiplier = multiplier;
                UpdateValueString();

                //set as "new" so it flashes a little
                FlashNew();
            }
        }

        private void FlashNew()
        {
            lerp = new HackGameForwardLerpDrawHelper(3.0f, 2.0f, 1.0f, 2.0f, Color.Yellow, Color.White, 2.0f, new Vector2(0, 5.0f), Vector2.Zero, 2.0f);
        }

        private void UpdateValueString()
        {
            valuestring.UpdateString((UInt64)GetFinalValue());
        }

        public HackGameBoardNodeContent_Loot(HackGameBoardNodeContent_Loot_Color color) : base()
        {
            LootColor = color;

            switch (color)
            {
                case HackGameBoardNodeContent_Loot_Color.HackGameBoardNodeContent_Loot_Color_Blue:
                    baseValue = 5000;
                    HackTimerMax = 2.0f;
                    break;
                case HackGameBoardNodeContent_Loot_Color.HackGameBoardNodeContent_Loot_Color_Yellow:
                    baseValue = 25000;
                    HackTimerMax = 4.0f;
                    break;
                case HackGameBoardNodeContent_Loot_Color.HackGameBoardNodeContent_Loot_Color_Black:
                    baseValue = 100000;
                    HackTimerMax = 8.0f;
                    break;
            }

            HackTimerRemaining = HackTimerMax;

            hackbackgroundstringbuilder = new StringBuilder(HackBackgroundMaxDrawDots);

            UpdateValueString();
            FlashNew();
        }

        public override void DrawSelf(SpriteBatch sb, HackGameBoardElement_Node node, HackNodeGameBoardMedia gameboarddrawing, Vector2 Nodedrawpos, float zoom)
        {
            Color drawColor = Color.White;
            Texture2D tex = null;

            if (Empty)
            {
                drawColor = new Color(.05f, .05f, .05f);
            }


            switch(LootColor)
            {
                case HackGameBoardNodeContent_Loot_Color.HackGameBoardNodeContent_Loot_Color_Blue:
                    tex = gameboarddrawing.Loot_Blue_Texture;
                    break;

                case HackGameBoardNodeContent_Loot_Color.HackGameBoardNodeContent_Loot_Color_Black:
                    tex = gameboarddrawing.Loot_Black_Texture;
                    break;

                case HackGameBoardNodeContent_Loot_Color.HackGameBoardNodeContent_Loot_Color_Yellow:
                    tex = gameboarddrawing.Loot_Yellow_Texture;
                    break;
        }

            sb.Draw(tex, Nodedrawpos, null, drawColor, 0, Vector2.Zero, zoom, SpriteEffects.None, 0); 

            //draw the 2x/4x
            if (valueMultiplier == 2.0f)
            {
                sb.Draw(gameboarddrawing.Loot_2x_Score_Texture, Nodedrawpos, null, drawColor, 0, Vector2.Zero, zoom * pulseEffect.CurrentScale(), SpriteEffects.None, 0);
            }

            else if (valueMultiplier == 4.0f)
            {
                sb.Draw(gameboarddrawing.Loot_4x_Score_Texture, Nodedrawpos, null, drawColor, 0, Vector2.Zero, zoom * pulseEffect.CurrentScale(), SpriteEffects.None, 0);
            }

            //draw the timing ring
            //0-10% - 0
            //11-35% - 1
            //36-60% - 2
            //61-85% - 3
            //86-100% - 4
            if (!Empty && PlayerHacking != null)
            {
                if (PlayerHacking.IsHacking() == true)
                {
                Texture2D timingTex = null;

                float pctTiming = HackTimerMax != 0.0f ? 1.0f - (HackTimerRemaining / HackTimerMax) : 0.0f;
                {
                    if (pctTiming <= .1f)
                    {
                        timingTex = gameboarddrawing.TimingRingEmpty;
                    }
                    else if (pctTiming > .1f && pctTiming <= .35f)
                    {
                        timingTex = gameboarddrawing.TimingRing1_4;
                    }
                    else if (pctTiming > .35f && pctTiming <= .60f)
                    {
                        timingTex = gameboarddrawing.TimingRing2_4;
                    }
                    else if (pctTiming > .60f && pctTiming <= .85f)
                    {
                        timingTex = gameboarddrawing.TimingRing3_4;
                    }
                    else if (pctTiming > .85f)
                    {
                        timingTex = gameboarddrawing.TimingRingComplete;
                    }
                }

                sb.Draw(timingTex, Nodedrawpos, null, Color.White, 0, Vector2.Zero, zoom, SpriteEffects.None, 0);

                }
            }


            //draw the $ amount
            if (!Empty)
            {

                drawOffsetX = (int)(HackGameBoard.elementSize * zoom / 2.0f - (gameboarddrawing.LootAmount_Font.MeasureString(valuestring.outputstring).X) * zoom / 2.0f);
                 

                if (lerp != null && lerp.IsAlive())
                {
                    drawOffsetX = (int)(HackGameBoard.elementSize / 2.0f - (gameboarddrawing.LootAmount_Font.MeasureString(valuestring.outputstring).X * lerp.CurrentScale()) / 2.0f);
                    sb.DrawString(gameboarddrawing.LootAmount_Font, valuestring.outputstring, Nodedrawpos + new Vector2(drawOffsetX, -20.0f * zoom) + lerp.CurrentPosition(), lerp.CurrentColor(), 0, Vector2.Zero, lerp.CurrentScale() * zoom, SpriteEffects.None, 0);
                }

                else
                {
                    sb.DrawString(gameboarddrawing.LootAmount_Font, valuestring.outputstring, Nodedrawpos + new Vector2(drawOffsetX, -20.0f * zoom), Color.White, 0, Vector2.Zero, zoom, SpriteEffects.None, 1);
                }
            }

        }

        public override void OnAgentEnter(HackGameAgent agent, HackGameBoard board, HackGameBoardElement_Node node)
        {

        }

        public override void OnAgentStay(HackGameAgent agent, HackGameBoard board, HackGameBoardElement_Node node)
        {
            if (!Empty && agent is HackGameAgent_Player)
            {
                PlayerHacking = (HackGameAgent_Player)agent;
                PlayerHacking.SetHacking(true);
                //our first time in!
                board.GetMedia().StartHackLoopSound();
                HackBackgroundTextUpdateTimer = HackBackgroundTextUpdateTimerMax;
            }
        }

        public override void OnAgentExit(HackGameAgent agent, HackGameBoard board, HackGameBoardElement_Node node)
        {
            if (agent is HackGameAgent_Player)
            {
                ((HackGameAgent_Player)(agent)).SetHacking(false);
                PlayerHacking = null;
                board.GetMedia().StopHackLoopSound();
                //reset timer
                HackTimerRemaining = HackTimerMax;
                HackBackgroundTextUpdateTimer = 0.0f;
            }
        }

        public override void UpdateState(GameTime time, HackGameBoard board, HackGameBoardElement_Node node)
        {
            if (lerp != null && lerp.IsAlive())
            {
                lerp.Update(time);
            }

            pulseEffect.Update(time);

            if (!Empty && PlayerHacking != null && PlayerHacking.IsHacking() == true)
            {
                HackTimerRemaining -= (float)time.ElapsedGameTime.TotalSeconds * board.GetSpeedUpFactor();
                float pctTiming = HackTimerMax != 0.0f ? 1.0f - (HackTimerRemaining / HackTimerMax) : 0.0f;
                board.SetHackLoopSoundAmountComplete(pctTiming);
                HackBackgroundTextUpdateTimer -= (float)time.ElapsedGameTime.TotalSeconds;
                if (HackBackgroundTextUpdateTimer <= 0)
                {
                    HackBackgroundTextUpdateTimer = HackBackgroundTextUpdateTimerMax;

                    //draw the right number of dots
                    pctTiming = HackTimerMax != 0.0f ? 1.0f - (HackTimerRemaining / HackTimerMax) : 0.0f;
                    int numdots = HackBackgroundMaxDrawDots - (int)((float)HackBackgroundMaxDrawDots * pctTiming);//blah
                    hackbackgroundstringbuilder.Remove(0, hackbackgroundstringbuilder.Length);
                    for(int i = 0; i < numdots; i++)
                    {
                        hackbackgroundstringbuilder.Append(board.r.NextDouble() > 0.5f ? '0' : '1');
                    }
                    board.AddBackgroundTextStandard(new StringBuilder(hackbackgroundstringbuilder.ToString()), 0); //have to create a copy in order for it to be unique in the list
                }
                if (HackTimerRemaining <= 0.0f)
                {
                    Empty = true;
                    PlayerHacking.SetHacking(false);
                    PlayerHacking.HackSuccess();
                    board.StopHackLoopSound();
                    board.PlayHackSuccessSound();
                    board.AwardNodeContents(this);
                    //board.PopUpScoreboard(4.0f);

                    board.AddBackgroundTextAward(new StringBuilder("CRACKER SUCCESSFUL"), 0);
                    board.AddBackgroundTextAward(new StringBuilder("CONTENTS UNENCRYPTED"), 0.25f);
                    board.AddBackgroundTextAward(new StringBuilder("DELETING TRACES"), 0.5f);
                }
            }
        }
    }


    class HackGameBoardNodeContent_Exit : HackGameBoardNodeContent
    {
        float timeToNextPing = 0.05f;
        const float timePerPing = 0.75f;
        bool StartPing_Draw = false;
        const float timePerFlash = 0.75f;
        float timeToNextFlash = 0.05f;
        bool StartFlash_Draw = false;
        HackGameForwardLerpDrawHelper lerp;
        bool active = true;

        public HackGameBoardNodeContent_Exit()
            : base()
        {
            FlashNew();
        }

        public override void DrawSelf(SpriteBatch sb, HackGameBoardElement_Node node, HackNodeGameBoardMedia gameboarddrawing, Vector2 Nodedrawpos, float zoom)
        {
            if (active)
            {
                if (StartPing_Draw)
                {
                    node.AddUIElement(gameboarddrawing.PingTexture, 1.5f, new Vector2(41.0f * zoom, 41.0f * zoom), new Vector2(41.0f * zoom, 41.0f * zoom), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(0.0f, 0, 0, 0), 0.2f, 2.0f, 0.0f);
                    //gameboarddrawing.PlayerPingSound.Play();
                    StartPing_Draw = false;
                }
            }

                if (StartFlash_Draw)
                {
                    FlashNew();
                    StartFlash_Draw = false;
                }
            

            Vector2 newdrawpos = new Vector2(Nodedrawpos.X + (HackGameBoard.elementSize * zoom / 2.0f - HackGameBoard.elementSize * zoom * lerp.CurrentScale() / 2.0f), Nodedrawpos.Y);

            sb.Draw(gameboarddrawing.ExitTexture, newdrawpos + lerp.CurrentPosition() * zoom, null, lerp.CurrentColor(), 0, Vector2.Zero, zoom * lerp.CurrentScale(), SpriteEffects.None, 0);
        }
        public override void UpdateState(GameTime time, HackGameBoard board, HackGameBoardElement_Node node)
        {
            float floatt = (float)time.ElapsedGameTime.TotalSeconds;

            timeToNextPing -= floatt;
            if (timeToNextPing <= 0)
            {
                StartPing_Draw = true;
                timeToNextPing = timePerPing;
            }

            timeToNextFlash -= floatt;
            if (timeToNextFlash <= 0)
            {
                StartFlash_Draw = true;
                timeToNextFlash = timePerFlash;
            }

            if (lerp != null)
            {
                lerp.Update(time);
            }
        }

        private void FlashNew()
        {
            lerp = new HackGameForwardLerpDrawHelper(0.66f, 0.9f, 1.0f, 0.66f, Color.Azure, Color.White, 0.66f, Vector2.Zero, Vector2.Zero, 2.0f);
        }

        public override void OnAgentEnter(HackGameAgent agent, HackGameBoard board, HackGameBoardElement_Node node)
        {
            if (agent is HackGameAgent_Player)
            {
                //YOU DID IT! EXIT!
                ((HackGameAgent_Player)agent).SetIsExiting();
                active = false;
            }
        }
        public override void OnAgentExit(HackGameAgent agent, HackGameBoard board, HackGameBoardElement_Node node) { }
        public override void OnAgentStay(HackGameAgent agent, HackGameBoard board, HackGameBoardElement_Node node) { }
    }

    class HackGameBoardNodeContent_Weapon_Multimissile : HackGameBoardNodeContent
    {
        bool fired = false;
        bool drawFire = false;

        HackGameReversibleLerpDrawHelper pulseEffect = new HackGameReversibleLerpDrawHelper(0.3f, 0.01f, 0.9f, 1.0f, Color.White, Color.White, Vector2.Zero, Vector2.Zero);

        public override void OnAgentEnter(HackGameAgent agent, HackGameBoard board, HackGameBoardElement_Node node)
        {
            if (!fired && agent is HackGameAgent_Player)
            {
                fired = true;
                drawFire = true;

                HackGameAgent_Projectile_Multimissile multi_north = new HackGameAgent_Projectile_Multimissile(board, HackGameAgent.MovementDirection.MovementDirection_North);
                board.AddAgent(multi_north);
                HackGameAgent_Projectile_Multimissile multi_south = new HackGameAgent_Projectile_Multimissile(board, HackGameAgent.MovementDirection.MovementDirection_South);
                board.AddAgent(multi_south);
                HackGameAgent_Projectile_Multimissile multi_east = new HackGameAgent_Projectile_Multimissile(board, HackGameAgent.MovementDirection.MovementDirection_East);
                board.AddAgent(multi_east);
                HackGameAgent_Projectile_Multimissile multi_west = new HackGameAgent_Projectile_Multimissile(board, HackGameAgent.MovementDirection.MovementDirection_West);
                board.AddAgent(multi_west);
            }
        }

        public override void OnAgentExit(HackGameAgent agent, HackGameBoard board, HackGameBoardElement_Node node) { }
        public override void OnAgentStay(HackGameAgent agent, HackGameBoard board, HackGameBoardElement_Node node) { }

        public override void DrawSelf(SpriteBatch sb, HackGameBoardElement_Node node, HackNodeGameBoardMedia gameboarddrawing, Vector2 Nodedrawpos, float zoom)
        {
            if (drawFire == true)
            {
                drawFire = false;
                node.AddUIElement(gameboarddrawing.WeaponPingTexture, 0.75f, new Vector2(41.0f * zoom, 41.0f * zoom), new Vector2(41.0f * zoom, 41.0f * zoom), new Color(1.0f, 0.4f, 0.0f, 0.0f), new Color(1.0f, 1.0f, 0.0f, 0.0f), 0.2f, 4.0f, 0.0f);
                gameboarddrawing.MissileLaunchSound.Play();
            }

            if (!fired)
            {
                sb.Draw(gameboarddrawing.Weapon_Multimissile_texture, Nodedrawpos + new Vector2(40.0f * zoom, 40.0f * zoom), null, Color.White, 0, new Vector2(40.0f, 40.0f), pulseEffect.CurrentScale() * zoom, SpriteEffects.None, 0);
            }
        }

        public override void UpdateState(GameTime time, HackGameBoard board, HackGameBoardElement_Node node)
        { pulseEffect.Update(time);  }
    }

    class HackGameBoardNodeContent_Weapon_Heatseeker : HackGameBoardNodeContent
    {
        bool fired = false;
        bool drawFire = false;

        HackGameReversibleLerpDrawHelper pulseEffect = new HackGameReversibleLerpDrawHelper(0.3f, 0.02f, 0.9f, 1.0f, Color.White, Color.White, Vector2.Zero, Vector2.Zero);

        public override void OnAgentEnter(HackGameAgent agent, HackGameBoard board, HackGameBoardElement_Node node)
        {
            if (!fired && agent is HackGameAgent_Player)
            {
                fired = true;
                drawFire = true;

                HackGameAgent_Projectile_Heatseeker heatseeker = new HackGameAgent_Projectile_Heatseeker(board);
                board.AddAgent(heatseeker);
            }
        }

        public override void OnAgentExit(HackGameAgent agent, HackGameBoard board, HackGameBoardElement_Node node) { }
        public override void OnAgentStay(HackGameAgent agent, HackGameBoard board, HackGameBoardElement_Node node) { }

        public override void DrawSelf(SpriteBatch sb, HackGameBoardElement_Node node, HackNodeGameBoardMedia gameboarddrawing, Vector2 Nodedrawpos, float zoom)
        {
            if (drawFire == true)
            {
                drawFire = false;
                node.AddUIElement(gameboarddrawing.WeaponPingTexture, 0.75f, new Vector2(41.0f * zoom, 41.0f * zoom), new Vector2(41.0f * zoom, 41.0f * zoom), new Color(1.0f, 0.4f, 0.0f, 0.0f), new Color(1.0f, 1.0f, 0.0f, 0.0f), 0.2f, 4.0f, 0.0f);
                gameboarddrawing.MissileLaunchSound.Play();
            }

            if (!fired)
            {
                sb.Draw(gameboarddrawing.Weapon_Heatseeker_texture, Nodedrawpos+ new Vector2(40.0f * zoom, 40.0f * zoom), null, Color.White, 0, new Vector2(40.0f, 40.0f), zoom * pulseEffect.CurrentScale(), SpriteEffects.None, 0);
            }
        }

        public override void UpdateState(GameTime time, HackGameBoard board, HackGameBoardElement_Node node)
        { pulseEffect.Update(time); }
     }

    class HackGameBoardNodeContent_Weapon_Decoy : HackGameBoardNodeContent
    {
        bool fired = false;
        bool drawFire = false;

        HackGameReversibleLerpDrawHelper pulseEffect = new HackGameReversibleLerpDrawHelper(0.3f, 0.03f, 0.9f, 1.0f, Color.White, Color.White, Vector2.Zero, Vector2.Zero);

        public override void OnAgentEnter(HackGameAgent agent, HackGameBoard board, HackGameBoardElement_Node node)
        {
            if (!fired && agent is HackGameAgent_Player)
            {
                fired = true;
                drawFire = true;
            }
        }

        public override void OnAgentExit(HackGameAgent agent, HackGameBoard board, HackGameBoardElement_Node node) { }
        public override void OnAgentStay(HackGameAgent agent, HackGameBoard board, HackGameBoardElement_Node node) { }

        public override void DrawSelf(SpriteBatch sb, HackGameBoardElement_Node node, HackNodeGameBoardMedia gameboarddrawing, Vector2 Nodedrawpos, float zoom)
        {
            if (drawFire == true)
            {
                drawFire = false;
                node.AddUIElement(gameboarddrawing.WeaponPingTexture, 0.75f, new Vector2(41.0f * zoom, 41.0f * zoom), new Vector2(41.0f * zoom, 41.0f * zoom), new Color(1.0f, 0.4f, 0.0f, 0.0f), new Color(1.0f, 1.0f, 0.0f, 0.0f), 0.2f, 4.0f, 0.0f);
            }

            if (!fired)
            {
                sb.Draw(gameboarddrawing.Weapon_Decoy_texture, Nodedrawpos + new Vector2(41.0f * zoom, 41.0f * zoom), null, Color.White, 0, new Vector2(40.0f, 40.0f), zoom * pulseEffect.CurrentScale(), SpriteEffects.None, 0);
            }
        }

        public override void UpdateState(GameTime time, HackGameBoard board, HackGameBoardElement_Node node)
        { pulseEffect.Update(time); }
    }

    class HackGameBoardNodeContent_Weapon_Mortar : HackGameBoardNodeContent
    {
        bool fired = false;
        bool drawFire = false;

        HackGameReversibleLerpDrawHelper pulseEffect = new HackGameReversibleLerpDrawHelper(0.3f, 0.01f, 0.9f, 1.0f, Color.White, Color.White, Vector2.Zero, Vector2.Zero);

        public override void OnAgentEnter(HackGameAgent agent, HackGameBoard board, HackGameBoardElement_Node node)
        {
            if (!fired && agent is HackGameAgent_Player)
            {
                fired = true;
                drawFire = true;

                HackGameAgent_Projectile_Mortar mortar = new HackGameAgent_Projectile_Mortar(board);
                board.AddAgent(mortar);
            }
        }

        public override void OnAgentExit(HackGameAgent agent, HackGameBoard board, HackGameBoardElement_Node node) { }
        public override void OnAgentStay(HackGameAgent agent, HackGameBoard board, HackGameBoardElement_Node node) { }

        public override void DrawSelf(SpriteBatch sb, HackGameBoardElement_Node node, HackNodeGameBoardMedia gameboarddrawing, Vector2 Nodedrawpos, float zoom)
        {
            if (drawFire == true)
            {
                drawFire = false;
                node.AddUIElement(gameboarddrawing.WeaponPingTexture, 0.75f, new Vector2(41.0f * zoom, 41.0f * zoom), new Vector2(41.0f * zoom, 41.0f * zoom), new Color(1.0f, 0.4f, 0.0f, 0.0f), new Color(1.0f, 1.0f, 0.0f, 0.0f), 0.2f, 4.0f, 0.0f);
                gameboarddrawing.MissileLaunchSound.Play();
            }

            if (!fired)
            {
                sb.Draw(gameboarddrawing.Weapon_Mortar_texture, Nodedrawpos + new Vector2(40.0f * zoom, 40.0f * zoom), null, Color.White, 0, new Vector2(40.0f, 40.0f), pulseEffect.CurrentScale() * zoom, SpriteEffects.None, 0);
            }
        }

        public override void UpdateState(GameTime time, HackGameBoard board, HackGameBoardElement_Node node)
        { pulseEffect.Update(time); }
    }

}
