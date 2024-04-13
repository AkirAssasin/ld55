using System.Collections.Generic;
using UnityEngine;

public class MaterialListController : MonoBehaviour
{
    [SerializeField] GameObject m_listItemPrefab;
    [SerializeField] RectTransform m_contentTransform;

    readonly List<MaterialListItemController> m_listItems = new List<MaterialListItemController>();
    int m_listItemsInUse = 0;
    public int Count => m_listItemsInUse;

    public void Initialize(PlayerData playerData)
    {
        Resize(playerData.m_inventory.Count);
        int index = 0;
        foreach (var (materialID, count) in playerData.m_inventory)
        {
            MaterialListItemController listItem = m_listItems[index++];
            listItem.Initialize(materialID, count);
        }
    }

    void Resize(int newSize)
    {
        while (m_listItems.Count <= newSize)
        {
            MaterialListItemController listItem = Instantiate(m_listItemPrefab).GetComponent<MaterialListItemController>();
            m_listItems.Add(listItem);
        }
        for (int X = m_listItemsInUse; X < newSize; ++X)
        {
            m_listItems[X].transform.SetParent(m_contentTransform, false);
        }
        for (int X = newSize; X < m_listItems.Count; ++X)
        {
            m_listItems[X].transform.SetParent(null);
        }
        m_listItemsInUse = newSize;
    }

    public void SelectAll()
    {
        for (int X = 0; X < m_listItemsInUse; ++X)
        {
            m_listItems[X].SelectAll();
        }
    }

    public MaterialListItemController GetItem(int index)
    {
        return m_listItems[index];
    }
}
