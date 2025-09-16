using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemRegistry : MonoBehaviour
{
    public static ItemRegistry Instance { get; private set; }

    private static Dictionary<string, ItemData> _byKey = new Dictionary<string, ItemData>();

    // Se ejecuta antes de cargar la primera escena
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Instance == null)
        {
            var go = new GameObject("~ItemRegistry");
            Instance = go.AddComponent<ItemRegistry>();
            DontDestroyOnLoad(go);
            Instance.LoadAll();
        }
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        if (_byKey.Count == 0) LoadAll();
    }

    void LoadAll()
    {
        _byKey.Clear();
        var all = Resources.LoadAll<ItemData>("Items"); // -> Assets/Resources/Items/*.asset
        Debug.Log("[ItemRegistry] Cargando items desde Resources/Items: " + all.Length);
        for (int i = 0; i < all.Length; i++)
        {
            var it = all[i];
            if (it == null) continue;
            if (string.IsNullOrEmpty(it.key))
            {
                Debug.LogWarning("[ItemRegistry] Item con key vacía: " + it.name);
                continue;
            }
            _byKey[it.key] = it;
            Debug.Log("[ItemRegistry] + " + it.key + " (" + it.displayName + ")");
        }
    }

    public ItemData Get(string key)
    {
        if (string.IsNullOrEmpty(key)) return null;
        ItemData data;
        _byKey.TryGetValue(key, out data);
        return data;
    }
}