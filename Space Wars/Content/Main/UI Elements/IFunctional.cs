using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Wars.Content.Main.UI_Elements
{
    public interface IFunctional
    {
        public void Interact();
        public void AddBehaviour(DelegateMethod func);
        public void ApplyBehaviours();
    }
}
