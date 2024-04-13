using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Data")]
    [SerializeField] MaterialData[] m_materialDatas;
    [SerializeField] Color[] m_rarityColors;

    [Header("Summoning UI")]
    [SerializeField] MaterialListController m_materialList;

    PlayerData m_player;

    void Awake()
    {
        Instance = this;
    }

    public static Color GetRarityColor(int rarity)
    {
        return Instance.m_rarityColors[rarity];
    }

    public static MaterialData GetMaterialData(int id)
    {
        return Instance.m_materialDatas[id];
    }

    public static int GetMaterialCount()
    {
        return Instance.m_materialDatas.Length;
    }

    public static int GetMaterialID(MaterialData data)
    {
        return System.Array.IndexOf(Instance.m_materialDatas, data);
    }

    void Start()
    {
        m_player = new PlayerData();
        m_materialList.Initialize(m_player);
    }
}
