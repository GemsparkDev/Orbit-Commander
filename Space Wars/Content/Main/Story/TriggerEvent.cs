using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Wars.Content.Main.Story;
public class TriggerEvent(float _start, Action<float> _action) : IEvent
{
    private bool complete = false;

    public bool Update(float _time)
    {
        if (_time < _start)
        {
            return true;
        }
        if (!complete)
        {
            _action(_time);
            complete = true;
        }
        return !complete;
    }
}
