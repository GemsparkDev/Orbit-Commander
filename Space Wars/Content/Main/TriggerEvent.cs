using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Wars.Content.Main;
public class TriggerEvent : IEvent
{
    private float start;
    private Action<float> action;
    private bool complete = false;
    public TriggerEvent(float _start, Action<float> _action)
    {
        start = _start;
        action = _action;
    }
    public bool Update(float _time)
    {
        if (_time < start)
        {
            return true;
        }
        if (!complete)
        {
            action(_time);
            complete = true;
        }
        return !complete;
    }
}
