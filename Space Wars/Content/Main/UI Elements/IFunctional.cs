namespace Space_Wars.Content.Main.UI_Elements
{
    public interface IFunctional
    {
        public void Interact();
        public void AddBehaviour(DelegateMethod func);
        public void ApplyBehaviours();
    }
}
