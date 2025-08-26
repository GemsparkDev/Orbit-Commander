using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Wars.Content.Main.Story;
public class EndlessEvent(float _start, Action<float> _action) : IEvent
{
    public bool Update(float _time)
    {
        _action(_time - _start);
        return _time < _start;
    }
}
