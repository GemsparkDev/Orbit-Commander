using System;

namespace Space_Wars.Content.Main;

public class Event(float _start, float _end, Action<float> _action) : IEvent
{
    public bool Update(float _time)
    {
        if (_start > _time)
        {
            return true;
        }
        if (_time > _end)
        {
            return false;
        }
        _action(_time - _start);
        return true;
    }
}

