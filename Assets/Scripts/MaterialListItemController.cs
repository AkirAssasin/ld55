using TMPro;
using UnityEngine;

public class MaterialListItemController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI m_nameTextMesh, m_countTextMesh, m_selectedCountTextMesh;
    [SerializeField] GameObject m_selectedCountObject, m_deselectButtonObject;

    int m_materialID = 0;
    int m_maxCount = 999;
    int m_selectedCount;

    void Start()
    {
        SetSelectedCount(0);
    }

    public void Initialize(int materialID, int maxCount)
    {
        m_materialID = materialID;
        
        MaterialData materialData = GameManager.GetMaterialData(m_materialID);
        m_nameTextMesh.text = materialData.name;
        m_nameTextMesh.color = GameManager.GetRarityColor(materialData.m_rarity);

        m_maxCount = maxCount;
        m_countTextMesh.text = m_maxCount.ToString();
        SetSelectedCount(m_selectedCount);
    }

    void SetSelectedCount(int selectedCount)
    {
        m_selectedCount = Mathf.Max(Mathf.Min(selectedCount, m_maxCount), 0);
        m_selectedCountObject.SetActive(m_selectedCount > 0);
        m_deselectButtonObject.SetActive(m_selectedCount > 0);
        m_selectedCountTextMesh.text = m_selectedCount.ToString();
    }

    public void SelectAll() => SetSelectedCount(m_maxCount);

    public void OnClicked() => SetSelectedCount(m_selectedCount + 1);
    public void OnDeselectClicked() => SetSelectedCount(m_selectedCount - 1);
}
