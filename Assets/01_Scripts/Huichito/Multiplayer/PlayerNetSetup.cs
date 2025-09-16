// PlayerNetSetup.cs
using System.Linq;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class PlayerNetSetup : MonoBehaviourPunCallbacks
{
    [Tooltip("Scripts que SOLO deben correr en el cliente local (movimiento, combate, input, etc.).")]
    public MonoBehaviour[] localOnly;

    [Header("Opcional si tu prefab trae cámara propia")]
    [SerializeField] Camera[] camerasInChildren;
    [SerializeField] AudioListener[] audioListenersInChildren;

    void Awake()
    {
        // Autodescubrimiento por si te olvidas de asignar arrays:
        if (camerasInChildren == null || camerasInChildren.Length == 0)
            camerasInChildren = GetComponentsInChildren<Camera>(true);
        if (audioListenersInChildren == null || audioListenersInChildren.Length == 0)
            audioListenersInChildren = GetComponentsInChildren<AudioListener>(true);
    }

    void Start()
    {
        bool isLocal = photonView.IsMine;

        // 1) Scripts de input SOLO en el local
        if (!isLocal)
            foreach (var mb in localOnly) if (mb) mb.enabled = false;

        // 2) Cámaras/AudioListener del prefab: solo en local
        foreach (var c in camerasInChildren) if (c) c.enabled = isLocal;
        foreach (var al in audioListenersInChildren) if (al) al.enabled = isLocal;

        // 3) Cámara de escena que sigue al local
        if (isLocal)
        {
            var cam = Camera.main;
            if (cam)
            {
                var follow = cam.GetComponent<CameraFollow>();
                if (follow) follow.target = transform;
            }
        }
    }
}
