using UnityEngine;

public enum GolemStatType
{
    Strength,
    Vitality,
    Intelligence,
    Obedience,
    Speed,
    Count
}

[CreateAssetMenu(fileName = "New Material Data", menuName = "Scriptable Objects/Material Data")]
public class MaterialData : ScriptableObject
{
    public MaterialRarityData m_rarity;
    public GolemStatType m_type;
}
