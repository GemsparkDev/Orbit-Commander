using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrbitCommander.Components;
using OrbitCommander.Entities;
using OrbitCommander.Core;
using OrbitCommander.Particles;
using System;
using System.Collections.Generic;
using System.Linq;
using static OrbitCommander.Core.Mission;

namespace OrbitCommander.MissionComponents;
internal class WaveSpawner : IMissionComponent
{
    public WaveSpawner(List<(int cost, Func<Vector2, Vector2, float, Team, Entity> enemy)> _enemyCreditValues,
    List<Func<Vector2, Vector2, float, Team, Entity>> _bosses, float _timerModifier, bool _isAggressive)
    {
        enemyCreditValues = _enemyCreditValues;
        bosses = _bosses;
        TimerModifier = _timerModifier;
        currentBoss = Util.Random.Next(bosses.Count);
        isAggressive = _isAggressive;
    }
    public WaveSpawner(Func<(List<(int, Func<Vector2, Vector2, float, Team, Entity>)> _enemyCreditValues,
    List<Func<Vector2, Vector2, float, Team, Entity>> _bosses)> _spawner, float _timerModifier, bool _isAggressive)
    {
        var (_enemyCreditValues, _bosses) = _spawner();
        enemyCreditValues = _enemyCreditValues;
        bosses = _bosses;
        TimerModifier = _timerModifier;
        currentBoss = Util.Random.Next(bosses.Count);
        isAggressive = _isAggressive;
    }
    List<(int cost, Func<Vector2, Vector2, float, Team, Entity> enemy)> enemyCreditValues;
    List<Func<Vector2, Vector2, float, Team, Entity>> bosses;
    private List<Entity> enemiesSpawned = [];
    private bool currentWaveActive = false;
    private float waveTimer = 5;
    private float maxWaveTimer = 5;
    private bool isAggressive;
    private int currentBoss;
    public float TimerModifier { get; set; }
    public int EnemiesSpawned { get; set; } = 0;
    public int Wave { get { return Engine.SaveGame.CurrentMission.Wave; } set { Engine.SaveGame.CurrentMission.Wave = value; } }
    public void Draw(SpriteBatch _spriteBatch)
    {
    }

    public void Initialize()
    {
    }

    public void Update()
    {
        var isReady = true;
        foreach (var enemy in enemiesSpawned)
        {
            if (enemy.Health > 0)
            {
                isReady = false;
            }
        }
        if (!currentWaveActive)
        {
            if (waveTimer <= 0)
            {
                foreach (var enemy in enemiesSpawned)
                {
                    Engine.SaveGame.CurrentMission.Add(enemy);
                    float angle = MathF.Atan2(-enemy.Position.X, enemy.Position.Y);
                    float height = Assets.DimsOf(Sprites.Dot).X;
                    for (float i = 0; i < 500; i++)
                    {
                        var dir = Vector2.Normalize(enemy.Position);
                        float pow = (500 - i) / 500;
                        ParticleManager.Add(new Particle(Assets.Get(Sprites.Dot), 0.5f, enemy.Position + dir * i * height, Vector2.Zero, angle, 0, new Color(255, 0, 0), Color.Transparent));
                    }
                    if (isAggressive)
                    {
                        waveTimer = (enemiesSpawned.Count * 4f + 5f) * TimerModifier;
                        maxWaveTimer = waveTimer;
                    }
                }
                currentWaveActive = true;
            }
            else
            {
                waveTimer -= Engine.DeltaSeconds;
                foreach (var enemy in enemiesSpawned)
                {
                    ParticleManager.Add(new Particle(enemy.Texture, enemy.Position, enemy.Angle, new Color(255, 127, 0) * (Util.Random.NextSingle() / 2 + 0.25f)));
                }
            }
        }
        if (isAggressive && currentWaveActive)
        {
            if (waveTimer <= 0)
            {
                isReady = true;
            }
            else
            {
                waveTimer -= Engine.DeltaSeconds;
            }
        }
        if (isReady)
        {
            currentWaveActive = false;
            enemiesSpawned.Clear();
            EnemiesSpawned = 0;
            waveTimer = 10f * TimerModifier;
            maxWaveTimer = waveTimer;
            Wave++;
            Engine.SaveGame.CurrentMission.DecayPickups();
            if ((Wave - 1) % 20 == 0)
            {
                SoundManager.ChangeTrack(Assets.Get(Sound.main));
            }
            if (Wave % 20 == 0)
            {
                SoundManager.ChangeTrack(Assets.Get(Sound.boss));
            }

            if (missions[Engine.SaveGame.CurrentMissionIndex].data.PlayerProgression > 1 && Wave % 20 == 0)
            {
                var pos = Engine.SaveGame.CurrentMission.NewSpawnLocation();
                Entity boss = bosses[currentBoss](pos, Vector2.Zero, MathF.Atan2(-pos.X, pos.Y), Team.Hostile);
                if (Wave == 40)
                {
                    boss = bosses[2](pos, Vector2.Zero, 0, Team.Hostile);
                }
                enemiesSpawned.Add(boss);
                EnemiesSpawned = 1;
                currentBoss = (currentBoss + 1) % bosses.Count;
            }
            else
            {
                float difficulty = 0;
                if (isAggressive)
                {
                    difficulty = (int)(Math.Pow(Wave, 1.5) + 5);
                }
                else
                {
                    difficulty = (int)((Wave + 1) * MathF.Log(Wave + 1, MathF.E) - Wave) + 1;
                }
                List<int> newCosts = [];
                int availableEnemies = Math.Min(enemyCreditValues.Count, Wave / 10 + 1 + (isAggressive ? 1 : 0));
                Entity squadLeader = null;
                int count = 0;
                for (int i = 0; i < availableEnemies; i++)
                {
                    newCosts.Add(enemyCreditValues[i].cost);
                }
                var enemyCredits = Util.Random.Next((int)difficulty, (int)(difficulty * 2));
                while (enemyCredits > 0)
                {
                    int i = Util.Random.Next(0, availableEnemies);
                    if (Util.Random.Next(0, enemyCreditValues[i].cost / 2) == 0 && newCosts[i] <= enemyCredits)
                    {
                        Vector2 pos;
                        if (squadLeader != null && (count < 2 || Util.Random.Next(0, 4) != 0))
                        {
                            var offset = Vector2.Normalize(new Vector2(squadLeader.Position.X, squadLeader.Position.Y));
                            int isOdd = count % 2 == 0 ? 1 : -1;

                            pos = squadLeader.Position
                                //Horizontal offset
                                + new Vector2(offset.Y, -offset.X) * 10 * isOdd * (count / 2 + 1)
                                //Vertical offset
                                + offset * (count / 2 + 1) * 10;
                            count++;
                        }
                        else
                        {
                            pos = Engine.SaveGame.CurrentMission.NewSpawnLocation();
                            squadLeader = null;
                            count = 0;
                        }
                        var enemy = enemyCreditValues[i].enemy(pos, Engine.SaveGame.Player.Velocity, MathF.Atan2(-pos.X, pos.Y), Team.Hostile);
                        enemiesSpawned.Add(enemy);
                        squadLeader ??= enemy;
                        enemyCredits -= newCosts[i];
                        newCosts[i] += 1;
                        EnemiesSpawned++;
                    }
                    if (enemyCredits < newCosts.Min(c => c))
                    {
                        break;
                    }
                }
            }
            for (int i = 0; i < enemiesSpawned.Count / 4 + 1; i++)
            {
                Entity enemy = enemiesSpawned[Util.Random.Next(0, enemiesSpawned.Count)];
                var comp = enemy.GetComponent<Behaviour>();
                if (comp != null)
                {
                    comp.AddBehaviour(enemy.DropItem(ItemFactory.NewScrap));
                }
                else
                {
                    enemy.AddComponent(new Behaviour().AddBehaviour(enemy.DropItem(ItemFactory.NewScrap)));
                }
            }
        }
        UI.EnemiesLeft.text = (currentWaveActive ? enemiesSpawned.Where(x => x.Health > 0).Count() : 0).ToString();
        Events.UpdateEnemyCountdownUI(waveTimer, maxWaveTimer, Wave);
    }
    public IMissionComponent Clone()
    {
        return new WaveSpawner(enemyCreditValues, bosses, TimerModifier, isAggressive);
    }
}
