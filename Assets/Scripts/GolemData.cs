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
        m_skillChance.Add(100);
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

    public GolemData SummonGolem(string name, Dictionary<int, int> materials)
    {
        //create golem with random weakness
        GolemData golemData = new(name, GameManager.GetRandomElement(), materials);

        //generate stats
        for (int X = 0; X < (int)GolemStatType.Count; ++X)
        {
            int minStatInt = (int)m_minStats[X], maxStatInt = (int)m_maxStats[X];
            golemData.m_stats[X] = Random.Range(minStatInt, maxStatInt + 1);
        }
        golemData.m_health = golemData.GetMaxHealth();

        //generate skills
        GameManager.GetRandomSkills(m_skillChance, golemData.m_skills);
        golemData.m_skills = golemData.m_skills.OrderBy(s => s.m_skillPriority).ToList();

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
    static readonly string[] s_namePool = new[]
    {
        "Ava", "Bella", "Charlotte", "Daisy", "Emma", "Fiona", "Grace", "Hannah", "Isabella",
        "Jasmine", "Katherine", "Lily", "Mia", "Natalie", "Olivia", "Penelope", "Quinn", "Rachel",
        "Sophia", "Taylor", "Ursula", "Victoria", "Willow", "Ximena", "Yara", "Zoe"
    };

    public static string GetRandomName()
    {
        return s_namePool[Random.Range(0, s_namePool.Length)];
    }

    public const int StatMax = 100, SkillMax = 6;

    public readonly Dictionary<int, int> m_originalMaterials;

    public readonly string m_name;
    public readonly int[] m_stats = new int[(int)GolemStatType.Count];

    public readonly ElementTypeData m_elementType;

    public List<BaseSkillData> m_skills = new List<BaseSkillData>();

    public int m_health;

    public GolemData(string name, ElementTypeData elementType, Dictionary<int, int> originalMaterials)
    {
        m_name = name;
        m_elementType = elementType;
        m_originalMaterials = originalMaterials;
    }

    public static string GetStatLabel(GolemStatType statType)
    {
        return statType.ToString();
    }

    public int GetMaxHealth()
    {
        return (1 + m_stats[(int)GolemStatType.Vitality]) * 10;
    }

    public string GetStatString(GolemStatType statType)
    {
        if (statType == GolemStatType.Vitality)
        {
            return $"{m_health} / {GetMaxHealth()}";
        }
        return m_stats[(int)statType].ToString();
    }

    public bool CanBeCrowned() => false;

    public bool DoStatsRoll(GolemStatType statType)
    {
         return Random.Range(0, StatMax) < m_stats[(int)statType];
    }

    public void Feed(GolemData eatThis)
    {
        for (int X = 0; X < (int)GolemStatType.Count; ++X)
        {
            float effectiveness = 1f;
            if (eatThis.m_stats[X] < m_stats[X])
            {
                effectiveness = (float)eatThis.m_stats[X] / m_stats[X];
            }
            int gain = (int)(Mathf.Abs(eatThis.m_stats[X] - m_stats[X]) * effectiveness * Random.value);
            m_stats[X] = Mathf.Min(m_stats[X] + gain, StatMax);
        }

        List<BaseSkillData> skills = new List<BaseSkillData>(m_skills);
        skills.AddRange(eatThis.m_skills);
        while (skills.Count > SkillMax) skills.RemoveAt(Random.Range(0, skills.Count));

        m_skills.Clear();
        m_skills.AddRange(skills.OrderBy(s => s.m_skillPriority));

        m_health = GetMaxHealth();
    }
}
