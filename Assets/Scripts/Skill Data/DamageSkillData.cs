using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Damage Skill Data", menuName = "Scriptable Objects/Damage Skill Data")]
public class DamageSkillData : BaseSkillData
{
    const float WeakElementTypeMultiplier = 2;

    public ElementTypeData m_elementType;
    public int m_level;

    public override bool m_isHarmful => true;
    public override int m_skillPriority => m_isSingleTarget ? 2 : 1;

    public override Sprite GetIcon(out Color? color)
    {
        color = m_elementType.m_color;
        return m_elementType.m_icon;
    }

    public override bool TryDoSmartMove(GolemInCombat self, List<GolemInCombat> combatState, bool forceDoAction, out GolemInCombat target)
    {
        GolemInCombat[] targets = CombatManager.GetAllInTeamOrNot(combatState, self.m_team, true);
        for (int X = 0; X < targets.Length; ++X)
        {
            target = targets[X];
            if (target.m_golem.m_elementType.IsWeakTo(m_elementType))
            {
                return true;
            }
        }

        target = targets[Random.Range(0, targets.Length)];
        return forceDoAction;
    }

    public override void PerformAction(GolemInCombat self, List<GolemInCombat> combatState, GolemInCombat target, out string log)
    {
        if (m_isSingleTarget)
        {
            //single target damage
            DoDamage(self, target);
        }
        else
        {
            //all target damage
            GolemInCombat[] targets = CombatManager.GetAllInTeamOrNot(combatState, self.m_team, true);
            for (int X = 0; X < targets.Length; ++X)
            {
                DoDamage(self, targets[X]);
            }
        }
        log = MakeBasicLog(self, target);
    }

    void DoDamage(GolemInCombat self, GolemInCombat target)
    {
        float damage = self.GetRawDamage() * m_level;
        if (target.m_golem.m_elementType.IsWeakTo(m_elementType))
        {
            damage *= WeakElementTypeMultiplier;
        }
        target.TakeDamage(damage);
    }
}
