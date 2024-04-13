using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseSkillData : ScriptableObject
{
    public virtual Sprite GetIcon(out Color? color)
    {
        color = null;
        return null;
    }
}
