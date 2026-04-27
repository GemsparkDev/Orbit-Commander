using Space_Wars.Content.Main.Entities;
using System;

namespace Space_Wars.Content.Main.MissionComponents;
public class Conditional(ICondition[] _conditions, Func<Conditional> _result)
{
    private float restartTimer = -1;
    public void Initialize() { }
    public Conditional Update()
    {
        bool allCompleted = true;
        foreach (var objective in _conditions)
        {
            allCompleted &= objective.IsComplete();
        }
        if (allCompleted)
        {
            restartTimer = 5;
        }
        if (restartTimer > 0)
        {
            restartTimer -= Engine.DeltaSeconds;
            if (restartTimer <= 0)
            {
                var conditional = _result();
                conditional.Initialize();
                return conditional;
            }
        }
        return this;
    }
    public void CompleteCustomRule(Entity _target)
    {
        foreach (var objective in _conditions)
        {
            if (objective is Custom)
            {
                (objective as Custom).CustomCompleteRule(_target);
            }
        }
    }
}
