using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Space_Wars.Content.Main.Entities;
using Space_Wars.Content.Main.Particles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Wars.Content.Main.Components;
public class Atmosphere : Component
{
    private Texture2D atmosphere;
    private float atmosphereStrength;
    private float mass;
    public bool IsSun { get; set; } = false;
    public Atmosphere(Entity _entity, float _atmosphereStrength, float _mass) : base(_entity)
    {
        atmosphereStrength = _atmosphereStrength;
        mass = _mass;
        atmosphere = Engine.Self.RenderAtmosphere(AtmosphereRadius(), _atmosphereStrength, Entity.ColliderRadius, Entity.Color, this);
    }
    public override void Update()
    {
        foreach(var _entity in Engine.SaveGame.CurrentMission.Entities)
        {
            AttractEntity(_entity);
        }
        AttractEntity(Engine.SaveGame.Player);
    }
    private void AttractEntity(Entity _entity)
    {
        if (_entity == Entity)
        {
            return;
        }
        Vector2 relativePosition = _entity.Position - Entity.Position;
        float distance = relativePosition.Length();
        Vector2 acceleration = Vector2.Zero;
        if (distance < AtmosphereRadius())
        {
            float strength = GetAtmosphereDensity(distance);
            //Drag
            Vector2 relativeVelocity = (Entity.Velocity - _entity.Velocity);
            Vector2 drag = relativeVelocity * strength / 40;
            acceleration += drag;
            float q = (drag * relativeVelocity * relativeVelocity).Length() / 15;
            for (float i = 0; i < 5 * 60 * Engine.DeltaSeconds; i++)
            {
                if (Util.Random.NextSingle() < q * q / 2)
                {
                    Vector2 pos = Util.ToUnitVector(Util.Random.NextSingle() * MathF.Tau) * Util.Random.NextSingle() * 8;
                    ParticleManager.Add(new Particle(Assets.Get(Sprites.Circle), 0.5f + Util.Random.NextSingle() / 5, _entity.Position - _entity.Velocity + pos,
                        (_entity.Velocity + Entity.Velocity) / 2, 0, 0, Color.Yellow * 0.5f, new Color(1f, 0.5f, 0f, 0f))
                    { experienceGravity = true });
                }
            }
            _entity.ApplyWork(q);
            if (IsSun)
            {
                _entity.ConductHeat(Entity.Temperature * strength, MathF.Tanh(strength));
            }
            else
            {
                _entity.ConductHeat(Entity.Temperature, MathF.Tanh(strength));
            }
            //Buoyancy
            acceleration += relativePosition * strength * mass / 5 / (distance * distance * distance);
            if (strength > 2)
            {
                _entity.Statuses?.ApplyStatus(new Pressure(Color.Red, IsSun));
                if (_entity.GetComponent<Health>() != null && _entity.Health <= 0)
                {
                    _entity.isExpired = true;
                }
            }
        }
        _entity.Velocity += acceleration * Engine.DeltaSeconds * 60;
    }
    public override void Draw(SpriteBatch _spriteBatch)
    {
        _spriteBatch.Draw(atmosphere, Entity.Position, null, Color.White, 0, new Vector2(atmosphere.Width / 2, atmosphere.Height / 2), 1, 0, 0);
    }
    public float GetAtmosphereDensity(Entity _entity)
    {
        float distance = Vector2.Distance(_entity.Position, Entity.Position);
        if (distance > AtmosphereRadius())
        {
            return 0;
        }
        return GetAtmosphereDensity(distance);
    }
    public float GetAtmosphereDensity(float r)
    {
        float gravityForce = mass / Entity.ColliderRadius / Entity.ColliderRadius;
        return atmosphereStrength * MathF.Pow(2, -gravityForce * (r - Entity.ColliderRadius) / atmosphereStrength / 4);
    }
    private float AtmosphereRadius()
    {
        float gravityForce = mass / Entity.ColliderRadius / Entity.ColliderRadius;
        return MathF.Log2(0.1f / atmosphereStrength) * 4 * atmosphereStrength / (-gravityForce) + Entity.ColliderRadius;
    }
}
