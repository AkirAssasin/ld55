using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GolemListItemController : PoolableObject<GolemListItemController>
{
    [SerializeField] TextMeshProUGUI m_labelTextMesh, m_valueTextMesh;
    [SerializeField] Image m_icon;
    [SerializeField] Action m_onClicked;

    RectTransform m_rectTransform;

    void Awake()
    {
        m_rectTransform = GetComponent<RectTransform>();
    }

    public void Initialize(RectTransform parent, string label, string value, Sprite icon, Color? iconColor = null, Action onClicked = null)
    {
        m_rectTransform.SetParent(parent, false);

        m_labelTextMesh.text = label;
        if (value != null)
        {
            m_valueTextMesh.text = value;
            
            m_valueTextMesh.enabled = true;
            m_icon.enabled = false;
        }
        else if (icon != null)
        {
            m_icon.sprite = icon;
            m_icon.color = iconColor ?? Color.white;

            m_valueTextMesh.enabled = false;
            m_icon.enabled = true;
        }
        else  m_valueTextMesh.enabled = m_icon.enabled = false;
        m_onClicked = onClicked;
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
