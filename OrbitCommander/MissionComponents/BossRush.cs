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
internal class BossRush : IMissionComponent
{
    public BossRush()
    {
        bosses = Util.TierOneBosses();
        bosses.AddRange(Util.TierTwoBosses());
        bosses.AddRange(Util.TierThreeBosses());
    }
    List<Func<Vector2, Vector2, float, Team, Entity>> bosses;
    private Entity currentBoss;
    private bool currentWaveActive = false;
    private float waveTimer = 0;
    private float maxWaveTimer = 5;
    private int index = 0;
    public void Draw(SpriteBatch _spriteBatch)
    {
    }

    public void Initialize()
    {
        SoundManager.ChangeTrack(Assets.Get(Sound.boss));
    }

    public void Update()
    {
        var isReady = true;
        if(currentBoss != null)
        {
            isReady = currentBoss.isExpired;
        }
        if (isReady)
        {
            currentWaveActive = false;
            waveTimer = 5f;
            maxWaveTimer = waveTimer;
            Engine.SaveGame.CurrentMission.Wave++;
            var pos = Engine.SaveGame.CurrentMission.NewSpawnLocation();
            currentBoss = bosses[index](pos, Vector2.Zero, Util.ToAngle(pos), Team.Hostile);
            index = (index + 1) % bosses.Count;
        }
        if (!currentWaveActive)
        {
            if (waveTimer <= 0)
            {
                Engine.SaveGame.CurrentMission.Add(currentBoss);
                float height = Assets.DimsOf(Sprites.Dot).X;
                var dir = Vector2.Normalize(currentBoss.Position);
                for (float i = 0; i < 500; i++)
                {
                    ParticleManager.Add(new Particle(Assets.Get(Sprites.Dot), 0.5f, currentBoss.Position + dir * i * height, Vector2.Zero, Util.ToAngle(dir), 0, new Color(255, 0, 0), Color.Transparent));
                }
                currentWaveActive = true;
            }
            else
            {
                waveTimer -= Engine.DeltaSeconds;
                ParticleManager.Add(new Particle(currentBoss.Texture, currentBoss.Position, currentBoss.Angle, new Color(255, 127, 0) * (Util.Random.NextSingle() / 2 + 0.25f)));
            }
        }
        UI.EnemiesLeft.text = (currentWaveActive ? (currentBoss.isExpired == false ? 1 : 0) : 0).ToString();
        Events.UpdateEnemyCountdownUI(waveTimer, maxWaveTimer, Engine.SaveGame.CurrentMission.Wave);
    }
}
