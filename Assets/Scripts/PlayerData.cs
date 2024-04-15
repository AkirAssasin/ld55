using System.Collections.Generic;
using UnityEngine;

public class PlayerData
{
    //material ID to count
    public Dictionary<int, int> m_inventory = new Dictionary<int, int>();

    //health potions
    public int m_potionCount = 0;

    //golems
    public List<GolemData> m_golems = new List<GolemData>();
    public int m_maxGolemSlots { get; private set; } = 10;

    public PlayerData()
    {
        //temp
        AddIntoInventory(0, 1);
    }

    public void AddIntoInventory(int materialID, int count)
    {
        if (materialID == -1)
        {
            m_potionCount += count;
            return;
        }
        if (m_inventory.TryGetValue(materialID, out int existing)) count += existing;
        m_inventory[materialID] = count;
    }

    public int RemoveFromInventory(int materialID, int count)
    {
        if (materialID == -1)
        {
            m_potionCount -= count;
            return m_potionCount;
        }
        int remaining = (m_inventory[materialID] -= count);
        if (remaining <= 0)
        {
            remaining = 0;
            m_inventory.Remove(materialID);
        }
        return remaining;
    }

    public bool HasOpenGolemSlot()
    {
        return m_golems.Count < m_maxGolemSlots;
    }
}
