using System;
using TMPro;
using UnityEngine;

public class UnmakeListItemController : PoolableObject<UnmakeListItemController>
{
    [SerializeField] TextMeshProUGUI m_nameTextMesh;

    public GolemData m_golem;
    Action<UnmakeListItemController> m_onSalvage, m_onFeed;

    RectTransform m_rectTransform;

    void Awake()
    {
        m_rectTransform = GetComponent<RectTransform>();
    }

    public void Initialize(RectTransform parent, GolemData golem,
        Action<UnmakeListItemController> onSalvage, Action<UnmakeListItemController> onFeed)
    {
        m_rectTransform.SetParent(parent, false);

        m_golem = golem;
        m_nameTextMesh.text = m_golem.m_name;

        m_onSalvage = onSalvage;
        m_onFeed = onFeed;
    }

    new public void Pool()
    {
        if (base.Pool())
        {
            m_rectTransform.SetParent(null, false);
            m_onSalvage = m_onFeed = null;
        }
    }

    public void OnSalvageClicked() => m_onSalvage(this);

    public void OnFeedClicked() => m_onFeed(this);
}
