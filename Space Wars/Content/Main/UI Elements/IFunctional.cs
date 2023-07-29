using Microsoft.Xna.Framework;

namespace Space_Wars.Content.Main.UI_Elements
{
    public interface IFunctional
    {
        public void Interact(Vector2 parentPosition);
        public void ContinuousInteract(Vector2 parentPosition);
        public void AddBehaviour(DelegateMethod func);
        public void ApplyBehaviours();
    }
}
