using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Space_Wars.Content.Main.Entities;
using Space_Wars.Content.Main.Particles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
//using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Space_Wars.Content.Main.Components;

namespace Space_Wars.Content.Main;
public interface ICollider
{
    public bool Collide(Entity _entity);
    public bool IsColliding(Vector2 _position, Vector2 _velocity, float _radius, bool _override, out float _end);
    public void Draw(SpriteBatch _spriteBatch);
    public string Print();
    protected static bool ComputeCollisions(float _distance, Vector2 _closestPoint, Vector2 _velocity, Entity _entity)
    {
        var normalVector = (_entity.Position - _closestPoint) / _distance;
        Vector2 relativePosition = _entity.Position - _closestPoint + normalVector * _entity.ColliderRadius;
        Vector2 futurePosition = relativePosition + (_entity.Velocity - _velocity) - 2 * normalVector * _entity.ColliderRadius;
        bool cond = Vector2.Dot(normalVector, relativePosition) * Vector2.Dot(normalVector, futurePosition) < 0;
        if(cond)
        {
            var frictionVector = new Vector2(normalVector.Y, -normalVector.X);
            var relativeVelocity = -_entity.Velocity; //Include velocity if colliders are moving in the future.
            int collisionForce = (int)Math.Floor((relativeVelocity).Length() / 2);
            if (_entity as Pickup == null && (collisionForce > 5 || _entity.GetComponent<Attack>() != null))
            {
                _entity.Collide(collisionForce);
            }
            float verticalVelocity = Vector2.Dot(relativeVelocity, normalVector);
            _entity.Velocity += normalVector * verticalVelocity + frictionVector * Vector2.Dot(relativeVelocity, frictionVector) * 0.1f;
            _entity.Position += Vector2.Normalize(_entity.Position - _closestPoint) * (_entity.ColliderRadius - _distance);
        }
        return cond;
    }
}
public class LineCollider(Vector2 _start, Vector2 _end, bool _isVisual = false) : ICollider
{
    public bool Collide(Entity _entity)
    {
        if(_isVisual)
        {
            return false;
        }
        Vector2 length = _start - _end;
        float distanceBetween = Math.Clamp(Vector2.Dot((_start - _entity.Position), length) / length.LengthSquared(), 0, 1);
        Vector2 closestPoint = _start-length * distanceBetween;
        float distance = Vector2.Distance(closestPoint, _entity.Position);
        return ICollider.ComputeCollisions(distance, closestPoint, Vector2.Zero, _entity);
    }
    public bool IsColliding(Vector2 _position, Vector2 _entityVelocity, float _radius, bool _override, out float end)
    {
        end = _entityVelocity.Length();
        if(_isVisual && !_override)
        {
            return false;
        }
        Vector2 length = _start - _end;
        float distanceBetween = Math.Clamp(Vector2.Dot((_start - _position), length) / length.LengthSquared(), 0, 1);
        Vector2 closestPoint = _start - length * distanceBetween;
        float distance = Vector2.Distance(closestPoint, _position);
        var normalVector = (_position - closestPoint) / distance;
        Vector2 relativePosition = _position - closestPoint + normalVector * _radius;
        Vector2 futurePosition = relativePosition + (_entityVelocity) - 2 * normalVector * _radius;
        if(Vector2.Dot(normalVector, relativePosition) * Vector2.Dot(normalVector, futurePosition) < 0)
        {
            ParticleManager.Add(new Particle(Assets.Get(Sprites.Circle), closestPoint, 0, Color.Red));
        }
        return Vector2.Dot(normalVector, relativePosition) * Vector2.Dot(normalVector, futurePosition) < 0;
    }
    public void Move(Vector2 _offset)
    {
        _start += _offset;
        _end += _offset;
    }
    public void Rotate(float _rad)
    {
        float angle = MathF.Atan2((_end.Y - _start.Y),(_end.X - _start.X)) + MathF.PI/2;
        Vector2 com = (_end + _start) / 2;
        float length = (_end - com).Length();
        Vector2 dir = Util.ToUnitVector(angle + _rad);
        _end = com + dir * length;
        _start = com - dir * length;
    }
    public void Draw(SpriteBatch _spriteBatch)
    {
        float angle = MathF.Atan2((_start.Y - _end.Y), (_start.X - _end.X)) - MathF.PI/2;
        Vector2 dir = Util.ToUnitVector(angle);
        float f = 1;
        if(_isVisual)
        {
            f = 0.5f;
        }
        for (float d = 0; d < (_end - _start).Length() / 4; d += 2)
        {
            _spriteBatch.Draw(Assets.Get(Sprites.Dot), _start + dir * d * 4, null, Color.White * f, angle, Assets.DimsOf(Sprites.Dot)/2, Vector2.One, 0, 0);
        }
    }
    public string Print()
    {
        return $"new LineCollider(new Vector2({_start.X},{_start.Y}),new Vector2({_end.X},{_end.Y},{_isVisual})),";
    }
}
public class ArcCollider : ICollider
{
    private Vector2 position;
    private float sprayAngle;
    private float sprayCone;
    private float radius;
    private bool isVisual;
    public ArcCollider(Vector2 _position, float _sprayCone, float _sprayAngle, float _radius, bool _isVisual = false)
    {
        position = _position; sprayAngle = _sprayAngle; radius = _radius; sprayCone = _sprayCone; isVisual = _isVisual;
    }
    public ArcCollider(Vector2 _start, Vector2 _end, Vector2 _position, bool _isVisual = false)
    {
        radius = Vector2.Distance(_start, _position);
        position = _position;
        sprayCone = MathF.Acos(Vector2.Dot(_start - _position, _end - _position) / (radius * radius));
        sprayAngle = Util.ToAngle(_start / 2 + _end / 2 - _position);
        isVisual = _isVisual;
    }
    public bool Collide(Entity _entity)
    {
        if(isVisual)
        {
            return false;
        }
        Vector2 relativePosition = _entity.Position - position;
        float relativeAngle = -Math.Clamp(MathF.Asin(Util.Cross(relativePosition, Util.ToUnitVector(sprayAngle)) / relativePosition.Length()), -sprayCone/2, sprayCone/2);
        Vector2 closestPoint = Util.ToUnitVector(sprayAngle + relativeAngle) * radius + position;
        float distance = Vector2.Distance(_entity.Position, closestPoint);
        return ICollider.ComputeCollisions(distance, closestPoint, Vector2.Zero, _entity);
    }
    public bool IsColliding(Vector2 _position, Vector2 _velocity, float _radius, bool _override, out float end)
    {
        end = _velocity.Length();
        if(isVisual && !_override)
        {
            return false;
        }
        Vector2 relativePosition = _position - position;
        float relativeAngle = -Math.Clamp(MathF.Asin(Util.Cross(relativePosition, Util.ToUnitVector(sprayAngle)) / relativePosition.Length()), -sprayCone / 2, sprayCone / 2);
        Vector2 closestPoint = Util.ToUnitVector(sprayAngle + relativeAngle) * radius + position;
        float distance = Vector2.Distance(_position, closestPoint);
        var normalVector = (_position - closestPoint) / distance;
        Vector2 entityPosition = _position - closestPoint + normalVector * _radius;
        Vector2 futurePosition = entityPosition + _velocity - 2 * normalVector * _radius;
        if (Vector2.Dot(normalVector, entityPosition) * Vector2.Dot(normalVector, futurePosition) < 0)
        {
            ParticleManager.Add(new Particle(Assets.Get(Sprites.Circle), closestPoint, 0, Color.Red));
        }
        return Vector2.Dot(normalVector, entityPosition) * Vector2.Dot(normalVector, futurePosition) < 0;
    }
    public void Draw(SpriteBatch _spriteBatch)
    {
        float f = 1;
        if(isVisual)
        {
            f = 0.5f;
        }
        Vector2 normalVector;
        float increment = MathF.Tau / radius;
        int count = (int)Math.Ceiling(Math.Truncate(sprayCone / increment));
        if (count % 2 == 0 && count != 0)
        {
            for (float angle = increment / 2; angle < sprayCone / 2; angle += increment)
            {
                DrawParticle(angle);
                DrawParticle(-angle);
            }
        }
        else
        {
            DrawParticle(0);
            for (float angle = increment; angle < sprayCone / 2; angle += increment)
            {
                DrawParticle(angle);
                DrawParticle(-angle);
            }
        }
        if (sprayCone >= MathF.Tau - float.Epsilon)
        {
            DrawParticle(MathF.PI);
        }
        void DrawParticle(float angle)
        {
            normalVector = Util.ToUnitVector(angle + sprayAngle) * radius;
            _spriteBatch.Draw(Assets.Get(Sprites.Dot), position + normalVector, null, Color.White*f, angle, Assets.DimsOf(Sprites.Dot)/2, 1, 0, 0);
        }
    }
    public string Print()
    {
        return $"new ArcCollider(new Vector2({position.X},{position.Y}),{sprayCone}f,{sprayAngle}f,{radius}f,{isVisual}),";
    }
}
