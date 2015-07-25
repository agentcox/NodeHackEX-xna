using System;
using System.Collections.Generic;
using System.Linq; //SHOULDN'T BE DOING THIS BUT...
using System.Text;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.IO;
using NodeDefinition;
using Pathfinding;
using HackPrototype;

namespace HackPrototype
{
    abstract class HackGameAgent
    {

        public enum HackGameAgent_State
        {
            HackGameAgent_State_Inactive,
            HackGameAgent_State_SpawningIn,

            HackGameAgent_State_Active,

            HackGameAgent_State_BeingKilled,
            HackGameAgent_State_Killed,
            HackGameAgent_State_ExitingOut,

            HackGameAgent_State_Exited
        }

        public const float CollideTWindow = 0.20f;
        public const float CollideTWindowCrossing = 0.10f;

        protected Point currentBoardElementLocation = new Point();
        protected Point currentBoardElementDestination = new Point();
        protected Stack<Point> nextBoardElementDestinations = new Stack<Point>();
        protected List<WorldSpaceUIElement> agentUISprites = new List<WorldSpaceUIElement>();
        protected List<HackGameAgent_Trail> trails = new List<HackGameAgent_Trail>();
        protected PathFinder pf = new PathFinder();
        float t = 0.0f; //distance to destination
        bool justArrived = false;
        bool justLeaving = false;
        bool justStaying = false;
        Point justLeavingNode = Point.Zero;
        protected Vector2 lastDrawPos;
        protected HackGameTimer killTimer;
        MovementDirection storedMovementDirection = MovementDirection.MovementDirection_None;

        int maxThinkAheadLength = 15;


        private HackGameAgent_State currentState = HackGameAgent_State.HackGameAgent_State_Inactive; //EVERYTHING STARTS INACTIVE.

        public enum MovementDirection
        {
            MovementDirection_West,
            MovementDirection_East,
            MovementDirection_South,
            MovementDirection_North,
            MovementDirection_None,
        }

        public virtual void Kill(float timetoKill)
        {
            if (currentState != HackGameAgent_State.HackGameAgent_State_Killed)
            {
                if (timetoKill <= 0)
                {
                    SetCurrentState(HackGameAgent_State.HackGameAgent_State_BeingKilled);
                }
                else
                {
                    killTimer = new HackGameTimer(timetoKill);
                }
            }
        }

        public void AddTrail(HackGameAgent_Trail trail)
        {
            trails.Add(trail);
        }

        public void SetRandomDestination(HackGameBoard board)
        {
            Point current = this.getCurrentBoardLocation();
            Point dest = current;

            //get all "nodes"
            List<Point> allNodes = new List<Point>();
            for (int i = 0; i < board.GetGameBoardSize(); i++)
            {
                for (int k = 0; k < board.GetGameBoardSize(); k++)
                {
                    if (board.board[i, k].type == HackGameBoardElementBaseType.HackGameBoardElementBaseType_Node &&
                        board.board[i, k].GetCurrentState() == HackGameBoardElement.HackGameBoardElement_State.HackGameBoardElement_State_Active)
                    {
                        allNodes.Add(new Point(k, i));
                    }
                }
            }

            if (allNodes.Count > 0)
            {
                TryDestination(allNodes[board.r.Next(0, allNodes.Count)], board);

                if (nextBoardElementDestinations.Count > maxThinkAheadLength)
                {
                    //shorten to maxThinkAheadLength
                    List<Point> rev = new List<Point>();
                    while (nextBoardElementDestinations.Count > 0)
                    {
                        rev.Add(nextBoardElementDestinations.Pop());
                    }

                    rev.Reverse();
                    rev.RemoveRange(0, rev.Count - maxThinkAheadLength);

                    while (board.GetElementAtPoint(rev[0]).type != HackGameBoardElementBaseType.HackGameBoardElementBaseType_Node && rev.Count > 0)
                    {
                        rev.RemoveAt(0);
                    }

                    if (rev.Count <= 0)
                    {
                        // uh oh. we were too aggressive.
                        //reset.
                        TryDestination(allNodes[board.r.Next(0, allNodes.Count)], board);
                    }

                    else
                    {
                        foreach (Point p in rev)
                        {
                            nextBoardElementDestinations.Push(p);
                        }
                    }
                }
            }
            else
            {
                Kill(0);
            }
        }

        public virtual HackGameAgent_State GetCurrentState()
        {
            return currentState;
        }

        public virtual void SetCurrentState(HackGameAgent_State state)
        {
            HackGameAgent_State oldState = currentState;
            currentState = state;

            EnteringNewState(oldState, state);
        }

        public abstract void EnteringNewState(HackGameAgent_State oldState, HackGameAgent_State newState);

        public virtual void SpawnIn()
        {
            if (currentState == HackGameAgent_State.HackGameAgent_State_Inactive)
            {
                SetCurrentState(HackGameAgent_State.HackGameAgent_State_SpawningIn);
            }
        }

        public virtual bool IsActive()
        {
            return currentState == HackGameAgent_State.HackGameAgent_State_Active;
        }

        protected virtual void SetToRemove()
        {
            SetCurrentState(HackGameAgent_State.HackGameAgent_State_Killed);
        }
        public bool IsReadyToRemove()
        {
            return currentState == HackGameAgent_State.HackGameAgent_State_Killed;
        }

        public HackGameAgent(HackGameBoard board)
        {
            pf.Initialize(board);
        }

        public Stack<Point> GetNextDestinations()
        {
            return nextBoardElementDestinations;
        }

        public MovementDirection getMovementDirection()
        {
            if (currentBoardElementDestination == currentBoardElementLocation)
            {
                return MovementDirection.MovementDirection_None;
            }
            else if (currentBoardElementDestination.Y > currentBoardElementLocation.Y)
            {
                return MovementDirection.MovementDirection_South;
            }
            else if (currentBoardElementDestination.Y < currentBoardElementLocation.Y)
            {
                return MovementDirection.MovementDirection_North;
            }
            else if (currentBoardElementDestination.X > currentBoardElementLocation.X)
            {
                return MovementDirection.MovementDirection_East;
            }
            else
            {
                return MovementDirection.MovementDirection_West;
            }
        }

        public MovementDirection getStoredMovementDirection()
        {
            return storedMovementDirection;
        }

        public void forgetNextDestinations()
        {
            this.nextBoardElementDestinations.Clear();
        }

        public bool setCurrentBoardLocation(Point location, HackGameBoard board)
        {
            if (location.X > board.GetGameBoardSize() || location.Y > board.GetGameBoardSize() || location.X < 0 || location.Y < 0)
            {
                return false;
            }
            else
            {
                justLeavingNode = currentBoardElementLocation;
                justLeaving = true;
                justArrived = true;
                this.currentBoardElementLocation = location;
                this.t = 0;
                this.currentBoardElementDestination = location;
                return true;
            }
        }

        public bool setDestinationBoardLocation(Point location, HackGameBoard board)
        {
            if (location.X > board.GetGameBoardSize() || location.Y > board.GetGameBoardSize() || location.X < 0 || location.Y < 0)
            {
                return false;
            }
            else
            {
                justLeavingNode = currentBoardElementDestination;
                justLeaving = true;
                this.currentBoardElementDestination = location;
                this.t = 0;
                return true;
            }
        }

        public bool IsBoardElementCurrentLocation(Point location, HackGameBoard board)
        {
            if (location == this.currentBoardElementLocation)
                return true;
            return false;
        }

        public bool IsBoardElementAdjacentToCurrent(Point location, HackGameBoard board)
        {
            if (Math.Abs(location.X - currentBoardElementLocation.X) + Math.Abs(location.Y - currentBoardElementLocation.Y) == 1)
            {
                return true;
            }
            return false;
        }


        public Point getCurrentBoardLocation()
        {
            return currentBoardElementLocation;
        }

        public Point getDestinationBoardLocation()
        {
            return currentBoardElementDestination;
        }

        public float getTtoDestination()
        {
            return t;
        }

        public void setTtoDestination(float newT)
        {
            t = newT;
            if (t >= 1.0f)
            {
                ArrivedAtDestination();
                t = 0.0f;
            }
        }

        public Vector2 getLocalDrawOffset(float elementSize, float zoom)
        {
            if (getMovementDirection() == MovementDirection.MovementDirection_None)
            {
                return Vector2.Zero;
            }

            else
            {
                //we don't scale by zoom here because zoom is applied later in the draw call
                float elementDrawScale = elementSize;

                switch (getMovementDirection())
                {
                    case MovementDirection.MovementDirection_North:
                        return new Vector2(0.0f, -1.0f * elementDrawScale * t);
                    case MovementDirection.MovementDirection_South:
                        return new Vector2(0.0f, 1.0f * elementDrawScale * t);
                    case MovementDirection.MovementDirection_West:
                        return new Vector2(-1.0f * elementDrawScale * t, 0.0f);
                    case MovementDirection.MovementDirection_East:
                        return new Vector2(1.0f * elementDrawScale * t, 0.0f);
                    default:
                        return Vector2.Zero;
                }

            }

        }

        public int GetLengthToDestination(Point startNode, Point endNode, HackGameBoard board)
        {
            PathFinder pf = new PathFinder();
            pf.Initialize(board);
            pf.Reset(startNode, endNode);
            bool success = pf.SearchPath();
            if (success == true)
            {
                return pf.FinalPath().Count;
            }
            else
            {
                return -1;
            }
        }

        public virtual bool TryDestination(Point node, HackGameBoard board)
        {
            if (!IsDestinationAllowed(node, board))
            {
                return false;
            }


            pf.Reset(this.currentBoardElementLocation, node);
            bool success = pf.SearchPath();
            if (success == true)
            {
                //add the final path to the stack.
                LinkedList<Point> path = pf.FinalPath();
                path.RemoveFirst();
                foreach (Point p in path.Reverse())
                {
                    nextBoardElementDestinations.Push(p);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public virtual void DrawCurrentState(HackNodeGameBoardMedia gameboarddrawing, SpriteBatch sb, Vector2 drawPos, float zoom)
        {
            lastDrawPos = drawPos;
            foreach (WorldSpaceUIElement element in agentUISprites)
            {
                element.DrawSelf(sb, drawPos, zoom);
            }
        }

        public virtual void HasCollidedWith(HackGameAgent otherAgent, HackGameBoard board)
        {
            //do nothing.
        }

        public virtual bool IsCollidingWith(HackGameAgent otherAgent)
        {
            if (otherAgent == this)
            {
                return false;
            }

            if (!this.IsActive() || !otherAgent.IsActive())
            {
                return false;
            }

            else
            {
                //start with the simplest case
                if (this.getMovementDirection() == MovementDirection.MovementDirection_None && otherAgent.getMovementDirection() == MovementDirection.MovementDirection_None
                    && this.currentBoardElementLocation == otherAgent.currentBoardElementLocation)
                {
                    return true;

                }
                else if (otherAgent.getMovementDirection() == MovementDirection.MovementDirection_None)
                {
                    //he's stationary, we just need to see if we're imminent to him and our distance to him is in tolerance
                    if (this.getMovementDirection() != MovementDirection.MovementDirection_None &&
                        this.currentBoardElementDestination == otherAgent.currentBoardElementLocation &&
                        this.getTtoDestination() > 1.0 - HackGameAgent.CollideTWindow)
                    {
                        return true;
                    }
                    return false;

                }
                else if (this.getMovementDirection() == MovementDirection.MovementDirection_None)
                {
                    //we're stationary, we just need to see if he's imminent to use and his distance to us is in tolerance
                    if (otherAgent.getMovementDirection() != MovementDirection.MovementDirection_None &&
                        otherAgent.currentBoardElementDestination == this.currentBoardElementLocation &&
                        otherAgent.getTtoDestination() > 1.0 - HackGameAgent.CollideTWindow)
                    {
                        return true;
                    }
                    return false;
                }
                else
                {
                    //well, we're both moving.
                    //so - if source and destination are both the same..
                    if (this.getCurrentBoardLocation() == otherAgent.getCurrentBoardLocation() &&
                        this.getDestinationBoardLocation() == otherAgent.getDestinationBoardLocation())
                    {
                        if (Math.Abs(this.getTtoDestination() - otherAgent.getTtoDestination()) < HackGameAgent.CollideTWindow)
                        {
                            return true;
                        }
                        return false;
                    }

                    //if we're coming from different places but destination is the same...
                    else if (this.getDestinationBoardLocation() == otherAgent.getDestinationBoardLocation())
                    {
                        //then two things must be true - we must each be w/in threshhold of destination and of each other.
                        if (this.getTtoDestination() < HackGameAgent.CollideTWindowCrossing && otherAgent.getTtoDestination() < HackGameAgent.CollideTWindowCrossing &&
                            Math.Abs(this.getTtoDestination() - otherAgent.getTtoDestination()) < HackGameAgent.CollideTWindowCrossing)
                        {
                            return true;
                        }
                        return false;

                    }

                    //if we're "swapping places" - a head-on collision
                    else if (this.getDestinationBoardLocation() == otherAgent.getCurrentBoardLocation() &&
                        otherAgent.getDestinationBoardLocation() == this.getCurrentBoardLocation())
                    {
                        //now you have to measure your T vs his 1.0 - T.
                        float fixedT = 1.0f - otherAgent.getTtoDestination();
                        if (Math.Abs(this.getTtoDestination() - fixedT) < HackGameAgent.CollideTWindow)
                        {
                            return true;
                        }
                        return false;
                    }

                    else
                    {
                        return false;
                    }
                }
            }
        }


        public virtual void UpdateState(GameTime time, HackGameBoard board, HackNodeGameBoardMedia drawing)
        {
            if (killTimer != null && killTimer.IsAlive())
            {
                killTimer.Update(time);
                if (!killTimer.IsAlive())
                {
                    SetCurrentState(HackGameAgent_State.HackGameAgent_State_BeingKilled);
                }
            }

            if (justStaying == true)
            {
                justStaying = false;
                HackGameBoardElement element = board.GetElementAtPoint(currentBoardElementLocation);
                if (element.type == HackGameBoardElementBaseType.HackGameBoardElementBaseType_Node)
                {
                    ((HackGameBoardElement_Node)(element)).OnAgentStay(this, board);
                }
            }

            if (justLeaving == true)
            {
                justLeaving = false;
                justStaying = false;
                HackGameBoardElement element = board.GetElementAtPoint(justLeavingNode);
                if (element.type == HackGameBoardElementBaseType.HackGameBoardElementBaseType_Node)
                {
                    ((HackGameBoardElement_Node)(element)).OnAgentExit(this, board);
                }
            }
            if (justArrived == true)
            {
                justArrived = false;
                justStaying = true;
                HackGameBoardElement element = board.GetElementAtPoint(currentBoardElementDestination);
                if (element.type == HackGameBoardElementBaseType.HackGameBoardElementBaseType_Node)
                {
                    ((HackGameBoardElement_Node)(element)).OnAgentEnter(this, board);
                }
                //kill self if on a lethal node
                if (element.IsLethal())
                {
                    Kill(0);
                }
            }


            //ui
            for (int i = 0; i < agentUISprites.Count; i++)
            {
                agentUISprites[i].UpdateState(time, board);
                if (!agentUISprites[i].Alive())
                {
                    agentUISprites.RemoveAt(i);
                }
            }

            //do not allow storedMovementDirection to "snap" north when an AI is at a destination
            MovementDirection currentMovementDirection = getMovementDirection();
            if (currentMovementDirection != MovementDirection.MovementDirection_None)
            {
                storedMovementDirection = getMovementDirection();
            }

            //update any trails
            if (trails != null && trails.Count > 1)
            {
                for (int i = trails.Count - 1; i > 0; i--)
                {
                    trails[i].SetToOtherAgentData(trails[i - 1]);
                }
            }
            if (trails != null && trails.Count > 0)
            {
                if (this is HackGameAgent_Player)
                {
                    trails[0].SetToPlayerData((HackGameAgent_Player)(this));
                }
                else
                {
                    trails[0].SetToOtherAgentData(this);
                }
            }


        }
        public abstract bool IsDestinationAllowed(Point node, HackGameBoard board);

        public virtual void ArrivedAtDestination()
        {
            this.currentBoardElementLocation = this.currentBoardElementDestination;
            justArrived = true;
        }

        public void AddUIElement(Texture2D tex, float lifetimeSeconds, Vector2 offsetFromParent, float delay)
        {
            WorldSpaceUIElement element = new WorldSpaceUIElement(tex, lifetimeSeconds, offsetFromParent, delay);
            agentUISprites.Add(element);
        }

        public void AddUIElement(Texture2D tex, float lifetimeSeconds, Vector2 offsetFromParent_Start, Vector2 offsetFromParent_End, Color color_Start, Color color_End, float scale_Start, float scale_End, float delay)
        {

            WorldSpaceUIElement element = new WorldSpaceUIElement(tex, lifetimeSeconds, offsetFromParent_Start, offsetFromParent_End, color_Start, color_End, scale_Start, scale_End, delay);
            agentUISprites.Add(element);
        }

    }

    class HackGameAgent_Player_StateData_ExitingOut
    {
        public FlashingElement flyOutFlash;
        public HackGameForwardLerpDrawHelper flyOutLerp;

        public bool lerping = false;
        public bool Starting = false;

        public bool DrawImpact = false;
        public bool StartSpawnSound = false;

        public float totalTimer;

        public HackGameTimer flashTimer;

        const float playerExitOutTotalLerpTime = 1.0f;
        const float playerExitOutFlashTime = 0.25f;
        const float playerExitOutTotalFlashTime = 2.0f;

        const float playerExitOutTotalTime = 3.0f;

        public HackGameAgent_Player_StateData_ExitingOut()
        {
            flyOutFlash = new FlashingElement(playerExitOutFlashTime, true, FlashingElement.FlashingElement_OperationType.FlashingElement_OperationType_Normal);
            flashTimer = new HackGameTimer(playerExitOutTotalFlashTime);
            totalTimer = playerExitOutTotalTime;
            Starting = true;
        }

        public void StartLerping()
        {
            flyOutLerp = new HackGameForwardLerpDrawHelper(playerExitOutTotalLerpTime, 1.0f, 10.0f, playerExitOutTotalLerpTime, Color.White, Color.White, playerExitOutTotalLerpTime, Vector2.Zero, Vector2.Zero, playerExitOutTotalLerpTime);
            lerping = true;
        }
    }

    class HackGameAgent_Player_StateData_SpawningIn
    {
        public bool flashing = false;
        public HackGameForwardLerpDrawHelper dropInLerp;
        public FlashingElement dropInFlash;
        public bool DrawImpact = false;
        public bool StartSpawnSound = true;

        public float totalTimer;

        const float playerSpawnInTotalLerpTime = 1.0f;
        const float playerSpawnInFlashTime = 0.25f;

        const float playerSpawnInTotalTime = 3.0f;

        public HackGameAgent_Player_StateData_SpawningIn()
        {
            dropInLerp = new HackGameForwardLerpDrawHelper(playerSpawnInTotalLerpTime, 10.0f, 1.0f, playerSpawnInTotalLerpTime, Color.White, Color.White, playerSpawnInTotalLerpTime, Vector2.Zero, Vector2.Zero, playerSpawnInTotalLerpTime);
            totalTimer = playerSpawnInTotalTime;
        }

        public void StartFlashing()
        {
            dropInFlash = new FlashingElement(playerSpawnInFlashTime, true, FlashingElement.FlashingElement_OperationType.FlashingElement_OperationType_Normal);
            flashing = true;
        }
    }

    class HackGameAgent_Trail : HackGameAgent
    {

        float alphaLevel = 1.0f;
        float specialZoom = 1.0f;
        bool visible = true;

        HackGameBoard gameboard;
        Texture2D drawTexture;

        public HackGameAgent_Trail(HackGameBoard board, float alpha, Texture2D texture)
            : base(board)
        {
            gameboard = board;
            alphaLevel = alpha;
            drawTexture = texture;
            SetCurrentState(HackGameAgent_State.HackGameAgent_State_Active);
        }

        public void SetVisible(bool isVisible)
        {
            visible = isVisible;
        }

        public void SetToOtherAgentData(HackGameAgent otherAgent)
        {
            this.setCurrentBoardLocation(otherAgent.getCurrentBoardLocation(), gameboard);
            this.setDestinationBoardLocation(otherAgent.getDestinationBoardLocation(), gameboard);
            this.setTtoDestination(otherAgent.getTtoDestination());
            this.SetCurrentState(otherAgent.GetCurrentState());

            if (otherAgent is HackGameAgent_Trail)
            {
                this.specialZoom = ((HackGameAgent_Trail)(otherAgent)).specialZoom;
            }
        }

        public void SetToPlayerData(HackGameAgent_Player player)
        {
            SetToOtherAgentData((HackGameAgent)(player));
            //now set special zoom
            if (player.GetCurrentState() == HackGameAgent_State.HackGameAgent_State_SpawningIn)
            {
                specialZoom = player.getSpawnInZoom();
            }
            else if (player.GetCurrentState() == HackGameAgent_State.HackGameAgent_State_ExitingOut)
            {
                specialZoom = player.getExitOutZoom();
            }

            else
            {
                specialZoom = 1.0f;
            }

        }

        public override void DrawCurrentState(HackNodeGameBoardMedia gameboarddrawing, SpriteBatch sb, Vector2 drawPos, float zoom)
        {
            switch (GetCurrentState())
            {
                case HackGameAgent_State.HackGameAgent_State_Active:
                    DrawActiveState(gameboarddrawing, sb, drawPos, zoom);
                    break;
                case HackGameAgent_State.HackGameAgent_State_SpawningIn:
                case HackGameAgent_State.HackGameAgent_State_ExitingOut:
                    DrawSpawningInState(gameboarddrawing, sb, drawPos, zoom);
                    break;

            }
            base.DrawCurrentState(gameboarddrawing, sb, drawPos, zoom);
        }

        private void DrawActiveState(HackNodeGameBoardMedia gameboarddrawing, SpriteBatch sb, Vector2 drawPos, float zoom)
        {
            if (visible)
            {
                sb.Draw(drawTexture, drawPos + new Vector2(41.0f * zoom, 41.0f * zoom), null, new Color(alphaLevel, alphaLevel, alphaLevel), 0.0f, new Vector2(41.0f, 41.0f), zoom * specialZoom, SpriteEffects.None, 0);
            }
        }

        private void DrawSpawningInState(HackNodeGameBoardMedia gameboarddrawing, SpriteBatch sb, Vector2 drawPos, float zoom)
        {
            if (visible)
            {
                sb.Draw(drawTexture, drawPos, null, new Color(alphaLevel, alphaLevel, alphaLevel), 0.0f, Vector2.Zero, zoom * specialZoom, SpriteEffects.None, 0);
            }
        }


        public override void EnteringNewState(HackGameAgent.HackGameAgent_State oldState, HackGameAgent.HackGameAgent_State newState)
        {
        }

        public override bool IsDestinationAllowed(Point node, HackGameBoard board)
        {
            return true;
        }


        /*
        public abstract void DrawBeingKilledState(HackNodeGameBoardMedia gameboarddrawing, SpriteBatch sb, Vector2 drawPos, float zoom);

        public abstract void DrawActiveState(HackNodeGameBoardMedia gameboarddrawing, SpriteBatch sb, Vector2 drawPos, float zoom);

        public abstract void UpdateActiveState(GameTime time, HackGameBoard board, HackNodeGameBoardMedia drawing);

        public override void ArrivedAtDestination()
        {
            base.ArrivedAtDestination();
        }
        */
    }


    class HackGameAgent_Player : HackGameAgent
    {

        float movementSpeed = 1.05f;
        bool StartPing_Update = false;
        bool StartPing_Draw = false;
        bool isHacking = false; //whether we're actively stealing something;
        bool hackSuccess = false; //did we just complete a hack?
        bool killEffectPlayed = false;
        bool justKilled = false;

        HackGameReversibleLerpDrawHelper pulseEffect = new HackGameReversibleLerpDrawHelper(0.5f, 0.1f, 1.0f, 1.50f, Color.White, Color.White, Vector2.Zero, Vector2.Zero);

        HackGameAgent_Player_StateData_SpawningIn spawnInData;
        HackGameAgent_Player_StateData_ExitingOut exitOutData;

        public HackGameAgent_Player(HackGameBoard board)
            : base(board)
        {
            board.SetPlayer(this);
        }

        public override void HasCollidedWith(HackGameAgent otherAgent, HackGameBoard board)
        {
            /*
            //DEBUG! If player hits an AI, kill it!
            if (otherAgent is HackGameAgent_AI)
            {
                //GET HIM!
                otherAgent.Kill();
            }
             */

            if (otherAgent is HackGameAgent_AI)
            {
                this.Kill(0); //I'M DEAD!
            }
        }

        public override void Kill(float timeToKill)
        {
            SetHacking(false);
            justKilled = true;
            base.Kill(0);
        }

        protected override void SetToRemove()
        {
            //do NOT REMOVE THE PLAYER.
            //base.SetToRemove();
        }

        public override void DrawCurrentState(HackNodeGameBoardMedia gameboarddrawing, SpriteBatch sb, Vector2 drawPos, float zoom)
        {
            switch (GetCurrentState())
            {
                case HackGameAgent_State.HackGameAgent_State_SpawningIn:
                    DrawSpawningInState(gameboarddrawing, sb, drawPos, zoom);
                    break;
                case HackGameAgent_State.HackGameAgent_State_Active:
                    DrawActiveState(gameboarddrawing, sb, drawPos, zoom);
                    break;
                case HackGameAgent_State.HackGameAgent_State_BeingKilled:
                    DrawBeingKilledState(gameboarddrawing, sb, drawPos, zoom);
                    break;
                case HackGameAgent_State.HackGameAgent_State_ExitingOut:
                    DrawExitingOutState(gameboarddrawing, sb, drawPos, zoom);
                    break;
            }
            base.DrawCurrentState(gameboarddrawing, sb, drawPos, zoom);
        }

        private void DrawExitingOutState(HackNodeGameBoardMedia gameboarddrawing, SpriteBatch sb, Vector2 drawPos, float zoom)
        {
            if (exitOutData.StartSpawnSound)
            {
                exitOutData.StartSpawnSound = false;
                //gameboarddrawing.WhooshSound.Play();
                gameboarddrawing.StartExitSound.Play();
                AddUIElement(gameboarddrawing.PingTexture, 1.5f, new Vector2(41.0f * zoom, 41.0f * zoom), new Vector2(41.0f * zoom, 41.0f * zoom), new Color(0.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0), 0.2f, 10.0f, 0.0f);
            }
            if (exitOutData.DrawImpact)
            {
                AddUIElement(gameboarddrawing.PingTexture, 1.5f, new Vector2(41.0f * zoom, 41.0f * zoom), new Vector2(41.0f * zoom, 41.0f * zoom), new Color(0.0f, 1.0f, 0, 0.0f), new Color(0.0f, 0, 0, 0), 0.2f, 10.0f, 0.0f);
                exitOutData.DrawImpact = false;
                gameboarddrawing.ExplosionSound.Play();
            }

            if (!exitOutData.lerping)
            {
                if (exitOutData.flyOutFlash.IsOn())
                {
                    sb.Draw(gameboarddrawing.PlayerTexture, drawPos, null, Color.White, 0.0f, Vector2.Zero, zoom, SpriteEffects.None, 0);
                }
            }

            else
            {
                sb.Draw(gameboarddrawing.PlayerTexture, drawPos, null, Color.White, 0.0f, Vector2.Zero, zoom * exitOutData.flyOutLerp.CurrentScale(), SpriteEffects.None, 0);
            }
        }

        private void DrawSpawningInState(HackNodeGameBoardMedia gameboarddrawing, SpriteBatch sb, Vector2 drawPos, float zoom)
        {
            if (spawnInData.StartSpawnSound)
            {
                spawnInData.StartSpawnSound = false;
                gameboarddrawing.WhooshSound.Play();
            }
            if (spawnInData.DrawImpact)
            {
                AddUIElement(gameboarddrawing.PingTexture, 1.0f, new Vector2(41.0f * zoom, 41.0f * zoom), new Vector2(41.0f * zoom, 41.0f * zoom), new Color(0.0f, 1.0f, 0, 0.0f), new Color(0.0f, 0, 0, 0), 0.2f, 2.5f, 0.0f);
                spawnInData.DrawImpact = false;
                gameboarddrawing.ThumpSound.Play();
            }

            if (spawnInData.flashing)
            {
                if (spawnInData.dropInFlash.IsOn())
                {
                    sb.Draw(gameboarddrawing.PlayerTexture, drawPos, null, Color.White, 0.0f, Vector2.Zero, zoom, SpriteEffects.None, 0);
                }
            }

            else
            {
                sb.Draw(gameboarddrawing.PlayerTexture, drawPos, null, Color.White, 0.0f, Vector2.Zero, zoom * spawnInData.dropInLerp.CurrentScale(), SpriteEffects.None, 0);
            }
        }

        private void DrawBeingKilledState(HackNodeGameBoardMedia gameboarddrawing, SpriteBatch sb, Vector2 drawPos, float zoom)
        {
            if (killEffectPlayed == false)
            {
                killEffectPlayed = true;
                gameboarddrawing.explosion.AddParticles(drawPos + new Vector2(41.0f * zoom, 41.0f * zoom), Vector2.Zero);
                gameboarddrawing.playerDeathParticles.AddParticles(drawPos + new Vector2(41.0f * zoom, 41.0f * zoom), Vector2.Zero);
                gameboarddrawing.ExplosionSound.Play();
                gameboarddrawing.GameOverSound.Play();
                gameboarddrawing.StopHackLoopSound();
                gameboarddrawing.StopWarningLoopSound();
            }
        }

        private void DrawActiveState(HackNodeGameBoardMedia gameboarddrawing, SpriteBatch sb, Vector2 drawPos, float zoom)
        {

            sb.Draw(gameboarddrawing.PlayerTexture, drawPos + new Vector2(41.0f * zoom, 41.0f * zoom), null, Color.White, 0.0f, new Vector2(41.0f, 41.0f), zoom * pulseEffect.CurrentScale(), SpriteEffects.None, 0);

            if (StartPing_Draw)
            {
                AddUIElement(gameboarddrawing.PingTexture, 1.0f, new Vector2(41.0f * zoom, 41.0f * zoom), new Vector2(41.0f * zoom, 41.0f * zoom), new Color(0.0f, 1.0f, 0, 0.0f), new Color(0.0f, 0, 0, 0), 0.2f, 1.5f, 0.0f);
                //gameboarddrawing.PlayerPingSound.Play();
                StartPing_Draw = false;
            }
            if (hackSuccess)
            {
                hackSuccess = false;
                AddUIElement(gameboarddrawing.PingTexture, 0.5f, new Vector2(41.0f * zoom, 41.0f * zoom), new Vector2(41.0f * zoom, 41.0f * zoom), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(0.0f, 0, 0, 0), 0.2f, 2.5f, 0.0f);
            }
        }

        public override void EnteringNewState(HackGameAgent.HackGameAgent_State oldState, HackGameAgent.HackGameAgent_State newState)
        {
            if (newState == HackGameAgent_State.HackGameAgent_State_SpawningIn && oldState == HackGameAgent_State.HackGameAgent_State_Inactive)
            {
                spawnInData = new HackGameAgent_Player_StateData_SpawningIn();
            }

            if (newState == HackGameAgent_State.HackGameAgent_State_ExitingOut)
            {
                exitOutData = new HackGameAgent_Player_StateData_ExitingOut();
            }
        }

        public override void UpdateState(GameTime time, HackGameBoard board, HackNodeGameBoardMedia drawing)
        {

            switch (GetCurrentState())
            {
                case HackGameAgent_State.HackGameAgent_State_SpawningIn:
                    UpdateSpawningInState(time, board, drawing);
                    break;
                case HackGameAgent_State.HackGameAgent_State_Active:
                    UpdateActiveState(time, board, drawing);
                    break;
                case HackGameAgent_State.HackGameAgent_State_ExitingOut:
                    UpdateExitingOutState(time, board, drawing);
                    break;
                case HackGameAgent_State.HackGameAgent_State_BeingKilled:
                    UpdateBeingKilledState(time, board, drawing);
                    break;
            }

            pulseEffect.Update(time);
            base.UpdateState(time, board, drawing);
        }

        public void UpdateBeingKilledState(GameTime time, HackGameBoard board, HackNodeGameBoardMedia drawing)
        {
            if (justKilled == true)
            {
                justKilled = false;
                board.GetGame().StopMusic();
            }
        }

        private void UpdateExitingOutState(GameTime time, HackGameBoard board, HackNodeGameBoardMedia drawing)
        {
            if (exitOutData.Starting)
            {
                exitOutData.Starting = false;
                exitOutData.StartSpawnSound = true;
                Vector2 newCam = board.GetCameraOffsetToCenterOnElement(getCurrentBoardLocation(), board.GetScreen().GetCamera().GetCameraZoom(), board.GetGame().GraphicsDevice);
                board.GetScreen().GetCamera().SetCameraOffsetAndZoom(newCam, board.GetScreen().GetCamera().GetCameraZoom(), board);
                board.GetScreen().LockCamera();
                board.SetKilledAnim(board.GetPlayer().getCurrentBoardLocation());
                board.ClearBackgroundTextPending();
                board.FadeOutBackgroundText(3.0f);
                board.FreezeCollapseTimer();
                board.ticker.ClearOverride();
            }
            exitOutData.totalTimer -= (float)time.ElapsedGameTime.TotalSeconds;
            if (exitOutData.totalTimer <= 0)
            {
                SetCurrentState(HackGameAgent_State.HackGameAgent_State_Exited);

            }
            else
            {
                if (exitOutData.lerping)
                {
                    exitOutData.flyOutLerp.Update(time);
                }
                else
                {
                    exitOutData.flyOutFlash.Update(time);
                    exitOutData.flashTimer.Update(time);
                    if (!exitOutData.flashTimer.IsAlive())
                    {
                        exitOutData.StartLerping();
                        board.KillAllAI();
                        board.StartExitEffect();
                        exitOutData.DrawImpact = true;
                        exitOutData.StartSpawnSound = true;
                        board.EndCollapse();
                    }

                }
            }
        }

        private void UpdateSpawningInState(GameTime time, HackGameBoard board, HackNodeGameBoardMedia drawing)
        {
            spawnInData.totalTimer -= (float)time.ElapsedGameTime.TotalSeconds;
            if (spawnInData.totalTimer <= 0)
            {
                SetCurrentState(HackGameAgent_State.HackGameAgent_State_Active);
            }
            else
            {
                if (spawnInData.flashing)
                {
                    spawnInData.dropInFlash.Update(time);
                }
                else
                {
                    spawnInData.dropInLerp.Update(time);
                    if (!spawnInData.dropInLerp.IsAlive())
                    {
                        spawnInData.DrawImpact = true;
                        spawnInData.StartFlashing();

                        board.AddBackgroundTextStandard(new StringBuilder("ACTIVATING ALL SYSTEMS"), 0);
                        board.AddBackgroundTextStandard(new StringBuilder("HACK SENSORS [OK]"), 1);
                        board.AddBackgroundTextStandard(new StringBuilder("HACK ATTACK [OK]"), 0.25f);
                        board.AddBackgroundTextStandard(new StringBuilder("HACK UNIT [OK]"), 0.25f);
                        board.AddBackgroundTextStandard(new StringBuilder("HACK SWEEP [OK]"), 0.25f);
                        board.AddBackgroundTextStandard(new StringBuilder("HACK TORPEDO [OK]"), 0.25f);
                        board.AddBackgroundTextStandard(new StringBuilder("HACK SENSORS [OK]"), 0.50f);
                        board.AddBackgroundTextStandard(new StringBuilder("HACK ATTACK [OK]"), 0.25f);
                        board.AddBackgroundTextStandard(new StringBuilder("HACK UNIT [OK]"), 0.25f);
                        board.AddBackgroundTextStandard(new StringBuilder("HACK SWEEP [OK]"), 0.25f);
                        board.AddBackgroundTextNewline(1.0f);
                        for (int i = 0; i < 10; i++)
                        {
                            board.AddBackgroundTextNewline(0.1f);
                        }
                        board.AddBackgroundTextStandard(new StringBuilder("ALL SYSTEMS GO!"), 0.5f);
                    }
                }
            }
        }

        private void UpdateActiveState(GameTime time, HackGameBoard board, HackNodeGameBoardMedia drawing)
        {
            if (StartPing_Update)
            {
                //be sure we're in a node.
                if (board.InBoard(getCurrentBoardLocation()) && board.GetElementAtPoint(getCurrentBoardLocation()).type == HackGameBoardElementBaseType.HackGameBoardElementBaseType_Node)
                {
                    //do some other stuff here.
                    board.AddBackgroundTextStandard(new StringBuilder("HACKNODE PING #0198"), 0);
                    board.AddBackgroundTextStandard(new StringBuilder("HACKNODE OK"), 0.75f);
                    StartPing_Draw = true;
                    StartPing_Update = false;
                }
            }
            if (this.getMovementDirection() != MovementDirection.MovementDirection_None)
            {
                setTtoDestination(getTtoDestination() + movementSpeed * (float)time.ElapsedGameTime.TotalSeconds * board.GetSpeedUpFactor());
            }
            else if (nextBoardElementDestinations.Count > 0)
            {
                setDestinationBoardLocation(nextBoardElementDestinations.Pop(), board);
            }

            if (isHacking)
            {
                drawing.binaryOneFountain_emitter.Update(time, lastDrawPos + new Vector2(41.0f, 41.0f));
                drawing.binaryZeroFountain_emitter.Update(time, lastDrawPos + new Vector2(41.0f, 41.0f));
            }
        }

        public bool HasExited()
        {
            return GetCurrentState() == HackGameAgent_State.HackGameAgent_State_Exited;
        }

        public void SetIsExiting()
        {
            SetCurrentState(HackGameAgent_State.HackGameAgent_State_ExitingOut);
        }

        public Stack<Point> TryDestinationBase(Point start, Point end, HackGameBoard board)
        {
            Stack<Point> outStack = new Stack<Point>();
            pf.Reset(start, end);
            bool success = pf.SearchPath();
            if (success == true)
            {
                //add the final path to the stack.
                LinkedList<Point> path = pf.FinalPath();
                path.RemoveFirst();
                foreach (Point p in path.Reverse())
                {
                    outStack.Push(p);
                }
            }
            return outStack;
        }

        public override bool TryDestination(Point node, HackGameBoard board)
        {

            if (!IsDestinationAllowed(node, board))
            {
                return false;
            }

            //start with the simplest case.
            //the player isn't moving. - or the stack is empty.
            if ((currentBoardElementDestination == currentBoardElementLocation ||
                (getTtoDestination() >= 1.0f || getTtoDestination() <= 0.0f)))
            {

                pf.Reset(this.currentBoardElementLocation, node);
                bool success = pf.SearchPath();
                if (success == true)
                {
                    //add the final path to the stack.
                    LinkedList<Point> path = pf.FinalPath();
                    path.RemoveFirst();
                    foreach (Point p in path.Reverse())
                    {
                        nextBoardElementDestinations.Push(p);
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }

            else
            {

                if (node == getCurrentBoardLocation() || node == getDestinationBoardLocation())
                {
                    //you can't kill it off entirely, you have to clear it all the way back to the first valid node.
                    if (board.board[getDestinationBoardLocation().Y, getDestinationBoardLocation().X].type == HackGameBoardElementBaseType.HackGameBoardElementBaseType_Node
                        && board.board[getDestinationBoardLocation().Y, getDestinationBoardLocation().X].GetCurrentState() == HackGameBoardElement.HackGameBoardElement_State.HackGameBoardElement_State_Active)
                    {
                        nextBoardElementDestinations.Clear();
                    }
                    else
                    {
                        foreach (Point p in nextBoardElementDestinations)
                        {
                            if (p != getCurrentBoardLocation() && p != getDestinationBoardLocation() &&
                                board.GetElementAtPoint(p).type == HackGameBoardElementBaseType.HackGameBoardElementBaseType_Node &&
                                board.GetElementAtPoint(p).GetCurrentState() == HackGameBoardElement.HackGameBoardElement_State.HackGameBoardElement_State_Active)
                            {
                                Stack<Point> leftToDestination = TryDestinationBase(getCurrentBoardLocation(), p, board);
                                Stack<Point> fromDestination = TryDestinationBase(getDestinationBoardLocation(), node, board);
                                List<Point> reverser = new List<Point>();
                                if (fromDestination.Count > 0)
                                {
                                    nextBoardElementDestinations.Clear();

                                    while (fromDestination.Count > 0)
                                    {
                                        reverser.Add(fromDestination.Pop());
                                    }
                                    while (leftToDestination.Count > 0)
                                    {
                                        reverser.Add(leftToDestination.Pop());
                                    }

                                    foreach (Point pp in reverser)
                                    {
                                        nextBoardElementDestinations.Push(pp);
                                    }

                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                        }
                    }
                    return true;
                }

                else
                {
                    //we need to create a new path that starts with all bridges up to the next node
                    if (board.board[getDestinationBoardLocation().Y, getDestinationBoardLocation().X].type == HackGameBoardElementBaseType.HackGameBoardElementBaseType_Node
                        && board.board[getDestinationBoardLocation().Y, getDestinationBoardLocation().X].GetCurrentState() == HackGameBoardElement.HackGameBoardElement_State.HackGameBoardElement_State_Active)
                    {
                        Stack<Point> fromDestination = TryDestinationBase(getDestinationBoardLocation(), node, board);
                        if (fromDestination.Count > 0)
                        {
                            nextBoardElementDestinations.Clear();
                            nextBoardElementDestinations = fromDestination;
                            return true;
                        }
                        else
                        {
                            return false;
                        }

                        //return TryDestination(node, board);
                    }
                    else //our immediate destination is not a node.
                    {
                        //find the next destination that IS a node, put a path together that starts with the path to that node.
                        /*
                        List<Point> old_path = new List<Point>();
                        Point currentCheckP = nextBoardElementDestinations.Pop();
                        old_path.Add(getCurrentBoardLocation());
                        old_path.Add(getDestinationBoardLocation());
                        old_path.Add(currentCheckP);
                        while (board.board[getDestinationBoardLocation().Y, getDestinationBoardLocation().X].type != HackGameBoardElementBaseType.HackGameBoardElementBaseType_Node && nextBoardElementDestinations.Count > 0)
                        {
                            currentCheckP = nextBoardElementDestinations.Pop();
                            old_path.Add(currentCheckP);
                        }

                        nextBoardElementDestinations.Clear();
                        TryDestination(node, board);
                        old_path.Reverse();
                        old_path.AddRange(nextBoardElementDestinations.ToList());
                        old_path.Reverse();
                        nextBoardElementDestinations.Clear();
                        foreach (Point p in old_path)
                        {
                            nextBoardElementDestinations.Push(p);
                        }
                         */

                        foreach (Point p in nextBoardElementDestinations)
                        {
                            if (p != getCurrentBoardLocation() && p != getDestinationBoardLocation() &&
                                board.GetElementAtPoint(p).type == HackGameBoardElementBaseType.HackGameBoardElementBaseType_Node
                                && board.GetElementAtPoint(p).GetCurrentState() == HackGameBoardElement.HackGameBoardElement_State.HackGameBoardElement_State_Active)
                            {
                                Stack<Point> fromDestination = TryDestinationBase(getDestinationBoardLocation(), node, board);
                                if (fromDestination.Count > 0)
                                {
                                    nextBoardElementDestinations.Clear();
                                    nextBoardElementDestinations = fromDestination;
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            }

                        }
                        return false;
                        //return false;
                    }
                }
            }
            /*
        else if (node == getCurrentBoardLocation() || node == getDestinationBoardLocation() || IsInDestinationPath(node))
        {
                
                
                
            //the player is already moving. one of three cases


            //ELIMINATED - TRYING ALL-KILL//1. it's the player's location or destination - kill path
            //ELIMINATED - TRYING ALL-KILL//2. it's a node already on the path - shorten path
            //ELIMINATED - TRYING ALL-KILL//3. it's a node outside the path - lengthen path

            if (node == getCurrentBoardLocation() || node == getDestinationBoardLocation())
            {
                //you can't kill it off entirely, you have to clear it all the way back to the first valid node.
                if (board.board[getDestinationBoardLocation().Y, getDestinationBoardLocation().X].type == HackGameBoardElementBaseType.HackGameBoardElementBaseType_Node)
                {
                    nextBoardElementDestinations.Clear();
                }
                else
                {
                    foreach (Point p in nextBoardElementDestinations)
                    {
                        if (p != getCurrentBoardLocation() && p != getDestinationBoardLocation() &&
                            board.GetElementAtPoint(p).type == HackGameBoardElementBaseType.HackGameBoardElementBaseType_Node)
                        {
                            //hah, recursion.
                            nextBoardElementDestinations.Clear();
                            return TryDestination(p, board);
                        }
                    }
                }
                return true;
            }

            else if (IsInDestinationPath(node))
            {
                foreach (Point p in nextBoardElementDestinations)
                {
                    if (p == node)
                    {
                        //hah, recursion.
                        nextBoardElementDestinations.Clear();
                        return TryDestination(p, board);
                    }
                }
            }

            return false;
        }

        else // it's lengthening the path
        {
            if (nextBoardElementDestinations.Count > 0)
            {
                //find the last node in the stack
                pf.Reset(nextBoardElementDestinations.ElementAt(nextBoardElementDestinations.Count - 1), node);
                bool success = pf.SearchPath();
                if (success == true)
                {
                    //add the final path to the END OF THE stack.
                    //1 turn stack into old list
                    //2 clear stack
                    //3 push new reverse list onto stack
                    //4 push old list onto stack
                    LinkedList<Point> new_path = pf.FinalPath();
                    new_path.RemoveFirst();

                    List<Point> old_path = nextBoardElementDestinations.ToList();
                    nextBoardElementDestinations.Clear();

                    foreach (Point p in new_path.Reverse())
                    {
                        nextBoardElementDestinations.Push(p);
                    }
                    old_path.Reverse();
                    foreach (Point p in old_path)
                    {
                        nextBoardElementDestinations.Push(p);
                    }
                    return true;
                }
            }
            return false;
        }
             */
        }


        public override void ArrivedAtDestination()
        {
            //send out a ping
            StartPing();
            base.ArrivedAtDestination();
        }

        public void StartPing()
        {
            StartPing_Update = true;
        }

        public bool IsInDestinationPath(Point node)
        {
            if (node.X == this.currentBoardElementDestination.X && node.Y == this.currentBoardElementDestination.Y)
            {
                return true;
            }
            foreach (Point p in this.nextBoardElementDestinations)
            {
                if (node.X == p.X && node.Y == p.Y)
                {
                    return true;
                }
            }

            return false;
        }



        public override bool IsDestinationAllowed(Point node, HackGameBoard board)
        {
            //if location is a node
            if (board.InBoard(node) &&
                board.board[node.Y, node.X].type == HackGameBoardElementBaseType.HackGameBoardElementBaseType_Node &&
                board.IsNodeAlive(board, node))
            {
                return true;
            }
            return false;
        }


        public void SetHacking(bool p)
        {
            isHacking = p;
        }

        public bool IsHacking()
        {
            return isHacking;
        }

        public void HackSuccess()
        {
            hackSuccess = true;
        }

        public float getSpawnInZoom()
        {
            if (spawnInData != null && spawnInData.dropInLerp != null)
            {
                return spawnInData.dropInLerp.CurrentScale();
            }
            else
            {
                return 1.0f;
            }
        }

        public float getExitOutZoom()
        {
            if (exitOutData != null && exitOutData.flyOutLerp != null)
            {
                return exitOutData.flyOutLerp.CurrentScale();
            }
            else
            {
                return 1.0f;
            }
        }
    }

    class HackGameAgent_AI : HackGameAgent
    {

        enum HackGameAgent_AI_State
        {
            HackGameAgent_AI_State_Wander,
            HackGameAgent_AI_State_Investigate,
            HackGameAgent_AI_State_Prosecute
        };

        enum HackGameAgent_AI_Wander_SubState
        {
            HackGameAgent_AI_State_Wander_Substate_Ping,
            HackGameAgent_AI_State_Wander_Substate_AnalyzePing,
            HackGameAgent_AI_State_Wander_Substate_Loiter
        };


        HackGameAgent_AI_State currentAIState = HackGameAgent_AI_State.HackGameAgent_AI_State_Wander;
        HackGameAgent_AI_Wander_SubState wanderSubState = HackGameAgent_AI_Wander_SubState.HackGameAgent_AI_State_Wander_Substate_Loiter;
        HackGameAgent_AI_StateData_SpawningIn spawnInData;

        float Wander_movementSpeed = 0.79f;
        float Investigate_movementSpeed = 1.04f;
        float Prosecute_movementSpeed = 1.3f;

        bool StartPing_Update = false;
        bool StartPing_Draw = false;


        public HackGameAgent_AI(HackGameBoard board)
            : base(board)
        {

        }

        public void SetInitialRandom(HackGameBoard board, HackGameAgent_Player player)
        {
            //get all "nodes"
            List<Point> allNodes = new List<Point>();
            for (int i = 0; i < board.GetGameBoardSize(); i++)
            {
                for (int k = 0; k < board.GetGameBoardSize(); k++)
                {
                    if (board.board[i, k].type == HackGameBoardElementBaseType.HackGameBoardElementBaseType_Node &&
                        board.board[i, k].GetCurrentState() == HackGameBoardElement.HackGameBoardElement_State.HackGameBoardElement_State_Active)
                    {
                        if (player.getCurrentBoardLocation() != new Point(k, i))
                        {
                            allNodes.Add(new Point(k, i));
                        }
                    }
                }
            }

            if (allNodes.Count > 0)
            {
                Point newCur = allNodes[board.r.Next(0, allNodes.Count)];
                setCurrentBoardLocation(newCur, board);
            }
            else
            {
                Kill(0);
            }
        }



        public override void DrawCurrentState(HackNodeGameBoardMedia gameboarddrawing, SpriteBatch sb, Vector2 drawPos, float zoom)
        {

            switch (GetCurrentState())
            {
                case HackGameAgent_State.HackGameAgent_State_SpawningIn:
                    DrawSpawningInState(gameboarddrawing, sb, drawPos, zoom);
                    break;
                case HackGameAgent_State.HackGameAgent_State_Active:
                    DrawActiveState(gameboarddrawing, sb, drawPos, zoom);
                    break;
                case HackGameAgent_State.HackGameAgent_State_BeingKilled:
                    DrawBeingKilledState(gameboarddrawing, sb, drawPos, zoom);
                    break;
            }
            base.DrawCurrentState(gameboarddrawing, sb, drawPos, zoom);
        }

        private void DrawSpawningInState(HackNodeGameBoardMedia gameboarddrawing, SpriteBatch sb, Vector2 drawPos, float zoom)
        {
            if (spawnInData.StartSpawnSound)
            {
                gameboarddrawing.WhooshSound.Play();
                spawnInData.StartSpawnSound = false;
            }
            if (spawnInData.DrawImpact)
            {
                AddUIElement(gameboarddrawing.PingTexture, 1.0f, new Vector2(41.0f * zoom, 41.0f * zoom), new Vector2(41.0f * zoom, 41.0f * zoom), new Color(1.0f, 0.0f, 0, 0.0f), new Color(0.0f, 0, 0, 0), 0.2f, 2.5f, 0.0f);
                spawnInData.DrawImpact = false;
                gameboarddrawing.ThumpSound.Play();
            }

            if (spawnInData.flashing)
            {
                if (spawnInData.dropInFlash.IsOn())
                {
                    sb.Draw(gameboarddrawing.AITexture, drawPos, null, Color.White, 0.0f, Vector2.Zero, zoom, SpriteEffects.None, 0);
                }
            }

            else
            {
                sb.Draw(gameboarddrawing.AITexture, drawPos, null, Color.White, 0.0f, Vector2.Zero, zoom * spawnInData.dropInLerp.CurrentScale(), SpriteEffects.None, 0);
            }
        }

        private void DrawBeingKilledState(HackNodeGameBoardMedia gameboarddrawing, SpriteBatch sb, Vector2 drawPos, float zoom)
        {
            gameboarddrawing.explosion.AddParticles(drawPos + new Vector2(41.0f * zoom, 41.0f * zoom), Vector2.Zero);
            gameboarddrawing.AIDeathParticles.AddParticles(drawPos + new Vector2(41.0f * zoom, 41.0f * zoom), Vector2.Zero);
            gameboarddrawing.ExplosionSound.Play();
            SetToRemove();
        }

        private void DrawActiveState(HackNodeGameBoardMedia gameboarddrawing, SpriteBatch sb, Vector2 drawPos, float zoom)
        {
            //because we have to rotate these guys using a center origin,
            //we have to apply a backward draw shift.
            Vector2 scaleVector = Vector2.Zero;
            scaleVector.X += drawPos.X + 40.0f * zoom;
            scaleVector.Y += drawPos.Y + 40.0f * zoom;

            float rotation = 0.0f; //default north
            switch (this.getStoredMovementDirection())
            {
                case MovementDirection.MovementDirection_South:
                    rotation = MathHelper.ToRadians(180.0f);
                    break;
                case MovementDirection.MovementDirection_West:
                    rotation = MathHelper.ToRadians(270.0f);
                    break;
                case MovementDirection.MovementDirection_East:
                    rotation = MathHelper.ToRadians(90.0f);
                    break;
                default:
                    break;
            }
            sb.Draw(gameboarddrawing.AITexture, scaleVector, null, Color.White, rotation, new Vector2(40.0f, 40.0f), zoom, SpriteEffects.None, 0);

            if (StartPing_Draw)
            {
                AddUIElement(gameboarddrawing.PingTexture, 1.0f, new Vector2(41.0f, 41.0f), new Vector2(41.0f, 41.0f), new Color(1.0f, 0.0f, 0, 0.0f), new Color(0.0f, 0, 0, 0), 0.2f, 1.5f, 0.0f);
                StartPing_Draw = false;
            }
        }

        public override void EnteringNewState(HackGameAgent.HackGameAgent_State oldState, HackGameAgent.HackGameAgent_State newState)
        {
            if (newState == HackGameAgent_State.HackGameAgent_State_SpawningIn && oldState == HackGameAgent_State.HackGameAgent_State_Inactive)
            {
                spawnInData = new HackGameAgent_AI_StateData_SpawningIn();
            }
        }

        public override void UpdateState(GameTime time, HackGameBoard board, HackNodeGameBoardMedia drawing)
        {

            switch (GetCurrentState())
            {
                case HackGameAgent_State.HackGameAgent_State_SpawningIn:
                    UpdateSpawningInState(time, board, drawing);
                    break;
                case HackGameAgent_State.HackGameAgent_State_Active:
                    UpdateActiveState(time, board, drawing);
                    break;
            }

            base.UpdateState(time, board, drawing);


        }

        private void UpdateSpawningInState(GameTime time, HackGameBoard board, HackNodeGameBoardMedia drawing)
        {
            spawnInData.totalTimer -= (float)time.ElapsedGameTime.TotalSeconds;
            if (spawnInData.totalTimer <= 0)
            {
                this.SetCurrentState(HackGameAgent_State.HackGameAgent_State_Active);
            }
            else
            {
                if (spawnInData.flashing)
                {
                    spawnInData.dropInFlash.Update(time);
                }
                else
                {
                    spawnInData.dropInLerp.Update(time);
                    if (!spawnInData.dropInLerp.IsAlive())
                    {
                        spawnInData.DrawImpact = true;
                        spawnInData.StartFlashing();
                        board.AddBackgroundTextEmergency(new StringBuilder("[WARNING] AI ACTIVATING"), 0);
                        board.AddBackgroundTextEmergency(new StringBuilder("SENSORS PICKING UP ICE"), 0.5f);
                        board.AddBackgroundTextEmergency(new StringBuilder("DANGER-DANGER-DANGER"), 0.25f);
                    }
                }
            }
        }

        private void UpdateActiveState(GameTime time, HackGameBoard board, HackNodeGameBoardMedia drawing)
        {
            switch (currentAIState)
            {
                case HackGameAgent_AI_State.HackGameAgent_AI_State_Wander:
                    Wander_Update(time, board);
                    break;
            }

            if (StartPing_Update)
            {
                //be sure we're in a node.
                if (board.InBoard(getCurrentBoardLocation()) && board.GetElementAtPoint(getCurrentBoardLocation()).type == HackGameBoardElementBaseType.HackGameBoardElementBaseType_Node)
                {
                    //do some other stuff here.
                    StartPing_Draw = true;
                    StartPing_Update = false;
                }
            }
        }

        public void Wander_Update(GameTime time, HackGameBoard board)
        {

            if (this.getMovementDirection() != MovementDirection.MovementDirection_None)
            {
                setTtoDestination(getTtoDestination() + Wander_movementSpeed * (float)time.ElapsedGameTime.TotalSeconds * board.GetSpeedUpFactor());
            }

            else if (nextBoardElementDestinations.Count > 0)
            {
                setDestinationBoardLocation(nextBoardElementDestinations.Pop(), board);
            }

            else
            {
                // we are in the "arrived" sub-mode.
                switch (wanderSubState)
                {
                    case HackGameAgent_AI_Wander_SubState.HackGameAgent_AI_State_Wander_Substate_Ping:
                        Wander_Ping(board);
                        break;
                    case HackGameAgent_AI_Wander_SubState.HackGameAgent_AI_State_Wander_Substate_AnalyzePing:
                        Wander_AnalyzePing(board);
                        break;
                    case HackGameAgent_AI_Wander_SubState.HackGameAgent_AI_State_Wander_Substate_Loiter:
                        Wander_Loiter(board);
                        break;
                }
            }

        }

        private void Wander_Loiter(HackGameBoard board)
        {
            // for now, just move.
            SetRandomDestination(board);
        }

        private void Wander_AnalyzePing(HackGameBoard board)
        {
            //move right away to loiter.
            wanderSubState = HackGameAgent_AI_Wander_SubState.HackGameAgent_AI_State_Wander_Substate_Loiter;
        }

        private void Wander_Ping(HackGameBoard board)
        {
            board.AddBackgroundTextEmergency(new StringBuilder("AI #3180 PING..."), 0);
            board.AddBackgroundTextEmergency(new StringBuilder("AI RESOLVE OK"), 0.75f);

            StartPing_Update = true;
            //move right away to analyze.
            wanderSubState = HackGameAgent_AI_Wander_SubState.HackGameAgent_AI_State_Wander_Substate_AnalyzePing;
        }


        public override void ArrivedAtDestination()
        {
            switch (currentAIState)
            {
                case HackGameAgent_AI_State.HackGameAgent_AI_State_Wander:
                    if (nextBoardElementDestinations.Count <= 0)
                    {
                        wanderSubState = HackGameAgent_AI_Wander_SubState.HackGameAgent_AI_State_Wander_Substate_Ping;
                    }
                    break;
            }

            base.ArrivedAtDestination();
        }

        public override bool IsDestinationAllowed(Point node, HackGameBoard board)
        {
            //don't allow movement yet if you're moving.
            if (this.currentBoardElementLocation != this.currentBoardElementDestination
                || (this.getTtoDestination() < 1.0f && this.getTtoDestination() > 0.0f))
            {
                return false;
            }
            return true;
        }
    }

    class HackGameAgent_AI_StateData_SpawningIn
    {
        public bool flashing = false;
        public HackGameForwardLerpDrawHelper dropInLerp;
        public FlashingElement dropInFlash;
        public bool DrawImpact = false;
        public bool StartSpawnSound = true;


        public float totalTimer;

        const float AISpawnInTotalLerpTime = 1.0f;
        const float AISpawnInFlashTime = 0.25f;

        const float AISpawnInTotalTime = 3.0f;

        public HackGameAgent_AI_StateData_SpawningIn()
        {
            dropInLerp = new HackGameForwardLerpDrawHelper(AISpawnInTotalLerpTime, 10.0f, 1.0f, AISpawnInTotalLerpTime, Color.White, Color.White, AISpawnInTotalLerpTime, Vector2.Zero, Vector2.Zero, AISpawnInTotalLerpTime);
            totalTimer = AISpawnInTotalTime;
        }

        public void StartFlashing()
        {
            dropInFlash = new FlashingElement(AISpawnInFlashTime, true, FlashingElement.FlashingElement_OperationType.FlashingElement_OperationType_Normal);
            flashing = true;
        }
    }

    abstract class HackGameAgent_Projectile : HackGameAgent
    {



        public HackGameAgent_Projectile(HackGameBoard b)
            : base(b)
        {
            //do nothin'
        }

        public override void UpdateState(GameTime time, HackGameBoard board, HackNodeGameBoardMedia drawing)
        {
            switch (GetCurrentState())
            {
                case HackGameAgent_State.HackGameAgent_State_Active:
                    UpdateActiveState(time, board, drawing);
                    break;
            }

            base.UpdateState(time, board, drawing);
        }

        public override void DrawCurrentState(HackNodeGameBoardMedia gameboarddrawing, SpriteBatch sb, Vector2 drawPos, float zoom)
        {
            switch (GetCurrentState())
            {
                case HackGameAgent_State.HackGameAgent_State_Active:
                    DrawActiveState(gameboarddrawing, sb, drawPos, zoom);
                    break;
                case HackGameAgent_State.HackGameAgent_State_BeingKilled:
                    DrawBeingKilledState(gameboarddrawing, sb, drawPos, zoom);
                    break;
            }


            base.DrawCurrentState(gameboarddrawing, sb, drawPos, zoom);
        }



        public override void EnteringNewState(HackGameAgent.HackGameAgent_State oldState, HackGameAgent.HackGameAgent_State newState)
        {
        }

        public override bool IsDestinationAllowed(Point node, HackGameBoard board)
        {
            //if location is a node
            if (board.InBoard(node) && board.board[node.Y, node.X].type == HackGameBoardElementBaseType.HackGameBoardElementBaseType_Node
                && board.IsNodeAlive(board, node))
            {
                return true;
            }
            return false;
        }



        public abstract void DrawBeingKilledState(HackNodeGameBoardMedia gameboarddrawing, SpriteBatch sb, Vector2 drawPos, float zoom);

        public abstract void DrawActiveState(HackNodeGameBoardMedia gameboarddrawing, SpriteBatch sb, Vector2 drawPos, float zoom);

        public abstract void UpdateActiveState(GameTime time, HackGameBoard board, HackNodeGameBoardMedia drawing);

        public override void ArrivedAtDestination()
        {
            base.ArrivedAtDestination();
        }


    }

    class HackGameAgent_Projectile_Multimissile : HackGameAgent_Projectile
    {
        HackGameBoard ourBoard;
        const float lifeTimeMultimissile = 10.0f;
        const float multimissile_movementSpeed = 3.0f;
        bool killEffectPlayed = false;
        MovementDirection permanentDirection = MovementDirection.MovementDirection_None;
        const float multimissileSplashDamageThreshhold = 0.8f;


        public HackGameAgent_Projectile_Multimissile(HackGameBoard b, MovementDirection fireDirection)
            : base(b)
        {
            killTimer = new HackGameTimer(lifeTimeMultimissile);
            ourBoard = b;
            setCurrentBoardLocation(ourBoard.GetPlayer().getCurrentBoardLocation(), b);
            //set direction
            permanentDirection = fireDirection;
            SetCurrentState(HackGameAgent_State.HackGameAgent_State_Active);
            if (!ChooseNextPoint())
            {
                SetToRemove();
            }
        }

        public override void DrawBeingKilledState(HackNodeGameBoardMedia gameboarddrawing, SpriteBatch sb, Vector2 drawPos, float zoom)
        {
            if (killEffectPlayed == false)
            {
                killEffectPlayed = true;
                gameboarddrawing.explosion.AddParticles(drawPos + new Vector2(41.0f * zoom, 41.0f * zoom), Vector2.Zero);
                gameboarddrawing.ExplosionSound.Play();
                SetToRemove();
            }
        }

        public override void DrawActiveState(HackNodeGameBoardMedia gameboarddrawing, SpriteBatch sb, Vector2 drawPos, float zoom)
        {
            sb.Draw(gameboarddrawing.ProjectileTexture, drawPos, null, Color.White, 0.0f, Vector2.Zero, zoom, SpriteEffects.None, 0);
        }

        public override void ArrivedAtDestination()
        {
            base.ArrivedAtDestination();
            if (!ChooseNextPoint())
            {
                AffectNearbyAIs(ourBoard);
                Kill(0);
            }
        }

        public override void HasCollidedWith(HackGameAgent otherAgent, HackGameBoard board)
        {
            if (otherAgent is HackGameAgent_AI && IsActive() && otherAgent.IsActive())
            {
                //KABOOM!
                otherAgent.Kill(0);
                //DON'T KILL SELF, KEEP GOING
                board.AddBackgroundTextAward(new StringBuilder("000000 000000 000000 00  00"), 0);
                board.AddBackgroundTextAward(new StringBuilder("0    0 0    0 0    0 0 00 0"), 0.1f);
                board.AddBackgroundTextAward(new StringBuilder("00000  0    0 0    0 0    0"), 0.1f);
                board.AddBackgroundTextAward(new StringBuilder("0    0 0    0 0    0 0    0"), 0.1f);
                board.AddBackgroundTextAward(new StringBuilder("000000 000000 000000 0    0"), 0.1f);
            }

            base.HasCollidedWith(otherAgent, board);
        }

        public void AffectNearbyAIs(HackGameBoard board)
        {
            HackGameAgent[] agents = board.GetAgents();
            for (int i = 0; i < agents.Length; i++)
            {
                if (agents[i] is HackGameAgent_AI && agents[i].GetCurrentState() == HackGameAgent_State.HackGameAgent_State_Active)
                {
                    if ((agents[i].getCurrentBoardLocation() == getCurrentBoardLocation() && agents[i].getTtoDestination() < multimissileSplashDamageThreshhold) ||
                        (agents[i].getDestinationBoardLocation() == getCurrentBoardLocation() && agents[i].getTtoDestination() > 1.0 - multimissileSplashDamageThreshhold))
                    {
                        agents[i].Kill(0);
                        board.AddBackgroundTextAward(new StringBuilder("000000 000000 000000 00  00"), 0);
                        board.AddBackgroundTextAward(new StringBuilder("0    0 0    0 0    0 0 00 0"), 0.1f);
                        board.AddBackgroundTextAward(new StringBuilder("00000  0    0 0    0 0    0"), 0.1f);
                        board.AddBackgroundTextAward(new StringBuilder("0    0 0    0 0    0 0    0"), 0.1f);
                        board.AddBackgroundTextAward(new StringBuilder("000000 000000 000000 0    0"), 0.1f);
                    }
                }
            }
        }

        private bool ChooseNextPoint()
        {
            Point current = currentBoardElementLocation;
            HackGameBoardElement node = ourBoard.getElementInDirection(current, permanentDirection);
            Point nodepoint = ourBoard.getPointInDirection(current, permanentDirection);
            if (node == null || nodepoint == current)
            {
                return false;
            }
            else if (permanentDirection == MovementDirection.MovementDirection_North || permanentDirection == MovementDirection.MovementDirection_South)
            {
                if (node.type == HackGameBoardElementBaseType.HackGameBoardElementBaseType_Node || node.type == HackGameBoardElementBaseType.HackGameBoardElementBaseType_Bridge_NS)
                {
                    setDestinationBoardLocation(nodepoint, ourBoard);
                }
                else
                {
                    return false;
                }
            }
            else if (permanentDirection == MovementDirection.MovementDirection_East || permanentDirection == MovementDirection.MovementDirection_West)
            {
                if (node.type == HackGameBoardElementBaseType.HackGameBoardElementBaseType_Node || node.type == HackGameBoardElementBaseType.HackGameBoardElementBaseType_Bridge_EW)
                {
                    setDestinationBoardLocation(nodepoint, ourBoard);
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        public override void UpdateActiveState(GameTime time, HackGameBoard board, HackNodeGameBoardMedia drawing)
        {
            if (this.getMovementDirection() != MovementDirection.MovementDirection_None)
            {
                setTtoDestination(getTtoDestination() + multimissile_movementSpeed * (float)time.ElapsedGameTime.TotalSeconds * board.GetSpeedUpFactor());
            }
            else if (nextBoardElementDestinations.Count > 0)
            {
                setDestinationBoardLocation(nextBoardElementDestinations.Pop(), board);
            }
        }
    }

    class HackGameAgent_Projectile_Heatseeker : HackGameAgent_Projectile
    {
        HackGameBoard ourBoard;
        HackGameAgent target;
        HackGameTimer pingTimer;

        const float lifeTimeHeatseeker = 10.0f;
        const float heatseeker_movementSpeed = 3.0f;

        const float heatseeker_pingTime = 0.6f;

        bool playPing = false;
        //bool inLeadPursuit = true;
        bool killEffectPlayed = false;

        public HackGameAgent_Projectile_Heatseeker(HackGameBoard b)
            : base(b)
        {
            killTimer = new HackGameTimer(lifeTimeHeatseeker);
            ourBoard = b;
            setCurrentBoardLocation(ourBoard.GetPlayer().getCurrentBoardLocation(), b);
            //pick closest target
            target = PickClosestTarget(ourBoard);
            AlignToTarget(target, ourBoard);
            SetCurrentState(HackGameAgent_State.HackGameAgent_State_Active);
            pingTimer = new HackGameTimer(heatseeker_pingTime);

        }

        private void AlignToTarget(HackGameAgent target, HackGameBoard ourBoard)
        {
            if (target == null || !target.IsActive())
            {
                //WACKY MISSILE!
                target = null;
                SetRandomDestination(ourBoard);
            }
            else
            {
                //easy case - enemy is going to be at a node as his next immediate destination
                if (ourBoard.GetElementAtPoint(target.getDestinationBoardLocation()).type == HackGameBoardElementBaseType.HackGameBoardElementBaseType_Node)
                {
                    TryDestination(target.getDestinationBoardLocation(), ourBoard);
                }

                //harder case - enemy is on a bridge somewhere, not immediately headed for a node
                else
                {
                    //you need to find the target's next NODE it's going to hit.
                    bool found = false;
                    Stack<Point> nextDestinations = target.GetNextDestinations();
                    foreach (Point p in nextDestinations.Reverse())
                    {
                        if (ourBoard.GetElementAtPoint(p).type == HackGameBoardElementBaseType.HackGameBoardElementBaseType_Node)
                        {
                            TryDestination(p, ourBoard);
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        //WACKY MISSILE!
                        target = null;
                        SetRandomDestination(ourBoard);
                    }
                }
            }
        }

        private HackGameAgent PickClosestTarget(HackGameBoard ourBoard)
        {
            int maxdistance = int.MaxValue;
            int checkdist = 0;
            HackGameAgent currentTarget = null;
            Point playerLoc = ourBoard.GetPlayer().getCurrentBoardLocation();

            HackGameAgent[] targets = ourBoard.GetAgents();
            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] is HackGameAgent_AI)
                {
                    checkdist = GetLengthToDestination(getCurrentBoardLocation(), targets[i].getCurrentBoardLocation(), ourBoard);
                    if (checkdist != -1 && checkdist < maxdistance)
                    {
                        maxdistance = checkdist;
                        currentTarget = targets[i];
                    }
                }
            }

            return currentTarget;
        }

        public override void ArrivedAtDestination()
        {
            Point node = currentBoardElementLocation;
            base.ArrivedAtDestination();

            //if we're out of moves, realign
            if (nextBoardElementDestinations.Count <= 0)
            {
                AlignToTarget(target, ourBoard);
            }
            /*
            if (ourBoard.InBoard(node) && ourBoard.board[node.Y, node.X].type == HackGameBoardElementBaseType.HackGameBoardElementBaseType_Node)
            {
                
            }*/
        }

        public override void HasCollidedWith(HackGameAgent otherAgent, HackGameBoard board)
        {
            if (otherAgent is HackGameAgent_AI && IsActive() && otherAgent.IsActive())
            {
                //KABOOM!
                otherAgent.Kill(0);
                Kill(0);
                board.AddBackgroundTextAward(new StringBuilder("000000 000000 000000 00  00"), 0);
                board.AddBackgroundTextAward(new StringBuilder("0    0 0    0 0    0 0 00 0"), 0.1f);
                board.AddBackgroundTextAward(new StringBuilder("00000  0    0 0    0 0    0"), 0.1f);
                board.AddBackgroundTextAward(new StringBuilder("0    0 0    0 0    0 0    0"), 0.1f);
                board.AddBackgroundTextAward(new StringBuilder("000000 000000 000000 0    0"), 0.1f);
            }
            base.HasCollidedWith(otherAgent, board);
        }

        public override void UpdateActiveState(GameTime time, HackGameBoard board, HackNodeGameBoardMedia drawing)
        {
            pingTimer.Update(time);
            if (!pingTimer.IsAlive())
            {
                pingTimer.Reset(heatseeker_pingTime);
                playPing = true;
            }
            if (this.getMovementDirection() != MovementDirection.MovementDirection_None)
            {
                setTtoDestination(getTtoDestination() + heatseeker_movementSpeed * (float)time.ElapsedGameTime.TotalSeconds * board.GetSpeedUpFactor());
            }
            else if (nextBoardElementDestinations.Count > 0)
            {
                setDestinationBoardLocation(nextBoardElementDestinations.Pop(), board);
            }
        }

        public override void DrawBeingKilledState(HackNodeGameBoardMedia gameboarddrawing, SpriteBatch sb, Vector2 drawPos, float zoom)
        {
            if (killEffectPlayed == false)
            {
                killEffectPlayed = true;
                gameboarddrawing.explosion.AddParticles(drawPos + new Vector2(41.0f * zoom, 41.0f * zoom), Vector2.Zero);
                gameboarddrawing.ExplosionSound.Play();
                SetToRemove();
            }
        }

        public override void DrawActiveState(HackNodeGameBoardMedia gameboarddrawing, SpriteBatch sb, Vector2 drawPos, float zoom)
        {
            if (playPing)
            {
                playPing = false;
                AddUIElement(gameboarddrawing.PingTexture, 0.25f, new Vector2(41.0f, 41.0f), new Vector2(41.0f, 41.0f), new Color(1.0f, 0.4f, 0, 0.0f), new Color(0.0f, 0, 0, 0), 0.2f, 0.8f, 0.0f);
                gameboarddrawing.ProximityAlertSound.Play();
            }
            sb.Draw(gameboarddrawing.ProjectileTexture, drawPos, null, Color.White, 0.0f, Vector2.Zero, zoom, SpriteEffects.None, 0);
        }


    }

    class HackGameAgent_Projectile_Mortar : HackGameAgent_Projectile
    {
        HackGameBoard ourBoard;
        const float lerpOutTime = 1.5f;
        const float fragmentFallTimeStartMin = 0.25f;
        const float fragmentFallTimeStartMax = 1.5f;
        HackGameForwardLerpDrawHelper lerp;

        static Point[] mortarFragmentTargets = new Point[16] {
            new Point(1,0),
            new Point(0,1),
            new Point(1,1),
            new Point(-1,-1),
            new Point(-1,1),
            new Point(1,-1),
            new Point(-1,0),
            new Point(0,-1),

            new Point(2,2),
            new Point(2,-2),
            new Point(-2,2),
            new Point(-2,-2),
            new Point(2,0),
            new Point(0,2),
            new Point(-2,0),
            new Point(0,-2)
        };


        public HackGameAgent_Projectile_Mortar(HackGameBoard b)
            : base(b)
        {
            ourBoard = b;
            setCurrentBoardLocation(ourBoard.GetPlayer().getCurrentBoardLocation(), b);
            SetCurrentState(HackGameAgent_State.HackGameAgent_State_Active);
            lerp = new HackGameForwardLerpDrawHelper(
                lerpOutTime, 1.0f, 10.0f, lerpOutTime, Color.White, Color.White, lerpOutTime, Vector2.Zero, Vector2.Zero, lerpOutTime);
        }

        public override void DrawBeingKilledState(HackNodeGameBoardMedia gameboarddrawing, SpriteBatch sb, Vector2 drawPos, float zoom)
        {
            //nothing
        }

        public override void DrawActiveState(HackNodeGameBoardMedia gameboarddrawing, SpriteBatch sb, Vector2 drawPos, float zoom)
        {
            sb.Draw(gameboarddrawing.ProjectileTexture, drawPos + lerp.CurrentPosition(), null, lerp.CurrentColor(), 0.0f, Vector2.Zero, zoom * lerp.CurrentScale(), SpriteEffects.None, 0);
        }

        public override void UpdateActiveState(GameTime time, HackGameBoard board, HackNodeGameBoardMedia drawing)
        {
            lerp.Update(time);
            if (!lerp.IsAlive())
            {
                int totalspawned = 0;
                //spawn all the awesome fragments.
                foreach (Point p in mortarFragmentTargets)
                {
                    Point destinationPoint = new Point(currentBoardElementLocation.X + p.X, currentBoardElementLocation.Y + p.Y);
                    if (ourBoard != null && ourBoard.IsNodeAlive(ourBoard, destinationPoint))
                    {
                        HackGameBoardElement el = ourBoard.GetElementAtPoint(destinationPoint);
                        if (el.type == HackGameBoardElementBaseType.HackGameBoardElementBaseType_Node ||
                            el.type == HackGameBoardElementBaseType.HackGameBoardElementBaseType_Bridge_EW ||
                            el.type == HackGameBoardElementBaseType.HackGameBoardElementBaseType_Bridge_NS)
                        {
                            //fire off a mortar fragment at this node
                            HackGameAgent_Projectile_MortarFragment fragment = new HackGameAgent_Projectile_MortarFragment(board, destinationPoint, MathHelper.Lerp(fragmentFallTimeStartMin, fragmentFallTimeStartMax, (float)ourBoard.r.NextDouble()));
                            board.AddAgent(fragment);
                            totalspawned++;
                        }
                    }
                }

                if (totalspawned > 0)
                {
                    board.GetMedia().MortarFallSound.Play();
                }

                Kill(0);
            }
        }

    }


    class HackGameAgent_Projectile_MortarFragment : HackGameAgent_Projectile
    {
            HackGameBoard ourBoard;
            const float lerpOutTime = 1.0f;
            const float mortarSplashDamageThreshhold = 0.8f;
            HackGameForwardLerpDrawHelper lerp;
            float timeToStart;
            bool killEffectPlayed = false;

           

            public HackGameAgent_Projectile_MortarFragment(HackGameBoard b, Point p, float timeToStartFalling)
                : base(b)
            {
                ourBoard = b;
                setCurrentBoardLocation(p, b);
                SetCurrentState(HackGameAgent_State.HackGameAgent_State_SpawningIn);
                timeToStart = timeToStartFalling;

                lerp = new HackGameForwardLerpDrawHelper(
                lerpOutTime, 10.0f, 1.0f, lerpOutTime, Color.White, Color.White, lerpOutTime, Vector2.Zero, Vector2.Zero, lerpOutTime);
            }

            public override void UpdateActiveState(GameTime time, HackGameBoard board, HackNodeGameBoardMedia drawing)
            {
                lerp.Update(time);
                if (!lerp.IsAlive())
                {
                    //it has landed. create an explosion and hurt stuff.
                    AffectNearbyAIs(ourBoard);
                    Kill(0);
                }
            }

            public override void UpdateState(GameTime time, HackGameBoard board, HackNodeGameBoardMedia drawing)
            {
                switch (GetCurrentState())
                {
                    case HackGameAgent_State.HackGameAgent_State_SpawningIn:
                        UpdateSpawningInState(time, board, drawing);
                        break;
                }

                base.UpdateState(time, board, drawing);
            }

            private void UpdateSpawningInState(GameTime time, HackGameBoard board, HackNodeGameBoardMedia drawing)
            {
                timeToStart -= (float)time.ElapsedGameTime.TotalSeconds;
                if (timeToStart <= 0)
                {
                    SetCurrentState(HackGameAgent_State.HackGameAgent_State_Active);
                }

            }


            public override void DrawBeingKilledState(HackNodeGameBoardMedia gameboarddrawing, SpriteBatch sb, Vector2 drawPos, float zoom)
            {
                if (killEffectPlayed == false)
                {
                    killEffectPlayed = true;
                    gameboarddrawing.explosion.AddParticles(drawPos + new Vector2(41.0f * zoom, 41.0f * zoom), Vector2.Zero);
                    gameboarddrawing.ExplosionSound.Play();
                    SetToRemove();
                }
            }

            public override void DrawActiveState(HackNodeGameBoardMedia gameboarddrawing, SpriteBatch sb, Vector2 drawPos, float zoom)
            {
                sb.Draw(gameboarddrawing.ProjectileTexture, drawPos + lerp.CurrentPosition(), null, lerp.CurrentColor(), 0.0f, Vector2.Zero, zoom * lerp.CurrentScale(), SpriteEffects.None, 0);
            }

            public void AffectNearbyAIs(HackGameBoard board)
            {
                HackGameAgent[] agents = board.GetAgents();
                for (int i = 0; i < agents.Length; i++)
                {
                    if (agents[i] is HackGameAgent_AI && agents[i].GetCurrentState() == HackGameAgent_State.HackGameAgent_State_Active)
                    {
                        if ((agents[i].getCurrentBoardLocation() == getCurrentBoardLocation() && agents[i].getTtoDestination() < mortarSplashDamageThreshhold) ||
                            (agents[i].getDestinationBoardLocation() == getCurrentBoardLocation() && agents[i].getTtoDestination() > 1.0 - mortarSplashDamageThreshhold))
                        {
                            agents[i].Kill(0);
                            board.AddBackgroundTextAward(new StringBuilder("000000 000000 000000 00  00"), 0);
                            board.AddBackgroundTextAward(new StringBuilder("0    0 0    0 0    0 0 00 0"), 0.1f);
                            board.AddBackgroundTextAward(new StringBuilder("00000  0    0 0    0 0    0"), 0.1f);
                            board.AddBackgroundTextAward(new StringBuilder("0    0 0    0 0    0 0    0"), 0.1f);
                            board.AddBackgroundTextAward(new StringBuilder("000000 000000 000000 0    0"), 0.1f);
                        }
                    }
                }
            }
    }


    class HackGameAgent_Collapser : HackGameAgent
    {

        HackGameAgent_AI_StateData_SpawningIn spawnInData;
        HackGameTimer collapseTimer;
        FlashingElement activeFlasher;
        StringBuilder timerString;
        float collapseTimeSeconds;

        HackGameReversibleLerpDrawHelper pulseEffect = new HackGameReversibleLerpDrawHelper(0.3f, 0.00f, 0.9f, 1.0f, Color.White, Color.Black, Vector2.Zero, Vector2.Zero);

        public HackGameAgent_Collapser(HackGameBoard board, float secondsToCollapse, Point location)
            : base(board)
        {
            collapseTimeSeconds = secondsToCollapse;
            setCurrentBoardLocation(location, board);
        }

        public override void EnteringNewState(HackGameAgent.HackGameAgent_State oldState, HackGameAgent.HackGameAgent_State newState)
        {
            if (newState == HackGameAgent_State.HackGameAgent_State_SpawningIn && oldState == HackGameAgent_State.HackGameAgent_State_Inactive)
            {
                spawnInData = new HackGameAgent_AI_StateData_SpawningIn();
            }

            if (newState == HackGameAgent_State.HackGameAgent_State_Active)
            {
                collapseTimer = new HackGameTimer(collapseTimeSeconds);
                activeFlasher = new FlashingElement(0.3f, true, FlashingElement.FlashingElement_OperationType.FlashingElement_OperationType_Normal);
                timerString = new StringBuilder();
                UpdateTimerString();
            }
        }

        private void UpdateTimerString()
        {
            timerString.Remove(0, timerString.Length);
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

        public override bool IsDestinationAllowed(Point node, HackGameBoard board)
        {
            return false;
        }

        public override void DrawCurrentState(HackNodeGameBoardMedia gameboarddrawing, SpriteBatch sb, Vector2 drawPos, float zoom)
        {

            switch (GetCurrentState())
            {
                case HackGameAgent_State.HackGameAgent_State_SpawningIn:
                    DrawSpawningInState(gameboarddrawing, sb, drawPos, zoom);
                    break;
                case HackGameAgent_State.HackGameAgent_State_Active:
                    DrawActiveState(gameboarddrawing, sb, drawPos, zoom);
                    break;
                case HackGameAgent_State.HackGameAgent_State_BeingKilled:
                    DrawBeingKilledState(gameboarddrawing, sb, drawPos, zoom);
                    break;
            }
            base.DrawCurrentState(gameboarddrawing, sb, drawPos, zoom);
        }

        private void DrawSpawningInState(HackNodeGameBoardMedia gameboarddrawing, SpriteBatch sb, Vector2 drawPos, float zoom)
        {
            if (spawnInData.StartSpawnSound)
            {
                gameboarddrawing.WhooshSound.Play();
                spawnInData.StartSpawnSound = false;
            }
            if (spawnInData.DrawImpact)
            {
                AddUIElement(gameboarddrawing.CollapserPingTexture, 1.0f, new Vector2(41.0f * zoom, 41.0f * zoom), new Vector2(41.0f * zoom, 41.0f * zoom), new Color(1.0f, 0.0f, 0, 0.0f), new Color(0.0f, 0, 0, 0), 0.2f, 2.5f, 0.0f);
                spawnInData.DrawImpact = false;
                gameboarddrawing.ThumpSound.Play();
            }

            if (spawnInData.flashing)
            {
                if (spawnInData.dropInFlash.IsOn())
                {
                    sb.Draw(gameboarddrawing.CollapserTexture, drawPos, null, Color.White, 0.0f, Vector2.Zero, zoom, SpriteEffects.None, 0);
                }
            }

            else
            {
                sb.Draw(gameboarddrawing.CollapserTexture, drawPos, null, Color.White, 0.0f, Vector2.Zero, zoom * spawnInData.dropInLerp.CurrentScale(), SpriteEffects.None, 0);
            }
        }

        private void DrawActiveState(HackNodeGameBoardMedia gameboarddrawing, SpriteBatch sb, Vector2 drawPos, float zoom)
        {
            Vector2 scaleVector = Vector2.Zero;
            scaleVector.X += drawPos.X + 40.0f * zoom;
            scaleVector.Y += drawPos.Y + 40.0f * zoom;
            sb.Draw(gameboarddrawing.CollapserTexture, scaleVector, null, pulseEffect.CurrentColor(), 0, new Vector2(40.0f, 40.0f), zoom, SpriteEffects.None, 0);


            Vector2 textVector = scaleVector;
            textVector -= gameboarddrawing.Collapse_Node_Font.MeasureString(timerString) / 2;

            //sb.DrawString(gameboarddrawing.Collapse_Node_Font, timerString, textVector, Color.White, 0, Vector2.Zero, zoom, SpriteEffects.None, 0);
        }

        private void DrawBeingKilledState(HackNodeGameBoardMedia gameboarddrawing, SpriteBatch sb, Vector2 drawPos, float zoom)
        {
            gameboarddrawing.explosion.AddParticles(drawPos + new Vector2(41.0f * zoom, 41.0f * zoom), Vector2.Zero);
            //gameboarddrawing.AIDeathParticles.AddParticles(drawPos + new Vector2(41.0f * zoom, 41.0f * zoom), Vector2.Zero);
            gameboarddrawing.ExplosionSound.Play();
            SetToRemove();
        }

        public override void UpdateState(GameTime time, HackGameBoard board, HackNodeGameBoardMedia drawing)
        {

            switch (GetCurrentState())
            {
                case HackGameAgent_State.HackGameAgent_State_SpawningIn:
                    UpdateSpawningInState(time, board, drawing);
                    break;
                case HackGameAgent_State.HackGameAgent_State_Active:
                    UpdateActiveState(time, board, drawing);
                    break;
            }
            pulseEffect.Update(time);
            base.UpdateState(time, board, drawing);


        }

        private void UpdateActiveState(GameTime time, HackGameBoard board, HackNodeGameBoardMedia drawing)
        {
            if (activeFlasher != null)
            {
                activeFlasher.Update(time);
            }

            if (collapseTimer != null)
            {
                collapseTimer.Update(time);
                if (collapseTimer.IsAlive() == false)
                {
                    board.GetElementAtPoint(getCurrentBoardLocation()).Kill(0);
                    List<HackGameBoardElement> nearby = new List<HackGameBoardElement>();
                    HackGameBoardElement west = board.getElementInDirection(getCurrentBoardLocation(), MovementDirection.MovementDirection_West);
                    if (west != null)
                    {
                        nearby.Add(west);
                    }

                    HackGameBoardElement east = board.getElementInDirection(getCurrentBoardLocation(), MovementDirection.MovementDirection_East);
                    if (east != null)
                    {
                        nearby.Add(east);
                    }

                    HackGameBoardElement north = board.getElementInDirection(getCurrentBoardLocation(), MovementDirection.MovementDirection_North);
                    if (north != null)
                    {
                        nearby.Add(north);
                    }

                    HackGameBoardElement south = board.getElementInDirection(getCurrentBoardLocation(), MovementDirection.MovementDirection_South);
                    if (south != null)
                    {
                        nearby.Add(south);
                    }

                    foreach (HackGameBoardElement el in nearby)
                    {
                        if (el.type == HackGameBoardElementBaseType.HackGameBoardElementBaseType_Bridge_NS || el.type == HackGameBoardElementBaseType.HackGameBoardElementBaseType_Bridge_EW)
                        {
                            el.Kill(2.0f); //slight delay
                        }
                    }

                    //and - kill self.
                    Kill(0);
                }
                UpdateTimerString();
            }
        }

        private void UpdateSpawningInState(GameTime time, HackGameBoard board, HackNodeGameBoardMedia drawing)
        {
            spawnInData.totalTimer -= (float)time.ElapsedGameTime.TotalSeconds;
            if (spawnInData.totalTimer <= 0)
            {
                this.SetCurrentState(HackGameAgent_State.HackGameAgent_State_Active);
            }
            else
            {
                if (spawnInData.flashing)
                {
                    spawnInData.dropInFlash.Update(time);
                }
                else
                {
                    spawnInData.dropInLerp.Update(time);
                    if (!spawnInData.dropInLerp.IsAlive())
                    {
                        spawnInData.DrawImpact = true;
                        spawnInData.StartFlashing();
                        board.AddBackgroundTextEmergency(new StringBuilder("[EMERGENCY] NODE COLLAPSING"), 0);
                        board.AddBackgroundTextEmergency(new StringBuilder("[EMERGENCY] CLEAR NODE IMMEDIATELY"), 0.10f);
                    }
                }
            }
        }
    }
}
