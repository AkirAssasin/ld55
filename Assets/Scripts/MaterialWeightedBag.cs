using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ExpeditionAffinity
{
    public readonly bool m_healthPotion  =false;
    readonly MaterialRarityData m_rarity = null;
    readonly MaterialData m_material = null;
    readonly GolemStatType m_materialType = GolemStatType.Count;

    public ExpeditionAffinity()
    {
        switch (Random.Range(0, 4))
        {
            case 0:
                m_healthPotion = true;
                break;
            case 1:
                m_rarity = GameManager.GetRandomMaterialData().m_rarity;
                break;
            case 2:
                m_material = GameManager.GetRandomMaterialData();
                break;
            case 3:
                m_materialType = GameManager.GetRandomMaterialData().m_type;
                break;
        }
    }

    public bool Matches(MaterialData materialData)
    {
        if (materialData.m_rarity == m_rarity) return true;
        if (materialData == m_material) return true;
        if (materialData.m_type == m_materialType) return true;
        return false;
    }

    public string GetAffinityString()
    {
        if (m_rarity != null) return m_rarity.name;
        if (m_material != null) return m_material.name;
        if (m_materialType != GolemStatType.Count) return GolemData.GetStatLabel(m_materialType);
        if (m_healthPotion) return "Health Potion";
        return "None";
    }
}

public class MaterialWeightedBag
{
    const int AffinityMultiplier = 4;

    readonly int[] m_weight;
    readonly int m_totalWeight;

    public MaterialWeightedBag(MaterialData[] materialDatas, GolemData golemData)
    {
        m_weight = new int[materialDatas.Length];
        for (int X = 0; X < materialDatas.Length; ++X)
        {
            MaterialData material = materialDatas[X];
            int weight = material.m_rarity.m_expeditionWeight;

            //do golem stuff here
            if (golemData.m_expeditionAffinity != null && golemData.m_expeditionAffinity.Matches(material))
            {
                weight *= AffinityMultiplier;
            }
            m_weight[X] = weight;
            m_totalWeight += m_weight[X];
        }

        //hackiest shit ever but here's the weight for health potion
        int potionWeight = 64;
        if (golemData.m_expeditionAffinity != null && golemData.m_expeditionAffinity.m_healthPotion)
        {
            potionWeight *= AffinityMultiplier;
        }
        m_totalWeight += potionWeight;
    }

    public int GetRandomMaterial()
    {
        int w = Random.Range(0, m_totalWeight);
        for (int X = 0; X < m_weight.Length; ++X)
        {
            w -= m_weight[X];
            if (w < 0) return X;
        }
        return -1; //is a health potion
    }
}
