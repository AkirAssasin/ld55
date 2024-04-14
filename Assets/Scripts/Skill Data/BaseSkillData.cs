using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseSkillData : ScriptableObject
{
    public bool m_isSingleTarget;

    public abstract bool m_isHarmful { get; }
    public abstract int m_skillPriority { get; }

    public virtual Sprite GetIcon(out Color? color)
    {
        color = null;
        return null;
    }

    public abstract bool TryDoSmartMove(GolemInCombat self, List<GolemInCombat> combatState, bool forceDoAction, out GolemInCombat target);

    public abstract void PerformAction(GolemInCombat self, List<GolemInCombat> combatState, GolemInCombat target);
}
