using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MaterialListController : MonoBehaviour
{
    [SerializeField] GameObject m_listItemPrefab;
    [SerializeField] RectTransform m_contentTransform;
    [SerializeField] GolemInfoPanelController m_builderPanel;
    [SerializeField] Button m_summonButton;

    readonly List<MaterialListItemController> m_listItems = new List<MaterialListItemController>();

    GolemBuilder m_golemBuilder = new GolemBuilder();

    public void Initialize(PlayerData playerData)
    {
        m_builderPanel.ResetInputFields();
        for (int X = 0; X < m_listItems.Count; ++X)
        {
            m_listItems[X].Pool();
        }
        m_listItems.Clear();
        foreach (var (materialID, count) in playerData.m_inventory)
        {
            MaterialListItemController listItem = MaterialListItemController.GetFromPool(m_listItemPrefab);
            listItem.Initialize(m_contentTransform, materialID, count, Recalculate);
            m_listItems.Add(listItem);
        }
        Recalculate();
    }

    void Recalculate()
    {
        m_golemBuilder.Reset();
        bool canSummon = false;
        foreach (var listItem in m_listItems.OrderBy(MaterialListItemController.GetPriorityInGolemBuilding))
        {
            MaterialData material = listItem.GetSelectionData(out int selectedCount);
            if (selectedCount > 0)
            {
                canSummon = true;
                m_golemBuilder.AddMaterial(material, selectedCount);
            }
        }
        m_summonButton.interactable = canSummon;
        m_golemBuilder.CalculateSkillChance();
        m_builderPanel.Initialize(m_golemBuilder);
    }

    public void SelectAll()
    {
        for (int X = 0; X < m_listItems.Count; ++X)
        {
            m_listItems[X].SelectAll();
        }
        Recalculate();
    }

    public MaterialListItemController GetItem(int index)
    {
        return m_listItems[index];
    }

    public int SummonGolem(PlayerData player)
    {
        for (int X = 0; X < m_listItems.Count; ++X)
        {
            m_listItems[X].RemoveFromPlayerInventory(player);
        }
        GolemData golem = m_golemBuilder.SummonGolem(m_builderPanel.GetInputFieldName());
        player.m_golems.Add(golem);
        return player.m_golems.Count - 1;
    }
}
