using UnityEngine;

[CreateAssetMenu(fileName = "New Damage Skill Data", menuName = "Scriptable Objects/Damage Skill Data")]
public class DamageSkillData : BaseSkillData
{
    public ElementTypeData m_elementType;
    public int m_level;
    public bool m_singleTarget;

    public override Sprite GetIcon(out Color? color)
    {
        color = m_elementType.m_color;
        return m_elementType.m_icon;
    }
}
