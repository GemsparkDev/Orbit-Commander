using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Space_Wars.Content.Main.Entities;

namespace Space_Wars.Content.Main.Components;
internal interface IStealth
{
    public int StealthAbility { get; }
    public int SensingAbility { get; }
}
public class Stealth(Entity _entity) : Component(_entity), IStealth
{
    public int StealthAbility { get; set; }
    public int SensingAbility { get; set; }
}
//public class (Entity _entity) : Component(_entity), IStealth
//{
//    public int StealthAbility { get; }
//    public int SensingAbility { get; }
//}
