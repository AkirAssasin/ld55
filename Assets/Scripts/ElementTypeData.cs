using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Element Type Data", menuName = "Scriptable Objects/Element Type Data")]
public class ElementTypeData : ScriptableObject
{
    public Sprite m_icon;
    public Color m_color;
    public List<ElementTypeData> m_weakTo;

    public bool IsWeakTo(ElementTypeData otherType)
    {
        return m_weakTo.Contains(otherType);
    }
}
