using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    enum GameState
    {
        MainLobby,
        Summoning,
        Combat,
        Count
    }

    public static GameManager Instance { get; private set; }

    [Header("Game Data")]
    [SerializeField] MaterialData[] m_materialDatas;
    [SerializeField] ElementTypeData[] m_elementTypeDatas;
    [SerializeField] BaseSkillData[] m_guaranteedSkillDatas;
    [SerializeField] BaseSkillData[] m_skillDatas;

    [Header("Summoning UI")]
    [SerializeField] Button m_summonButton;
    [SerializeField] GameObject m_summoningUIParent;
    [SerializeField] MaterialListController m_materialList;

    [Header("Golem Inspect UI")]
    [SerializeField] GameObject m_golemInspectUIParent;
    [SerializeField] GolemInfoPanelController m_golemInspectPanel;

    GameState m_currentGameState = GameState.Count;
    PlayerData m_player;

    void Awake()
    {
        Instance = this;
    }

    #region Static Game Data Stuff

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

    public static ElementTypeData GetRandomElement()
    {
        return Instance.m_elementTypeDatas[Random.Range(0, Instance.m_elementTypeDatas.Length)];
    }

    public static void GetRandomSkills(List<int> skillChance, List<BaseSkillData> skills)
    {
        List<BaseSkillData> skillPool = new List<BaseSkillData>(Instance.m_guaranteedSkillDatas);
        for (int X = 0; X < skillChance.Count; ++X)
        {
            if (Random.Range(0, 100) >= skillChance[X])
            {
                //failed the roll
                continue;
            }

            int skillIndex = Random.Range(0, skillPool.Count);
            skills.Add(skillPool[skillIndex]);
            skillPool.RemoveAt(skillIndex);
            
            //guaranteed skill taken; fill with remaining
            if (X == 0) skillPool.AddRange(Instance.m_skillDatas);
        }
    }

    #endregion

    void Start()
    {
        m_player = new PlayerData();

        m_golemInspectUIParent.SetActive(false);
        m_summoningUIParent.SetActive(false);

        ChangeGameState(GameState.MainLobby);
    }

    void ChangeGameState(GameState nextGameState)
    {
        switch (m_currentGameState)
        {
            case GameState.MainLobby:
                m_golemInspectUIParent.SetActive(false);
                break;

            case GameState.Summoning:
                m_summoningUIParent.SetActive(false);
                break;
        }
        m_currentGameState = nextGameState;
        switch (m_currentGameState)
        {
            case GameState.MainLobby:
                m_summonButton.interactable = (m_player.HasOpenGolemSlot() && m_player.m_inventory.Count > 0);
                break;

            case GameState.Summoning:
                m_materialList.Initialize(m_player);
                m_summoningUIParent.SetActive(true);
                break;
        }
    }

    #region UI Button Callbacks

    public void OnSummonClicked() //button to enter summoning
    {
        ChangeGameState(GameState.Summoning);
    }

    public void OnSummonCompleted() //button to complete summoning
    {
        GolemData golem = m_materialList.SummonGolem(m_player);
        ChangeGameState(GameState.MainLobby);
        OpenGolemInspectUI(golem);
    }
    public void OnSummoningCancelled() => ChangeGameState(GameState.MainLobby);

    public void CloseGolemInspectUI()
    {
        m_golemInspectUIParent.SetActive(false);
    }

    #endregion

    public void OpenGolemInspectUI(GolemData golem)
    {
        m_golemInspectPanel.Initialize(golem, false);
        m_golemInspectUIParent.SetActive(true);
    }
}
