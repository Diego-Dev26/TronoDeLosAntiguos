using System.Collections.Generic;
using UnityEngine;

public class ShopInventory : MonoBehaviour
{
    public List<ItemData> lootTable = new List<ItemData>();
    public ShopPlate[] plates;
    public bool allowDuplicates = false;

    void Start()
    {
        if (plates == null || plates.Length == 0 || lootTable == null || lootTable.Count == 0) return;

        var used = new HashSet<ItemData>();
        for (int i = 0; i < plates.Length; i++)
        {
            ItemData pick = Random.Range(0, lootTable.Count) >= 0 ? lootTable[Random.Range(0, lootTable.Count)] : null;
            if (!allowDuplicates)
            {
                int guard = 0;
                while (pick != null && used.Contains(pick) && guard++ < 50)
                    pick = lootTable[Random.Range(0, lootTable.Count)];
            }
            if (pick != null)
            {
                plates[i].item = pick;
                // Forzar re-init si la placa ya estaba en escena:
                // (Solo necesario si quieres que se refresque también en Editor)
            }
            used.Add(pick);
        }
    }
}
