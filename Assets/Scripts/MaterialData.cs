using UnityEngine;

public enum MaterialType
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
    public int m_rarity;
    public MaterialType m_type;
}
