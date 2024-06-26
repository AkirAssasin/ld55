using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GolemInfoPanelController : MonoBehaviour
{
    [SerializeField] GameObject m_listItemPrefab;
    [SerializeField] RectTransform m_statsListTransform, m_skillsListTransform;
    [SerializeField] TextMeshProUGUI m_nameTextMesh;
    [SerializeField] GameObject m_nameInputFieldObject;

    readonly List<GolemListItemController> StatsListItems = new List<GolemListItemController>(),
        SkillsListItems = new List<GolemListItemController>();

    TMP_InputField m_nameInputField;

    void Start()
    {
        m_nameInputField = m_nameInputFieldObject.GetComponent<TMP_InputField>();
    }

    public void ResetAll()
    {
        for (int X = 0; X < StatsListItems.Count; ++X)
            StatsListItems[X].Pool();
        StatsListItems.Clear();

        for (int X = 0; X < SkillsListItems.Count; ++X)
            SkillsListItems[X].Pool();
        SkillsListItems.Clear();
    }

    public void Initialize(GolemBuilder golemBuilder)
    {
        ResetAll();
        for (GolemStatType statType = 0; statType < GolemStatType.Count; ++statType)
        {
            //add stats info
            AddStatListItem(statType, golemBuilder.GetStatOutcomeString(statType));
        }
        for (int X = 0; X < golemBuilder.m_skillChance.Count; ++X)
        {
            //add skill chance info
            AddSkillListItem("", $"{golemBuilder.m_skillChance[X]}%", null);
        }
        m_nameInputFieldObject.SetActive(true);
        m_nameTextMesh.enabled = false;
    }

    public void Initialize(GolemData golem)
    {
        ResetAll();
        for (GolemStatType statType = 0; statType < GolemStatType.Count; ++statType)
        {
            //add stats info
            AddStatListItem(statType, golem.GetStatString(statType));
        }
        for  (int X = 0; X < golem.m_skills.Count; ++X)
        {
            //add skill info
            AddSkillListItem(null, null, golem.m_skills[X]);
        }

        //add element info
        GolemListItemController listItem = GolemListItemController.GetFromPool(m_listItemPrefab);
        listItem.Initialize(m_statsListTransform, "Element", null, golem.m_elementType.m_icon, golem.m_elementType.m_color);
        StatsListItems.Add(listItem);

        //add expedition
        if (golem.m_expeditionAffinity != null)
        {
            AddSkillListItem("Affinity", golem.m_expeditionAffinity.GetAffinityString(), null);
        }

        m_nameInputFieldObject.SetActive(false);
        m_nameTextMesh.text = golem.m_name;
        m_nameTextMesh.enabled = true;
    }

    public string GetInputFieldName()
    {
        string name = m_nameInputField.text.Trim();
        if (string.IsNullOrEmpty(name))
        {
            name = GolemData.GetRandomName();
        }
        return name;
    }

    public void ResetInputFields()
    {
        if (m_nameInputField != null)
        {
            //can be null because of uncertain start call
            m_nameInputField.text = string.Empty;
        }
    }

    void AddStatListItem(GolemStatType statType, string value)
    {
        GolemListItemController listItem = GolemListItemController.GetFromPool(m_listItemPrefab);
        listItem.Initialize(m_statsListTransform, GolemData.GetStatLabel(statType), value, null);
        StatsListItems.Add(listItem);
    }

    void AddSkillListItem(string label, string value, BaseSkillData skill)
    {
        GolemListItemController listItem = GolemListItemController.GetFromPool(m_listItemPrefab);
        Action callback = null;
        Sprite icon = null;
        Color? color = null;
        if (skill != null)
        {
            //replace with skill info
            icon = skill.GetIcon(out color);
            label = skill.name;
            value = null;
        }
        listItem.Initialize(m_skillsListTransform, label, value, icon, color, callback);
        SkillsListItems.Add(listItem);
    }
}
