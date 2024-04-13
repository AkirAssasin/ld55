using UnityEngine;

[CreateAssetMenu(fileName = "New Rarity Data", menuName = "Scriptable Objects/Material Rarity Data")]
public class MaterialRarityData : ScriptableObject
{
    public Color m_color;
    public float m_minValueIncrease;
    public float m_maxValueIncrease;
    public int m_effectivenessCutoff;
}
