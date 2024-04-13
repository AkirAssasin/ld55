using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData
{
    //material ID to count
    public Dictionary<int, int> m_inventory = new Dictionary<int, int>();

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

    public void RemoveFromInventory(int materialID, int count)
    {
        int remaining = (m_inventory[materialID] -= count);
        if (remaining <= 0)
        {
            m_inventory.Remove(materialID);
        }
    }
}
