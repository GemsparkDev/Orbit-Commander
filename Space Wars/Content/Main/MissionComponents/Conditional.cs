using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Space_Wars.Content.Main;
using Space_Wars.Content.Main.Entities;

namespace Space_Wars.Content.Main.MissionComponents;
public class Conditional(ICondition[] _conditions, Func<Conditional> _result)
{
    public void Initialize()
    {
        foreach(var condition in _conditions)
        {
            condition.Initialize();
        }
    }
    public Conditional Update()
    {
        bool allCompleted = true;
        foreach (var objective in _conditions)
        {
            allCompleted &= objective.IsComplete();
        }
        if (allCompleted)
        {
            var conditional = _result();
            conditional.Initialize();
            return conditional;
        }
        return this;
    }
    public void CompleteCustomRule(Entity _target)
    {
        foreach (var objective in _conditions)
        {
            if (objective is EntityCondition)
            {
                (objective as EntityCondition).CustomCompleteRule(_target);
            }
        }
    }
    public Conditional Clone()
    {
        List<ICondition> conds = [];
        foreach(var condition in _conditions)
        {
            conds.Add(condition.Clone());
        }
        return new Conditional(conds.ToArray(), _result);
    }
}
