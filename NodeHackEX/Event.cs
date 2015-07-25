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
using System.Globalization;

namespace HackPrototype
{

    abstract class HackGameBoardEventTrigger
    {
        public HackGameBoardEventTrigger(string triggerdatastring) { }
        public abstract bool Update(GameTime time, HackGameBoard board);
    }

    class HackGameBoardEventTrigger_Timed : HackGameBoardEventTrigger
    {
        float timeremainingseconds;

        public HackGameBoardEventTrigger_Timed(string triggerdatastring)
            : base(triggerdatastring)
        {
            timeremainingseconds = HackGameBoardEvent.TryParseTime(triggerdatastring);
        }

        public HackGameBoardEventTrigger_Timed(float seconds)
            : base("0:00.0")
        {
            timeremainingseconds = seconds;
        }

        public override bool Update(GameTime time, HackGameBoard board)
        {
            if (timeremainingseconds > 0)
            {
                timeremainingseconds -= (float)time.ElapsedGameTime.TotalSeconds;
                if (timeremainingseconds <= 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
    }


    class HackGameBoardEventTrigger_PlayerScore : HackGameBoardEventTrigger
    {
        UInt64 targetscore;

        public HackGameBoardEventTrigger_PlayerScore(string triggerdatastring)
            : base(triggerdatastring)
        {
            bool success = UInt64.TryParse(triggerdatastring, out targetscore);
            if (!success)
            {
                throw new InvalidOperationException("HackGameBoardEventTrigger_PlayerScore got a badly-formatted trigger string.");
            }
        }

        public override bool Update(GameTime time, HackGameBoard board)
        {
            if (board.GetScore() >= targetscore)
                return true;
            return false;
        }
    }

    abstract class HackGameBoardEvent
    {
        public enum HackGameBoardEvent_Type
        {
            HackGameBoardEvent_Type_ThrowText, //0
            HackGameBoardEvent_Type_SpawnAI, //1
            HackGameBoardEvent_Type_RaiseAlertLevel, //2
            HackGameBoardEvent_Type_OpenExit, //3
            HackGameBoardEvent_Type_SpawnPlayer, //4
            HackGameBoardEvent_Type_CameraSnap, //5
            HackGameBoardEvent_Type_CameraLerp, //6
            HackGameBoardEvent_Type_BeginCollapse, //7
            HackGameBoardEvent_Type_SetSpeed //8
        }

        HackGameBoardEvent_Type type;
        HackGameBoardEventTrigger trigger;

        public HackGameBoardEvent(HackGameBoardEvent_Type eventtype, string typedatastring, HackGameBoardEventTrigger eventtrigger) { type = eventtype; trigger = eventtrigger; }
        public bool Update(GameTime time, HackGameBoard board)
        {
            return (trigger.Update(time, board));
        }
        public HackGameBoardEvent_Type GetEventType()
        {
            return type;
        }

        public static float TryParseTime(string datastring)
        {
            //xx:yy.zz
            char[] delims = { ':', '.' };

            string[] bits = datastring.Split(delims, StringSplitOptions.RemoveEmptyEntries);
            if (bits.Length < 3)
            {
                throw new InvalidOperationException("HackGameBoardEvent_TryParseTime got a badly-formatted typedata string.");
            }

            return ((float)(int.Parse(bits[0])) * 60.0f) + ((float)(int.Parse(bits[1]))) + ((float)(int.Parse(bits[2])) * 0.01f);
        }

        public static Point TryParsePoint(string datastring)
        {
            Point retpoint = new Point();

            //xx:yy.zz
            char[] delims = { ',' };

            string[] bits = datastring.Split(delims, StringSplitOptions.RemoveEmptyEntries);
            if (bits.Length != 2)
            {
                throw new InvalidOperationException("HackGameBoardEvent_TryParsePoint got a badly-formatted typedata string.");
            }

            CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            ci.NumberFormat.NumberDecimalSeparator = ".";
            ci.NumberFormat.NumberGroupSeparator = ",";

            retpoint.X = int.Parse(bits[0],ci);
            retpoint.Y = int.Parse(bits[1],ci);

            return retpoint;
        }

        public static Vector3 TryParseVector3(string datastring)
        {
            Vector3 retpoint = new Vector3();

            //xx:yy.zz
            char[] delims = { ',' };

            string[] bits = datastring.Split(delims, StringSplitOptions.RemoveEmptyEntries);
            if (bits.Length != 3)
            {
                throw new InvalidOperationException("HackGameBoardEvent_TryParseVector3 got a badly-formatted typedata string.");
            }

            CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            ci.NumberFormat.NumberDecimalSeparator = ".";
            ci.NumberFormat.NumberGroupSeparator = ",";

            retpoint.X = float.Parse(bits[0], ci);
            retpoint.Y = float.Parse(bits[1], ci);
            retpoint.Z = float.Parse(bits[2], ci);
            
            /*
            retpoint.X = float.Parse(bits[0]);
            retpoint.Y = float.Parse(bits[1]);
            retpoint.Z = float.Parse(bits[2]);
             * */
            return retpoint;
        }
    }

    class HackGameBoardEvent_CameraSnap : HackGameBoardEvent
    {
        Point location;
        float zoom;

        public HackGameBoardEvent_CameraSnap(string typedatastring, HackGameBoardEventTrigger trigger)
            : base(HackGameBoardEvent_Type.HackGameBoardEvent_Type_CameraSnap, typedatastring, trigger)
        {
            Vector3 parsed = TryParseVector3(typedatastring);
            location.X = (int)parsed.X;
            location.Y = (int)parsed.Y;
            zoom = parsed.Z;
        }

        public Point GetSnapToElement()
        {
            return location;
        }

        public float GetSnapToZoom()
        {
            return zoom;
        }
    }

    class HackGameBoardEvent_CameraLerp : HackGameBoardEvent
    {
        Point location;
        float zoom;

        public HackGameBoardEvent_CameraLerp(string typedatastring, HackGameBoardEventTrigger trigger)
            : base(HackGameBoardEvent_Type.HackGameBoardEvent_Type_CameraLerp, typedatastring, trigger)
        {
            Vector3 parsed = TryParseVector3(typedatastring);
            location.X = (int)parsed.X;
            location.Y = (int)parsed.Y;
            zoom = parsed.Z;
        }

        public Point GetLerpToElement()
        {
            return location;
        }

        public float GetLerpToZoom()
        {
            return zoom;
        }
    }

    class HackGameBoardEvent_SpawnPlayer : HackGameBoardEvent
    {
        Point location;

        public HackGameBoardEvent_SpawnPlayer(string typedatastring, HackGameBoardEventTrigger trigger)
            : base(HackGameBoardEvent_Type.HackGameBoardEvent_Type_SpawnPlayer, typedatastring, trigger)
        {
            location = TryParsePoint(typedatastring);
        }

        public Point GetPlayerPosition()
        {
            return location;
        }
    }

    class HackGameBoardEvent_ThrowText : HackGameBoardEvent
    {
        string textToThrow;

        public HackGameBoardEvent_ThrowText(string typedatastring, HackGameBoardEventTrigger trigger)
            : base(HackGameBoardEvent_Type.HackGameBoardEvent_Type_ThrowText, typedatastring, trigger)
        {
            textToThrow = typedatastring;
        }

        public string GetText()
        {
            return textToThrow;
        }
    }

    class HackGameBoardEvent_SpawnAI : HackGameBoardEvent
    {
        public HackGameBoardEvent_SpawnAI(string typedatastring, HackGameBoardEventTrigger trigger)
            : base(HackGameBoardEvent_Type.HackGameBoardEvent_Type_SpawnAI, typedatastring, trigger)
        {

        }
    }

    class HackGameBoardEvent_RaiseAlertLevel : HackGameBoardEvent
    {
        public HackGameBoardEvent_RaiseAlertLevel(string typedatastring, HackGameBoardEventTrigger trigger)
            : base(HackGameBoardEvent_Type.HackGameBoardEvent_Type_RaiseAlertLevel, typedatastring, trigger)
        {

        }
    }

    class HackGameBoardEvent_OpenExit : HackGameBoardEvent
    {

        Point location;

        public HackGameBoardEvent_OpenExit(string typedatastring, HackGameBoardEventTrigger trigger)
            : base(HackGameBoardEvent_Type.HackGameBoardEvent_Type_OpenExit, typedatastring, trigger)
        {
            location = TryParsePoint(typedatastring);
        }

        public Point GetExitLocation()
        {
            return location;
        }
    }

    class HackGameBoardEvent_BeginCollapse : HackGameBoardEvent
    {

        float timeToCollapse;

        public HackGameBoardEvent_BeginCollapse(string typedatastring, HackGameBoardEventTrigger trigger)
            : base(HackGameBoardEvent_Type.HackGameBoardEvent_Type_BeginCollapse, typedatastring, trigger)
        {
            timeToCollapse = TryParseTime(typedatastring);
        }

        public float GetTimeToCollapse()
        {
            return timeToCollapse;
        }
    }

    class HackGameBoardEvent_SetSpeed : HackGameBoardEvent
    {

        float speedFactor;

        public HackGameBoardEvent_SetSpeed(string typedatastring, HackGameBoardEventTrigger trigger)
            : base(HackGameBoardEvent_Type.HackGameBoardEvent_Type_SetSpeed, typedatastring, trigger)
        {
            speedFactor = float.Parse(typedatastring);
        }

        public float GetSpeedFactor()
        {
            return speedFactor;
        }
    }
}
