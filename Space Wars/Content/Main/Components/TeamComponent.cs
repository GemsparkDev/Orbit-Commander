namespace Space_Wars.Content.Main.Components;

public class TeamComponent : IComponent
{
    public ComponentType Type { get; } = ComponentType.TeamComponent;
    public bool IsFriendly { get; private set; }
    public int SensingAbility { get; private set; }
    public int StealthAbility { get; private set; }
    public TeamComponent(bool _isFriendly, int _sensingAbility, int _stealthAbility)
    {
        IsFriendly = _isFriendly;
        SensingAbility = _sensingAbility;   
        StealthAbility = _stealthAbility;
    }
    public bool IsValid { get { return true; } }
}
