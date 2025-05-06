using System;

namespace Space_Wars.Content.Main;

public class Event : IEvent
{
    private float start;
    private float end;
    private Action<float> action;
    public Event(float _start, float _end, Action<float> _action)
    {
        start = _start;
        end = _end;
        action = _action;
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
        action(_time - start);
        return true;
    }
}

