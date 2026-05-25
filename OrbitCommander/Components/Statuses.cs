using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrbitCommander.Entities;
using System.Collections.Generic;
using System.Linq;
using OrbitCommander.Core;

namespace OrbitCommander.Components;
public class Statuses(Entity _entity) : IComponent
{
    List<Status> effects = [];
    public int StealthChange { get; private set; }
    public int SensingChange { get; private set; }
    public void Update()
    {
        StealthChange = 0;
        SensingChange = 0;
        effects = [.. effects.Where(x => !x.IsExpired)];
        foreach (var effect in effects)
        {
            effect.Update(_entity);
            StealthChange += effect.StealthChange();
            SensingChange += effect.SensingChange();
        }
        _entity.Temperature *= Util.FIED(0.707f); //Radiative
        if (_entity.Temperature > 1)
        {
            ApplyStatus(new Fire(1, Color.Orange));
        }
        if (_entity.Temperature < -1)
        {
            ApplyStatus(new Frost(1));
        }
    }
    public void ApplyStatus(Status _status)
    {
        if (_status == null)
        {
            return;
        }
        foreach (var status in effects)
        {
            if (status.Type == _status.Type)
            {
                status.Reset();
                return;
            }
        }
        effects.Add(_status);
    }
    public void Clear()
    {
        effects = [];
    }
    public virtual int ModifyDamage(int _damage)
    {
        int damage = _damage;
        foreach (var status in effects)
        {
            damage = status.ModifyDamage(damage);
        }
        return damage;
    }
    public void Draw(SpriteBatch _spriteBatch, Entity _parent)
    {
        float maxOffset = (float)(effects.Count - 1) / 2;
        foreach (var effect in effects)
        {
            _spriteBatch.Draw(Assets.Get(effect.Icon), _parent.Position + new Vector2(maxOffset * 20, 20), null, Color.White, 0, Assets.DimsOf(effect.Icon) / 2, 1, 0, 0);
            maxOffset -= 1;
        }
    }
}