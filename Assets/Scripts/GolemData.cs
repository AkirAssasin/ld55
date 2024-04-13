using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GolemBuilder
{
    const float ExceedEffectivenessPenalty = 0.1f;
    const float SkillChanceMultiplier = 1f;

    public readonly float[] m_minStats = new float[(int)GolemStatType.Count];
    public readonly float[] m_maxStats = new float[(int)GolemStatType.Count];
    public readonly List<int> m_skillChance = new List<int>();

    public void Reset()
    {
        for (int X = 0; X < (int)GolemStatType.Count; ++X)
        {
            m_minStats[X] = m_maxStats[X] = 0;
        }
    }

    public void AddMaterial(MaterialData data, int count)
    {
        int typeIndex = (int)data.m_type;
        for (int X = 0; X < count; ++X)
        {
            float currentMax = m_maxStats[typeIndex];
            float addMin = data.m_rarity.m_minValueIncrease;
            float addMax = data.m_rarity.m_maxValueIncrease;
            if (currentMax >= data.m_rarity.m_effectivenessCutoff)
            {
                addMin *= ExceedEffectivenessPenalty;
                addMax *= ExceedEffectivenessPenalty;
            }
            m_minStats[typeIndex] += addMin;
            m_maxStats[typeIndex] += addMax;
        }
    }

    public void CalculateSkillChance()
    {
        m_skillChance.Clear();
        float[] sortedMaxStats = m_maxStats.OrderBy(s => s).ToArray();
        for (int X = 1; X < sortedMaxStats.Length; ++X)
        {
            int chance = (int)((sortedMaxStats[X] - sortedMaxStats[X - 1]) * SkillChanceMultiplier);
            if (chance > 0)
            {
                m_skillChance.Add(chance);
            }
        }
    }

    public GolemData SummonGolem(string name)
    {
        GolemData golemData = new GolemData(name);
        for (int X = 0; X < (int)GolemStatType.Count; ++X)
        {
            int minStatInt = (int)m_minStats[X], maxStatInt = (int)m_maxStats[X];
            golemData.m_stats[X] = Random.Range(minStatInt, maxStatInt + 1);
        }
        return golemData;
    }

    public string GetStatOutcomeString(GolemStatType statType)
    {
        int X = (int)statType;
        return $"{(int)m_minStats[X]} - {(int)m_maxStats[X]}";
    }
}

public class GolemData
{
    public const int StatMax = 100;

    public readonly string m_name;
    public readonly int[] m_stats = new int[(int)GolemStatType.Count];

    public GolemData(string name)
    {
        m_name = name;
    }

    public static string GetStatLabel(GolemStatType statType)
    {
        return statType.ToString();
    }

    public string GetStatString(GolemStatType statType)
    {
        return m_stats[(int)statType].ToString();
    }
}
