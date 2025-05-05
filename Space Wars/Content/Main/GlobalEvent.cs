using Space_Wars.Content.Main.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Wars.Content.Main;
public class GlobalEvent : IEvent
{
    private float start;
    private float end;
    private Action<float> action;
    public GlobalEvent(float _start, float _end, Action<float> _action)
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
