using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MaterialWeightedBag
{
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

            m_weight[X] = weight;
            m_totalWeight += m_weight[X];
        }

        //hackiest shit ever but here's the weight for health potion
        m_totalWeight += 64;
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
