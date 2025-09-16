using Photon.Pun;
using UnityEngine;
using TMPro;
using System.Collections;

public class PlayerHUDPhoton : MonoBehaviour
{
    public TMP_Text coinsText;
    public TMP_Text damageText;
    public TMP_Text healthText;
    public TMP_Text moveText;
    public TMP_Text atkText;
    public TMP_Text statusText; // opcional para mensajes ("Compra realizada", "Monedas insuficientes", etc.)

    PlayerWalletPhoton wallet;
    PlayerStatsPhoton stats;

    void Start()
    {
        StartCoroutine(BindLocalPlayer());
    }

    IEnumerator BindLocalPlayer()
    {
        // espera a que spawnee el player local
        while (wallet == null || stats == null)
        {
            var all = FindObjectsOfType<PhotonView>();
            foreach (var pv in all)
            {
                if (pv.IsMine)
                {
                    if (wallet == null) wallet = pv.GetComponent<PlayerWalletPhoton>();
                    if (stats == null) stats = pv.GetComponent<PlayerStatsPhoton>();
                }
            }
            yield return null;
        }
        StartCoroutine(RefreshLoop());
    }

    IEnumerator RefreshLoop()
    {
        var wait = new WaitForSeconds(0.15f);
        while (true)
        {
            if (wallet) coinsText.text = $"Coins: {wallet.coins}";
            if (stats)
            {
                damageText.text = $"DMG: {stats.damage:0.##}";
                healthText.text = $"HP: {stats.currentHealth:0.##}/{stats.maxHealth:0.##}";
                moveText.text = $"Move: {stats.moveSpeed:0.##}";
                atkText.text = $"AtkSpd: {stats.attackSpeed:0.##}";
            }
            yield return wait;
        }
    }

    public void ShowStatus(string msg, float seconds = 1.2f)
    {
        if (!statusText) return;
        StopAllCoroutines();
        StartCoroutine(StatusRoutine(msg, seconds));
        StartCoroutine(RefreshLoop()); // reanudar bucle
    }

    IEnumerator StatusRoutine(string msg, float seconds)
    {
        statusText.text = msg;
        statusText.gameObject.SetActive(true);
        yield return new WaitForSeconds(seconds);
        statusText.gameObject.SetActive(false);
    }
}
