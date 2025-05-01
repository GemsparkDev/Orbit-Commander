using System.Collections.Generic;
using System.ComponentModel;

namespace Space_Wars.Content.Main.Components;
public class ComponentList
{
    private Dictionary<ComponentType, IComponent> components = new();
    public T GetComponent<T>(ComponentType _componentType)
    {
        return components.ContainsKey(_componentType) ? (T)components[_componentType] : default;
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
