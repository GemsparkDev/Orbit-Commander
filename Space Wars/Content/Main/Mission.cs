using Space_Wars.Content.Main.Entities;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Linq;
using System.Diagnostics;

namespace Space_Wars.Content.Main;
public class Mission
{
    public bool Completed { get; set; } = false;
    public string Name { get; }
    public string Description { get; }
    public GravitationalSource Planet { get; }
    //Save original entity parameters to allow cloning
    public List<(Func<Vector2, Vector2, float, Entity>, Vector2, Vector2, float, Condition[])> CopyObjectives { get; }
    public List<(Entity entity, Condition[] conditions)> MissionObjectives { get; } = new();
    public int MissionScrap { get; set; } = 0;
    private float timerModifier;
    public int WaveGoal { get; } = 0;
    public float restartTimer = -1;

    public int Wave { get; private set; } = 0;
    private List<(int cost, DelegateEnemy enemy)> enemyCreditValues;
    private List<DelegateEnemy> bosses;
    private Enemy aliveBoss = null;
    private int currentBoss;
    private float waveTimer = 5;
    private float maxWaveTimer = 5;
    private float difficulty;
    private Player player { get { return EntityManager.Player; } }
    private Random random = new();
    public delegate Enemy DelegateEnemy(Vector2 position, Vector2 velocity, float angle, bool _isFriendly = false);
    public int EnemiesSpawned { get; private set; } = 0;

    public Mission(GravitationalSource _planet, List<GravitationalSource> _moons, List<(Func<Vector2, Vector2, float, Entity> newEntity, Vector2 position, Vector2 velocity, float angle, Condition[] conditions)> _missionObjectives, string _name, string _description, float _timerModifier, int _waveGoal = 0, int _enemyTier = 0)
    {
        Name = _name;
        Description = _description;
        Planet = _planet;
        CopyObjectives = _missionObjectives;
        foreach (var (newEntity, position, velocity, angle, conditions) in _missionObjectives)
        {
            MissionObjectives.Add((newEntity(position, velocity, angle), (Condition[])conditions.Clone()));
        }
        if (_moons.Count > 0)
        {
            Planet.moons = _moons;
        }
        foreach (GravitationalSource moon in _moons)
        {
            if (moon.velocity != Vector2.Zero)
            {
                continue;
            }
            Vector2 normalVelocity = Vector2.Normalize(new Vector2(moon.position.Y, -moon.position.X));
            moon.velocity = normalVelocity * Planet.GetOrbitalVelocity(moon.position);
        }
        WaveGoal = _waveGoal;
        timerModifier = _timerModifier;

        if (_enemyTier == 0)
        {
            enemyCreditValues = new()
            {
                (1, Enemy.NewAdvancedFighter),
                (3, Enemy.NewCarrier),
                (3, Enemy.NewSniper),
                (4, Enemy.NewShotgunner),
            };
        }
        else if (_enemyTier == 1)
        {
            enemyCreditValues = new()
            {
                (1, Enemy.NewAdvancedFighter),
                (2, Enemy.NewHovercraft),
            };
        }
        else
        {
            enemyCreditValues = new()
            {
            };
        }

        bosses = new()
        {
            Enemy.NewSymmetryBoss,
            Enemy.NewOverloadBoss,
            Enemy.NewExcursionBoss,
            Enemy.NewWyvernBoss,
        };
        currentBoss = random.Next(bosses.Count);
    }
    public void Update()
    {
        Planet.Update();
        bool allCompleted = true;
        foreach (var (entity, conditions) in MissionObjectives)
        {
            foreach (var condition in conditions)
            {
                if (condition == Condition.Protect && entity.isExpired)
                {
                    FailMission();
                }
                if (condition == Condition.Kill && !entity.isExpired)
                {
                    allCompleted = false;
                }
                if (condition == Condition.CustomIncomplete)
                {
                    allCompleted = false;
                }
            }
        }
        if (WaveGoal > 0 && (WaveGoal >= EntityManager.CurrentMission.Wave))
        {
            allCompleted = false;
        }
        if (allCompleted && restartTimer == -1)
        {
            EntityManager.MarkMissionComplete();
        }
        if (restartTimer != -1)
        {
            if (restartTimer > 0)
            {
                restartTimer -= Engine.DeltaSeconds;
                return;
            }
            EventHandler.MissionSelectTrigger();
        }
        if (timerModifier == -1)
        {
            return;
        }

        if (aliveBoss != null)
        {
            if (aliveBoss.isExpired)
            {
                aliveBoss = null;
                SoundManager.ChangeTrack(Assets.Get(Sound.main));
            }
            else
            {
                return;
            }
        }
        waveTimer -= Engine.DeltaSeconds;
        EventHandler.UpdateEnemyCountdownUI(waveTimer, maxWaveTimer, Wave);
        if (waveTimer <= 0)
        {
            EnemiesSpawned = 0;
            Wave++;
            if (Wave % 20 == 0)
            {
                SoundManager.ChangeTrack(Assets.Get(Sound.boss));
                Enemy boss = bosses[currentBoss](NewSpawnLocation(), Vector2.Zero, 0);
                if (Wave == 40)
                {
                    boss = bosses[2](NewSpawnLocation(), Vector2.Zero, 0);
                }
                EntityManager.Add(boss);
                waveTimer = 4;
                maxWaveTimer = waveTimer;
                EnemiesSpawned = 1;
                currentBoss = (currentBoss + 1) % bosses.Count;
                aliveBoss = boss;
                EventHandler.UpdateEnemyCountdownUI(waveTimer, maxWaveTimer, Wave);
                return;
            }
            else
            {
                difficulty = (int)((Wave + 1) * MathF.Log(Wave + 1, MathF.E) - Wave) / 15 + 1;
                SpawnWaveBatch(random.Next((int)(3 * difficulty), (int)(5 * difficulty)));
                waveTimer = (4f * EnemiesSpawned + 5) * timerModifier;
                maxWaveTimer = waveTimer;
            }
        }
    }
    public void Initialize()
    {
        Planet.RenderSurface();
        foreach(var (entity, _) in MissionObjectives)
        {
            EntityManager.Add(entity);
        }
    }
    public void MarkComplete()
    {
        if (restartTimer != -1)
        {
            return;
        }
        Completed = true;
        MissionScrap = 0;
        restartTimer = 2;
    }
    public void FailMission()
    {
        if (restartTimer != -1)
        {
            return;
        }
        restartTimer = 2;
    }
    public void CompleteCustomRule(Entity _target) 
    {
        for(int i = 0; i < MissionObjectives.Count; i++)
        {
            (Entity entity, Condition[] conditions) = MissionObjectives[i];
            if (_target == entity)
            {
                for(int j = 0; j < conditions.Length; j++)
                {
                    if (conditions[j] == Condition.CustomIncomplete)
                    {
                        conditions[j] = Condition.CustomComplete;
                    }
                }
            }
        }
    }
    public Mission Clone()
    {
        return new Mission(Planet.Copy(), new(), CopyObjectives, Name, Description, timerModifier, WaveGoal) { Completed = this.Completed};
    }
    private void SpawnWaveBatch(int enemyCredits)
    {
        List<int> newCosts = new();
        int availableEnemies = Math.Min(enemyCreditValues.Count, Wave / 10 + 1);
        for (int i = 0; i < availableEnemies; i++)
        {
            newCosts.Add(enemyCreditValues[i].cost);
        }
        while (enemyCredits > 0)
        {
            for (int i = 0; i < availableEnemies; i++)
            {
                if (random.Next(0, enemyCreditValues[i].cost / 2) == 0 && newCosts[i] <= enemyCredits)
                {
                    EntityManager.Add(enemyCreditValues[i].enemy(NewSpawnLocation(), player.velocity, 0));
                    enemyCredits -= newCosts[i];
                    newCosts[i] += 1;
                    EnemiesSpawned++;
                }
                if (enemyCredits < newCosts.Min(c => c))
                {
                    return;
                }
            }
        }
    }
    private Vector2 NewSpawnLocation()
    {
        //Creates a position vector defined by the angle from the player in radians and the distance from the edge of the screen
        float angle = (random.NextSingle() - 0.5f) * MathF.Tau;
        float distanceMultiplier = 1 + (random.NextSingle() - 0.5f) / 4;
        float distance = (Engine.ScreenSize.X + Engine.ScreenSize.Y) / 2 * distanceMultiplier;
        Vector2 spawnLocation = Engine.ToUnitVector(angle) * distance;
        return spawnLocation + player.position;
    }
}
