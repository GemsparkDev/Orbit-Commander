using System;
using Space_Wars.Content.Main.Entities;

namespace Space_Wars.Content.Main;

public class EntityEvent : IEvent
{
    private float start;
    private float end;
    private Action<float, Entity> action;
    private Entity entity;
    public EntityEvent(float _start, float _end, Action<float, Entity> _action, Entity _entity)
    {
        start = _start;
        end = _end;
        action = _action;
        entity = _entity;
    }

    public bool Update(float _time)
    {
        if (start > _time)
        {
            return true;
        }
        if (_time > end)
        {
            return false;
        }
        action(_time - start, entity);
        return true;
    }
}

