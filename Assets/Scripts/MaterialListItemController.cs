using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MaterialListItemController : PoolableObject<MaterialListItemController>
{
    [SerializeField] TextMeshProUGUI m_nameTextMesh, m_countTextMesh, m_selectedCountTextMesh;
    [SerializeField] GameObject m_selectedCountObject, m_deselectButtonObject;

    int m_materialID = 0;
    int m_maxCount = 999;
    int m_selectedCount;
    Action m_onChangedByUI = null;

    RectTransform m_rectTransform;

    void Awake()
    {
        m_rectTransform = GetComponent<RectTransform>();
    }

    public void Initialize(RectTransform parent, int materialID, int maxCount, Action onChangedByUI = null)
    {
        m_rectTransform.SetParent(parent, false);
        m_materialID = materialID;
        m_onChangedByUI = onChangedByUI;
        
        MaterialData materialData = GameManager.GetMaterialData(m_materialID);
        m_nameTextMesh.text = materialData.name;
        m_nameTextMesh.color = materialData.m_rarity.m_color;

        m_selectedCount = 0;
        SetMaxCount(maxCount);
    }

    new public void Pool()
    {
        if (base.Pool())
        {
            m_rectTransform.SetParent(null, false);
            m_onChangedByUI = null;
        }
    }

    void SetMaxCount(int maxCount)
    {
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

    public MaterialData GetSelectionData(out int selectedCount)
    {
        selectedCount = m_selectedCount;
        return GameManager.GetMaterialData(m_materialID);
    }

    public static int GetPriorityInGolemBuilding(MaterialListItemController listItem)
    {
        return GameManager.GetMaterialData(listItem.m_materialID).m_rarity.m_effectivenessCutoff;
    }

    public void SelectAll() => SetSelectedCount(m_maxCount);

    public void RemoveFromPlayerInventory(PlayerData player, Dictionary<int, int> materialsUsed)
    {
        materialsUsed[m_materialID] = m_selectedCount;
        int remaining = player.RemoveFromInventory(m_materialID, m_selectedCount);
        SetMaxCount(remaining);
    }

    public void OnClicked()
    {
        SetSelectedCount(m_selectedCount + 1);
        m_onChangedByUI?.Invoke();
    }

    public void OnDeselectClicked()
    {
        SetSelectedCount(m_selectedCount - 1);
        m_onChangedByUI?.Invoke();
    }
}
