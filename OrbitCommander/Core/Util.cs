using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OrbitCommander.Components;
using OrbitCommander.Entities;
using OrbitCommander.MissionComponents;
using OrbitCommander.Particles;
using OrbitCommander.Story;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UILib.Content;

namespace OrbitCommander.Core;
public static class Util
{
    public static Random Random { get; } = new();
    public static Vector2 ToUnitVector(float _angle)
    {
        return new Vector2(MathF.Sin(_angle), -MathF.Cos(_angle));
    }
    public static float Lerp(float _valueOne, float _valueTwo, float _length)
    {
        return _valueOne * (1 - _length) + _valueTwo * _length;
    }
    public static float ToAngle(Vector2 _direction)
    {
        //Rotated 90 degrees due to asset rotation
        return MathF.Atan2(_direction.X, -_direction.Y);
    }
    public static float OneToNegOne()
    {
        return Random.NextSingle() * 2 - 1f;
    }
    //Frame independent exponential decay 
    public static float FIED(float _decayPerSecond)
    {
        return MathF.Pow(_decayPerSecond, Engine.DeltaSeconds);
    }
    public static Vector2 RotateVector2(Vector2 v, float a)
    {
        float cos = MathF.Cos(a);
        float sin = MathF.Sin(a);
        return new Vector2(v.X * cos - v.Y * sin, v.X * sin + v.Y * cos);
    }
    public static float Cross(Vector2 v1, Vector2 v2)
    {
        return v1.X * v2.Y - v1.Y * v2.X;
    }
    public static Entity Nearest(Vector2 _position, Entity[] _entities)
    {
        return _entities.MinBy(x => Vector2.DistanceSquared(x.Position, _position));
    }
    public static void FiringParticles(Vector2 _position, Vector2 _velocity, Vector2 _direction)
    {
        for (int i = 0; i < 5; i++)
        {
            var color = Random.Next(0, 4) switch
            {
                0 => Color.Yellow,
                1 => new Color(0.2f, 0.2f, 0.2f),
                2 => Color.Wheat,
                3 => Color.Orange,
                _ => Color.White,
            };
            ParticleManager.Add(new Particle(Assets.Get(Sprites.Circle), 0.25f, _position - _velocity, _velocity + _direction * 2
                + new Vector2(OneToNegOne(), OneToNegOne()) / 2 + _direction * (OneToNegOne() - 0.25f) * 1.5f, 0, 0, color, new Color(0.3f, 0.2f, 0.1f, 0f)));
        }
        ParticleManager.Add(new Particle(Assets.Get(Sprites.Dot), 60, _position - _velocity, _velocity
            + new Vector2(_direction.Y + OneToNegOne() / 2, -_direction.X + OneToNegOne() / 4), 0, OneToNegOne() / 5, Color.Yellow, Color.Transparent)
        { experienceGravity = true });
    }
    public static void Explode(Vector2 _position, Vector2 _velocity, int _damage, float _radius)
    {
        int particles = Random.Next(15, 25);
        for (int i = 0; i < particles; i++)
        {
            float angle = Random.NextSingle() * MathF.PI * 2;
            Vector2 particleVelocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (Random.NextSingle() * 2 + 2);
            ParticleManager.Add(new Particle(Assets.Get(Sprites.Dot), 0.25f, _position - _velocity, particleVelocity + _velocity, angle, 0, Color.Yellow, new Color(255, 0, 0, 0)));
        }
        particles = Random.Next(8, 16);
        for (int i = 0; i < particles; i++)
        {
            float angle = Random.NextSingle() * MathF.PI * 2;
            Vector2 particleVelocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (Random.NextSingle() * 2 + 2);
            ParticleManager.Add(new Particle(Assets.Get(Sprites.Circle), 0.25f, _position - _velocity, particleVelocity + _velocity, angle, 0, Color.DarkSlateGray, Color.Transparent));
        }
        Engine.SaveGame.CurrentMission.Explode(_damage, _radius, _position);
        Engine.ShakeScreen(150 / ((_position - Engine.Camera.Position).Length() + 300));
    }
    public static Vector2 PredictEnemy(Entity nearestEnemy, Entity shooter, float speed, float offset = 0)
    {
        Vector2 d = nearestEnemy.Position - shooter.Position;
        Vector2 v = nearestEnemy.Velocity - shooter.Velocity;
        float cross = d.X * v.Y - d.Y * v.X;
        float sinTheta = Math.Clamp(cross / (d.Length() * speed), -1, 1);
        Vector2 vel = ToUnitVector(offset + ToAngle(d) + MathF.Asin(sinTheta));
        return shooter.Velocity + vel * 12;
    }
    public static void Autosave()
    {
        string filePath = Path.Combine(Directory.GetCurrentDirectory(), $"Content\\Saves\\Save_{Engine.SaveSlot}.txt");
        using var outputFile = new StreamWriter(filePath);
        outputFile.WriteLine(Engine.SaveGame.Serialize());
    }
    public static void Save()
    {
        Autosave();
        Events.QuitToMenu();
    }
    public static Cutscene RestartCutscene()
    {
        List<string> text =
        [
            "Kernel Ship-Master ver 3.1.1 - Copyright(C) In-Tech 2059",
            "Detected x86 P5 Pentium @ 250MHz, 4MB available",
            "Booting with parameters -v -f",
            "Error: Retrying",
            "Error: Retrying",
            "Error: Retrying",
            "Fatal Error: Boot sector missing or corrupted.",
            "Please insert disk image.",
            "Image Detected, booting from disk.",
            "Loading core.bin...",
            "Loading navnet.bin...",
            "Loading music_player.bin...",
            "Load complete, initiating system check.",
            "Hull:",
            "Guns:",
            "Engn:",
            "Snsr:",
            "Core:",
            "Please restart modules to restore functionality.",
            "                           @@@@@@                ",
            "                       @@@@@@@@@@@@@@            ",
            "                   @@@@@@@@@@@@@@@@@@@@@@        ",
            "               @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@    ",
            "           =      @@@@@@@@@      @@@@@@@@@       ",
            "           ====      @@@    %%%%    @@@      ####",
            "           =======       %%%%%%%%%%       #######",
            "           ==========  %%%%%%%%%%%%%%  ##########",
            "           =========  %%%%%%%%%%%%%%%%  #########",
            "                    Welcome aboard pilot         ",
            "           =========  %%%%%%%%%%%%%%%%  #########",
            "           ==========  %%%%%%%%%%%%%%  ##########",
            "           ============  %%%%%%%%%%   ###########",
            "           =============    %%%%    #############",
            "           ================      ################",
            "               =============    #############    ",
            "                   =========    #########        ",
            "                       =====    #####            ",
        ];
        Cutscene scene = null;
        Vector2 screen = Engine.BackBuffer / 2;
        var t5 = new TextActor(new Vector2(80, 260) * 3 - screen, "Failed\nFailed\nFailed\nFailed\nFailed\n")
        {
            TextSize = 1.45f * UIManager.UIScale,
            TextColor = Color.Red
        };
        var floppy = new Actor(Assets.Get(Sprites.Floppy), new Vector2(Engine.BackBuffer.X * 4 / 5, Engine.BackBuffer.Y), Color.Gray, MathF.PI / 8) { Scale = UIManager.UIScale };
        var floppyFlat = new Actor(Assets.Get(Sprites.FloppyFlat), new Vector2(Engine.BackBuffer.X * 4 / 5, Engine.BackBuffer.Y), Color.White, 0) { Scale = UIManager.UIScale };
        var floppyVel = Vector2.Zero;
        var ledGlow = new Actor(Assets.Get(Sprites.LEDGlow), UI.FloppyTerminal.position + (new Vector2(72.5f, 94.5f) * UIManager.UIScale - Assets.DimsOf(Sprites.Terminal) / 2) * UIManager.UIScale, Color.Red, 0) { Scale = UIManager.UIScale };
        float floppyAngVel = Util.OneToNegOne();
        List<IActor> actors = [];
        for (int i = 0; i < text.Count; i++)
        {
            actors.Add(new TextActor(new Vector2(60, 20 * 3 * i) - screen, text[i]) { TextSize = 1.5f * UIManager.UIScale });
        }
        actors.Add(t5);
        float ts = 0.2f;
        float trueTime = 0;
        SoundEffectInstance computerSounds = null;
        //Ensure planets still orbit and render
        List<IEvent> events =
        [
            new TriggerEvent(0, delegate(float time)
            {
                Engine.UIManager.ScreenWindow = UI.CutsceneGlobalMenu;
                for(ModuleType i = 0; i < (ModuleType)5; i++)
                {
                    Engine.SaveGame.Player.modules[i].isFailed = true;
                }
            }),
            new EndlessEvent(delegate(float time)
            {
                trueTime += Engine.DeltaSeconds;
                if(trueTime / 28 - Math.Truncate(trueTime / 28) <= Engine.DeltaSeconds / 28 + float.Epsilon)
                {
                    computerSounds = Assets.Get(Sound.ComputerSounds).CreateInstance();
                    computerSounds.Play();
                }
            }),
            new Event(0, ts * 3, delegate (float time)
            {
                var a = actors[(int)(time/ts)] as TextActor;
                a.Index = a.Text.Length;
            }),
            new Event(ts*3, 8f + Engine.DeltaSeconds, delegate (float time)
            {
                var a = actors[(int)(time/2) + 2] as TextActor;
                a.Index = a.Text.Length;
            }),
            new Event(8 + ts * 4, Engine.DeltaSeconds, delegate(float time)
            {
                var a = actors[7] as TextActor;
                a.Index = a.Text.Length;
                scene.IsPaused = true;
                //Check this line for differing UI scales
                if(Input.OldMouseState.LeftButton == ButtonState.Pressed && MathF.Abs(UI.FloppyTerminal.position.X - floppy.Position.X + 200) < 200 && MathF.Abs(UI.FloppyTerminal.position.Y + 175 - floppy.Position.Y) < 75)
                {
                    floppy.Color = Color.White * (MathF.Sin(Engine.Time * 4) / 8 + 0.875f);
                    floppy.Angle = MathF.Sin(Engine.Time * 5) / 20;
                    if(Input.NewMouseState.LeftButton == ButtonState.Released)
                    {
                        scene.IsPaused = false;
                    }
                }
                else
                {
                    floppy.Color = Color.White;
                }
            }),
            new Event(0, 8 + ts * 4 + Engine.DeltaSeconds, delegate(float time) //Render floppy overtop of inserter
            {
                Engine.Self.QueueShaderException(floppy);
                var mousePos = new Vector2(Input.OldMouseState.X, Input.OldMouseState.Y);
                if(Input.NewMouseState.LeftButton == ButtonState.Pressed && Vector2.Distance(floppy.Position, mousePos) < 100 * UIManager.UIScale)
                {
                    var newPos = new Vector2(Input.NewMouseState.X, Input.NewMouseState.Y);
                    floppyVel = newPos - mousePos;
                    floppy.Position = newPos;
                    floppy.Angle *= Util.FIED(0.02f);
                    floppyAngVel = Util.OneToNegOne();
                    return;
                }
                if(floppy.Position.X < 0 || floppy.Position.X > Engine.BackBuffer.X)
                {
                    floppy.Position = new Vector2(Math.Clamp(floppy.Position.X, 0, Engine.BackBuffer.X), floppy.Position.Y);
                    floppyVel.X *= -0.2f;
                    floppyVel.Y *= Util.FIED(0.03f);
                    floppyAngVel = 0;
                }
                if(floppy.Position.Y < 0 || floppy.Position.Y > Engine.BackBuffer.Y)
                {
                    floppy.Position = new Vector2(floppy.Position.X, Math.Clamp(floppy.Position.Y, 0, Engine.BackBuffer.Y));
                    floppyVel.Y *= -0.2f;
                    floppyVel.X *= Util.FIED(0.03f);
                    floppyAngVel = 0;
                }
                floppyVel += new Vector2(0,18) * Engine.DeltaSeconds;
                floppy.Position += floppyVel;
                floppy.Angle += floppyAngVel * Engine.DeltaSeconds;
                if(Events.AcknowledgeMessage(Message.ToggleTerminal))
                {
                    UI.FloppyTerminal.enabled = !UI.FloppyTerminal.enabled;
                }
            }),
            new Event(8 + ts * 4 + Engine.DeltaSeconds,2,delegate(float time)
            {
                floppyFlat.Position = UI.FloppyTerminal.position + (new Vector2(107, 94.5f) * UIManager.UIScale - Assets.DimsOf(Sprites.Terminal) / 2) * UIManager.UIScale;
                floppyFlat.Color = Color.White * ((2f - time)/2f);
                Engine.Self.QueueShaderException(floppyFlat);
                Engine.Self.QueueShaderException(ledGlow);
            }),
            new TriggerEvent(10 + ts * 4, delegate(float time)
            {
                UI.FloppyTerminal.enabled = false;
            }),
            new Event(10 + ts * 5, ts * 10, delegate (float time)
            {
                if(time/ts - Math.Truncate(time/ts) <= Engine.DeltaSeconds/ts && time > ts * 5)
                {
                    PushTextUp();
                }
                var a = actors[(int)(time/ts) + 8] as TextActor;
                a.Index = a.Text.Length;
            }),
            new Event(10 + ts * 11, 6, delegate (float time)
            {
                if(time - Math.Truncate(time) <= Engine.DeltaSeconds && time > 0.5f)
                {
                    t5.Index += 7;
                    SoundManager.PlayGlobalSound(Assets.Get(Sound.Beep));
                }
            }),
            new TriggerEvent(16 + ts * 12, delegate(float time)
            {
                scene.IsPaused = true;
                PushTextUp();
                var a = actors[18] as TextActor;
                a.Index = a.Text.Length;
            }),
            new Event(16 + ts * 12, Engine.DeltaSeconds, delegate(float time)
            {
                bool notReady = UI.RestartSwitch.Intervals[0] < 0.95f;
                foreach(var module in Engine.SaveGame.Player.modules)
                {
                    notReady = module.Value.isFailed || notReady;
                }
                if(!notReady)
                {
                    scene.IsPaused = false;
                    UI.FuseMenu.enabled = false;
                    for(int i = 0; i < 13; i++)
                    {
                        PushTextUp();
                    }
                }
            }),
            new Event(16 + ts * 12, Engine.DeltaSeconds, delegate(float time)
            {
                Engine.SaveGame.Player.Update();
            }),
            new Event(16 + ts * 12 + Engine.DeltaSeconds, 2f, delegate (float time)
            {
                var a = actors[(int)(time*9) + 19] as TextActor;
                a.Index = a.Text.Length;
            }),
            new TriggerEvent(21 + ts * 12 + Engine.DeltaSeconds, delegate(float time)
            {
                foreach(var module in Engine.SaveGame.Player.modules)
                {
                    module.Value.isFailed = false;
                }
                computerSounds.Pause();
                Engine.UIManager.ScreenWindow = UI.GlobalMenu;
                UI.FloppyTerminal.enabled = false;
                UI.FuseMenu.enabled = false;
                Events.AcknowledgeMessage(Message.ToggleTerminal);
            })
        ];
        scene = new Cutscene(events, actors, new PlayingGame());
        return scene;
        void PushTextUp()
        {
            foreach (var actor in actors)
            {
                if (actor is TextActor text)
                {
                    text.Position += new Vector2(0, -20) * 3;
                }
            }
        }
    }
    public static Cutscene DayOneLog()
    {
        Cutscene scene;
        List<string> text =
        [
            "Day one log:",
            "System diagnostics indicate full memory corruption.",
            "Original mission parameters lost.",
            "Encounter with hostile force suggests wanted status.",
            "Recommended actions:",
            "Investigate nearby planets.",
            "Discover original mission.",
            "Survive.",
        ];
        List<IActor> actors = [];
        List<IEvent> events = [];
        Vector2 screen = Engine.BackBuffer / 2;
        float sum = 0;
        float ts = 0.65f;
        for (int i = 0; i < text.Count; i++)
        {
            var actor = new TextActor(new Vector2(60, 20 * 3 * i) - screen, text[i]) { TextSize = 1.5f * UIManager.UIScale };
            actors.Add(actor);
            events.Add(new Event(sum + i, text[i].Length * ts, delegate (float time)
            {
                float index = time / ts;
                actor.Index = (int)index + 1;
                if (index - MathF.Floor(index) <= Engine.DeltaSeconds / ts)
                {
                    SoundManager.PlayGlobalSound(Assets.Get(Sound.Interact));
                }
            }));
            sum += text[i].Length * ts;
        }
        events.Add(new Event(sum + text.Count, 1, delegate (float time) { }));
        scene = new Cutscene(events, actors, new MissionSelect());
        return scene;
    }
    public static Cutscene QueueCrossfireDialogue()
    {
        List<IEvent> events =
        [
            new TriggerEvent(0, delegate(float time)
            {
                Engine.DialogueManager.Add(new Dialogue(
                    [
                        "Incoming: Sir, an unknown ship appears to be approaching!",
                        "They seem to be hailing us... They're on our side!",
                        "Pilot! Aid us in this fight and we'll help you however we can!",
                        ], null));
            }),
        ];
        return new Cutscene(events, [], new PlayingGame());
    }
    public static Cutscene RepairCrashedShip()
    {
        var ship = new Actor(Assets.Get(Sprites.Mothership), Vector2.Zero, Color.White, 0);
        List<IActor> actors = [ship];
        List<IEvent> events =
        [
            new TriggerEvent(0, delegate(float time)
            {
                Engine.DialogueManager.Add(new Dialogue(
                    [
                        "All teams, rendezvous at the ship if you value your life!",
                        "Get this ship in the air!",
                    ], null));
            }),
            new Event(6, 3, delegate(float time)
            {
                ship.Position += new Vector2(MathF.Sin(MathF.Atan2(time, 1)), MathF.Cos(MathF.Atan2(time, 1))) * time;
            }),
            new TriggerEvent(9, delegate(float time)
            {
                Engine.DialogueManager.Add(new Dialogue(
                    [
                        "We barely got out of there.",
                        "Pilot, I applaud your courage. We're in your debt.",
                        "Your ship... I've never seen anything like it before.",
                        "Lets regroup at the base. We might have some information that can aid you on your journey."
                    ], null));
            }),
        ];
        Vector2 screen = Engine.BackBuffer / 2;
        float sum = 0;
        float ts = 0.65f;
        List<string> text =
        [
            "Day two log",
            "Cross referencing at insurgent group indicates connection between group and original mission parameters.",
            "Data indicates possible conflict of interests between group and mission.",
            "Recommended course of action:",
            "Gain trust within the group.",
            "Search for more information about original creators.",
        ];
        for (int i = 0; i < text.Count; i++)
        {
            var actor = new TextActor(new Vector2(60, 20 * 3 * i) - screen, text[i]) { TextSize = 1.5f * UIManager.UIScale };
            actors.Add(actor);
            events.Add(new Event(sum + i, text[i].Length * ts, delegate (float time)
            {
                float index = time / ts;
                actor.Index = (int)index + 1;
                if (index - MathF.Floor(index) <= Engine.DeltaSeconds / ts)
                {
                    SoundManager.PlayGlobalSound(Assets.Get(Sound.Interact));
                }
            }));
            sum += text[i].Length * ts;
        }
        events.Add(new Event(sum + text.Count, 1, delegate (float time) { }));
        return new Cutscene(events, actors, new MissionSelect());
    }
    public static Cutscene SentryDialogue()
    {
        List<IEvent> events =
        [
            new TriggerEvent(0, delegate(float _time)
            {
                Engine.DialogueManager.Add(new Dialogue(
                    [
                        "*incoming* Oye, you recievin' this?",
                        "Good, now listen up.",
                        "You've been deployed on one of them there planets that we've scoped out.",
                        "See that last battle beat us pretty badly, so we need materials!",
                        "We've deployed a turret as you can see, and we want you to defend it until we say you can leave.",
                        "We can't spare equipment to fix damage you sustain, so try to avoid getting hit.",
                        "Hey, we might be able to scrounge up some intel if you help us with this though.",
                        "See you soon.",
                    ], null));
            }),
        ];
        return new Cutscene(events, [], new PlayingGame());
    }
    public static ICollider[] SolarStation() => [
            new LineCollider(new Vector2(-1000, -5800), new Vector2(1000, -5800)),
    ];
    public static List<(int, Func<Vector2, Vector2, float, Team, Entity>)> TierOneEnemies()
    {
        return
        [
            (1, Entity.NewFighter),
            (3, Entity.NewSniper),
            (4, Entity.NewShotgunner),
            (4, Entity.NewCarrier),
        ];
    }
    public static List<Func<Vector2, Vector2, float, Team, Entity>> TierOneBosses()
    {
        return
        [
            Entity.NewSymmetryBoss,
            Entity.NewWyvernBoss,
            Entity.NewDeadeyeBoss,
        ];
    }
    public static List<(int, Func<Vector2, Vector2, float, Team, Entity>)> TierTwoEnemies()
    {
        return
        [
            (1, Entity.NewAdvancedFighter),
            (2, Entity.NewHovercraft),
            (2, Entity.NewHealer),
        ];
    }
    public static List<Func<Vector2, Vector2, float, Team, Entity>> TierTwoBosses()
    {
        return
        [
            Entity.NewOverloadBoss,
            Entity.NewSurgeBoss,
            Entity.NewStreamlineBoss
        ];
    }
    public static List<(int, Func<Vector2, Vector2, float, Team, Entity>)> TierThreeEnemies()
    {
        return
        [
            (1, Entity.NewStealthFighter),
            (2, Entity.NewHunter),
            (3, Entity.NewEngineer),
        ];
    }
    public static List<Func<Vector2, Vector2, float, Team, Entity>> TierThreeBosses()
    {
        return
        [
            Entity.NewPursuerBoss,
            Entity.NewContinuumBoss,
        ];
    }
    public static List<(int, Func<Vector2, Vector2, float, Team, Entity>)> AllEnemies()
    {
        return [.. TierOneEnemies(), .. TierTwoEnemies(), .. TierThreeEnemies()];
    }
    public static List<Func<Vector2, Vector2, float, Team, Entity>> AllBosses()
    {
        return [.. TierOneBosses(), .. TierTwoBosses(), .. TierThreeBosses()];
    }
    public static (List<(int, Func<Vector2, Vector2, float, Team, Entity>)>,
    List<Func<Vector2, Vector2, float, Team, Entity>>) T1()
    {
        return (TierOneEnemies(), TierOneBosses());
    }
    public static (List<(int, Func<Vector2, Vector2, float, Team, Entity>)>,
    List<Func<Vector2, Vector2, float, Team, Entity>>) T2()
    {
        return (TierTwoEnemies(), TierTwoBosses());
    }
    public static (List<(int, Func<Vector2, Vector2, float, Team, Entity>)>,
    List<Func<Vector2, Vector2, float, Team, Entity>>) T3()
    {
        return (TierThreeEnemies(), TierThreeBosses());
    }
    public static (List<(int, Func<Vector2, Vector2, float, Team, Entity>)>,
    List<Func<Vector2, Vector2, float, Team, Entity>>) All()
    {
        return (AllEnemies(), AllBosses());
    }
    //TODO: Find a better way to do this
    //Delegate stacking is messy
    public static Func<Conditional> SendPickup(float _distance, Func<GameState> _scene = null)
    {
        return delegate {
            return new Conditional([new Custom(Entity.NewPickupDrone(new Vector2(-2000, 2000), _distance))],
            Win(_scene));
        };
    }
    public static Func<Conditional> Win(Func<GameState> _scene = null)
    {
        return Begin(_scene ?? MissionSelect.New, delegate { Engine.SaveGame.CompleteMission(Engine.SaveGame.CurrentMission.Wave); return null; });
    }
    public static Func<Conditional> Begin(Func<GameState> _state, Func<Conditional> _nextConditional)
    {
        return delegate
        {
            CurrentGameState.SwitchState(_state());
            return _nextConditional();
        };
    }
}
