using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if PHOTON_UNITY_NETWORKING
using Photon.Pun;
#endif

public class GameCharacterSpawner : MonoBehaviour
{
    [Header("Spawn")]
    public Transform spawnPoint;

    [Header("Prefabs por clase")]
    public GameObject meleeRapidoPrefab;
    public GameObject meleeTanquePrefab;
    public GameObject arqueroPrefab;
    public GameObject magoAOEPrefab;

    [Header("Red (Photon opcional)")]
    public bool usePhoton = false;

    void Start()
    {
        // 1) leer lo seleccionado
        string id = PlayerPrefs.GetString("selected_character_id", "MeleeRapido");

        // 2) elegir prefab
        GameObject prefab = GetPrefabFor(id);
        if (!prefab) { Debug.LogWarning("ID no reconocido: " + id + ". Usando MeleeRapido."); prefab = meleeRapidoPrefab; }

        // 3) spawn
        Vector3 pos = spawnPoint ? spawnPoint.position : Vector3.zero;
        Quaternion rot = spawnPoint ? spawnPoint.rotation : Quaternion.identity;

        GameObject go;
#if PHOTON_UNITY_NETWORKING
        go = usePhoton ? PhotonNetwork.Instantiate(prefab.name, pos, rot) : Instantiate(prefab, pos, rot);
#else
        go = Instantiate(prefab, pos, rot);
#endif

        // 4) aplicar clase (si el prefab es genérico)
        var selector = go.GetComponent<PlayerClassSelectorSimple>();
        if (selector != null)
        {
            selector.clase = ParseClass(id);
            selector.ApplyClass(); // fuerza stats/armas según clase
        }
    }

    GameObject GetPrefabFor(string id)
    {
        switch (id)
        {
            case "MeleeRapido": return meleeRapidoPrefab;
            case "MeleeTanque": return meleeTanquePrefab;
            case "Arquero": return arqueroPrefab;
            case "MagoAOE": return magoAOEPrefab;
            default: return null;
        }
    }

    PlayerClassSelectorSimple.PlayerClass ParseClass(string id)
    {
        switch (id)
        {
            case "MeleeRapido": return PlayerClassSelectorSimple.PlayerClass.MeleeRapido;
            case "MeleeTanque": return PlayerClassSelectorSimple.PlayerClass.MeleeTanque;
            case "Arquero": return PlayerClassSelectorSimple.PlayerClass.Arquero;
            case "MagoAOE": return PlayerClassSelectorSimple.PlayerClass.MagoAOE;
            default: return PlayerClassSelectorSimple.PlayerClass.MeleeRapido;
        }
    }
}
