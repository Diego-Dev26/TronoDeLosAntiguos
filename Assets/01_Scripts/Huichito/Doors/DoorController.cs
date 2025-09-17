using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Tooltip("Si lo dejas vacío, se toman Renderers/Colliders del propio GameObject y sus hijos.")]
    public List<Renderer> renderers = new List<Renderer>();
    public List<Collider> colliders = new List<Collider>();

    [Header("Estado")]
    public bool isClosedOnStart = true;

    void Awake()
    {
        if (renderers.Count == 0) renderers.AddRange(GetComponentsInChildren<Renderer>(true));
        if (colliders.Count == 0) colliders.AddRange(GetComponentsInChildren<Collider>(true));
        Apply(isClosedOnStart);
    }

    public void Open() => Apply(false);
    public void Close() => Apply(true);

    void Apply(bool closed)
    {
        foreach (var r in renderers) if (r) r.enabled = closed;
        foreach (var c in colliders) if (c) c.enabled = closed;
        // Si prefieres ocultar completamente:
        // gameObject.SetActive(closed);
    }
}
