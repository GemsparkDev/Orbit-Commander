using System;

namespace Space_Wars.Content.Main.Story;
public class EndlessEvent(Action<float> _action) : IEvent
{
    public bool Update(float _time)
    {
        _action(_time);
        return false;
    }
}
