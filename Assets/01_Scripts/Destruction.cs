using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destruction : MonoBehaviour
{
    public string avoidTag = "Player";
    public string onlyDestroyTag = "SpawnPoint";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(avoidTag))
            return;

        if (string.IsNullOrEmpty(onlyDestroyTag) || other.CompareTag(onlyDestroyTag))
        {
            Destroy(other.gameObject);
        }
    }
}
