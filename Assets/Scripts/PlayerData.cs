using System.Collections.Generic;
using UnityEngine;

public class PlayerData
{
    //material ID to count
    public Dictionary<int, int> m_inventory = new Dictionary<int, int>();

    //golems
    public List<GolemData> m_golems = new List<GolemData>();
    public int m_maxGolemSlots { get; private set; } = 10;

    public PlayerData()
    {
        //temp
        for (int X = 0; X < 10; ++X)
        {
            AddIntoInventory(Random.Range(0, GameManager.GetMaterialCount()), 2);
        }
    }

    public void AddIntoInventory(int materialID, int count)
    {
        if (m_inventory.TryGetValue(materialID, out int existing)) count += existing;
        m_inventory[materialID] = count;
    }

    public int RemoveFromInventory(int materialID, int count)
    {
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
