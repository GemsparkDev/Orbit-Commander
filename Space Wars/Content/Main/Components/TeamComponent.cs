namespace Space_Wars.Content.Main.Components;

public class TeamComponent(bool _isFriendly, int _sensingAbility, int _stealthAbility) : IComponent
{
    public ComponentType Type { get; } = ComponentType.TeamComponent;
    public bool IsFriendly { get; } = _isFriendly;
    public int SensingAbility { get; } = _sensingAbility;
    public int StealthAbility { get; } = _stealthAbility;

    public bool IsValid { get { return true; } }
}
