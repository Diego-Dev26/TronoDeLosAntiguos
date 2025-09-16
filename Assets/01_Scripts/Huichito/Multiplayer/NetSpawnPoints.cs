// NetSpawnPoints.cs
using System.Linq;
using Photon.Pun;
using UnityEngine;

public class NetSpawnPoints : MonoBehaviour
{
    static Transform[] points;

    void Awake()
    {
        // busca todos los empties con tag PlayerSpawn
        points = GameObject.FindGameObjectsWithTag("PlayerSpawn")
                 .Select(go => go.transform).OrderBy(t => t.name).ToArray();
    }

    public static Vector3 GetSpawn()
    {
        if (points != null && points.Length > 0)
        {
            int idx = (PhotonNetwork.LocalPlayer.ActorNumber - 1) % points.Length;
            return points[idx].position;
        }
        // fallback: círculo
        int n = PhotonNetwork.LocalPlayer.ActorNumber;
        float r = 3f, ang = (n - 1) * Mathf.PI * 2f / 8f;
        return new Vector3(Mathf.Cos(ang), 1f, Mathf.Sin(ang)) * r;
    }
}
