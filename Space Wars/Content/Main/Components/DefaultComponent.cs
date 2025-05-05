using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Wars.Content.Main.Components;
public class DefaultComponent : IComponent
{
    public ComponentType Type { get; }
    public bool IsValid { get { return false; } }
}
