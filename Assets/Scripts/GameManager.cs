using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    const int HealthPerExpedition = 1;

    enum GameState
    {
        MainLobby,
        Summoning,
        Combat,
        Count
    }

    public static GameManager Instance { get; private set; }

    [Header("Game Data")]
    [SerializeField] int m_minGatherable;
    [SerializeField] int m_maxGatherable;
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
    [SerializeField] GameObject m_golemInspectOK, m_golemInspectUnmake,
        m_golemInspectCrown, m_golemInspectExpedition;

    [Header("Golem Slots UI")]
    [SerializeField] GameObject m_golemSlotPrefab;
    [SerializeField] RectTransform m_golemSlotParent;

    [Header("Expedition UI")]
    [SerializeField] GameObject m_expeditionUIParent;
    [SerializeField] TextMeshProUGUI m_expeditionResultsTextMesh, m_expeditionHealthRemainingTextMesh;
    [SerializeField] Button m_repeatExpeditionButton;
    
    Button m_golemInspectCrownButton, m_golemInspectUnmakeButton, m_golemInspectExpeditionButton;

    readonly List<GolemSlotController> m_golemSlots = new List<GolemSlotController>();
    GameState m_currentGameState = GameState.Count;
    PlayerData m_player;

    MaterialWeightedBag m_inspectGolemExpeditionBag = null;
    readonly Dictionary<int, int> m_lastExpeditionResults = new Dictionary<int, int>();
    int m_inspectGolemIndex = -1;

    void Awake()
    {
        Instance = this;
    }

    #region Static Game Data Stuff

    public static MaterialData GetMaterialData(int id)
    {
        return Instance.m_materialDatas[id];
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
        m_golemInspectCrownButton = m_golemInspectCrown.GetComponent<Button>();
        m_golemInspectUnmakeButton = m_golemInspectUnmake.GetComponent<Button>();
        m_golemInspectExpeditionButton = m_golemInspectExpedition.GetComponent<Button>();

        m_player = new PlayerData();

        m_golemInspectUIParent.SetActive(false);
        m_summoningUIParent.SetActive(false);
        m_expeditionUIParent.SetActive(false);

        RebuildGolemSlots();
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
                CheckCanSummon();
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
        int newGolemIndex = m_materialList.SummonGolem(m_player);
        ReassignGolemSlots();
        ChangeGameState(GameState.MainLobby);
        OpenGolemInspectUI(newGolemIndex, true);
    }
    public void OnSummoningCancelled() => ChangeGameState(GameState.MainLobby);

    public void CloseGolemInspectUI()
    {
        m_golemInspectUIParent.SetActive(false);
        m_inspectGolemIndex = -1;
    }

    public void DoExpedition()
    {
        GolemData golem = m_player.m_golems[m_inspectGolemIndex];
        if (m_inspectGolemExpeditionBag == null)
        {
            m_inspectGolemExpeditionBag = new MaterialWeightedBag(m_materialDatas, golem);
        }
        float value = Mathf.Max(golem.m_stats) / GolemData.StatMax;

        int min = (int)(m_minGatherable * value), max = (int)(m_maxGatherable * value);
        int final = 1 + Random.Range(min, max);

        m_lastExpeditionResults.Clear();
        for (int X = 0; X < final; ++X)
        {
            int gotThis = m_inspectGolemExpeditionBag.GetRandomMaterial();
            if (!m_lastExpeditionResults.TryGetValue(gotThis, out int count)) count = 0;
            m_lastExpeditionResults[gotThis] = count + 1;
        }

        string resultText = $"{golem.m_name} has found the following:";
        foreach (var (materialID, count) in m_lastExpeditionResults)
        {
            MaterialData materialData = GetMaterialData(materialID);
            m_player.AddIntoInventory(materialID, count);
            resultText += $"\n<color=#{ColorUtility.ToHtmlStringRGB(materialData.m_rarity.m_color)}>{materialData.name}</color> x{count}";
        }
        m_expeditionResultsTextMesh.text = resultText;

        golem.m_health -= HealthPerExpedition;
        OnInspectedGolemHealthChanged();
        CheckCanSummon();
        m_expeditionHealthRemainingTextMesh.text = golem.GetStatString(GolemStatType.Vitality);

        m_expeditionUIParent.SetActive(true);
    }

    public void CloseExpeditionResults()
    {
        m_expeditionUIParent.SetActive(false);
        OpenGolemInspectUI(m_inspectGolemIndex, false);
    }

    #endregion

    void OpenGolemInspectUI(int index, bool isNew)
    {
        m_inspectGolemIndex = index;
        GolemData golem = m_player.m_golems[m_inspectGolemIndex];

        m_golemInspectPanel.Initialize(golem, false);

        m_golemInspectCrownButton.interactable = golem.CanBeCrowned();

        m_golemInspectOK.SetActive(isNew);
        m_golemInspectCrown.SetActive(isNew);

        m_golemInspectUnmakeButton.interactable = (m_player.m_golems.Count >= 2);
        OnInspectedGolemHealthChanged();

        m_golemInspectUnmake.SetActive(!isNew);
        m_golemInspectExpedition.SetActive(!isNew);

        m_golemInspectUIParent.SetActive(true);
    }

    void OnGolemSlotClicked(int index)
    {
        if (index < m_player.m_golems.Count)
        {
            OpenGolemInspectUI(index, false);
        }
    }

    void RebuildGolemSlots()
    {
        for (int X = 0; X < m_golemSlots.Count; ++X)
            m_golemSlots[X].Pool();
        m_golemSlots.Clear();
        
        for (int X = 0; X < m_player.m_maxGolemSlots; ++X)
        {
            int index = X;
            GolemSlotController golemSlot = GolemSlotController.GetFromPool(m_golemSlotPrefab);
            golemSlot.Initialize(m_golemSlotParent, () => OnGolemSlotClicked(index));
            m_golemSlots.Add(golemSlot);
        }
        ReassignGolemSlots();
    }

    void ReassignGolemSlots()
    {
        for (int X = 0; X < m_golemSlots.Count; ++X)
        {
            GolemData golem = null;
            if (X < m_player.m_golems.Count)
            {
                golem = m_player.m_golems[X];
            }
            m_golemSlots[X].SetGolemData(golem);
        }
    }

    void CheckCanSummon()
    {
        m_summonButton.interactable = (m_player.HasOpenGolemSlot() && m_player.m_inventory.Count > 0);
    }

    void OnInspectedGolemHealthChanged()
    {
        GolemData golem = m_player.m_golems[m_inspectGolemIndex];
        bool canExpedition = (golem.m_health > HealthPerExpedition);
        m_golemInspectExpeditionButton.interactable = m_repeatExpeditionButton.interactable = canExpedition;

        m_golemSlots[m_inspectGolemIndex].SetHealth(golem);
    }
}
