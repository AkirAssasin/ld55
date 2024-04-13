using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Data")]
    [SerializeField] MaterialData[] m_materialDatas;
    [SerializeField] ElementTypeData[] m_elementTypeDatas;

    [Header("Summoning UI")]
    [SerializeField] GameObject m_summoningUIParent;
    [SerializeField] MaterialListController m_materialList;

    [Header("Golem Inspect UI")]
    [SerializeField] GameObject m_golemInspectUIParent;
    [SerializeField] GolemInfoPanelController m_golemInspectPanel;

    PlayerData m_player;

    void Awake()
    {
        Instance = this;
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

    public static ElementTypeData GetRandomElement()
    {
        return Instance.m_elementTypeDatas[Random.Range(0, Instance.m_elementTypeDatas.Length)];
    }

    void Start()
    {
        m_player = new PlayerData();

        m_golemInspectUIParent.SetActive(false);
        m_summoningUIParent.SetActive(false);

        OpenSummoningUI();
    }

    public void CloseSummoningUI()
    {
        m_summoningUIParent.SetActive(false);
    }

    public void OpenSummoningUI()
    {
        m_materialList.Initialize(m_player);
        m_summoningUIParent.SetActive(true);
    }

    public void OnSummonClicked()
    {
        GolemData golem = m_materialList.SummonGolem(m_player);
        CloseSummoningUI();
        OpenGolemInspectUI(golem);
    }

    public void OpenGolemInspectUI(GolemData golem)
    {
        m_golemInspectPanel.Initialize(golem);
        m_golemInspectUIParent.SetActive(true);
    }
}
