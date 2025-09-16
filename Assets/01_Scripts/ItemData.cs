using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Shop/Item")]
public class ItemData : ScriptableObject
{
    public string key = "MasDanio";
    public string displayName = "Más Daño";
    public GameObject displayPrefab;
    public Sprite icon;

    public List<StatDelta> deltas = new List<StatDelta>();

    public int minPrice = 5;
    public int maxPrice = 15;

    public int RollPrice()
    {
        return Mathf.Clamp(Random.Range(minPrice, maxPrice + 1), 0, 999999);
    }
}