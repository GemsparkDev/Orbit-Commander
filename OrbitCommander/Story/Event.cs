using System;

namespace OrbitCommander.Story;

public class Event(float _start, float _length, Action<float> _action) : IEvent
{
    public bool Update(float _time)
    {
        if (_start > _time)
        {
            return true;
        }
        if (_time > _start + _length)
        {
            return false;
        }
        _action(_time - _start);
        return true;
    }
}

