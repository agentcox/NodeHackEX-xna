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
    public class HackNodeGameBoardMedia
    {


        public Texture2D NodeBoxtexture;
        public Texture2D NodeEmptytexture;
        public Texture2D BridgeNStexture;
        public Texture2D BridgeEWtexture;

        public Texture2D NodeBox_Pathedtexture;
        public Texture2D BridgeNS_Pathedtexture;
        public Texture2D BridgeEW_Pathedtexture;

        public Texture2D Loot_Blue_Texture;
        public Texture2D Loot_Yellow_Texture;
        public Texture2D Loot_Black_Texture;

        public Texture2D Loot_2x_Score_Texture;
        public Texture2D Loot_4x_Score_Texture;

        public Texture2D Weapon_Multimissile_texture;
        public Texture2D Weapon_Heatseeker_texture;
        public Texture2D Weapon_Decoy_texture;
        public Texture2D Weapon_Mortar_texture;

        public Texture2D TimingRingEmpty;
        public Texture2D TimingRing1_4;
        public Texture2D TimingRing2_4;
        public Texture2D TimingRing3_4;
        public Texture2D TimingRingComplete;

        public Texture2D ExitTexture;

        public Texture2D PlayerTexture;
        public Texture2D AITexture;
        public Texture2D ProjectileTexture;

        public Texture2D PingTexture;
        public Texture2D WeaponPingTexture;
        public Texture2D CollapserPingTexture;

        public Texture2D AIQuestionMark_Left;
        public Texture2D AIQuestionMark_Center;
        public Texture2D AIQuestionMark_Right;

        public Texture2D AIExclamationMark_Left;
        public Texture2D AIExclamationMark_Center;
        public Texture2D AIExclamationMark_Right;

        public Texture2D LowerUI_Shell;

        public Texture2D TargetSlice_0_Percent;
        public Texture2D TargetSlice_25_Percent;
        public Texture2D TargetSlice_50_Percent;
        public Texture2D TargetSlice_75_Percent;
        public Texture2D TargetSlice_100_Percent;

        public Texture2D CollapserTexture;
        public Texture2D WhiteOneByOne;
        public Texture2D GradientLeft;

        public SpriteFont debugSpriteFont;

        public SpriteFont LowerUI_Score_Font;
        public SpriteFont LowerUI_Bonus_Font;

        public SpriteFont Overlay_Font;

        public SpriteFont Ticker_Font;

        public SpriteFont LootAmount_Font;

        public SpriteFont BG_Font;

        public SpriteFont Collapse_GiantNumbers_Font;
        public SpriteFont Collapse_Warning_Font;

        public SpriteFont Collapse_Node_Font;

        public ParticleSystem explosion;

        public ParticleSystem binaryOneFountain;
        public ParticleEmitter binaryOneFountain_emitter;

        public ParticleSystem binaryZeroFountain;
        public ParticleEmitter binaryZeroFountain_emitter;

        public ParticleSystem playerDeathParticles;
        public ParticleSystem AIDeathParticles;

        public SoundEffect ExplosionSound;
        public SoundEffect GameOverSound;
        public SoundEffect PlayerPingSound;
        public SoundEffect PlayerLockLocationSound;
        public SoundEffect HackSuccessfulSound;

        public SoundEffect AlertUpSound;
        public SoundEffect ThumpSound;
        public SoundEffect NodeRevealSound;
        public SoundEffect WhooshSound;
        public SoundEffect MessageSound;
        public SoundEffect StartExitSound;
        public SoundEffect MissileLaunchSound;
        public SoundEffect ProximityAlertSound;
        public SoundEffect MortarFallSound;

        public SoundEffect TimerTickSound;
        public SoundEffect TimerWarningSound;

        private SoundEffectInstance MoneyLoopSoundInstance;
        private SoundEffectInstance HackProgressSoundInstance;
        private SoundEffectInstance WarningLoopSoundInstance;
        private SoundEffectInstance MessageSoundInstance;

        private SoundEffect MoneyLoopSound;
        private SoundEffect HackProgressSound;

        private const float HackProgressMinPitch = -0.5f;
        private const float HackProgressMaxPitch = 0.5f;




        public HackNodeGameBoardMedia(Game game, ContentManager content)
        {
            NodeBoxtexture = content.Load<Texture2D>("Sprites\\nodebox");
            NodeEmptytexture = content.Load<Texture2D>("Sprites\\nodeempty");
            BridgeNStexture = content.Load<Texture2D>("Sprites\\north_south");
            BridgeEWtexture = content.Load<Texture2D>("Sprites\\east_west");

            NodeBox_Pathedtexture = content.Load<Texture2D>("Sprites\\nodebox_pathed");
            BridgeNS_Pathedtexture = content.Load<Texture2D>("Sprites\\north_south_pathed");
            BridgeEW_Pathedtexture = content.Load<Texture2D>("Sprites\\east_west_pathed");

            Loot_Blue_Texture = content.Load<Texture2D>("Sprites\\Loot\\blue_loot");
            Loot_Yellow_Texture = content.Load<Texture2D>("Sprites\\Loot\\yellow_loot");
            Loot_Black_Texture = content.Load<Texture2D>("Sprites\\Loot\\black_loot");

            Loot_2x_Score_Texture = content.Load<Texture2D>("Sprites\\Loot\\2x_score");
            Loot_4x_Score_Texture = content.Load<Texture2D>("Sprites\\Loot\\4x_score");

            Weapon_Multimissile_texture = content.Load<Texture2D>("Sprites\\weapon_multimissile");
            Weapon_Heatseeker_texture = content.Load<Texture2D>("Sprites\\weapon_heatseeker");
            Weapon_Decoy_texture = content.Load<Texture2D>("Sprites\\weapon_decoy");
            Weapon_Mortar_texture = content.Load<Texture2D>("Sprites\\weapon_mortar");

            PlayerTexture = content.Load<Texture2D>("Sprites\\player");
            AITexture = content.Load<Texture2D>("Sprites\\ai");
            ProjectileTexture = content.Load<Texture2D>("Sprites\\projectile");

            TimingRingEmpty = content.Load<Texture2D>("Sprites\\timing_ring_0");
            TimingRing1_4 = content.Load<Texture2D>("Sprites\\timing_ring_1");
            TimingRing2_4 = content.Load<Texture2D>("Sprites\\timing_ring_2");
            TimingRing3_4 = content.Load<Texture2D>("Sprites\\timing_ring_3");
            TimingRingComplete = content.Load<Texture2D>("Sprites\\timing_ring_4");

            ExitTexture = content.Load<Texture2D>("Sprites\\exit");

            PingTexture = content.Load<Texture2D>("Sprites\\ping_effect");
            WeaponPingTexture = content.Load<Texture2D>("Sprites\\weapon_ping_effect");
            CollapserPingTexture = content.Load<Texture2D>("Sprites\\collapser_ping_effect");

            AIQuestionMark_Left = content.Load<Texture2D>("Sprites\\ai_question_left");
            AIQuestionMark_Center = content.Load<Texture2D>("Sprites\\ai_question_center");
            AIQuestionMark_Right = content.Load<Texture2D>("Sprites\\ai_question_right");

            AIExclamationMark_Left = content.Load<Texture2D>("Sprites\\ai_exclaim_left");
            AIExclamationMark_Center = content.Load<Texture2D>("Sprites\\ai_exclaim_center");
            AIExclamationMark_Right = content.Load<Texture2D>("Sprites\\ai_exclaim_right");

            LowerUI_Shell = content.Load<Texture2D>("Sprites\\UI\\ui_lower_section_portrait");
            //LowerUI_BonusShell = content.Load<Texture2D>("Sprites\\UI\\ui_lower_section_bonus");
            //LowerUI_AlertShell = content.Load<Texture2D>("Sprites\\UI\\alert_level_text");

            //LowerUI_Alert_Light_Off = content.Load<Texture2D>("Sprites\\UI\\alert_level_light_off");
            //LowerUI_Alert_Light_On = content.Load<Texture2D>("Sprites\\UI\\alert_level_light_on");

            TargetSlice_0_Percent = content.Load<Texture2D>("Sprites\\UI\\target_slice_0_percent");
            TargetSlice_25_Percent = content.Load<Texture2D>("Sprites\\UI\\target_slice_25_percent");
            TargetSlice_50_Percent = content.Load<Texture2D>("Sprites\\UI\\target_slice_50_percent");
            TargetSlice_75_Percent = content.Load<Texture2D>("Sprites\\UI\\target_slice_75_percent");
            TargetSlice_100_Percent = content.Load<Texture2D>("Sprites\\UI\\target_slice_100_percent");

            CollapserTexture = content.Load<Texture2D>("Sprites\\collapser");
            WhiteOneByOne = content.Load<Texture2D>("Sprites\\white_1_1");
            GradientLeft = content.Load<Texture2D>("Sprites\\gradient_left");

            debugSpriteFont = content.Load<SpriteFont>("Fonts\\DebugUI");
            LowerUI_Bonus_Font = content.Load<SpriteFont>("Fonts\\LowerUI_Bonus");
            LowerUI_Score_Font = content.Load<SpriteFont>("Fonts\\LowerUI_Score");
            Overlay_Font = content.Load<SpriteFont>("Fonts\\OverlayFont");
            Ticker_Font = content.Load<SpriteFont>("Fonts\\TickerFont");
            LootAmount_Font = content.Load<SpriteFont>("Fonts\\LootAmountFont");
            BG_Font = content.Load<SpriteFont>("Fonts\\bgfontsheet");
            Collapse_GiantNumbers_Font = content.Load<SpriteFont>("Fonts\\GiantNumbersOnly");
            Collapse_Warning_Font = content.Load<SpriteFont>("Fonts\\WarningMoire");
            Collapse_Node_Font = content.Load<SpriteFont>("Fonts\\CollapseNodeCountFont");

            // create the particle systems and add them to the components list.
            explosion = new ParticleSystem(game, "ExplosionSettings") { DrawOrder = ParticleSystem.AdditiveDrawOrder };
            game.Components.Add(explosion);

            binaryOneFountain = new ParticleSystem(game, "BinaryOneEmitterSettings") { DrawOrder = ParticleSystem.AlphaBlendDrawOrder };
            binaryOneFountain_emitter = new ParticleEmitter(binaryOneFountain, 10.0f, new Vector2(200.0f, 200.0f));
            game.Components.Add(binaryOneFountain);

            binaryZeroFountain = new ParticleSystem(game, "BinaryZeroEmitterSettings") { DrawOrder = ParticleSystem.AlphaBlendDrawOrder };
            binaryZeroFountain_emitter = new ParticleEmitter(binaryZeroFountain, 5.0f, new Vector2(200.0f, 200.0f));
            game.Components.Add(binaryZeroFountain);

            playerDeathParticles = new ParticleSystem(game, "PlayerDeathParticleSettings") { DrawOrder = ParticleSystem.AlphaBlendDrawOrder };
            game.Components.Add(playerDeathParticles);
            AIDeathParticles = new ParticleSystem(game, "AIDeathParticleSettings") { DrawOrder = ParticleSystem.AlphaBlendDrawOrder };
            game.Components.Add(AIDeathParticles);

            //now load sounds
            ExplosionSound = content.Load<SoundEffect>("Sounds\\Explosion");
            AlertUpSound = content.Load<SoundEffect>("Sounds\\Alert_Up");
            GameOverSound = content.Load<SoundEffect>("Sounds\\Game_Over");
            PlayerPingSound = content.Load<SoundEffect>("Sounds\\Player_Ping");
            PlayerLockLocationSound = content.Load<SoundEffect>("Sounds\\Player_Move_Start");
            HackSuccessfulSound = content.Load<SoundEffect>("Sounds\\Hack_Successful");
            MoneyLoopSound = content.Load<SoundEffect>("Sounds\\Money_Loop_Short");
            HackProgressSound = content.Load<SoundEffect>("Sounds\\Hacking_Loop");
            MoneyLoopSoundInstance = MoneyLoopSound.CreateInstance();
            MoneyLoopSoundInstance.IsLooped = true;
            HackProgressSoundInstance = HackProgressSound.CreateInstance();
            HackProgressSoundInstance.IsLooped = true;
            ThumpSound = content.Load<SoundEffect>("Sounds\\Thump");
            WhooshSound = content.Load<SoundEffect>("Sounds\\Whoosh");
            NodeRevealSound = content.Load<SoundEffect>("Sounds\\NodeReveal");
            MessageSound = content.Load<SoundEffect>("Sounds\\MorseCode");
            StartExitSound = content.Load<SoundEffect>("Sounds\\StartExit");
            MissileLaunchSound = content.Load<SoundEffect>("Sounds\\MissileLaunch");
            ProximityAlertSound = content.Load<SoundEffect>("Sounds\\HeatSeeker_2");
            MortarFallSound = content.Load<SoundEffect>("Sounds\\MortarFall");
            TimerTickSound = content.Load<SoundEffect>("Sounds\\TimerTick");
            TimerWarningSound = content.Load<SoundEffect>("Sounds\\TimerWarning");
            WarningLoopSoundInstance = TimerWarningSound.CreateInstance();
            WarningLoopSoundInstance.IsLooped = true;

            MessageSoundInstance = MessageSound.CreateInstance();
            MessageSoundInstance.IsLooped = false;


        }

        public void UpdatePanAndZoomDeltas(Vector2 panDelta, float zoomDelta)
        {
            explosion.ApplyCameraOffsetDelta(panDelta, zoomDelta);
            binaryOneFountain.ApplyCameraOffsetDelta(panDelta, zoomDelta);
            binaryZeroFountain.ApplyCameraOffsetDelta(panDelta, zoomDelta);
            playerDeathParticles.ApplyCameraOffsetDelta(panDelta, zoomDelta);
            AIDeathParticles.ApplyCameraOffsetDelta(panDelta, zoomDelta);
        }

        public void StartWarningLoopSound()
        {
            WarningLoopSoundInstance.Play();
        }

        public void StopWarningLoopSound()
        {
            if (WarningLoopSoundInstance.State == SoundState.Playing || WarningLoopSoundInstance.State == SoundState.Paused)
            {
                WarningLoopSoundInstance.Stop();
            }
        }

        public void StartMessageSound()
        {
            if (MessageSoundInstance.State != SoundState.Playing)
            {
                MessageSoundInstance.Play();
            }
        }

        public void StopMessageSound()
        {
            if (MessageSoundInstance.State == SoundState.Playing || MessageSoundInstance.State == SoundState.Paused)
            {
                MessageSoundInstance.Stop();
            }
        }

        public void StartHackLoopSound()
        {
            HackProgressSoundInstance.Play();
            SetHackLoopSoundAmountComplete(0);
        }

        public void StopHackLoopSound()
        {
            if (HackProgressSoundInstance.State == SoundState.Playing || HackProgressSoundInstance.State == SoundState.Paused)
            {
                HackProgressSoundInstance.Stop();
            }
        }

        public void SetHackLoopSoundAmountComplete(float T)
        {
            HackProgressSoundInstance.Pitch = MathHelper.Lerp(HackProgressMinPitch, HackProgressMaxPitch, T);
        }

        public void StartMoneyLoopSound()
        {
            MoneyLoopSoundInstance.Play();
        }

        public void StopMoneyLoopSound()
        {
            MoneyLoopSoundInstance.Stop();
        }



        public void PauseAllSounds()
        {
            if (MoneyLoopSoundInstance != null && MoneyLoopSoundInstance.State == SoundState.Playing)
            {
                MoneyLoopSoundInstance.Pause();
            }

            if (HackProgressSoundInstance != null && HackProgressSoundInstance.State == SoundState.Playing)
            {
                HackProgressSoundInstance.Pause();
            }

            if (WarningLoopSoundInstance != null && WarningLoopSoundInstance.State == SoundState.Playing)
            {
                WarningLoopSoundInstance.Pause();
            }

            if (MessageSoundInstance != null && MessageSoundInstance.State == SoundState.Playing)
            {
                MessageSoundInstance.Pause();
            }
        }


        public void UnpauseAllSounds()
        {
            if (MoneyLoopSoundInstance != null && MoneyLoopSoundInstance.State == SoundState.Paused)
            {
                MoneyLoopSoundInstance.Resume();
            }

            if (HackProgressSoundInstance != null && HackProgressSoundInstance.State == SoundState.Paused)
            {
                HackProgressSoundInstance.Resume();
            }

            if (WarningLoopSoundInstance != null && WarningLoopSoundInstance.State == SoundState.Paused)
            {
                WarningLoopSoundInstance.Resume();
            }

            if (MessageSoundInstance != null && MessageSoundInstance.State == SoundState.Paused)
            {
                MessageSoundInstance.Resume();
            }
        }
    }



    public class FlashingElement
    {

        public enum FlashingElement_OperationType
        {
            FlashingElement_OperationType_Normal,
            FlashingElement_OperationType_StayOn,
            FlashingElement_OperationType_StayOff
        };

        FlashingElement_OperationType flashType = FlashingElement_OperationType.FlashingElement_OperationType_Normal;
        bool toggleOn;
        float currentTimer;
        float maxTimer;

        float afterTimer = 0;
        FlashingElement_OperationType afterType = FlashingElement_OperationType.FlashingElement_OperationType_Normal;

        public FlashingElement(float flashTime, bool startOn, FlashingElement_OperationType type)
        {
            Reset(flashTime, startOn, type);
        }

        public void Reset(float flashTime, bool startOn, FlashingElement_OperationType type)
        {
            toggleOn = startOn;
            maxTimer = flashTime;
            currentTimer = 0;
            flashType = type;
        }

        public void Update(GameTime t)
        {
            float floatt = (float)t.ElapsedGameTime.TotalSeconds;
            if (afterTimer > 0)
            {
                afterTimer -= floatt;
                if (afterTimer <= 0)
                {
                    afterTimer = 0;
                    flashType = afterType;
                }
            }

            if (flashType == FlashingElement_OperationType.FlashingElement_OperationType_Normal)
            {
                currentTimer += floatt;
                if (currentTimer >= maxTimer)
                {
                    currentTimer = 0;
                    if (toggleOn)
                        toggleOn = false;
                    else
                        toggleOn = true;
                }
            }
        }

        public void ChangeToModeAfter(float afterTime, FlashingElement_OperationType typeToChangeTo)
        {
            afterTimer = afterTime;
            if (afterTimer == 0)
            {
                flashType = typeToChangeTo;
            }
            else
            {
                afterType = typeToChangeTo;
            }
        }

        public bool IsOn()
        {
            if (flashType == FlashingElement_OperationType.FlashingElement_OperationType_StayOn)
                return true;
            else if (flashType == FlashingElement_OperationType.FlashingElement_OperationType_StayOff)
                return false;
            else return toggleOn;
        }
    }

    class HackGameTimer
    {
        //lifetime
        float lifeTimeLeft;

        public HackGameTimer(float lifeTimeSeconds)
        {
            lifeTimeLeft = lifeTimeSeconds;
        }

        public void Reset(float lifeTimeSeconds)
        {
            lifeTimeLeft = lifeTimeSeconds;
        }

        public void Update(GameTime t)
        {
            float floatt = (float)t.ElapsedGameTime.TotalSeconds;
            if (lifeTimeLeft > 0)
            {
                lifeTimeLeft -= floatt;
            }
        }

        public float GetLifeTimeLeft()
        {
            return lifeTimeLeft;
        }

        public bool IsAlive()
        {
            return (lifeTimeLeft > 0);
        }
    }

    abstract class HackGameLerpDrawHelper
    {
        //scale
        protected float initialScale;
        protected float targetScale;
        protected float currentScaleT;
        protected float scaleTPerSecond;

        protected float currentScale;

        //color
        protected Color initialColor;
        protected Color targetColor;
        protected float currentColorT;
        protected float colorTPerSecond;

        protected Color currentColor;


        //position
        protected Vector2 initialPosition;
        protected Vector2 targetPosition;
        protected float currentPositionT;
        protected float positionTPerSecond;

        protected Vector2 currentPosition;


        public HackGameLerpDrawHelper(float startScale, float endScale, float scaleSeconds, Color startColor, Color endColor, float colorSeconds, Vector2 startPosition, Vector2 endPosition, float positionSeconds)
        {
            scaleTPerSecond = 1.0f / scaleSeconds;
            colorTPerSecond = 1.0f / colorSeconds;
            positionTPerSecond = 1.0f / positionSeconds;

            initialScale = startScale;
            targetScale = endScale;

            currentScale = initialScale;

            initialColor = startColor;
            targetColor = endColor;

            currentColor = initialColor;

            initialPosition = startPosition;
            targetPosition = endPosition;

            currentPosition = startPosition;
        }

        public void Reset(float startScale, float endScale, float scaleSeconds, Color startColor, Color endColor, float colorSeconds, Vector2 startPosition, Vector2 endPosition, float positionSeconds)
        {
            scaleTPerSecond = 1.0f / scaleSeconds;
            colorTPerSecond = 1.0f / colorSeconds;
            positionTPerSecond = 1.0f / positionSeconds;

            initialScale = startScale;
            targetScale = endScale;

            currentScale = initialScale;

            initialColor = startColor;
            targetColor = endColor;

            currentColor = initialColor;

            initialPosition = startPosition;
            targetPosition = endPosition;

            currentPosition = startPosition;

            currentPositionT = 0;
            currentScaleT = 0;
            currentColorT = 0;
        }

        public Color CurrentColor()
        {
            return currentColor;
        }

        public float CurrentScale()
        {
            return currentScale;
        }

        public Vector2 CurrentPosition()
        {
            return currentPosition;
        }

        public void Update(GameTime t)
        {
            currentColor.R = (byte)(initialColor.R + ((float)(targetColor.R - initialColor.R) * currentColorT));
            currentColor.G = (byte)(initialColor.G + ((float)(targetColor.G - initialColor.G) * currentColorT));
            currentColor.B = (byte)(initialColor.B + ((float)(targetColor.B - initialColor.B) * currentColorT));
            currentColor.A = (byte)(initialColor.A + ((float)(targetColor.A - initialColor.A) * currentColorT));

            currentPosition.X = (byte)(initialPosition.X + ((float)(targetPosition.X - initialPosition.X) * currentPositionT));
            currentPosition.Y = (byte)(initialPosition.Y + ((float)(targetPosition.Y - initialPosition.Y) * currentPositionT));

            currentScale = MathHelper.Lerp(initialScale, targetScale, currentScaleT);
        }
    }

    class HackGameForwardLerpDrawHelper : HackGameLerpDrawHelper
    {

        //lifetime
        float lifeTimeLeft;

        public HackGameForwardLerpDrawHelper(float lifeTimeSeconds, float startScale, float endScale, float scaleSeconds, Color startColor, Color endColor, float colorSeconds, Vector2 startPosition, Vector2 endPosition, float positionSeconds) :
            base(startScale, endScale, scaleSeconds, startColor, endColor, colorSeconds, startPosition, endPosition, positionSeconds)
        {
            lifeTimeLeft = lifeTimeSeconds;

        }

        public void Reset(float lifeTimeSeconds, float startScale, float endScale, float scaleSeconds, Color startColor, Color endColor, float colorSeconds, Vector2 startPosition, Vector2 endPosition, float positionSeconds)
        {
            lifeTimeLeft = lifeTimeSeconds;

            base.Reset(startScale, endScale, scaleSeconds, startColor, endColor, colorSeconds, startPosition, endPosition, positionSeconds);
        }

        public float GetLifeTimeLeft()
        {
            return lifeTimeLeft;
        }

        public void Update(GameTime t)
        {
            float floatt = (float)t.ElapsedGameTime.TotalSeconds;
            
            if (lifeTimeLeft > 0)
            {
                if (currentScaleT < 1.0f)
                {
                    currentScaleT += scaleTPerSecond * floatt;
                    if (currentScaleT > 1.0f)
                        currentScaleT = 1.0f;
                }

                if (currentColorT < 1.0f)
                {
                    currentColorT += colorTPerSecond * floatt;
                    if (currentColorT > 1.0f)
                        currentColorT = 1.0f;
                }

                if (currentPositionT < 1.0f)
                {
                    currentPositionT += positionTPerSecond * floatt;
                    if (currentPositionT > 1.0f)
                        currentPositionT = 1.0f;
                }
                lifeTimeLeft -= floatt;
            }

            base.Update(t);
        }

        public bool IsAlive()
        {
            return (lifeTimeLeft > 0);
        }

    }

    class HackGameReversibleLerpDrawHelper : HackGameLerpDrawHelper
    {
        float timebetweenswitches = 0;

        float delayCount = 0;
        
        bool forward = true;
        float currentT = 0;

        public HackGameReversibleLerpDrawHelper(float secondsT, float secondsDelay, float startScale, float endScale, Color startColor, Color endColor, Vector2 startPosition, Vector2 endPosition) :
            base(startScale, endScale, secondsT, startColor, endColor, secondsT, startPosition, endPosition, secondsT)
        {
            timebetweenswitches = secondsDelay;
        }

        public void Reset(float secondsT, float secondsDelay, float startScale, float endScale, Color startColor, Color endColor, Vector2 startPosition, Vector2 endPosition)
        {
            timebetweenswitches = secondsDelay;
            forward = true;
            base.Reset(startScale, endScale, secondsT, startColor, endColor, secondsT, startPosition, endPosition, secondsT);
        }

        public void Update(GameTime t)
        {
            float floatt = (float)t.ElapsedGameTime.TotalSeconds;

            //check if we need to wait a sec
            if (delayCount > 0)
            {
                delayCount -= floatt;

                if (delayCount <= 0)
                {
                    FinishSwitch();
                }
            }

            else if (forward)
            {
                //we're adding to t
                if (currentScaleT < 1.0f)
                {
                    currentScaleT += scaleTPerSecond * floatt;
                    if (currentScaleT > 1.0f)
                        currentScaleT = 1.0f;
                }

                if (currentColorT < 1.0f)
                {
                    currentColorT += colorTPerSecond * floatt;
                    if (currentColorT > 1.0f)
                        currentColorT = 1.0f;
                }

                if (currentPositionT < 1.0f)
                {
                    currentPositionT += positionTPerSecond * floatt;
                    if (currentPositionT > 1.0f)
                        currentPositionT = 1.0f;
                }

                currentT += floatt;
                if (currentT >= 1.0f)
                {
                    StartSwitch();
                }
            }
            else
            {
                //we're subtracting from t
                if (currentScaleT > 0.0f)
                {
                    currentScaleT -= scaleTPerSecond * floatt;
                    if (currentScaleT < 0.0f)
                        currentScaleT = 0.0f;
                }

                if (currentColorT > 0.0f)
                {
                    currentColorT -= colorTPerSecond * floatt;
                    if (currentColorT < 0.0f)
                        currentColorT = 0.0f;
                }

                if (currentPositionT > 0.0f)
                {
                    currentPositionT -= positionTPerSecond * floatt;
                    if (currentPositionT < 0.0f)
                        currentPositionT = 0.0f;
                }

                currentT -= floatt;
                if (currentT <= 0.0f)
                {
                    StartSwitch();
                }
            }

            base.Update(t);
        }

        private void StartSwitch()
        {
            delayCount = timebetweenswitches;
            if (timebetweenswitches <= 0)
            {
                FinishSwitch();
            }
        }

        private void FinishSwitch()
        {
            delayCount = 0;

            if (forward)
            {
                forward = false;
                currentT = 1.0f;
            }
            else
            {
                forward = true;
                currentT = 0;
            }
        }

    }

    class WorldSpaceUIElement
    {

        Texture2D texture = null;

        float timeBeforeStart;
        float timeToLiveMax;
        float timeToLiveCurrent;

        float startScale = 1.0f;
        float endScale = 1.0f;
        float currentScale = 1.0f;

        Vector2 startOffsetFromParent;
        Vector2 endOffsetFromParent;
        Vector2 currentOffsetFromParent;

        Color startColor = Color.White;
        Color endColor = Color.White;
        Color currentColor = Color.White;

        public WorldSpaceUIElement(Texture2D tex, float lifetimeSeconds, Vector2 offsetFromParent, float delay)
        {
            startOffsetFromParent = offsetFromParent;
            endOffsetFromParent = offsetFromParent;
            currentOffsetFromParent = offsetFromParent;

            timeToLiveMax = lifetimeSeconds;
            timeToLiveCurrent = lifetimeSeconds;

            timeBeforeStart = delay;

            texture = tex;
        }

        public WorldSpaceUIElement(Texture2D tex, float lifetimeSeconds, Vector2 offsetFromParent_Start, Vector2 offsetFromParent_End, Color color_Start, Color color_End, float scale_Start, float scale_End, float delay)
        {

            timeToLiveMax = lifetimeSeconds;
            timeToLiveCurrent = lifetimeSeconds;

            texture = tex;

            startOffsetFromParent = offsetFromParent_Start;
            endOffsetFromParent = offsetFromParent_End;
            currentOffsetFromParent = offsetFromParent_Start;

            startColor = color_Start;
            endColor = color_End;
            currentColor = color_Start;

            startScale = scale_Start;
            endScale = scale_End;
            currentScale = scale_Start;

            timeBeforeStart = delay;
        }

        public bool Alive()
        {
            if (timeToLiveCurrent > 0.0f || timeBeforeStart > 0.0f)
            {
                return true;
            }
            return false;
        }

        public void Kill()
        {
            timeToLiveCurrent = 0.0f;
        }

        public void UpdateState(GameTime time, HackGameBoard board)
        {
            if (timeBeforeStart > 0.0f)
                timeBeforeStart -= (float)time.ElapsedGameTime.TotalSeconds;

            if (timeBeforeStart <= 0.0f)
            {
                timeToLiveCurrent -= (float)time.ElapsedGameTime.TotalSeconds;

                float t = timeToLiveMax != 0.0f ? 1.0f - (timeToLiveCurrent / timeToLiveMax) : 0.0f;

                currentScale = startScale + ((endScale - startScale) * t);

                currentColor.R = (byte)(startColor.R + ((float)(endColor.R - startColor.R) * t));
                currentColor.G = (byte)(startColor.G + ((float)(endColor.G - startColor.G) * t));
                currentColor.B = (byte)(startColor.B + ((float)(endColor.B - startColor.B) * t));
                currentColor.A = (byte)(startColor.A + ((float)(endColor.A - startColor.A) * t));

                currentOffsetFromParent.X = startOffsetFromParent.X + ((endOffsetFromParent.X - startOffsetFromParent.X) * t);
                currentOffsetFromParent.Y = startOffsetFromParent.Y + ((endOffsetFromParent.Y - startOffsetFromParent.Y) * t);
            }

        }

        public void DrawSelf(SpriteBatch sb, Vector2 drawPos, float zoom)
        {
            if (timeBeforeStart <= 0.0f)
            {

                Vector2 realOffset = currentOffsetFromParent;
                realOffset.X *= zoom;
                realOffset.Y *= zoom;

                Vector2 realOrigin = new Vector2(texture.Bounds.Width / 2.0f, texture.Bounds.Height / 2.0f);

                sb.Draw(texture, drawPos + realOffset, null, currentColor, 0f, realOrigin, currentScale * zoom, SpriteEffects.None, 0);
            }
        }
    }
}
