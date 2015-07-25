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

    public enum NodeDirection
    {
        NodeDirection_E = 0,
        NodeDirection_EW,
        NodeDirection_N,
        NodeDirection_NE,
        NodeDirection_NEW,
        NodeDirection_NS,
        NodeDirection_NSE,
        NodeDirection_NSEW,
        NodeDirection_NSW,
        NodeDirection_NW,
        NodeDirection_S,
        NodeDirection_SE,
        NodeDirection_SEW,
        NodeDirection_SW,
        NodeDirection_W,
        NodeDirection_ORPHAN
    };


    enum HackGameBoardElementBaseType
    {
        HackGameBoardElementBaseType_Node = 0,
        HackGameBoardElementBaseType_Bridge_EW,
        HackGameBoardElementBaseType_Bridge_NS,
        HackGameBoardElementBaseType_Empty,
        HackGameBoardElementBaseType_MAX
    }

    enum HackGameBoardElementLootType
    {
        HackGameBoardElementLootType_None = 0,
        HackGameBoardElementLootType_1,
        HackGameBoardElementLootType_2,
        HackGameBoardElementLootType_3,
        HackGameBoardElementLootType_4,
        HackGameBoardElementLootType_MAX
    }

    class HackGameBoardElement
    {

        public enum HackGameBoardElement_State
        {
            HackGameBoardElement_State_Inactive,
            HackGameBoardElement_State_SpawningIn,

            HackGameBoardElement_State_Active,

            HackGameBoardElement_State_BeingKilled,
            HackGameBoardElement_State_Killed,
        }

        public HackGameBoardElementBaseType type = HackGameBoardElementBaseType.HackGameBoardElementBaseType_Empty;
        protected HackGameBoardElement_State currentState = HackGameBoardElement_State.HackGameBoardElement_State_Inactive;
        HackGameTimer spawnTimer;
        HackGameTimer killTimer;
        protected HackGameBoardElement_StateData_SpawningIn spawningInData;
        protected HackGameBoardElement_StateData_BeingKilled beingKilledData;

        bool lethalToAll = false;

        public virtual void Kill(float timeToKill)
        {

            if (currentState != HackGameBoardElement_State.HackGameBoardElement_State_Killed && currentState != HackGameBoardElement_State.HackGameBoardElement_State_BeingKilled)
            {
                if (timeToKill <= 0)
                {
                    SetCurrentState(HackGameBoardElement_State.HackGameBoardElement_State_BeingKilled);
                }
                else
                {
                    killTimer = new HackGameTimer(timeToKill);
                }
            }
        }

        public virtual HackGameBoardElement_State GetCurrentState()
        {
            return currentState;
        }

        public virtual void SetCurrentState(HackGameBoardElement_State state)
        {
            HackGameBoardElement_State oldState = currentState;
            currentState = state;

            EnteringNewState(oldState, state);
        }

        public virtual void EnteringNewState(HackGameBoardElement_State oldState, HackGameBoardElement_State newState)
        {
            if (newState == HackGameBoardElement_State.HackGameBoardElement_State_SpawningIn && oldState == HackGameBoardElement_State.HackGameBoardElement_State_Inactive)
            {
                spawningInData = new HackGameBoardElement_StateData_SpawningIn();
            }

            if (newState == HackGameBoardElement_State.HackGameBoardElement_State_BeingKilled)
            {
                beingKilledData = new HackGameBoardElement_StateData_BeingKilled();
            }
        }

        public virtual bool IsLethal()
        {
            return lethalToAll;
        }

        public virtual void SpawnIn(float seconds)
        {
            if (currentState == HackGameBoardElement_State.HackGameBoardElement_State_Inactive)
            {
                spawnTimer = new HackGameTimer(seconds);
                if (seconds <= 0)
                {
                    SpawnNow();
                }
            }
        }

        private void SpawnNow()
        {
            if (currentState == HackGameBoardElement_State.HackGameBoardElement_State_Inactive)
            {
                SetCurrentState(HackGameBoardElement_State.HackGameBoardElement_State_SpawningIn);
            }
        }

        public virtual bool IsActive()
        {
            return currentState == HackGameBoardElement_State.HackGameBoardElement_State_Active;
        }

        protected virtual void SetToRemove()
        {
            SetCurrentState(HackGameBoardElement_State.HackGameBoardElement_State_Killed);
        }

        
       

        public HackGameBoardElement() {  }
        virtual public void DrawSelf(SpriteBatch sb, HackNodeGameBoardMedia gameboarddrawing, Vector2 drawpos, float zoom)
        {
        }


        virtual public void DrawPlayerPathingOverlay(SpriteBatch sb, HackNodeGameBoardMedia gameboarddrawing, Vector2 drawpos, float zoom) { }

        virtual public void UpdateState(GameTime time, HackGameBoard board)
        {
            if (killTimer != null && killTimer.IsAlive())
            {
                killTimer.Update(time);
                if (!killTimer.IsAlive())
                {
                    SetCurrentState(HackGameBoardElement_State.HackGameBoardElement_State_BeingKilled);
                }
            }
            if (GetCurrentState() == HackGameBoardElement_State.HackGameBoardElement_State_Inactive)
            {
                //check if spawntimer
                if (spawnTimer != null && spawnTimer.IsAlive())
                {
                    spawnTimer.Update(time);
                    if (!spawnTimer.IsAlive())
                    {
                        SpawnNow();
                    }
                }
            }

            if (GetCurrentState() == HackGameBoardElement_State.HackGameBoardElement_State_SpawningIn)
            {
                spawningInData.spawnInLerp.Update(time);
                spawningInData.spawnInTimer.Update(time);
                if (!spawningInData.spawnInTimer.IsAlive())
                {
                    SetCurrentState(HackGameBoardElement_State.HackGameBoardElement_State_Active);
                }
            }

            if (GetCurrentState() == HackGameBoardElement_State.HackGameBoardElement_State_BeingKilled)
            {
                beingKilledData.beingKilledTimer.Update(time);
                beingKilledData.beingKilledLerp.Update(time);
                
                //set to lethal
                lethalToAll = true;
                
                if (!beingKilledData.beingKilledTimer.IsAlive())
                {
                    SetCurrentState(HackGameBoardElement_State.HackGameBoardElement_State_Killed);
                }
            }
        }

        virtual public void OnAgentEnter(HackGameAgent agent, HackGameBoard board) { }
        virtual public void OnAgentExit(HackGameAgent agent, HackGameBoard board) { }
        virtual public void OnAgentStay(HackGameAgent agent, HackGameBoard board) { }
    }

    class HackGameBoardElement_StateData_SpawningIn
    {
        public HackGameForwardLerpDrawHelper spawnInLerp;
        public HackGameTimer spawnInTimer;
        const float spawnInTime = 1.0f;

        public HackGameBoardElement_StateData_SpawningIn()
        {
            spawnInLerp = new HackGameForwardLerpDrawHelper(spawnInTime, 1.0f, 1.0f, spawnInTime, Color.Black, Color.White, spawnInTime, Vector2.Zero, Vector2.Zero, spawnInTime);
            spawnInTimer = new HackGameTimer(spawnInTime);
        }
    }

    class HackGameBoardElement_StateData_BeingKilled
    {
        public HackGameForwardLerpDrawHelper beingKilledLerp;
        public HackGameTimer beingKilledTimer;
        const float beingKilledTime = 1.0f;

        public HackGameBoardElement_StateData_BeingKilled()
        {
            beingKilledLerp = new HackGameForwardLerpDrawHelper(beingKilledTime, 1.0f, 1.0f, beingKilledTime, Color.White, Color.Black, beingKilledTime, Vector2.Zero, Vector2.Zero, beingKilledTime);
            beingKilledTimer = new HackGameTimer(beingKilledTime);
        }

    }

    class HackGameBoardElement_Node : HackGameBoardElement
    {

        //public HackGameBoardElementLootType loot = HackGameBoardElementLootType.HackGameBoardElementLootType_None;

        public HackGameBoardNodeContent content = null;
        protected List<WorldSpaceUIElement> nodeUISprites = new List<WorldSpaceUIElement>();
        protected Vector2 lastDrawPos;
        bool drawImpact = false;

        public HackGameBoardElement_Node()
        {
            type = HackGameBoardElementBaseType.HackGameBoardElementBaseType_Node;
        }

        public HackGameBoardNodeContent GetContent()
        {
            return content;
        }

        public void SetContent(HackGameBoardNodeContent nodeContent)
        {
            content = nodeContent;
        }

        public void AddUIElement(Texture2D tex, float lifetimeSeconds, Vector2 offsetFromParent, float delay)
        {
            WorldSpaceUIElement element = new WorldSpaceUIElement(tex, lifetimeSeconds, offsetFromParent, delay);
            nodeUISprites.Add(element);
        }

        public void AddUIElement(Texture2D tex, float lifetimeSeconds, Vector2 offsetFromParent_Start, Vector2 offsetFromParent_End, Color color_Start, Color color_End, float scale_Start, float scale_End, float delay)
        {

            WorldSpaceUIElement element = new WorldSpaceUIElement(tex, lifetimeSeconds, offsetFromParent_Start, offsetFromParent_End, color_Start, color_End, scale_Start, scale_End, delay);
            nodeUISprites.Add(element);
        }


        override public void DrawSelf(SpriteBatch sb, HackNodeGameBoardMedia gameboarddrawing, Vector2 drawpos, float zoom)
        {
            base.DrawSelf(sb, gameboarddrawing, drawpos, zoom);

            switch (GetCurrentState())
            {
                case HackGameBoardElement_State.HackGameBoardElement_State_Active:
                    DrawSelfActiveState(sb, gameboarddrawing, drawpos, zoom);
                    break;
                case HackGameBoardElement_State.HackGameBoardElement_State_SpawningIn:
                    DrawSelfSpawningInState(sb, gameboarddrawing, drawpos, zoom);
                    break;
                case HackGameBoardElement_State.HackGameBoardElement_State_BeingKilled:
                    DrawSelfBeingKilledState(sb, gameboarddrawing, drawpos, zoom);
                    break;
            }
        }

        public override void EnteringNewState(HackGameBoardElement_State oldState, HackGameBoardElement_State newState)
        {
            base.EnteringNewState(oldState, newState);

            if (newState == HackGameBoardElement_State.HackGameBoardElement_State_Active)
            {
                drawImpact = true;
            }
        }

        private void DrawSelfSpawningInState(SpriteBatch sb, HackNodeGameBoardMedia gameboarddrawing, Vector2 drawpos, float zoom)
        {
            sb.Draw(gameboarddrawing.NodeBoxtexture, drawpos, null, spawningInData.spawnInLerp.CurrentColor(), 0.0f, Vector2.Zero, zoom, SpriteEffects.None, 0);
        }

        private void DrawSelfBeingKilledState(SpriteBatch sb, HackNodeGameBoardMedia gameboarddrawing, Vector2 drawpos, float zoom)
        {
            sb.Draw(gameboarddrawing.NodeBoxtexture, drawpos, null, beingKilledData.beingKilledLerp.CurrentColor(), 0.0f, Vector2.Zero, zoom, SpriteEffects.None, 0);
        }

        private void DrawSelfActiveState(SpriteBatch sb, HackNodeGameBoardMedia gameboarddrawing, Vector2 drawpos, float zoom)
        {
            lastDrawPos = drawpos;
            foreach (WorldSpaceUIElement element in nodeUISprites)
            {
                element.DrawSelf(sb, drawpos, zoom);
            }

            sb.Draw(gameboarddrawing.NodeBoxtexture, drawpos, null, Color.White, 0.0f, Vector2.Zero, zoom, SpriteEffects.None, 0);

            if (content != null)
            {
                content.DrawSelf(sb, this, gameboarddrawing, drawpos, zoom);
            }

            if (drawImpact == true)
            {
                drawImpact = false;
                AddUIElement(gameboarddrawing.NodeBoxtexture, 0.5f, new Vector2(41.0f * zoom, 41.0f * zoom), new Vector2(41.0f * zoom, 41.0f * zoom), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(0.0f, 0, 0, 0), 0.2f, 3.0f, 0.0f);
                gameboarddrawing.NodeRevealSound.Play();
            }
        }

        override public void DrawPlayerPathingOverlay(SpriteBatch sb, HackNodeGameBoardMedia gameboarddrawing, Vector2 drawpos, float zoom)
        {
            base.DrawSelf(sb, gameboarddrawing, drawpos, zoom);
            if (GetCurrentState() == HackGameBoardElement_State.HackGameBoardElement_State_Active)
            {
                sb.Draw(gameboarddrawing.NodeBox_Pathedtexture, drawpos, null, Color.White, 0.0f, Vector2.Zero, zoom, SpriteEffects.None, 0);
            }
        }

        public override void OnAgentEnter(HackGameAgent agent, HackGameBoard board)
        {
            if (content != null)
            {
                content.OnAgentEnter(agent, board, this);
            }
            base.OnAgentEnter(agent, board);
        }

        public override void OnAgentExit(HackGameAgent agent, HackGameBoard board)
        {
            if (content != null)
            {
                content.OnAgentExit(agent, board, this);
            }
            base.OnAgentExit(agent, board);
        }

        public override void OnAgentStay(HackGameAgent agent, HackGameBoard board)
        {
            if (content != null)
            {
                content.OnAgentStay(agent, board, this);
            }
            base.OnAgentStay(agent, board);
        }

        public override void UpdateState(GameTime time, HackGameBoard board)
        {
            //ui
            for (int i = 0; i < nodeUISprites.Count; i++)
            {
                nodeUISprites[i].UpdateState(time, board);
                if (!nodeUISprites[i].Alive())
                {
                    nodeUISprites.RemoveAt(i);
                }
            }

            switch (GetCurrentState())
            {
                case HackGameBoardElement_State.HackGameBoardElement_State_Active:
                    UpdateStateActive(time, board);
                    break;
            }
            
            base.UpdateState(time, board);
        }

        private void UpdateStateActive(GameTime time, HackGameBoard board)
        {
            if (content != null)
            {
                content.UpdateState(time, board, this);
            }
        }

    }

    class HackGameBoardElement_BridgeNorthSouth : HackGameBoardElement
    {
        public HackGameBoardElement_BridgeNorthSouth()
        {
            type = HackGameBoardElementBaseType.HackGameBoardElementBaseType_Bridge_NS;
        }


        override public void DrawSelf(SpriteBatch sb, HackNodeGameBoardMedia gameboarddrawing, Vector2 drawpos, float zoom)
        {
            base.DrawSelf(sb, gameboarddrawing, drawpos, zoom);
            switch (GetCurrentState())
            {
                case HackGameBoardElement_State.HackGameBoardElement_State_Active:
                    DrawSelfActiveState(sb, gameboarddrawing, drawpos, zoom);
                    break;
                case HackGameBoardElement_State.HackGameBoardElement_State_SpawningIn:
                    DrawSelfSpawningInState(sb, gameboarddrawing, drawpos, zoom);
                    break;
                case HackGameBoardElement_State.HackGameBoardElement_State_BeingKilled:
                    DrawSelfBeingKilledState(sb, gameboarddrawing, drawpos, zoom);
                    break;
            }
            
        }

        private void DrawSelfBeingKilledState(SpriteBatch sb, HackNodeGameBoardMedia gameboarddrawing, Vector2 drawpos, float zoom)
        {
            sb.Draw(gameboarddrawing.BridgeNStexture, drawpos, null, beingKilledData.beingKilledLerp.CurrentColor(), 0.0f, Vector2.Zero, zoom, SpriteEffects.None, 0);
        }

        private void DrawSelfSpawningInState(SpriteBatch sb, HackNodeGameBoardMedia gameboarddrawing, Vector2 drawpos, float zoom)
        {
            sb.Draw(gameboarddrawing.BridgeNStexture, drawpos, null, spawningInData.spawnInLerp.CurrentColor(), 0.0f, Vector2.Zero, zoom, SpriteEffects.None, 0);    
        }

        private void DrawSelfActiveState(SpriteBatch sb, HackNodeGameBoardMedia gameboarddrawing, Vector2 drawpos, float zoom)
        {
            sb.Draw(gameboarddrawing.BridgeNStexture, drawpos, null, Color.White, 0.0f, Vector2.Zero, zoom, SpriteEffects.None, 0);    
        }

        override public void DrawPlayerPathingOverlay(SpriteBatch sb, HackNodeGameBoardMedia gameboarddrawing, Vector2 drawpos, float zoom)
        {
            base.DrawSelf(sb, gameboarddrawing, drawpos, zoom);
            if (GetCurrentState() == HackGameBoardElement_State.HackGameBoardElement_State_Active)
            {
                sb.Draw(gameboarddrawing.BridgeNS_Pathedtexture, drawpos, null, Color.White, 0.0f, Vector2.Zero, zoom, SpriteEffects.None, 0);
            }
        }
    }

    class HackGameBoardElement_BridgeEastWest : HackGameBoardElement
    {
        public HackGameBoardElement_BridgeEastWest()
        {
            type = HackGameBoardElementBaseType.HackGameBoardElementBaseType_Bridge_EW;
        }


        override public void DrawSelf(SpriteBatch sb, HackNodeGameBoardMedia gameboarddrawing, Vector2 drawpos, float zoom)
        {
            base.DrawSelf(sb, gameboarddrawing, drawpos, zoom);
            switch (GetCurrentState())
            {
                case HackGameBoardElement_State.HackGameBoardElement_State_Active:
                    DrawSelfActiveState(sb, gameboarddrawing, drawpos, zoom);
                    break;
                case HackGameBoardElement_State.HackGameBoardElement_State_SpawningIn:
                    DrawSelfSpawningInState(sb, gameboarddrawing, drawpos, zoom);
                    break;
                case HackGameBoardElement_State.HackGameBoardElement_State_BeingKilled:
                    DrawSelfBeingKilledState(sb, gameboarddrawing, drawpos, zoom);
                    break;
            }
            
        }

        private void DrawSelfBeingKilledState(SpriteBatch sb, HackNodeGameBoardMedia gameboarddrawing, Vector2 drawpos, float zoom)
        {
            sb.Draw(gameboarddrawing.BridgeEWtexture, drawpos, null, beingKilledData.beingKilledLerp.CurrentColor(), 0.0f, Vector2.Zero, zoom, SpriteEffects.None, 0);
        }

        private void DrawSelfSpawningInState(SpriteBatch sb, HackNodeGameBoardMedia gameboarddrawing, Vector2 drawpos, float zoom)
        {
            sb.Draw(gameboarddrawing.BridgeEWtexture, drawpos, null, spawningInData.spawnInLerp.CurrentColor(), 0.0f, Vector2.Zero, zoom, SpriteEffects.None, 0);
        }

        private void DrawSelfActiveState(SpriteBatch sb, HackNodeGameBoardMedia gameboarddrawing, Vector2 drawpos, float zoom)
        {
            sb.Draw(gameboarddrawing.BridgeEWtexture, drawpos, null, Color.White, 0.0f, Vector2.Zero, zoom, SpriteEffects.None, 0);
        }

        override public void DrawPlayerPathingOverlay(SpriteBatch sb, HackNodeGameBoardMedia gameboarddrawing, Vector2 drawpos, float zoom)
        {
            base.DrawSelf(sb, gameboarddrawing, drawpos, zoom);
            if (GetCurrentState() == HackGameBoardElement_State.HackGameBoardElement_State_Active)
            {
                sb.Draw(gameboarddrawing.BridgeEW_Pathedtexture, drawpos, null, Color.White, 0.0f, Vector2.Zero, zoom, SpriteEffects.None, 0);
            }
        }
    }

    class HackGameBoardElement_Empty : HackGameBoardElement
    {
    
        public HackGameBoardElement_Empty()
        {
            type = HackGameBoardElementBaseType.HackGameBoardElementBaseType_Empty;
        }


        override public void DrawSelf(SpriteBatch sb, HackNodeGameBoardMedia gameboarddrawing, Vector2 drawpos, float zoom)
        {
            base.DrawSelf(sb, gameboarddrawing, drawpos, zoom);
        }
    }




    class HackGameBoard
    {
        public HackGameBoardElement[,] board;
        List<HackGameBoardEvent> events = new List<HackGameBoardEvent>();
        List<HackGameAgent> agents = new List<HackGameAgent>();
        List<HackGameAgent> deadList = new List<HackGameAgent>();
        List<HackGameAgent> newAgentsList = new List<HackGameAgent>();

        const int prohibitedContent1_length = 5;

        int[,] prohibitedContent1_ND_CCW = new int[5, 5] {
            {1, 3, 1, 0, 1},
            {0, 0, 2, 0, 2},
            {1, 3, 1, 3, 1},
            {2, 0, 2, 0, 0},
            {1, 0, 1, 3, 1}
        };
        int[,] prohibitedContent1_ND_CW = new int[5, 5] {
            {1, 0, 1, 3, 1},
            {2, 0, 2, 0, 0},
            {1, 3, 1, 3, 1},
            {0, 0, 2, 0, 2},
            {1, 3, 1, 0, 1}
        };
        //int[,] prohibitedContent2_ND_CW = new int[9, 9];
        //int[,] prohibitedContent2_ND_CCW = new int[9, 9];

        HackGameBoardElement[,] prohibitedContent1_CW = new HackGameBoardElement[5, 5];
        HackGameBoardElement[,] prohibitedContent1_CCW = new HackGameBoardElement[5, 5];
        //HackGameBoardElement[,] prohibitedContent2_CW = new HackGameBoardElement[9, 9];
        //HackGameBoardElement[,] prohibitedContent2_CCW = new HackGameBoardElement[9, 9];

        List<HackGameAgent_Trail> trails = new List<HackGameAgent_Trail>();

        List<HackGameBoardOverlayText> overlayList = new List<HackGameBoardOverlayText>();
        public Random r = new Random();
        int maxHeight;
        int maxWidth;
        int center;
        public const float elementSize = 80.0f;
        HackGameBoard_Scoring scoring;
        public HackGameBoard_Ticker ticker;
        HackGameBoard_BackgroundText bgtext;
        HackGameBoard_CollapseController collapse;
        HackNodeGameBoardMedia media;
        Game1 ourGame;
        GameplayScreen ourScreen;
        HackGameAgent_Player ourPlayer;
        CurrencyStringer temp_valuestring = new CurrencyStringer(0);
        HackGameBoard_ExitEffect exitEffect;
        private ulong targetCashToExit;
        float speedUpFactor = 1.0f;
        bool isBonusRound = false;

        //magic number to pad the outside of the map with blank spaces
        const int padCount = 3;

        public HackGameBoard(Game1 game, GameplayScreen screen, HackNodeGameBoardMedia mediaIn)
        {
            ourGame = game;
            ourScreen = screen;
            media = mediaIn;
            scoring = new HackGameBoard_Scoring(this);
            ticker = new HackGameBoard_Ticker();
            bgtext = new HackGameBoard_BackgroundText();
            exitEffect = new HackGameBoard_ExitEffect(this);

            LoadProhibitedContentMatrix();
            
        }

        private void LoadProhibitedContentMatrix()
        {
            //turn the int matrices into real content matrices
            //5x5 cw
            prohibitedContent1_CW = InternalLoadProhibitedContentMatrix(prohibitedContent1_ND_CW, prohibitedContent1_length);
            //5x5 ccw
            prohibitedContent1_CCW = InternalLoadProhibitedContentMatrix(prohibitedContent1_ND_CCW, prohibitedContent1_length);
        }

        private HackGameBoardElement[,] InternalLoadProhibitedContentMatrix(int[,] NDmatrix, int length)
        {
            HackGameBoardElement[,] outArray = new HackGameBoardElement[length, length];

            for (int i = 0; i < length; i++)
            {
                for (int k = 0; k < length; k++)
                {
                    switch (NDmatrix[k, i])
                    {
                        case 0:
                            outArray[k, i] = new HackGameBoardElement_Empty();
                            break;
                        case 1:
                            outArray[k, i] = new HackGameBoardElement_Node();
                            break;
                        case 2:
                            outArray[k, i] = new HackGameBoardElement_BridgeNorthSouth();
                            break;
                        case 3:
                            outArray[k, i] = new HackGameBoardElement_BridgeEastWest();
                            break;
                    }
                }
            }

            return outArray;
        }

        public ulong GetTargetCashToExit()
        {
            return targetCashToExit;
        }

        public Game1 GetGame()
        {
            return ourGame;
        }

        public GameplayScreen GetScreen()
        {
            return ourScreen;
        }

        public HackNodeGameBoardMedia GetMedia()
        {
            return media;
        }

        public UInt64 GetScore()
        {
            return scoring.GetScore();
        }

        public float GetSpeedUpFactor()
        {
            return speedUpFactor;
        }

        public void SetSpeedUpFactor(float factor)
        {
            if (factor > 0)
            {
                speedUpFactor = factor;
            }
        }


        public void PopUpScoreboard(float seconds)
        {
            //scoring.PopUp(seconds);
        }

        public void PopDownScoreboard()
        {
            //scoring.PopDown();
        }

        public bool InBoard(Point point)
        {
            if (point.X < 0 ||
                point.X >= maxWidth ||
                point.Y < 0 ||
                point.Y >= maxWidth)
            {
                return false;
            }
            return true;
        }

        public void SetPlayer(HackGameAgent_Player player)
        {
            ourPlayer = player;
        }

        public HackGameAgent_Player GetPlayer()
        {
            return ourPlayer;
        }

        public HackGameBoardElement GetElementAtPoint(Point p)
        {
            if (!InBoard(p))
            {
                return null;
            }

            return board[p.Y, p.X];
        }

        public HackGameBoardElement getElementInDirection(Point current, HackGameAgent.MovementDirection dir)
        {
            if(!InBoard(current))
            {
                return null;
            }

            else
            {
                switch (dir)
                {
                    case HackGameAgent.MovementDirection.MovementDirection_North:
                        return GetElementAtPoint(new Point(current.X, current.Y - 1));
                    case HackGameAgent.MovementDirection.MovementDirection_South:
                        return GetElementAtPoint(new Point(current.X, current.Y + 1));
                    case HackGameAgent.MovementDirection.MovementDirection_West:
                        return GetElementAtPoint(new Point(current.X-1, current.Y));
                    case HackGameAgent.MovementDirection.MovementDirection_East:
                        return GetElementAtPoint(new Point(current.X+1, current.Y));
                    default:
                        return null;
                }
            }
        }

        public Point getPointInDirection(Point current, HackGameAgent.MovementDirection dir)
        {
            switch (dir)
            {
                case HackGameAgent.MovementDirection.MovementDirection_North:
                    return new Point(current.X, current.Y - 1);
                case HackGameAgent.MovementDirection.MovementDirection_South:
                    return new Point(current.X, current.Y + 1);
                case HackGameAgent.MovementDirection.MovementDirection_West:
                    return new Point(current.X - 1, current.Y);
                case HackGameAgent.MovementDirection.MovementDirection_East:
                    return new Point(current.X + 1, current.Y);
                default:
                    return current;
            }
        }

        public HackGameAgent[] GetAgents()
        {
            return agents.ToArray();
        }

        private bool ModifyGameBoardElement(Point location, HackGameBoardElement new_element)
        {

            if (!IsNodeInsideBounds(location))
                return false;
            board[location.Y, location.X] = new_element;
            return true;
        }

        public bool IsNodeInsideBounds(Point location)
        {
            if (location.X < 0 || location.Y < 0 || location.X >= maxWidth || location.Y >= maxWidth)
            {
                return false;
            }
            return true;
        }

        private bool ClearOfAdjacentNodes(Point location)
        {

            /* LESS AGGRESSIVE MODE
            //you'll loop through every direction
            Point[] checkPoints = new Point[4];
            for (int i = 0; i < 4; i++)
            {
                checkPoints[i] = new Point();
            }
            checkPoints[0] = new Point(location.X + 1, location.Y); //east
            checkPoints[1] = new Point(location.X - 1, location.Y); //west
            checkPoints[2] = new Point(location.X, location.Y + 1); //south
            checkPoints[3] = new Point(location.X, location.Y - 1); //north

            for (int i = 0; i < 4; i++)
            {

                if (IsNodeInsideBounds(checkPoints[i]) &&
                    GetGameBoardElement(checkPoints[i]).type == HackGameBoardElementBaseType.HackGameBoardElementBaseType_Node)
                {
                    return false;
                }
            }
            return true;
             * */


            //HYPER AGGRESSIVE MODE

            //you'll loop through every direction
            Point[] checkPoints = new Point[8];
            for (int i = 0; i < 8; i++)
            {
                checkPoints[i] = new Point();
            }
            checkPoints[0] = new Point(location.X + 1, location.Y); //east
            checkPoints[1] = new Point(location.X - 1, location.Y); //west
            checkPoints[2] = new Point(location.X, location.Y + 1); //south
            checkPoints[3] = new Point(location.X, location.Y - 1); //north

            checkPoints[4] = new Point(location.X - 1, location.Y - 1); //north west
            checkPoints[5] = new Point(location.X - 1, location.Y + 1); //south west
            checkPoints[6] = new Point(location.X + 1, location.Y - 1); //north east
            checkPoints[7] = new Point(location.X + 1, location.Y + 1); //south east

            for (int i = 0; i < 8; i++)
            {

                if (IsNodeInsideBounds(checkPoints[i]) &&
                    GetGameBoardElement(checkPoints[i]).type == HackGameBoardElementBaseType.HackGameBoardElementBaseType_Node)
                {
                    return false;
                }
            }
            return true;
        }


        public bool CreateGameBoard(BoardWaveDefine waveDefine, Game1 ourGame)
        {


            float targetDensity = waveDefine.mapNodeDensity;
            float runningDensity = 0.0f;
            int bestDensityIndex = -1;
            isBonusRound = waveDefine.bonusRound;

            maxHeight = waveDefine.mapSize;
            maxWidth = waveDefine.mapSize;
            agents.Clear();
            
            if (ourGame.GetRandomBoardSeed() != 0)
            {
                r = new Random(ourGame.GetRandomBoardSeed());
            }
            else
            {
                int newRandomSeed = 0;
                r = new Random();
                while (newRandomSeed == 0)
                {
                    newRandomSeed = r.Next(int.MinValue, int.MaxValue);
                }
                r = new Random(newRandomSeed);
                ourGame.SetRandomBoardSeed(newRandomSeed);
            }

            List<HackGameBoardElement[,]> boards = new List<HackGameBoardElement[,]>();
            List<int> prohibitedBoards = new List<int>();
            //RUN THE BOARD CREATION ALGORITHM SEVERAL TIMES TO GENERATE LIST.
            int antiProhibitedCheckCount = 100;
            int j;
            for (j = 0; j < antiProhibitedCheckCount; j++) // have to do this in order to allow anti-prohibited content algorithm to run without exceptions
            {
                for (int n = 0; n < 10; n++)
                {

                    //Start the board creation algorithm.
                    board = new HackGameBoardElement[waveDefine.mapSize, waveDefine.mapSize];

                    for (int i = 0; i < waveDefine.mapSize; i++)
                    {
                        for (int k = 0; k < waveDefine.mapSize; k++)
                        {
                            board[i, k] = new HackGameBoardElement_Empty();
                        }
                    }


                    //now, start filling this stuff in.
                    //pick the center.
                    center = waveDefine.mapSize / 2;
                    Point focuspoint = new Point(center, center);

                    ModifyGameBoardElement(new Point(center, center), new HackGameBoardElement_Node());

                    GenerateRandomNextNode(waveDefine.mapNodeDensity, new Point(center, center), waveDefine.mapMaxBridgeLength, true);

                    //At this point, node geometry is set. Check for prohibited content.
                    if (ContainsProhibitedContent(board))
                    {
                        prohibitedBoards.Add(n);
                    }


                    //now fit in the cash.
                    GenerateCashForNodes(waveDefine.totalCashDensity, waveDefine.blackCashDensity, waveDefine.yellowCashDensity, focuspoint);

                    //now add the weapons.
                    GenerateWeaponsForNodes(waveDefine.heatseekerDensity, waveDefine.multimissileDensity, waveDefine.mortarDensity, focuspoint);

                    float currentDensity = GetMapNodeDensityUnpadded();
                    if (Math.Abs(currentDensity - targetDensity) < Math.Abs(runningDensity - targetDensity) && !prohibitedBoards.Contains(n))
                    {
                        runningDensity = currentDensity;
                        bestDensityIndex = n;
                    }

                    //deep copy the board into a new board.
                    boards.Add(board);
                }


                if (bestDensityIndex == -1)
                {
                    //somehow, all of our boards were eliminated.
                    //throw (new Exception("All boards contained prohibited content. Cannot continue."));
                    continue;
                }
                else
                {
                    break;
                }
            }

            if (j > antiProhibitedCheckCount - 1)
            {
                //somehow, all of our boards were eliminated - a hundred times.
                throw (new Exception("All boards contained prohibited content. Cannot continue."));
            }

            ourGame.DensityDelta = Math.Abs(targetDensity - runningDensity);

            //PICK OUT THE BEST BOARD, USE IT.
            board = boards[bestDensityIndex];
            boards.Clear();

            //Made a lot of garbage, get rid of it.
            GC.Collect();

                //now that this is created to the small size (minus padding), pad it
                PadBoard(padCount);

                //reset to new center
                maxWidth = (waveDefine.mapSize + padCount * 2);
                maxHeight = maxWidth;
                center = maxWidth / 2;



                //Draw this awesome spawn in animation
                SetSpawnInAnim();



                collapse = new HackGameBoard_CollapseController(60.0f, this, new Point(center, center));

            return true;
        }

        private bool ContainsProhibitedContent(HackGameBoardElement[,] board)
        {
            float failAtThreshhold = 1.00f;

            //go cell by cell, using each cell as top-left corner of the check square.
            if(InternalContainsProhibitedContent(prohibitedContent1_CW, prohibitedContent1_length, failAtThreshhold))
                return true;
            if(InternalContainsProhibitedContent(prohibitedContent1_CCW, prohibitedContent1_length, failAtThreshhold))
                return true;

            return false;
        }

        private bool InternalContainsProhibitedContent(HackGameBoardElement[,] checkMatrix, int checkMatrixLength, float failAtThreshhold)
        {
            int operatingHeight = maxHeight - checkMatrixLength;
            int operatingWidth = maxWidth - checkMatrixLength;
            int totalarea = checkMatrixLength * checkMatrixLength;

            int currenthits = 0;
            for (int i = 0; i < operatingHeight; i++)
            {
                for (int k = 0; k < operatingWidth; k++)
                {

                    //now a double loop inside the double loop
                    for (int ii = 0; ii < checkMatrixLength; ii++)
                    {
                        for (int kk = 0; kk < checkMatrixLength; kk++)
                        {
                            if (board[kk + k, ii + i].GetType() == (checkMatrix[kk, ii]).GetType())
                            {
                                currenthits++;
                            }
                        }
                    }

                    if ((float)currenthits / (float)totalarea >= failAtThreshhold)
                    {
                        return true;
                    }
                    else
                    {
                        //keep moving
                        currenthits = 0;
                    }
                }
            }
            return false;
        }

        private void PadBoard(int padAmount)
        {
            int paddedSize = maxWidth + padAmount * 2;
            HackGameBoardElement[,] paddedBoard = new HackGameBoardElement[paddedSize, paddedSize];

            for (int i = 0; i < paddedSize; i++)
            {
                for (int k = 0; k < paddedSize; k++)
                {
                    if (i < padAmount || i >= padAmount + maxHeight || k < padAmount || k >= padAmount + maxWidth)
                    {
                        paddedBoard[i, k] = new HackGameBoardElement_Empty();
                    }
                    else
                    {
                        paddedBoard[i, k] = board[i - padAmount, k - padAmount];
                    }
                }
            }

            board = paddedBoard;
        }

        private void GenerateRandomNextNode(float density, Point focuspoint, int maxBridgeLength, bool firstnode)
        {
            //throw new NotImplementedException();
            HackGameBoardElement focuselement = GetGameBoardElement(focuspoint);

            if (focuselement.type != HackGameBoardElementBaseType.HackGameBoardElementBaseType_Node) //node
            {
                return;
            }

            //you'll loop through every direction
            Point[] checkVectors = new Point[4];
            for (int i = 0; i < 4; i++)
            {
                checkVectors[i] = new Point();
            }
            checkVectors[0] = new Point(1, 0); //east
            checkVectors[1] = new Point(-1, 0); //west
            checkVectors[2] = new Point(0, 1); //south
            checkVectors[3] = new Point(0, -1); //north

            for (int i = 0; i < 4; i++)
            {

                //early out if > density
                if (r.NextDouble() > density && !firstnode)
                {
                    continue;
                }

                //early out if not valid node
                if(!IsNodeInsideBounds(new Point(focuspoint.X + checkVectors[i].X, focuspoint.Y + checkVectors[i].Y)))
                {
                    continue;
                }

                HackGameBoardElement checkelement = GetGameBoardElement(new Point(focuspoint.X + checkVectors[i].X, focuspoint.Y + checkVectors[i].Y));
                if (checkelement.type == HackGameBoardElementBaseType.HackGameBoardElementBaseType_Empty)
                {
                    //only start building from an empty node
                    int desiredlength = r.Next(2, maxBridgeLength + 1);
                    for (int k = desiredlength; k > 1; k--)
                    {
                        Point targetpoint = new Point(focuspoint.X + checkVectors[i].X * k, focuspoint.Y + checkVectors[i].Y * k);
                        if(!IsNodeInsideBounds(targetpoint))
                        {
                            continue;
                        }
                        //is k empty or a node? if neither, abort.
                        HackGameBoardElement targetelement = GetGameBoardElement(targetpoint);

                        if (targetelement.type != HackGameBoardElementBaseType.HackGameBoardElementBaseType_Node &&
                            targetelement.type != HackGameBoardElementBaseType.HackGameBoardElementBaseType_Empty)
                        {
                            continue;
                        }

                        if (targetelement.type == HackGameBoardElementBaseType.HackGameBoardElementBaseType_Empty 
                            && !ClearOfAdjacentNodes(targetpoint))
                        {
                            continue;
                        }

                        if (targetelement.type == HackGameBoardElementBaseType.HackGameBoardElementBaseType_Node)
                        {
                            //you're connecting to an existing bridge
                            int foo = 0; //put a breakpoint here!
                            foo = foo + 1;
                        }

                        Point bridgepoint = new Point();
                        HackGameBoardElement bridgend;
                        bool allBridgesClear = true;

                        //is the path to k clear?
                        if (targetpoint.X == focuspoint.X)
                        {
                            bridgepoint.X = focuspoint.X;
                            bridgepoint.Y = focuspoint.Y;
                            
                            //walk in the Y direction
                            int diff = (int)Math.Abs(targetpoint.Y - focuspoint.Y);
                            for (int b = 0; b < diff - 1; b++)
                            {
                                bridgepoint.Y += checkVectors[i].Y;
                                bridgend = GetGameBoardElement(bridgepoint);
                                if (bridgend.type != HackGameBoardElementBaseType.HackGameBoardElementBaseType_Empty)
                                {
                                    allBridgesClear = false;
                                    break;
                                }
                            }
                            //if clear, run through again and place bridges.
                            if (!allBridgesClear)
                            {
                                continue;
                            }
                            
                                //yep, place bridges
                            else
                            {
                                bridgepoint.X = focuspoint.X;
                                bridgepoint.Y = focuspoint.Y;

                                diff = (int)Math.Abs(targetpoint.Y - focuspoint.Y);
                                for (int b = 0; b < diff - 1; b++)
                                {
                                    bridgepoint.Y += checkVectors[i].Y;
                                    AddNode_BridgeNS(bridgepoint);
                                }
                            }
                        }
                        else
                        {
                            bridgepoint.X = focuspoint.X;
                            bridgepoint.Y = focuspoint.Y;
                            
                            //walk in the X direction
                            int diff = (int)Math.Abs(targetpoint.X - focuspoint.X);
                            for (int b = 0; b < diff; b++)
                            {
                                bridgepoint.X += checkVectors[i].X;
                                bridgend = GetGameBoardElement(bridgepoint);
                                if (bridgend.type != HackGameBoardElementBaseType.HackGameBoardElementBaseType_Empty)
                                {
                                    allBridgesClear = false;
                                    break;
                                }
                            }

                            //if clear, run through again and place bridges.
                            if (!allBridgesClear)
                            {
                                continue;
                            }

                                //yep, place bridges
                            else
                            {
                                bridgepoint.X = focuspoint.X;
                                bridgepoint.Y = focuspoint.Y;

                                diff = (int)Math.Abs(targetpoint.X - focuspoint.X);
                                for (int b = 0; b < diff - 1; b++)
                                {
                                    bridgepoint.X += checkVectors[i].X;
                                    AddNode_BridgeEW(bridgepoint);
                                }
                            }
                        }

                        //finally, is target empty? if so, recurse from there.
                        if (targetelement.type == HackGameBoardElementBaseType.HackGameBoardElementBaseType_Empty)
                        {
                            AddNode_NodeEmpty(targetpoint);
                            GenerateRandomNextNode(density, targetpoint, maxBridgeLength, false);
                        }


                        //do not continue this loop.
                        break;
                    }
                }
            }
        }

        private void GenerateCashForNodes(float totalPercentCashNodes, float percentBlack, float percentYellow, Point center)
        {
            //1. get list of all nodes.
            List<SortedHackGameBoardElement> allNodes = new List<SortedHackGameBoardElement>();
            for (int yK = 0; yK < maxWidth; yK++)
            {
                for (int xI = 0; xI < maxWidth; xI++)
                {
                    Point target = new Point(xI, yK);
                    if (board[yK, xI].type == HackGameBoardElementBaseType.HackGameBoardElementBaseType_Node)
                    {
                        allNodes.Add(new SortedHackGameBoardElement(board[yK, xI], GetDistanceBetweenPoints(target, center), new Point(xI, yK)));
                    }
                }
            }

            //2. eliminate center.
            for(int i = 0; i < allNodes.Count; i++)
            {
                if (allNodes[i].location == center)
                {
                    allNodes.RemoveAt(i);
                    break;
                }
            }

            //3. eliminate all but % totalPercentCashNodes.
            allNodes = ShuffleList<SortedHackGameBoardElement>(allNodes);
            int numtoremove = (int)((1.0f - totalPercentCashNodes) * (float)allNodes.Count);
            allNodes.RemoveRange(0, numtoremove);

            //4. sort by distance from center
            allNodes.Sort(DistanceSort);
            allNodes.Reverse();

            //5. determine number of black nodes, fill in from top down.
            int numblacknodes = (int)(percentBlack * (float)allNodes.Count);
            int numyellownodes = (int)(percentYellow * (float)allNodes.Count);

            for (int i = 0; i < allNodes.Count; i++)
            {
                if (numblacknodes > 0)
                {
                    //add black
                    AddNode_BlackLoot(allNodes[i].location);
                    numblacknodes--;
                }
                else if (numyellownodes > 0)
                {
                    //add yellow
                    AddNode_YellowLoot(allNodes[i].location);
                    numyellownodes--;
                }
                else
                {
                    //add blue
                    AddNode_BlueLoot(allNodes[i].location);
                }
            }
        }

        private void GenerateWeaponsForNodes(float HeatseekerDensity, float MultimissileDensity, float MortarDensity, Point center)
        {
            //1. get list of all nodes that are empty only, and are at more than two nodes distant from the center.
            List<SortedHackGameBoardElement> allNodes = new List<SortedHackGameBoardElement>();
            for (int yK = 0; yK < maxWidth; yK++)
            {
                for (int xI = 0; xI < maxWidth; xI++)
                {
                    Point target = new Point(xI, yK);
                    if (board[yK, xI].type == HackGameBoardElementBaseType.HackGameBoardElementBaseType_Node &&
                        ((HackGameBoardElement_Node)board[yK, xI]).GetContent() == null &&
                        GetDistanceBetweenPoints(new Point(xI, yK), center) > 1)
                    {
                        allNodes.Add(new SortedHackGameBoardElement(board[yK, xI], GetDistanceBetweenPoints(target, center), new Point(xI, yK)));
                    }
                }
            }

            //2. create a count of heatseekers and multimissiles
            int heatseekercount = (int)(HeatseekerDensity * (float)allNodes.Count);
            int multimissilecount = (int)(MultimissileDensity * (float)allNodes.Count);
            int mortarcount = (int)(MortarDensity * (float)allNodes.Count);

            //3. shuffle the node list
            allNodes = ShuffleList<SortedHackGameBoardElement>(allNodes);

            //4. run through list
            for (int i = 0; i < allNodes.Count; i++)
            {
                if (heatseekercount > 0)
                {
                    AddNode_Heatseeker(allNodes[i].location);
                    heatseekercount--;
                }
                else if (multimissilecount > 0)
                {
                    AddNode_Multimissile(allNodes[i].location);
                    multimissilecount--;
                }
                else if (mortarcount > 0)
                {
                    AddNode_Mortar(allNodes[i].location);
                    mortarcount--;
                }
                else
                {
                    //no more needed, stop filling.
                    break;
                }
            }
        }

        private class SortedHackGameBoardElement
        {
            public HackGameBoardElement element;
            public int distance = 0;
            public Point location;

            public SortedHackGameBoardElement(HackGameBoardElement in_element, int in_distance, Point in_location)
            {
                element = in_element;
                distance = in_distance;
                location = in_location;
            }
        }

        private int GetDistanceBetweenPoints(Point pointA, Point pointB)
        {
            return (int)(Math.Sqrt(
                (pointB.X - pointA.X) * (pointB.X - pointA.X) +
                (pointB.Y - pointA.Y) * (pointB.Y - pointA.Y)
                ));
        }

        private static int DistanceSort(SortedHackGameBoardElement x, SortedHackGameBoardElement y)
        {
            if (x.distance < y.distance)
                return -1;
            if (y.distance < x.distance)
                return 1;
            return 0;
        }

        private List<E> ShuffleList<E>(List<E> inputList)
        {
            List<E> randomList = new List<E>();
            int randomIndex = 0;
            while (inputList.Count > 0)
            {
                randomIndex = r.Next(0, inputList.Count); //Choose a random object in the list
                randomList.Add(inputList[randomIndex]); //add it to the new, random list
                inputList.RemoveAt(randomIndex); //remove to avoid duplicates
            } return randomList; //return the new random list
        }

private HackGameBoardElement GetGameBoardElement(Point focuspoint)
{
 	return board[focuspoint.Y, focuspoint.X];
}

        private bool AddNode_NodeEmpty(Point location)
        {
            return ModifyGameBoardElement(location, new HackGameBoardElement_Node());
        }

        private bool AddNode_BlueLoot(Point location)
        {
            HackGameBoardElement_Node hn = (HackGameBoardElement_Node)GetElementAtPoint(location);
            hn.SetContent(new HackGameBoardNodeContent_Loot(HackGameBoardNodeContent_Loot.HackGameBoardNodeContent_Loot_Color.HackGameBoardNodeContent_Loot_Color_Blue));
            return ModifyGameBoardElement(location, hn);
        }

        private bool AddNode_YellowLoot(Point location)
        {
            HackGameBoardElement_Node hn = (HackGameBoardElement_Node)GetElementAtPoint(location);
            hn.SetContent(new HackGameBoardNodeContent_Loot(HackGameBoardNodeContent_Loot.HackGameBoardNodeContent_Loot_Color.HackGameBoardNodeContent_Loot_Color_Yellow));
            return ModifyGameBoardElement(location, hn);
        }

        private bool AddNode_BlackLoot(Point location)
        {
            HackGameBoardElement_Node hn = (HackGameBoardElement_Node)GetElementAtPoint(location);
            hn.SetContent(new HackGameBoardNodeContent_Loot(HackGameBoardNodeContent_Loot.HackGameBoardNodeContent_Loot_Color.HackGameBoardNodeContent_Loot_Color_Black));
            return ModifyGameBoardElement(location, hn);
        }

        private bool AddNode_Heatseeker(Point location)
        {
            HackGameBoardElement_Node hn = (HackGameBoardElement_Node)GetElementAtPoint(location);
            hn.SetContent(new HackGameBoardNodeContent_Weapon_Heatseeker());
            return ModifyGameBoardElement(location, hn);
        }

        private bool AddNode_Multimissile(Point location)
        {
            HackGameBoardElement_Node hn = new HackGameBoardElement_Node();
            hn.SetContent(new HackGameBoardNodeContent_Weapon_Multimissile());
            return ModifyGameBoardElement(location, hn);
        }

        private bool AddNode_Mortar(Point location)
        {
            HackGameBoardElement_Node hn = new HackGameBoardElement_Node();
            hn.SetContent(new HackGameBoardNodeContent_Weapon_Mortar());
            return ModifyGameBoardElement(location, hn);
        }

        private bool AddNode_BridgeNS(Point location)
        {
            HackGameBoardElement_BridgeNorthSouth hn = new HackGameBoardElement_BridgeNorthSouth();
            return ModifyGameBoardElement(location, hn);
        }

        private bool AddNode_BridgeEW(Point location)
        {
            HackGameBoardElement_BridgeEastWest hn = new HackGameBoardElement_BridgeEastWest();
            return ModifyGameBoardElement(location, hn);
        }
        

        public bool LoadGameEvents(string eventsname, ContentManager content)
        {
            BoardEventDefines eventdef = content.Load<BoardEventDefines>(eventsname);

            for (int i = 0; i < eventdef.EventDefines.Length; i++)
            {
                HackGameBoardEventTrigger triggerToAdd;
                HackGameBoardEvent eventToAdd;

                switch (eventdef.EventDefines[i].trigger)
                {
                    case 0:
                        triggerToAdd = new HackGameBoardEventTrigger_Timed(eventdef.EventDefines[i].triggerdatastring);
                        break;
                    case 1:
                        triggerToAdd = new HackGameBoardEventTrigger_PlayerScore(eventdef.EventDefines[i].triggerdatastring);
                        break;
                    default:
                        throw new InvalidOperationException("Board Definition Contains Invalid Event Trigger Type for Element " + i);
                }

                switch (eventdef.EventDefines[i].type)
                {
                    case 0:
                        eventToAdd = new HackGameBoardEvent_ThrowText(eventdef.EventDefines[i].typedatastring, triggerToAdd);
                        break;
                    case 1:
                        eventToAdd = new HackGameBoardEvent_SpawnAI(eventdef.EventDefines[i].typedatastring, triggerToAdd);
                        break;
                    case 2:
                        eventToAdd = new HackGameBoardEvent_RaiseAlertLevel(eventdef.EventDefines[i].typedatastring, triggerToAdd);
                        break;
                    case 3:
                        eventToAdd = new HackGameBoardEvent_OpenExit(eventdef.EventDefines[i].typedatastring, triggerToAdd);
                        break;
                    case 4:
                        eventToAdd = new HackGameBoardEvent_SpawnPlayer(eventdef.EventDefines[i].typedatastring, triggerToAdd);
                        break;
                    case 5:
                        eventToAdd = new HackGameBoardEvent_CameraSnap(eventdef.EventDefines[i].typedatastring, triggerToAdd);
                        break;
                    case 6:
                        eventToAdd = new HackGameBoardEvent_CameraLerp(eventdef.EventDefines[i].typedatastring, triggerToAdd);
                        break;
                    case 7:
                        eventToAdd = new HackGameBoardEvent_BeginCollapse(eventdef.EventDefines[i].typedatastring, triggerToAdd);
                        break;
                    default:
                        throw new InvalidOperationException("Event Definition Contains Invalid Event Type for Element " + i);
                }

                events.Add(eventToAdd);
            }

            return true;
        }

        public void AddNewTextEvent(string text, float delay)
        {
            events.Add(new HackGameBoardEvent_ThrowText(text, new HackGameBoardEventTrigger_Timed(delay)));
        }

        public bool CreateGameEvents(BoardWaveDefine waveDef)
        {
            //heuristic to create a set of events based on magic numbers in the wave definition.            
            
            //1. camera snap
            string snapString = center.ToString("F1", ourGame.GetCI()) + "," + center.ToString("F1", ourGame.GetCI()) + "," + "1.0";
            events.Add(new HackGameBoardEvent_CameraSnap(snapString, new HackGameBoardEventTrigger_Timed(0.0f)));

            //2. wave text
            string waveText = "Wave " + waveDef.waveNumber;
            events.Add(new HackGameBoardEvent_ThrowText(waveText, new HackGameBoardEventTrigger_Timed(1.0f)));

            //2 and 1/2 :). title text
            string waveTitleText = waveDef.waveTitle;
            events.Add(new HackGameBoardEvent_ThrowText(waveTitleText, new HackGameBoardEventTrigger_Timed(3.0f)));

            //3. spawn player
            string spawnString = center.ToString(ourGame.GetCI()) + "," + center.ToString(ourGame.GetCI());
            events.Add(new HackGameBoardEvent_SpawnPlayer(spawnString, new HackGameBoardEventTrigger_Timed(4.0f)));

            //4a. Find out target $
            int fullNodeLoad = 0;

            foreach (HackGameBoardElement el in board)
            {
                if (el is HackGameBoardElement_Node)
                {
                    HackGameBoardElement_Node eln = (HackGameBoardElement_Node)el;
                    if (eln.GetContent() is HackGameBoardNodeContent_Loot)
                    {
                        HackGameBoardNodeContent_Loot loot = (HackGameBoardNodeContent_Loot)eln.GetContent();
                        fullNodeLoad += loot.GetFinalValue();
                    }
                }
            }

            ulong startLoad = (ulong)((float)fullNodeLoad * waveDef.ratioCashToExit);
            int places = 0;

            if (startLoad > 9999 && startLoad <= 99999)
            {
                places = 3;
            }
            if (startLoad > 99999 && startLoad <= 999999)
            {
                places = 4;
            }
            if (startLoad > 999999)
            {
                places = 5;
            }

            //do a little rounding to clean up the number
            targetCashToExit = RoundDown(startLoad, places);

            //4b. Target text
            CurrencyStringer stringer = new CurrencyStringer((ulong)targetCashToExit);
            string targetString = "TARGET: "+stringer.outputstring.ToString();
            events.Add(new HackGameBoardEvent_ThrowText(targetString, new HackGameBoardEventTrigger_Timed(6.0f)));

            //4b and 1/2 :) Hint text
            string hintText = waveDef.hintText;
            if(hintText != null && hintText != "")
            events.Add(new HackGameBoardEvent_ThrowText(hintText, new HackGameBoardEventTrigger_Timed(10.0f)));

            //4c. Target open exit
            events.Add(new HackGameBoardEvent_OpenExit(spawnString, new HackGameBoardEventTrigger_PlayerScore(targetCashToExit.ToString())));

            //4d. Target start collapse
            events.Add(new HackGameBoardEvent_BeginCollapse(waveDef.collapseSecondsString, new HackGameBoardEventTrigger_PlayerScore(targetCashToExit.ToString())));

            //4e. Target EXIT OPEN! string
            events.Add(new HackGameBoardEvent_ThrowText("Exit Open!", new HackGameBoardEventTrigger_PlayerScore(targetCashToExit.ToString())));

            //5. figure out rate of AI spawn
            if (waveDef.aiSpawnsPerMinute > 0 && waveDef.aiSpawnsTotal > 0)
            {
                float spawnTime;
                for (int i = 0; i < waveDef.aiSpawnsTotal; i++)
                {
                    spawnTime = 60.0f / waveDef.aiSpawnsPerMinute * (i+1);
                    events.Add(new HackGameBoardEvent_RaiseAlertLevel("", new HackGameBoardEventTrigger_Timed(spawnTime + waveDef.aiWaitToFirstSpawnSeconds)));
                    events.Add(new HackGameBoardEvent_SpawnAI("", new HackGameBoardEventTrigger_Timed(spawnTime + 2.0f + waveDef.aiWaitToFirstSpawnSeconds)));
                }
            }

            //6. handle any speed differences
            if (waveDef.speedUpFactor != 1.0f)
            {
                events.Add(new HackGameBoardEvent_ThrowText("Speed " + waveDef.speedUpFactor.ToString("F1", ourGame.GetCI()) + "x", new HackGameBoardEventTrigger_Timed(8.0f)));
                events.Add(new HackGameBoardEvent_SetSpeed(waveDef.speedUpFactor.ToString(), new HackGameBoardEventTrigger_Timed(0.0f)));
            }

            return true;
        }


        public ulong RoundDown(ulong number, int place)
        {
            ulong i = 1;
            if (place <= 0)
                return number;
            while (place > 0)
            {
                i = i * 10;
                place--;
            }

            ulong r = number % i;
            return number - r;
        }

        public bool LoadGameBoard(string mapname, ContentManager content)
        {
            BoardDefine boarddef = content.Load<BoardDefine>(mapname);

            //Load up ancillary items;
            bgtext.LoadContent(content);

            //test test - try bounding the board with one empty space - this requires making the board + 2 to X and Y.

            maxHeight = boarddef.size + 2;
            maxWidth = boarddef.size + 2;
            agents.Clear();
            bool found = false;

            //Start the board creation algorithm.
            board = new HackGameBoardElement[maxWidth, maxWidth];

            for (int i = 1; i < maxWidth - 1; i++)
            {
                for (int k = 1; k < maxWidth - 1; k++)
                {
                    found = false;
                    //find the item that has i = y, k = x;
                    foreach (NodeDefine nd in boarddef.NodeDefines)
                    {
                        
                        if (nd.X == k-1 && nd.Y == i-1)
                        {
                            //that's our node.
                            found = true;
                            switch (nd.type)
                            {
                                case 0: //empty
                                    board[i, k] = new HackGameBoardElement_Empty();
                                    break;

                                case 1: //node
                                    board[i, k] = new HackGameBoardElement_Node();
                                    if(nd.lootpresent)
                                    {
                                        //HackGameBoardNodeContent_Loot.HackGameBoardNodeContent_Loot_Level ll = (HackGameBoardNodeContent_Loot.HackGameBoardNodeContent_Loot_Level)(nd.lootvalue);
                                        HackGameBoardNodeContent_Loot.HackGameBoardNodeContent_Loot_Color lc = (HackGameBoardNodeContent_Loot.HackGameBoardNodeContent_Loot_Color)(nd.lootcolor); ;
                                        HackGameBoardNodeContent_Loot loot = new HackGameBoardNodeContent_Loot(lc);
                                        ((HackGameBoardElement_Node)(board[i, k])).SetContent(loot);
                                    }
                                    else if (nd.weaponpresent)
                                    {
                                        switch (nd.weapontype)
                                        {
                                            case 0:
                                                ((HackGameBoardElement_Node)(board[i, k])).SetContent(new HackGameBoardNodeContent_Weapon_Multimissile());
                                                break;
                                            case 1:
                                                ((HackGameBoardElement_Node)(board[i, k])).SetContent(new HackGameBoardNodeContent_Weapon_Heatseeker());
                                                break;
                                            case 2:
                                                ((HackGameBoardElement_Node)(board[i, k])).SetContent(new HackGameBoardNodeContent_Weapon_Decoy());
                                                break;
                                            case 3:
                                                ((HackGameBoardElement_Node)(board[i, k])).SetContent(new HackGameBoardNodeContent_Weapon_Mortar());
                                                break;
                                        }
                                    }
                                    break;
                                case 2: //ns bridge
                                    board[i, k] = new HackGameBoardElement_BridgeNorthSouth();
                                    break;
                                case 3:
                                    board[i, k] = new HackGameBoardElement_BridgeEastWest();
                                    break;
                            }
                        }
                    }
                    if (found == false)
                    {
                        //return false; //ack! missing node definition!
                    }
                }
            }

            //run around behind it and fill in the edges.
            for (int i = 0; i < maxWidth; i++)
            {
                for (int k = 0; k < maxWidth; k++)
                {
                    if (board[i,k] == null)
                    {
                        board[i, k] = new HackGameBoardElement_Empty();
                    }
                }
            }

            //Draw this awesome spawn in animation
            SetSpawnInAnim();


            //BAD BAD BAD!!! NEED TO HAVE THE CENTER OF THE BOARD DEFINED SOMEWHERE ELSE, NOT HERE!
            collapse = new HackGameBoard_CollapseController(60.0f, this, new Point(6, 6));

            return true;

        }

        private void SetSpawnInAnim()
        {
            
            int center = maxWidth / 2;
            
            for (int i = 0; i < maxWidth; i++)
            {
                for (int k = 0; k < maxWidth; k++)
                {
                    int xdist = Math.Abs(center - k);
                    int ydist = Math.Abs(center - i);
                    board[i, k].SpawnIn((float)0.2f * (xdist + ydist));
                }
            }
        }

        public void SetKilledAnim(Point center)
        {
            for (int i = 0; i < maxWidth; i++)
            {
                for (int k = 0; k < maxWidth; k++)
                {
                    int xdist = Math.Abs(center.X - k);
                    int ydist = Math.Abs(center.Y - i);
                    board[i, k].Kill(MathHelper.Clamp(2.0f - 0.2f * (xdist + ydist), 0.0f, 2.0f));
                }
            }
        }

        public int GetGameBoardSize()
        {
            return maxWidth;
        }

        public Vector2 GetMaxCameraOffsetBottomRight(float zoom, GraphicsDevice gd)
        {
            float elementDrawScale = elementSize * zoom;
            return new Vector2(-1 * (maxWidth - (gd.Viewport.Width / elementDrawScale)) * elementDrawScale, -1 * (maxWidth - (gd.Viewport.Height / elementDrawScale)) * elementDrawScale);
            /*
            float maxElements = (maxWidth * elementDrawScale);
            float viewportHoriz = (gd.Viewport.Width * elementDrawScale);
            float viewportVert = (gd.Viewport.Height * elementDrawScale);

            return new Vector2(-1 * (maxElements - viewportHoriz), -1 * (maxElements - viewportVert));
             */
        }

        public Vector2 GetCameraOffsetToCenterOnElement(Point element, float zoom, GraphicsDevice gd)
        {
            Vector2 retVector = new Vector2();
            float elementDrawScale = elementSize * zoom;
            Vector2 max = GetMaxCameraOffsetBottomRight(zoom, gd);

            Vector2 maxElements = new Vector2(gd.Viewport.Width / elementDrawScale, gd.Viewport.Height / elementDrawScale);

            //try to center on the desired element.
            //I want to put the center of the desired element in the center of the screen.
            retVector.X = -1 * (((elementDrawScale * element.X) - (maxElements.X * elementDrawScale / 2.0f)) + elementDrawScale / 2.0f);
            retVector.Y = -1 * (((elementDrawScale * element.Y) - (maxElements.Y * elementDrawScale / 2.0f)) + elementDrawScale / 2.0f);

            retVector.X = MathHelper.Clamp(retVector.X, max.X, 0);
            retVector.Y = MathHelper.Clamp(retVector.Y, max.Y, 0);

            return retVector;
        }

        public bool DrawGameBoard(HackNodeGameBoardMedia gameboarddrawing, SpriteBatch sb, Vector2 CameraOffsetFromTopLeft, float zoom, GraphicsDevice gd, HackGameAgent_Player player)
        {
            if (zoom <= 0.0f)
            {
                return false;
            }

            //TEMP TEMP
            //sb.DrawString(gameboarddrawing.debugSpriteFont, "Load Time: "+ ourGame.LoadTime.ToString(), new Vector2(0, 360.0f), new Color(.2f, .4f, .1f));
            //TEMP TEMP
            //sb.DrawString(gameboarddrawing.debugSpriteFont, "Density Delta: " + ourGame.DensityDelta.ToString(), new Vector2(0, 380.0f), new Color(.2f, .4f, .1f));

            Vector2 normalizedOffset = new Vector2(Math.Abs(CameraOffsetFromTopLeft.X), Math.Abs(CameraOffsetFromTopLeft.Y));

            float elementDrawScale = elementSize * zoom; //use zoom later.
            //first, find our array offset.
            Point topLeftArrayIndex = new Point((int)(MathHelper.Max((normalizedOffset.X / elementDrawScale), 0)),
                (int)(MathHelper.Max((normalizedOffset.Y / elementDrawScale), 0)));

            int maxElementsAcross = (int)((float)(gd.Viewport.Width) / elementDrawScale) + 2;
            int maxElementsDown = (int)((float)(gd.Viewport.Height) / elementDrawScale) + 2;

            Point bottomRightArrayIndex = new Point((int)(MathHelper.Min(maxElementsAcross + topLeftArrayIndex.X, maxWidth)),
                (int)(MathHelper.Min(maxElementsDown + topLeftArrayIndex.Y, maxWidth)));

            //then, find the incremental.
            float drawXModulus = CameraOffsetFromTopLeft.X % elementDrawScale;
            float drawYModulus = CameraOffsetFromTopLeft.Y % elementDrawScale;

            float drawXCounter = drawXModulus;
            float drawYCounter = drawYModulus;
            Vector2 drawpos = new Vector2(drawXCounter, drawYCounter);

            for (int i = topLeftArrayIndex.Y; i < bottomRightArrayIndex.Y; i++)
            {
                for (int k = topLeftArrayIndex.X; k < bottomRightArrayIndex.X; k++)
                {
                    //Replace this with an Element Draw
                    board[i, k].DrawSelf(sb, gameboarddrawing, drawpos, zoom);
                    if (player.IsInDestinationPath(new Point(k, i)))
                    {
                        board[i, k].DrawPlayerPathingOverlay(sb, gameboarddrawing, drawpos, zoom);
                    }

                    drawXCounter += elementDrawScale;
                    drawpos.X = drawXCounter;
                }
                drawYCounter += elementDrawScale;
                drawpos.Y = drawYCounter;
                drawXCounter = drawXModulus;
                drawpos.X = drawXCounter;
            }


            DrawAgents(gameboarddrawing, sb, topLeftArrayIndex, bottomRightArrayIndex, zoom, drawXModulus, drawYModulus, gd);

            //overlay text
            foreach (HackGameBoardOverlayText text in overlayList)
            {
                text.DrawSelf(gameboarddrawing, sb, gameboarddrawing.Overlay_Font);
            }

            //collapse controller
            collapse.DrawSelf(gameboarddrawing, sb, gd, player);

            return true;
        }

        public bool DrawGameBoardAdditive(HackNodeGameBoardMedia gameboarddrawing, SpriteBatch sb, Vector2 CameraOffsetFromTopLeft, float zoom, GraphicsDevice gd, HackGameAgent_Player player)
        {
            if (zoom <= 0.0f)
            {
                return false;
            }

            //TEMP TEMP
            //sb.Draw(gameboarddrawing.Temp_BG, Vector2.Zero, null, Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0.2f);
            //sb.DrawString(gameboarddrawing.debugSpriteFont, CameraOffsetFromTopLeft.ToString(), new Vector2(0, 300.0f), new Color(.2f, .4f, .1f));

            Vector2 normalizedOffset = new Vector2(Math.Abs(CameraOffsetFromTopLeft.X), Math.Abs(CameraOffsetFromTopLeft.Y));

            float elementDrawScale = elementSize * zoom; //use zoom later.
            //first, find our array offset.
            Point topLeftArrayIndex = new Point((int)(MathHelper.Max((normalizedOffset.X / elementDrawScale), 0)),
                (int)(MathHelper.Max((normalizedOffset.Y / elementDrawScale), 0)));

            int maxElementsAcross = (int)((float)(gd.Viewport.Width) / elementDrawScale) + 2;
            int maxElementsDown = (int)((float)(gd.Viewport.Height) / elementDrawScale) + 2;

            Point bottomRightArrayIndex = new Point((int)(MathHelper.Min(maxElementsAcross + topLeftArrayIndex.X, maxWidth)),
                (int)(MathHelper.Min(maxElementsDown + topLeftArrayIndex.Y, maxWidth)));

            //then, find the incremental.
            float drawXModulus = CameraOffsetFromTopLeft.X % elementDrawScale;
            float drawYModulus = CameraOffsetFromTopLeft.Y % elementDrawScale;

            float drawXCounter = drawXModulus;
            float drawYCounter = drawYModulus;
            Vector2 drawpos = new Vector2(drawXCounter, drawYCounter);


            DrawAgentsAdditive(gameboarddrawing, sb, topLeftArrayIndex, bottomRightArrayIndex, zoom, drawXModulus, drawYModulus, gd);

            return true;
        }

        public bool AddAgent(HackGameAgent agent)
        {
            if (agents.Contains(agent) || newAgentsList.Contains(agent))
            {
                return false;
            }
            else
            {
                newAgentsList.Add(agent);
                return true;
            }
        }

        public Point GetBoardLocationAtTouchLocation(Vector2 TouchLocation, Vector2 CameraOffset, float zoom, GraphicsDevice gd)
        {
            Vector2 normalizedOffset = new Vector2(Math.Abs(CameraOffset.X), Math.Abs(CameraOffset.Y));
            Vector2 normalizedTouch = new Vector2(Math.Abs(TouchLocation.X), Math.Abs(TouchLocation.Y));

            float elementDrawScale = elementSize * zoom; //use zoom later.

            Point loc = new Point((int)((normalizedTouch.X + normalizedOffset.X) / elementDrawScale),
                (int)((normalizedTouch.Y + normalizedOffset.Y) / elementDrawScale));
            return loc;
        }

        public void AddOverlayText(string text, float lifeTimeSeconds, float startScale, float endScale, float scaleSeconds, Color startColor, Color endColor, float colorSeconds)
        {
            overlayList.Add(new HackGameBoardOverlayText(text, lifeTimeSeconds, startScale, endScale, scaleSeconds, startColor, endColor, colorSeconds));
        }

        /* - UNTESTED, PROBABLY HAS BUGS.
        public Point GetDrawLocationAtElement(Point element, Vector2 CameraOffset, float zoom, GraphicsDevice gd)
        {
            float elementDrawScale = elementSize * zoom; //use zoom later.
            Vector2 normalizedOffset = new Vector2(Math.Abs(CameraOffset.X), Math.Abs(CameraOffset.Y));

            Point location = new Point(
                (int)(elementDrawScale * ((float)element.X) + normalizedOffset.X),
                (int)(elementDrawScale * ((float)element.Y) + normalizedOffset.Y)
                );

            return location;
        }
         */

        

        class HackGameBoardOverlayText
        {

            string text;
            HackGameForwardLerpDrawHelper lerpHelper;


            public HackGameBoardOverlayText(string textToDisplay, float lifeTimeSeconds, float startScale, float endScale, float scaleSeconds, Color startColor, Color endColor, float colorSeconds)
            {
                text = textToDisplay;
                lerpHelper = new HackGameForwardLerpDrawHelper(lifeTimeSeconds, startScale, endScale, scaleSeconds, startColor, endColor, colorSeconds, Vector2.Zero, Vector2.Zero, 1.0f);
            }

            public void Update(GameTime t)
            {
                lerpHelper.Update(t);  
            }

            public void DrawSelf(HackNodeGameBoardMedia drawing, SpriteBatch spritebatch, SpriteFont font)
            {
                Vector2 size = font.MeasureString(text) * lerpHelper.CurrentScale();
                Vector2 drawloc = new Vector2(spritebatch.GraphicsDevice.Viewport.Width / 2 - size.X / 2, spritebatch.GraphicsDevice.Viewport.Height / 2 - size.Y / 2);

                spritebatch.DrawString(font, text, drawloc, lerpHelper.CurrentColor(), 0, Vector2.Zero, lerpHelper.CurrentScale(), SpriteEffects.None, 0);
            }

        public bool IsAlive()
        {
            return (lerpHelper.IsAlive());
        }

    }




        public void DrawAgents(HackNodeGameBoardMedia gameboarddrawing, SpriteBatch sb, Point topLeftArrayIndex, Point bottomRightArrayIndex, float zoom, float drawXModulus, float drawYModulus, GraphicsDevice gd)
        {

            float elementDrawScale = elementSize * zoom; //use zoom later.

            foreach (HackGameAgent agent in agents)
            {
                if (agent != ourPlayer && !(agent is HackGameAgent_Trail))
                {
                    DrawAgent(agent, elementDrawScale, gameboarddrawing, sb, topLeftArrayIndex, bottomRightArrayIndex, zoom, drawXModulus, drawYModulus, gd);
                }
            }
            
            //draw the player last.
            DrawAgent(ourPlayer, elementDrawScale, gameboarddrawing, sb, topLeftArrayIndex, bottomRightArrayIndex, zoom, drawXModulus, drawYModulus, gd);
        }

        public void DrawAgentsAdditive(HackNodeGameBoardMedia gameboarddrawing, SpriteBatch sb, Point topLeftArrayIndex, Point bottomRightArrayIndex, float zoom, float drawXModulus, float drawYModulus, GraphicsDevice gd)
        {

            float elementDrawScale = elementSize * zoom; //use zoom later.

            foreach (HackGameAgent agent in agents)
            {
                if (agent is HackGameAgent_Trail)
                {
                    DrawAgent(agent, elementDrawScale, gameboarddrawing, sb, topLeftArrayIndex, bottomRightArrayIndex, zoom, drawXModulus, drawYModulus, gd);
                }
            }
        }

        private void DrawAgent(HackGameAgent agent, float elementDrawScale, HackNodeGameBoardMedia gameboarddrawing, SpriteBatch sb, Point topLeftArrayIndex, Point bottomRightArrayIndex, float zoom, float drawXModulus, float drawYModulus, GraphicsDevice gd)
        {
            Point current = agent.getCurrentBoardLocation();
            Vector2 drawPos = Vector2.Zero;
            if (current.X >= topLeftArrayIndex.X - 1 && current.X <= bottomRightArrayIndex.X + 2 &&
                current.Y >= topLeftArrayIndex.Y - 1 && current.Y <= bottomRightArrayIndex.Y + 2)
            {
                //it's in range to draw, so draw it.
                //find the position on the screen of the node they're in.
                drawPos.X = (current.X - topLeftArrayIndex.X) * elementDrawScale + drawXModulus;
                drawPos.Y = (current.Y - topLeftArrayIndex.Y) * elementDrawScale + drawYModulus;
                //apply the local offset.
                drawPos += agent.getLocalDrawOffset(elementDrawScale, zoom);

                agent.DrawCurrentState(gameboarddrawing, sb, drawPos, zoom);
            }
        }

        public void DrawDebugText(HackNodeGameBoardMedia drawing, SpriteBatch spriteBatch, GraphicsDevice gd, HackGameAgent_Player player)
        {
            spriteBatch.DrawString(drawing.debugSpriteFont, "Camera X:" + ourScreen.GetCamera().GetCameraOffset().X + " Y:" + ourScreen.GetCamera().GetCameraOffset().Y + " Zoom:" + ourScreen.GetCamera().GetCameraZoom(), Vector2.Zero, Color.Red);
        }

        public void UpdateState(GameTime gameTime, HackNodeGameBoardMedia drawing)
        {
            foreach (HackGameAgent agent in this.agents)
            {
                agent.UpdateState(gameTime, this, drawing);
            }

            for (int i = 0; i < GetGameBoardSize(); i++)
            {
                for (int k = 0; k < GetGameBoardSize(); k++)
                {
                    //store if lethal
                    bool oldLethal = board[i, k].IsLethal();
                    board[i, k].UpdateState(gameTime, this);
                    bool newLethal = board[i, k].IsLethal();

                    if (oldLethal == false && newLethal == true)
                    {
                        DestroyAllAgentsInNode(new Point(k, i));
                    }
                }
            }

            //resolve collisions
            foreach (HackGameAgent agent in this.agents)
            {
                foreach (HackGameAgent otherAgent in this.agents)
                {
                    if (agent.IsCollidingWith(otherAgent))
                    {
                        agent.HasCollidedWith(otherAgent, this);
                    }
                }
            }

            //cull any dead
            for (int i = 0; i < agents.Count; i++)
            {
                if (agents[i].IsReadyToRemove())
                {
                    deadList.Add(agents[i]);
                }
            }

            for (int i = 0; i < deadList.Count; i++)
            {
                agents.Remove(deadList[i]);
            }
            deadList.Clear();

            //add any new
            for (int i = 0; i < newAgentsList.Count; i++)
            {
                if (!agents.Contains(newAgentsList[i]))
                {
                    agents.Add(newAgentsList[i]);
                }
            }
            newAgentsList.Clear();

            //update scoring
            scoring.UpdateSelf(gameTime);

            //update ticker
            ticker.UpdateSelf(gameTime);

            //update bg text
            bgtext.UpdateSelf(gameTime);

            //update exit effect
            exitEffect.Update(gameTime);

            //update collapser
            collapse.UpdateSelf(this, gameTime);
            if (collapse.IsActive() && collapse.HasCollapsed())
            {
                //uh oh - kill everything.
                ClearBackgroundTextPending();
                FadeOutBackgroundText(3.0f);
                SetKilledAnim(GetPlayer().getCurrentBoardLocation());
                KillAllAI();
                ourPlayer.Kill(0); //I'M DEAD!
            }

            //update overlay list
            for (int i = 0; i < overlayList.Count; i++)
            {
                overlayList[i].Update(gameTime);
                if (!overlayList[i].IsAlive())
                {
                    overlayList.RemoveAt(i);
                }
            }

            if (ourPlayer.GetCurrentState() != HackGameAgent.HackGameAgent_State.HackGameAgent_State_Exited && ourPlayer.GetCurrentState() != HackGameAgent.HackGameAgent_State.HackGameAgent_State_Killed)
            {
                //update events
                for (int i = 0; i < events.Count; i++)
                {
                    if (events[i].Update(gameTime, this))
                    {
                        EventFireHandler(events[i]);
                        events.RemoveAt(i);
                    }
                }
            }
        }

        public void DestroyAllAgentsInNode(Point point)
        {
            for (int i = 0; i < agents.Count; i++)
            {
                if ((agents[i].getCurrentBoardLocation() == point && 
                    (agents[i].getTtoDestination() < 0.3 || agents[i].getMovementDirection() == HackGameAgent.MovementDirection.MovementDirection_None))
                    
                    ||
                    
                    (agents[i].getDestinationBoardLocation() == point &&
                    (agents[i].getTtoDestination() > 0.7) && agents[i].getMovementDirection() != HackGameAgent.MovementDirection.MovementDirection_None))
                {
                    if (agents[i].GetCurrentState() == HackGameAgent.HackGameAgent_State.HackGameAgent_State_Active ||
                        agents[i].GetCurrentState() == HackGameAgent.HackGameAgent_State.HackGameAgent_State_SpawningIn)
                    {
                        agents[i].Kill(0);
                    }
                }
            }
        }

        protected void EventFireHandler(HackGameBoardEvent ev)
        {
            if (ev is HackGameBoardEvent_ThrowText)
            {
                AddOverlayText(((HackGameBoardEvent_ThrowText)ev).GetText(), 3.0f, 1.0f, 1.5f, 5.0f, new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(0.0f, 0, 0, 0.1f), 3.0f);
            }
            else if (ev is HackGameBoardEvent_RaiseAlertLevel)
            {
                SetAlertLevel(GetAlertLevel() + 1);
            }
            else if (ev is HackGameBoardEvent_SpawnAI)
            {
                
                HackGameAgent_AI ai;
                ai = new HackGameAgent_AI(this);
                AddAgent(ai);
                ai.SetInitialRandom(this, GetPlayer());
                ai.SpawnIn();

                AddOverlayText("ENEMY AI DETECTED!", 1.2f, 0.5f, 2.5f, 1.2f, new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 0, 0, 0.1f), 1.2f);
                 
            }
            else if (ev is HackGameBoardEvent_OpenExit)
            {
                HackGameBoardElement element = GetElementAtPoint(((HackGameBoardEvent_OpenExit)ev).GetExitLocation());

                if (element.type != HackGameBoardElementBaseType.HackGameBoardElementBaseType_Node)
                {
                    throw new InvalidOperationException("EventFireHandler on HackGameBoardEvent_OpenExit: Location requested is not a HackGameBoardElementBaseType_Node!");
                }
                else
                {
                    HackGameBoardElement_Node node = (HackGameBoardElement_Node)element;
                    node.SetContent(new HackGameBoardNodeContent_Exit());
                }
            }
            else if (ev is HackGameBoardEvent_SpawnPlayer)
            {
                Point location = ((HackGameBoardEvent_SpawnPlayer)ev).GetPlayerPosition();
                HackGameAgent_Player p = GetPlayer();
                p.SpawnIn();
                p.setCurrentBoardLocation(location, this);
                
            }

            else if (ev is HackGameBoardEvent_CameraSnap)
            {
                Point location = ((HackGameBoardEvent_CameraSnap)ev).GetSnapToElement();
                float zoom = ((HackGameBoardEvent_CameraSnap)ev).GetSnapToZoom();

                Vector2 newCam = GetCameraOffsetToCenterOnElement(location, zoom, ourGame.GraphicsDevice);
                ourScreen.GetCamera().SetCameraOffsetAndZoom(newCam, zoom, this);
            }

            else if (ev is HackGameBoardEvent_CameraLerp)
            {
                Point location = ((HackGameBoardEvent_CameraLerp)ev).GetLerpToElement();
                float zoom = ((HackGameBoardEvent_CameraLerp)ev).GetLerpToZoom();

                Vector2 newCam = GetCameraOffsetToCenterOnElement(location, zoom, ourGame.GraphicsDevice);
                ourScreen.GetCamera().LerpToCameraOffsetAndZoom(newCam, zoom, this);
            }
            else if (ev is HackGameBoardEvent_BeginCollapse)
            {
                if (!collapse.IsActive())
                {
                    collapse.SetTimer(((HackGameBoardEvent_BeginCollapse)ev).GetTimeToCollapse());
                    collapse.Activate(this);
                }
                
            }

            else if (ev is HackGameBoardEvent_SetSpeed)
            {
                SetSpeedUpFactor(((HackGameBoardEvent_SetSpeed)ev).GetSpeedFactor());
            }
        }

        public IEnumerable<Point> OpenMapTiles(Point currentPos)
        {
            Point [] possibles = new Point[4];
            possibles[0] = new Point(currentPos.X, currentPos.Y + 1); // south
            possibles[1] = new Point(currentPos.X, currentPos.Y - 1); // north
            possibles[2] = new Point(currentPos.X + 1, currentPos.Y); // east
            possibles[3] = new Point(currentPos.X - 1, currentPos.Y); // west

            HackGameBoardElementBaseType currentType = board[currentPos.Y, currentPos.X].type;

            //if you're empty, nowhere to go.
            if (!InBoard(currentPos) || board[currentPos.Y, currentPos.X].type == HackGameBoardElementBaseType.HackGameBoardElementBaseType_Empty)
            {
                yield break;
            }

            //what's open from where I am? use the heuristics table.
            for(int i = 0; i < 4; i++)
            {
                if(InBoard(possibles[i]))
                {
                    //it's in the board, check what it is.
                    switch(board[possibles[i].Y, possibles[i].X].type)
                    {
                        case HackGameBoardElementBaseType.HackGameBoardElementBaseType_Empty:
                            break;
                        case HackGameBoardElementBaseType.HackGameBoardElementBaseType_Node:
                            //only ok if you're a bridge (N/S if i=0/1, E/W if i=2/3)
                            if((currentType == HackGameBoardElementBaseType.HackGameBoardElementBaseType_Bridge_NS && (i == 0 || i == 1))||
                                (currentType == HackGameBoardElementBaseType.HackGameBoardElementBaseType_Bridge_EW) && (i == 2 || i ==3))
                            {
                                 yield return new Point(possibles[i].X, possibles[i].Y);
                            }
                            break;
                        case HackGameBoardElementBaseType.HackGameBoardElementBaseType_Bridge_EW:
                            //only ok if you're another ew bridge or a node
                            if((currentType == HackGameBoardElementBaseType.HackGameBoardElementBaseType_Bridge_EW ||
                                currentType == HackGameBoardElementBaseType.HackGameBoardElementBaseType_Node) && (i == 2 || i == 3))
                            {
                                yield return new Point(possibles[i].X, possibles[i].Y);
                            }
                            break;
                        case HackGameBoardElementBaseType.HackGameBoardElementBaseType_Bridge_NS:
                            if((currentType == HackGameBoardElementBaseType.HackGameBoardElementBaseType_Bridge_NS ||
                                currentType == HackGameBoardElementBaseType.HackGameBoardElementBaseType_Node) && (i == 0 || i == 1))
                            {
                                yield return new Point(possibles[i].X, possibles[i].Y);
                            }
                            break;
                    }
                }
            }
            /*
            switch (this.board[currentPos.Y, currentPos.X].type)
            {
                if
                
            }
             */
        }

        public float GetMinZoom(GraphicsDevice graphicsDevice)
        {
            //Max of 3 values
            //Enough to fit all tiles in board in width
            //Enough to fit all tiles in board in height
            //.25f

            float heightZoomMin = (float)graphicsDevice.Viewport.Height / (GetGameBoardSize() * elementSize);
            float widthZoomMin = (float)graphicsDevice.Viewport.Width / (GetGameBoardSize() * elementSize);
            return MathHelper.Max(MathHelper.Max(heightZoomMin, widthZoomMin), 0.25f);

        }



        public void DrawScoring(HackNodeGameBoardMedia drawing, SpriteBatch spriteBatch, GraphicsDevice GraphicsDevice, HackGameAgent_Player player)
        {
            scoring.DrawSelf(drawing, spriteBatch, GraphicsDevice, player);
        }

        public void UpdateScoring(GameTime time)
        {
            scoring.UpdateSelf(time);
        }

        public void AddBackgroundTextAward(StringBuilder text, float timeDelay)
        {
            bgtext.AddItem(new HackGameBoard_BackgroundTextItem(text, timeDelay, bgtext.awardStartColor, bgtext.awardEndColor, bgtext.awardColorFadeTime));
        }

        public void AddBackgroundTextStandard(StringBuilder text, float timeDelay)
        {
            bgtext.AddItem(new HackGameBoard_BackgroundTextItem(text, timeDelay, bgtext.defaultStartColor, bgtext.defaultEndColor, bgtext.defaultColorFadeTime));
        }

        public void AddBackgroundTextEmergency(StringBuilder text, float timeDelay)
        {
            bgtext.AddItem(new HackGameBoard_BackgroundTextItem(text, timeDelay, bgtext.emergencyStartColor, bgtext.emergencyEndColor, bgtext.emergencyColorFadeTime));
        }

        public void AddBackgroundTextNewline(float timeDelay)
        {
            bgtext.AddItem(new HackGameBoard_BackgroundTextItem(new StringBuilder(" "), timeDelay, bgtext.defaultStartColor, bgtext.defaultEndColor, bgtext.defaultColorFadeTime));
        }



        public void ClearBackgroundTextPending()
        {
            bgtext.ClearAllUpcoming();
        }

        public void AwardNodeContents(HackGameBoardNodeContent contents)
        {   
                if (contents is HackGameBoardNodeContent_Loot)
                {
                    int scoreToAdd = 0;
                    HackGameBoardNodeContent_Loot loot = (HackGameBoardNodeContent_Loot)contents;
                    //temp, just add fixed amount vs. loot value

                    string lootString = "TEMPORARY";
                    scoreToAdd = loot.GetFinalValue();

                    switch (loot.GetColor())
                    {
                        case HackGameBoardNodeContent_Loot.HackGameBoardNodeContent_Loot_Color.HackGameBoardNodeContent_Loot_Color_Blue:
                            lootString = "COMMERCIAL CREDIT AGENCY DXM REPORTS THE LOSS OF A SECURE ACCOUNT VALUED AT $" + scoreToAdd;
                            break;
                        case HackGameBoardNodeContent_Loot.HackGameBoardNodeContent_Loot_Color.HackGameBoardNodeContent_Loot_Color_Yellow:
                            lootString = "INDUSTRIAL PLAYER TKA ADMITS THEY HAVE LOST CLIENT DATA WORTH $" + scoreToAdd;
                            break;
                        case HackGameBoardNodeContent_Loot.HackGameBoardNodeContent_Loot_Color.HackGameBoardNodeContent_Loot_Color_Black:
                            lootString = "MILITARY CONTRACTOR ION ISSUES REPORT OF LOST PERSONNEL FILES WORTH $" + scoreToAdd;
                            break;
                    }

                    ticker.SetNewTickerString(lootString, 1.0f);

                    scoring.AddScore(scoreToAdd);


                    temp_valuestring.UpdateString((UInt64)scoreToAdd);
                    AddOverlayText(temp_valuestring.outputstring.ToString(), 1.2f, 0.5f, 2.5f, 1.2f, new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(0.0f, 0, 0, 0.1f), 1.2f);
                }  
        }

        public void SetAlertLevel(int level)
        {
            if (level > GetAlertLevel())
            {
                //aaaoogah!
                scoring.SetCurrentAlertLevel(level);
                //PopUpScoreboard(4.0f);
                AddOverlayText("ALERT SOUNDED!", 1.2f, 0.5f, 2.5f, 1.2f, new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 0, 0, 0.1f), 1.2f);
                media.AlertUpSound.Play();
            }
            else if (level < GetAlertLevel())
            {
                //stand down.
                scoring.SetCurrentAlertLevel(level);
                //PopUpScoreboard(4.0f);
            }
        }

        public int GetAlertLevel()
        {
            return scoring.GetCurrentAlertLevel();
        }

        internal void DrawTicker(HackNodeGameBoardMedia drawing, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, HackGameAgent_Player player)
        {
            ticker.DrawSelf(drawing, spriteBatch, graphicsDevice, player);
        }

        internal void DrawBackgroundText(HackNodeGameBoardMedia drawing, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, HackGameAgent_Player player)
        {
            bgtext.DrawSelf(spriteBatch, graphicsDevice);
        }

        public void KillAllAI()
        {
            foreach (HackGameAgent ai in agents)
            {
                if (ai is HackGameAgent_AI)
                {
                    ai.Kill((float)(r.NextDouble() * 1.5));
                }
            }
        }



        public void FadeOutBackgroundText(float seconds)
        {
            bgtext.FadeOut(seconds);
        }

        public void FreezeCollapseTimer()
        {
            collapse.Freeze();
        }

        public void UnFreezeCollapseTimer()
        {
            collapse.Unfreeze();
        }

        public void EndCollapse()
        {
            collapse.Deactivate(this);
        }

        public void StartCollapse()
        {
            collapse.Activate(this);
        }

        public bool IsNodeAlive(HackGameBoard board, Point node)
        {
            HackGameBoardElement el = board.GetElementAtPoint(node);
            if (el == null)
                return false;

            if (el.GetCurrentState() != HackGameBoardElement.HackGameBoardElement_State.HackGameBoardElement_State_Active)
            {
                return false;
            }
            return true;
        }

        

       

        public void DrawExitEffect(HackNodeGameBoardMedia drawing, float zoom, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, HackGameAgent_Player player)
        {
            exitEffect.Draw(drawing, zoom, spriteBatch, graphicsDevice, player);
        }

        public void StartExitEffect()
        {
            exitEffect.StartEffect();
        }

        public bool LoadWave(Game1 ourGame)
        {
            BoardWaveDefine foundWave = ourGame.GetWave(ourGame.GetCurrentWave());
            //Load up ancillary items;
            bgtext.LoadContent(ourGame.Content);
            CreateGameBoard(foundWave, ourGame);
            CreateGameEvents(foundWave);
            return true;
        }

        public float GetMapNodeDensityUnpadded()
        {
            int totalcount = 0;
            int fullcount = 0;

            if (board == null)
                return 0.0f;
            else
            {
                totalcount = (maxWidth) * (maxHeight);
                if (totalcount == 0)
                    return 0.0f;

                for (int i = 0; i < maxHeight; i++)
                {
                    for (int k = 0; k < maxWidth; k++)
                    {
                        if (!(board[i,k] is HackGameBoardElement_Empty))
                        {
                            fullcount++;
                        }
                    }
                }

                return (float)fullcount / (float)totalcount;
            }
        }

        public void SetHackLoopSoundAmountComplete(float pctTiming)
        {
            if (media != null)
            {
                media.SetHackLoopSoundAmountComplete(pctTiming);
            }

        }

        public void StopHackLoopSound()
        {
            if (media != null)
            {
                media.StopHackLoopSound();
            }
        }

        public void PlayHackSuccessSound()
        {
            if (media != null)
            {
                media.HackSuccessfulSound.Play();
            }
        }

        public bool IsBonusRound()
        {
            return isBonusRound;
        }

        public void ApplyMultiplier(float nodeValueMultiplier)
        {
            foreach (HackGameBoardElement el in board)
            {
                if (el.type == HackGameBoardElementBaseType.HackGameBoardElementBaseType_Node)
                {
                    HackGameBoardElement_Node eln = (HackGameBoardElement_Node)el;
                    if (eln.GetContent() is HackGameBoardNodeContent_Loot)
                    {
                        HackGameBoardNodeContent_Loot loot = (HackGameBoardNodeContent_Loot)eln.GetContent();
                        loot.ApplyMultiplier(nodeValueMultiplier);
                    }
                }
            }
        }
    }

    class HackGameBoard_ExitEffect
    {
        public enum DrawExitEffect_State
        {
            DrawExitEffect_State_NotStarted,
            DrawExitEffect_State_PillarVertical, //reach up and down to vertical edges
            DrawExitEffect_State_PillarHorizontal //reach left and right to horizontal edges
        }

        DrawExitEffect_State drawExitEffectState = DrawExitEffect_State.DrawExitEffect_State_NotStarted;

        Rectangle vertical_leftPanel_start;
        Rectangle vertical_leftPanel_end;

        Rectangle vertical_rightPanel_start;
        Rectangle vertical_rightPanel_end;

        Rectangle vertical_middlePanel_start;
        Rectangle vertical_middlePanel_end;

        Rectangle horizontal_leftPanel_start;
        Rectangle horizontal_leftPanel_end;

        Rectangle horizontal_rightPanel_start;
        Rectangle horizontal_rightPanel_end;

        Rectangle horizontal_middlePanel_start;
        Rectangle horizontal_middlePanel_end;

        Rectangle current_middlePanel;
        Rectangle current_leftPanel;
        Rectangle current_rightPanel;

        Vector2 gradient_bounds = new Vector2(25.0f, 1.0f);

        float transitionT = 0;

        const float horizontalTPerSecond = 1.5f;
        const float verticalTPerSecond = 1.5f;

        public HackGameBoard_ExitEffect(HackGameBoard board)
        {
            Vector2 bounds = new Vector2(board.GetGame().GraphicsDevice.Viewport.Width, board.GetGame().GraphicsDevice.Viewport.Height);
            Vector2 center = new Vector2(bounds.X / 2, bounds.Y / 2);
            Vector2 elementsize = new Vector2(80.0f, 80.0f);

            vertical_middlePanel_start = new Rectangle((int)(center.X - elementsize.X / 2), (int)(center.Y - 1), (int)(elementsize.X), 1);
            vertical_middlePanel_end = new Rectangle((int)(center.X - elementsize.X / 2), 0, (int)(elementsize.X), (int)bounds.Y);

            vertical_leftPanel_start = new Rectangle(vertical_middlePanel_start.X - (int)gradient_bounds.X, vertical_middlePanel_start.Y, (int)gradient_bounds.X, 1);
            vertical_leftPanel_end = new Rectangle(vertical_leftPanel_start.X, 0, vertical_leftPanel_start.Width, (int)bounds.Y);

            vertical_rightPanel_start = new Rectangle((int)center.X + (int)elementsize.X / 2, vertical_middlePanel_start.Y, (int)gradient_bounds.X, 1);
            vertical_rightPanel_end = new Rectangle(vertical_rightPanel_start.X, 0, vertical_rightPanel_start.Width, (int)bounds.Y);

            horizontal_middlePanel_start = vertical_middlePanel_end;
            horizontal_middlePanel_end = new Rectangle(0, 0, (int)(bounds.X), (int)bounds.Y);

            horizontal_leftPanel_start = vertical_leftPanel_end;
            horizontal_leftPanel_end = new Rectangle((int)-gradient_bounds.X, 0, (int)center.X, (int)bounds.Y);

            horizontal_rightPanel_start = vertical_rightPanel_end;
            horizontal_rightPanel_end = new Rectangle((int)center.X, 0, (int)(bounds.X / 2) + (int)gradient_bounds.X, (int)bounds.Y);


            
        }

        private Rectangle LerpRectangles(Rectangle fromRect, Rectangle toRect, float t)
        {
            return new Rectangle(
                (int)MathHelper.Lerp(fromRect.X, toRect.X, t),
                (int)MathHelper.Lerp(fromRect.Y, toRect.Y, t),
                (int)MathHelper.Lerp(fromRect.Width, toRect.Width, t),
                (int)MathHelper.Lerp(fromRect.Height, toRect.Height, t));
        }

        public void StartEffect()
        {
            drawExitEffectState = DrawExitEffect_State.DrawExitEffect_State_PillarVertical;
        }

        public void Draw(HackNodeGameBoardMedia drawing, float zoom, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, HackGameAgent_Player player)
        {
            spriteBatch.Draw(drawing.GradientLeft, current_leftPanel, Color.White);
            spriteBatch.Draw(drawing.GradientLeft, current_rightPanel, null, Color.White, 0, Vector2.Zero, SpriteEffects.FlipHorizontally, 0);
            spriteBatch.Draw(drawing.WhiteOneByOne, current_middlePanel, null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
        }

        public void Update(GameTime t)
        {
            switch (drawExitEffectState)
            {
                case DrawExitEffect_State.DrawExitEffect_State_NotStarted:
                    break;
                case DrawExitEffect_State.DrawExitEffect_State_PillarHorizontal:
                    UpdatePillarHorizontal(t);
                    break;
                case DrawExitEffect_State.DrawExitEffect_State_PillarVertical:
                    UpdatePillarVertical(t);
                    break;
            }
        }

        private void UpdatePillarVertical(GameTime t)
        {
            transitionT += verticalTPerSecond * (float)t.ElapsedGameTime.TotalSeconds;

            current_middlePanel = LerpRectangles(vertical_middlePanel_start, vertical_middlePanel_end, transitionT);
            current_leftPanel = LerpRectangles(vertical_leftPanel_start, vertical_leftPanel_end, transitionT);
            current_rightPanel = LerpRectangles(vertical_rightPanel_start, vertical_rightPanel_end, transitionT);

            if (transitionT >= 1.0f)
            {
                drawExitEffectState = DrawExitEffect_State.DrawExitEffect_State_PillarHorizontal;
                transitionT = 0.0f;
            }
        }

        private void UpdatePillarHorizontal(GameTime t)
        {
            transitionT += horizontalTPerSecond * (float)t.ElapsedGameTime.TotalSeconds;

            current_middlePanel = LerpRectangles(horizontal_middlePanel_start, horizontal_middlePanel_end, transitionT);
            current_leftPanel = LerpRectangles(horizontal_leftPanel_start, horizontal_leftPanel_end, transitionT);
            current_rightPanel = LerpRectangles(horizontal_rightPanel_start, horizontal_rightPanel_end, transitionT);

            if (transitionT >= 1.0f)
            {
                transitionT = 1.0f;
            }
        }
    }






}
