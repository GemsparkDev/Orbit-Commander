using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Wars.Content.Main;
public interface IEvent
{
    public bool Update(float _time);
}
