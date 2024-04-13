using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GolemSlotController : PoolableObject<GolemSlotController>
{
    [SerializeField] TextMeshProUGUI m_nameTextMesh;
    [SerializeField] Image m_icon;

    RectTransform m_rectTransform;
    Action m_onClicked;

    void Awake()
    {
        m_rectTransform = GetComponent<RectTransform>();
    }

    public void Initialize(RectTransform parent, Action onClicked)
    {
        m_rectTransform.SetParent(parent, false);
        m_onClicked = onClicked;
        SetGolemData(null);
    }

    public void SetGolemData(GolemData golem)
    {
        if (golem != null)
        {
            m_nameTextMesh.text = golem.m_name;
            m_icon.sprite = golem.m_elementType.m_icon;
            m_icon.color = golem.m_elementType.m_color;
            m_icon.enabled = true;
        }
        else
        {
            m_nameTextMesh.text = "-";
            m_icon.enabled = false;
        }
    }

    new public void Pool()
    {
        if (base.Pool())
        {
            m_rectTransform.SetParent(null, false);
            m_onClicked = null;
        }
    }

    public void OnClicked() => m_onClicked?.Invoke();
}
