using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GolemInCombat
{
    public readonly GolemData m_golem;
    readonly GolemSlotController m_slotController;
    public readonly int m_team;

    public int m_currentActionValue;

    public GolemInCombat(GolemData golem, GolemSlotController slotController, int team)
    {
        m_golem = golem;
        m_slotController = slotController;
        m_team = team;
        ResetActionValue();
    }

    public void ResetActionValue()
    {
        int trueSpeed = 100 + m_golem.m_stats[(int)GolemStatType.Speed];
        m_currentActionValue = CombatManager.TimePerAction / trueSpeed;
    }

    public void PoolSlotController() => m_slotController.Pool();

    public BaseSkillData ChooseIntelligentAction(List<GolemInCombat> combatState, out GolemInCombat target)
    {
        for (int X = 0; X < m_golem.m_skills.Count; ++X)
        {
            if (m_golem.m_skills[X].TryDoSmartMove(this, combatState, X == m_golem.m_skills.Count - 1, out target)) return m_golem.m_skills[X];
        }
        throw new System.Exception("not smart enough");
    }

    public float GetRawDamage()
    {
        return m_golem.m_stats[(int)GolemStatType.Strength];
    }

    public void TakeDamage(float damage)
    {
        int finalDamage = Mathf.Max(Mathf.RoundToInt(damage), 1);
        Debug.Log($"{Time.time}: {m_golem.m_name} takes {finalDamage} damage");
        m_golem.m_health -= finalDamage;
        if (m_golem.m_health < 0)
        {
            m_golem.m_health = 0;
        }
        m_slotController.SetHealth(m_golem);
    }

    public void SetSelectorActive(bool active) => m_slotController.SetSelectorActive(active);
}

public enum CombatState
{
    WaitingForNextTurn,
    SelectingAction,
    PerformingAction,
    OutOfCombat
}

public class CombatManager : MonoBehaviour
{

    [SerializeField] GameObject m_golemSlotPrefab, m_skillListItemPrefab;
    [SerializeField] RectTransform[] m_teamSlotParents;
    [SerializeField] TextMeshProUGUI m_combatLogTextMesh;
    [SerializeField] RectTransform m_skillList;

    public const int BaseSpeed = 100;
    public const int TimePerAction = 10000;

    readonly List<GolemInCombat> m_golemsInCombat = new List<GolemInCombat>();
    public CombatState m_combatState { get; private set; } = CombatState.OutOfCombat;
    public int m_lastWinningTeam { get; private set; } = -1;

    //current turn update
    Akir.Coroutine m_currentTurnUpdate;

    //current turn stuff
    GolemInCombat m_currentTurnOwner = null;
    BaseSkillData m_currentTurnSkill = null;
    GolemInCombat m_currentTurnTarget = null;
    bool m_allowPickTarget = false;

    //skills list
    readonly List<GolemListItemController> m_skillListItems = new List<GolemListItemController>();

    void Awake()
    {
        //initialize coroutine
        m_currentTurnUpdate = new Akir.Coroutine(this);
    }

    void ResetSkillList()
    {
        for (int X = 0; X < m_skillListItems.Count; ++X)
        {
            m_skillListItems[X].Pool();
        }
        m_skillListItems.Clear();
    }

    //reset combat
    public void ResetCombat()
    {
        for (int X = 0; X < m_golemsInCombat.Count; ++X)
        {
            m_golemsInCombat[X].PoolSlotController();
        }
        m_golemsInCombat.Clear();
        ResetSkillList();
        m_currentTurnOwner = null;
        m_currentTurnUpdate.Stop();
        m_combatState = CombatState.OutOfCombat;
    }

    void OnGolemClicked(GolemInCombat clicked)
    {
        if (m_allowPickTarget)
        {
            m_currentTurnTarget = clicked;
            m_allowPickTarget = false;
        }
    }

    public void AddGolemIntoCombat(GolemData golem, int team)
    {
        GolemSlotController golemSlot = GolemSlotController.GetFromPool(m_golemSlotPrefab);
        GolemInCombat golemInCombat = new GolemInCombat(golem, golemSlot, team);
        golemSlot.Initialize(m_teamSlotParents[team], () => OnGolemClicked(golemInCombat));
        golemSlot.SetGolemData(golem);
        m_golemsInCombat.Add(golemInCombat);
        m_currentTurnOwner = null;
    }

    public void StartCombat()
    {
        if (m_combatState == CombatState.OutOfCombat)
            m_combatState = CombatState.WaitingForNextTurn;

        m_combatLogTextMesh.text = "Entering combat!";
        m_currentTurnUpdate.Start(WaitForLeftClickCoroutine());
    }

    //update
    void Update()
    {
        //wait for combat process
        if (m_combatState == CombatState.OutOfCombat) return;
        if (m_currentTurnUpdate.Running) return;

        //update based on state
        switch (m_combatState)
        {
            case CombatState.WaitingForNextTurn:
                {
                    //deselect previous
                    m_currentTurnOwner?.SetSelectorActive(false);

                    //get the golem with lowest action point
                    m_currentTurnOwner = m_golemsInCombat.OrderBy(g => g.m_currentActionValue).First();
                    m_currentTurnOwner.SetSelectorActive(true);
                    m_currentTurnSkill = null;
                    m_currentTurnTarget = null;
                    m_allowPickTarget = false;

                    //fast-forward time to the start of this golem's turn
                    int advanceTime = m_currentTurnOwner.m_currentActionValue;
                    for (int X = 0; X < m_golemsInCombat.Count; ++X)
                        m_golemsInCombat[X].m_currentActionValue -= advanceTime;
                    
                    //go next state
                    m_combatState = CombatState.SelectingAction;
                    m_combatLogTextMesh.text = $"It's {m_currentTurnOwner.m_golem.m_name}'s turn!";
                    m_currentTurnUpdate.Start(WaitForLeftClickCoroutine());
                    break;
                }
            case CombatState.SelectingAction:
                {
                    //check if obedient
                    if (m_currentTurnOwner.m_team == 0 && m_currentTurnOwner.m_golem.DoStatsRoll(GolemStatType.Obedience))
                    {
                        //is obedient; wait for user action
                        m_combatLogTextMesh.text = $"{m_currentTurnOwner.m_golem.m_name} is awaiting your command.";
                        m_currentTurnUpdate.Start(WaitForManuallySelectSkillCoroutine());
                        break;
                    }
                    
                    //auto battle
                    if (m_currentTurnOwner.m_golem.DoStatsRoll(GolemStatType.Intelligence))
                    {
                        //is intelligent; pick an intelligent action
                        m_currentTurnSkill = m_currentTurnOwner.ChooseIntelligentAction(m_golemsInCombat, out m_currentTurnTarget);
                    }
                    else
                    {
                        //is stupid; pick random
                        m_currentTurnSkill = m_currentTurnOwner.m_golem.m_skills[Random.Range(0, m_currentTurnOwner.m_golem.m_skills.Count)];
                        if (m_currentTurnSkill.m_isSingleTarget)
                        {
                            //pick a random golem based on team
                            GolemInCombat[] targets = GetAllInTeamOrNot(m_golemsInCombat, m_currentTurnOwner.m_team, m_currentTurnSkill.m_isHarmful);
                            m_currentTurnTarget = targets[Random.Range(0, targets.Length)];
                        }
                    }
                    m_combatState = CombatState.PerformingAction;
                    break;
                }
            case CombatState.PerformingAction:
                {
                    //perform action
                    m_currentTurnUpdate.Start(PerformActionCoroutine());
                    break;
                }
        }
    }

    public static GolemInCombat[] GetAllInTeamOrNot(IEnumerable<GolemInCombat> golems, int team, bool getNotInTeam)
    {
        return golems.Where(g => (g.m_team == team) ^ getNotInTeam).ToArray();
    }

    IEnumerator WaitForManuallySelectSkillCoroutine()
    {
        //pick skill
        for (int X = 0; X < m_currentTurnOwner.m_golem.m_skills.Count; ++X)
        {
            BaseSkillData skill = m_currentTurnOwner.m_golem.m_skills[X];
            GolemListItemController listItem = GolemListItemController.GetFromPool(m_skillListItemPrefab);
            m_skillListItems.Add(listItem);

            Sprite icon = skill.GetIcon(out Color? iconColor);
            listItem.Initialize(m_skillList, skill.name, null, icon, iconColor, () => ManuallySelectSkill(skill));
        }
        while (m_combatState == CombatState.SelectingAction) yield return null;
    }

    IEnumerator WaitForManuallySelectTargetCoroutine()
    {
        //pick skill
        while (m_currentTurnTarget == null) yield return null;
    }

    void ManuallySelectSkill(BaseSkillData skill)
    {
        m_currentTurnSkill = skill;
        m_currentTurnTarget = null;
        if (m_currentTurnSkill.m_isSingleTarget)
        {
            //wait for picking target
            m_combatLogTextMesh.text = $"Select a target for {m_currentTurnSkill.name}.";
            m_allowPickTarget = true;
            m_currentTurnUpdate.Start(WaitForManuallySelectTargetCoroutine());
        }
        else
        {
            //aight go
            m_combatState = CombatState.PerformingAction;
        }
    }

    IEnumerator WaitForLeftClickCoroutine()
    {
        do
        {
            yield return null;
        }
        while (!Input.GetMouseButtonDown(0));
    }

    IEnumerator PerformActionCoroutine()
    {
        //reset skill list
        ResetSkillList();

        //perform action
        if (m_currentTurnSkill != null)
        {
            m_currentTurnSkill.PerformAction(m_currentTurnOwner, m_golemsInCombat, m_currentTurnTarget, out string log);
            m_combatLogTextMesh.text = log;
        }
        else
        {
            
            m_combatLogTextMesh.text = $"{m_currentTurnOwner.m_golem.m_name} does nothing!";
        }

        //reset action value
        m_currentTurnOwner.ResetActionValue();

        //check if game over
        m_lastWinningTeam = -1;
        bool isWin = true;
        for (int X = m_golemsInCombat.Count - 1; X > -1; --X)
        {
            GolemInCombat golemInCombat = m_golemsInCombat[X];
            if (golemInCombat.m_golem.m_health > 0)
            {
                //still alive
                if (m_lastWinningTeam == -1)
                {
                    m_lastWinningTeam = golemInCombat.m_team;
                }
                else if (m_lastWinningTeam != golemInCombat.m_team)
                {
                    isWin = false;
                }
            }
            else
            {
                //no longer alive
                yield return WaitForLeftClickCoroutine();
                m_combatLogTextMesh.text = $"{golemInCombat.m_golem.m_name} has been defeated!";
                yield return WaitForLeftClickCoroutine();

                m_golemsInCombat.RemoveAt(X);
                golemInCombat.PoolSlotController();
            }
        }
        if (isWin)
        {
            //someone won...
            m_combatState = CombatState.OutOfCombat;
            Debug.Log($"{Time.time}: team {m_lastWinningTeam} won");
        }
        else
        {
            //the battle continues...
            m_lastWinningTeam = -1;
            m_combatState = CombatState.WaitingForNextTurn;
        }

        //wait for user input
        yield return WaitForLeftClickCoroutine();
    }
}
