using System.Collections.Generic;
using System.ComponentModel;

namespace Space_Wars.Content.Main.Components;
public class ComponentList
{
    private Dictionary<ComponentType, IComponent> components = new();
    public IComponent GetComponent(ComponentType _componentType)
    {
        return components.ContainsKey(_componentType) ? components[_componentType] : new DefaultComponent();
    }
    public bool Add(IComponent _component)
    {
        if(components.ContainsKey(_component.Type))
        {
            return false;
        }
        components.Add(_component.Type, _component);
        return true;
    }
    public bool Remove(ComponentType _componentType)
    {
        return components.Remove(_componentType);
    }
}
