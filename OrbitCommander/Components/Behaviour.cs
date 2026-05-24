using System.Collections.Generic;

namespace OrbitCommander.Components;
internal class Behaviour() : Component()
{
    private List<IEnumerator<int>> behaviours = [];
    public Behaviour AddBehaviour(IEnumerable<int> behaviour)
    {
        behaviours.Add(behaviour.GetEnumerator());
        return this;
    }
    public override void Update()
    {
        for (int i = 0; i < behaviours.Count; i++)
        {
            if (!behaviours[i].MoveNext())
            {
                behaviours.RemoveAt(i--);
            }
        }
    }
}
