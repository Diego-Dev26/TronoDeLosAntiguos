using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemRegistry : MonoBehaviour
{
    public static ItemRegistry Instance { get; private set; }
    private readonly Dictionary<string, ItemData> _byKey = new Dictionary<string, ItemData>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        var all = Resources.LoadAll<ItemData>("Items");
        foreach (var it in all)
        {
            if (string.IsNullOrEmpty(it.key)) continue;
            _byKey[it.key] = it;
        }
    }

    public ItemData Get(string key)
    {
        _byKey.TryGetValue(key, out var data);
        return data;
    }
}